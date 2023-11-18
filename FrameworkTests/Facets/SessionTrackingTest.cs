using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Sessions;

namespace FrameworkTests.Facets
{
    [TestFixture]
    public class SessionTrackingTest : BaseFacetCallingTest
    {
        #region "Backend definition"
        
        public class MyFacet : Facet
        {
            public static string LastSessionId = null;

            public MyFacet(ServerSessionIdRepository repo)
            {
                LastSessionId = repo.SessionId;
            }
            
            public void MyProcedure()
            {
                // nothing
            }

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
        
        [SetUp]
        public void SetUp()
        {
            CreateApplication(new Type[] {
                typeof(MyFacet)
            });
        }
        
        /*
         * TODO: test session, logging, legacy entrypoint
         * - add request-scoped services & facades
         * - test concurrency (asynchronous & multithreading)
         */

        [Test]
        public async Task ItStartsNewSession()
        {
            MyFacet.LastSessionId = null;
            IOwinResponse response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray()
            );
            await AssertOkVoidResponse(response);
            Assert.IsNotNull(MyFacet.LastSessionId);

            IList<string> setCookies = response.Headers.GetValues("Set-Cookie");
            Assert.IsNotNull(setCookies);
            string sessionCookie = setCookies.FirstOrDefault(
                c => c.Contains("unisave_session_id")
            );
            Assert.IsNotNull(sessionCookie);
            Assert.IsTrue(
                sessionCookie.StartsWith("unisave_session_id=" + MyFacet.LastSessionId + ";")
            );
            
            Assert.IsTrue(sessionCookie.ToLowerInvariant().Contains("path=/"));
            Assert.IsTrue(sessionCookie.ToLowerInvariant().Contains("httponly"));
            Assert.IsTrue(sessionCookie.ToLowerInvariant().Contains("expires="));
        }
        
        [Test]
        public async Task ItRefreshesSession()
        {
            MyFacet.LastSessionId = null;
            IOwinResponse response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray(),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=123456789;";
                }
            );
            await AssertOkVoidResponse(response);
            Assert.AreEqual("123456789", MyFacet.LastSessionId);
            
            IList<string> setCookies = response.Headers.GetValues("Set-Cookie");
            Assert.IsNotNull(setCookies);
            string sessionCookie = setCookies.FirstOrDefault(
                c => c.Contains("unisave_session_id")
            );
            Assert.IsNotNull(sessionCookie);
            Assert.IsTrue(
                sessionCookie.StartsWith("unisave_session_id=123456789")
            );
            
            Assert.IsTrue(sessionCookie.ToLowerInvariant().Contains("path=/"));
            Assert.IsTrue(sessionCookie.ToLowerInvariant().Contains("httponly"));
            Assert.IsTrue(sessionCookie.ToLowerInvariant().Contains("expires="));
        }

        [Test]
        public async Task SessionStoresValue()
        {
            // check two session have no foo set
            
            IOwinResponse response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.GetFoo),
                arguments: new JsonArray(),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=firstSession;";
                }
            );
            var firstFoo = await GetReturnedValue<string>(response);
            Assert.IsNull(firstFoo);
            
            response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.GetFoo),
                arguments: new JsonArray(),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=secondSession;";
                }
            );
            var secondFoo = await GetReturnedValue<string>(response);
            Assert.IsNull(secondFoo);
            
            // set values for both sessions
            
            await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.SetFoo),
                arguments: new JsonArray("first"),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=firstSession;";
                }
            );
            
            await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.SetFoo),
                arguments: new JsonArray("second"),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=secondSession;";
                }
            );
            
            // check both sessions have their proper values
            
            response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.GetFoo),
                arguments: new JsonArray(),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=firstSession;";
                }
            );
            firstFoo = await GetReturnedValue<string>(response);
            Assert.AreEqual("first", firstFoo);
            
            response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.GetFoo),
                arguments: new JsonArray(),
                request => {
                    request.Headers["Cookie"] = "unisave_session_id=secondSession;";
                }
            );
            secondFoo = await GetReturnedValue<string>(response);
            Assert.AreEqual("second", secondFoo);
        }
    }
}