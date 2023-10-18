using System;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Facets;

namespace FrameworkTests.Facets
{
    [TestFixture]
    public class FacetCallTest : BaseFacetCallingTest
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

        [SetUp]
        public void SetUp()
        {
            CreateApplication(new Type[] {
                typeof(MyFacet)
            });
        }

        [Test]
        public async Task ItRunsMyProcedure()
        {
            MyFacet.flag = false;
            
            IOwinResponse response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray()
            );
            await AssertOkVoidResponse(response);
            
            // check that facet was actually invoked
            Assert.IsTrue(MyFacet.flag);
        }

        [Test]
        public async Task ItRunsMyParametrizedProcedure()
        {
            MyFacet.flag = false;
            
            IOwinResponse response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyParametrizedProcedure),
                arguments: new JsonArray() { true }
            );
            await AssertOkVoidResponse(response);
            
            Assert.IsTrue(MyFacet.flag);
            
            response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyParametrizedProcedure),
                arguments: new JsonArray() { false }
            );
            await AssertOkVoidResponse(response);
            
            Assert.IsFalse(MyFacet.flag);
        }

        [Test]
        public async Task ItRunsFunctions()
        {
            IOwinResponse response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.SquaringFunction),
                arguments: new JsonArray() { 5 }
            );
            int returned = await GetReturnedValue<int>(response);
            Assert.AreEqual(25, returned);
        }
        
        [Test]
        public async Task ItHandlesUnknownExceptions()
        {
            IOwinResponse response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.ThrowingProcedure),
                arguments: new JsonArray()
            );
            Exception e = await GetThrownException<Exception>(response);
            Assert.IsInstanceOf<InvalidOperationException>(e);
            Assert.AreEqual("Hello world!", e.Message);
            
            JsonObject body = await GetResponseBody(response);
            Assert.IsFalse(body["isKnownException"].AsBoolean);
        }
    }
}