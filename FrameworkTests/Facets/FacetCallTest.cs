using NUnit.Framework;
using FrameworkTests.Facets.Stubs;
using FrameworkTests.TestingUtils;
using Unisave.Exceptions;
using Unisave.Facets;
using Unisave.Utils;

namespace FrameworkTests.Facets
{
    [TestFixture]
    public class FacetCallTest : BackendTestCase
    {
        [Test]
        public void ItRunsMyProcedure()
        {
            SomeFacet.flag = false;
            
            Assert.IsNull(SessionId);
            
            OnFacet<SomeFacet>().CallSync(
                nameof(SomeFacet.MyProcedure)
            );
            
            Assert.IsNotNull(SessionId);

            Assert.IsTrue(SomeFacet.flag);
        }

        [Test]
        public void ItRunsMyParametrizedProcedure()
        {
            SomeFacet.flag = false;
            
            OnFacet<SomeFacet>().CallSync(
                nameof(SomeFacet.MyParametrizedProcedure),
                true
            );
            Assert.IsTrue(SomeFacet.flag);
            
            OnFacet<SomeFacet>().CallSync(
                nameof(SomeFacet.MyParametrizedProcedure),
                false
            );
            Assert.IsFalse(SomeFacet.flag);
        }

        [Test]
        public void ItChecksParentFacet()
        {
            var e = Assert.Catch<FacetSearchException>(() => {
                OnFacet("WrongFacet").CallSync(
                    nameof(SomeFacet.MyProcedure)
                );
            });
            
            StringAssert.Contains("Facet 'WrongFacet' was not found.", e.Message);
        }

        [Test]
        public void ItChecksMethodExistence()
        {
            var e = Assert.Catch<MethodSearchException>(() => {
                OnFacet<SomeFacet>().CallSync(
                    "NonExistingMethod"
                );
            });
            
            StringAssert.Contains("doesn't have public method called", e.ToString());
        }

        [Test]
        public void ItChecksAmbiguousMethods()
        {
            var e = Assert.Catch<MethodSearchException>(() => {
                OnFacet<SomeFacet>().CallSync(
                    nameof(SomeFacet.AmbiguousMethod),
                    false
                );
            });
            
            StringAssert.Contains("has multiple methods called", e.ToString());
        }

        [Test]
        public void ItChecksPublicMethods()
        {
            var e = Assert.Catch<MethodSearchException>(() => {
                OnFacet<SomeFacet>().CallSync(
                    "PrivateProcedure"
                );
            });
            
            StringAssert.Contains("has to be public in order to be called", e.ToString());
        }

        [Test]
        public void ItRunsFunctions()
        {
            int result = OnFacet<SomeFacet>().CallSync<int>(
                nameof(SomeFacet.SquaringFunction),
                5
            );
            
            Assert.AreEqual(25, result);
        }
    }
}
