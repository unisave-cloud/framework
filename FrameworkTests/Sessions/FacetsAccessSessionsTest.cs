using System;
using System.Threading.Tasks;
using FrameworkTests.Testing;
using FrameworkTests.Testing.Facets;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Facades;
using Unisave.Facets;

namespace FrameworkTests.Sessions
{
    [TestFixture]
    public class FacetsAccessSessionsTest : BackendApplicationFixture
    {
        #region "Backend definition"
        
        public class MyFacet : Facet
        {
            public void SetFoo(string value)
            {
                Session.Set("foo", value);
            }

            public string GetFoo()
            {
                return Session.Get<string>("foo");
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
        public async Task SessionStoresValue()
        {
            // check two session have no foo set
            
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.GetFoo),
                arguments: new JsonArray(),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=firstSession;";
                }
            );
            var firstFoo = await response.GetReturnedValue<string>();
            Assert.IsNull(firstFoo);
            
            response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.GetFoo),
                arguments: new JsonArray(),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=secondSession;";
                }
            );
            var secondFoo = await response.GetReturnedValue<string>();
            Assert.IsNull(secondFoo);
            
            // set values for both sessions
            
            await (await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.SetFoo),
                arguments: new JsonArray("first"),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=firstSession;";
                }
            )).AssertOkVoidResponse();
            
            await (await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.SetFoo),
                arguments: new JsonArray("second"),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=secondSession;";
                }
            )).AssertOkVoidResponse();
            
            // check both sessions have their proper values
            
            response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.GetFoo),
                arguments: new JsonArray(),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=firstSession;";
                }
            );
            firstFoo = await response.GetReturnedValue<string>();
            Assert.AreEqual("first", firstFoo);
            
            response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.GetFoo),
                arguments: new JsonArray(),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=secondSession;";
                }
            );
            secondFoo = await response.GetReturnedValue<string>();
            Assert.AreEqual("second", secondFoo);
        }
    }
}