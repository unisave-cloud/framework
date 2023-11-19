using System;
using System.Threading.Tasks;
using FrameworkTests.Testing;
using FrameworkTests.Testing.Facets;
using FrameworkTests.Testing.Foundation;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Facets;

namespace FrameworkTests.Facets
{
    [TestFixture]
    public class FacetCallTest : BackendApplicationFixture
    {
        #region "Backend definition"
        
        public class MyFacet : Facet
        {
            public static bool flag = false;
            
            public void MyProcedure()
            {
                flag = true;
            }
            
            public void MyParametrizedProcedure(bool value)
            {
                flag = value;
            }

            public int SquaringFunction(int value)
            {
                return value * value;
            }

            public void ThrowingProcedure()
            {
                throw new InvalidOperationException("Hello world!");
            }
        }
        
        #endregion

        public override void SetUpBackendApplication()
        {
            CreateApplication(new Type[] {
                typeof(MyFacet)
            });
        }

        [Test]
        public async Task ItRunsMyProcedure()
        {
            MyFacet.flag = false;
            
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray()
            );
            await response.AssertOkVoidResponse();
            
            // check that facet was actually invoked
            Assert.IsTrue(MyFacet.flag);
        }

        [Test]
        public async Task ItRunsMyParametrizedProcedure()
        {
            MyFacet.flag = false;
            
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyParametrizedProcedure),
                arguments: new JsonArray() { true }
            );
            await response.AssertOkVoidResponse();
            
            Assert.IsTrue(MyFacet.flag);
            
            response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyParametrizedProcedure),
                arguments: new JsonArray() { false }
            );
            await response.AssertOkVoidResponse();
            
            Assert.IsFalse(MyFacet.flag);
        }

        [Test]
        public async Task ItRunsFunctions()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.SquaringFunction),
                arguments: new JsonArray() { 5 }
            );
            int returned = await response.GetReturnedValue<int>();
            Assert.AreEqual(25, returned);
        }
        
        [Test]
        public async Task ItHandlesUnknownExceptions()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.ThrowingProcedure),
                arguments: new JsonArray()
            );
            Exception e = await response.GetThrownException<Exception>();
            Assert.IsInstanceOf<InvalidOperationException>(e);
            Assert.AreEqual("Hello world!", e.Message);
            
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            Assert.IsFalse(body["isKnownException"].AsBoolean);
        }
    }
}