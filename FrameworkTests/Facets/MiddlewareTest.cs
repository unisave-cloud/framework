using FrameworkTests.Facets.Stubs;
using FrameworkTests.TestingUtils;
using NUnit.Framework;

namespace FrameworkTests.Facets
{
    [TestFixture]
    public class MiddlewareTest
    {
        /// <summary>
        /// This field is edited by the middleware as it runs
        /// </summary>
        public static string middlewareLog;
        
        [Test]
        public void ItRunsMiddlewareAsProperlyDefined()
        {
            middlewareLog = null;
            ExecuteFramework.Begin()
                .ExecuteFacet<FacetWithMiddleware>("MethodWithoutMiddleware")
                .AssertSuccess();
            Assert.AreEqual("C1,C2,B2,C2',C1'", middlewareLog);
            
            middlewareLog = null;
            ExecuteFramework.Begin()
                .ExecuteFacet<FacetWithMiddleware>("MethodWithMiddleware")
                .AssertSuccess();
            Assert.AreEqual("C1,C2,M1,M2,B1,M2',M1',C2',C1'", middlewareLog);
        }
    }
}