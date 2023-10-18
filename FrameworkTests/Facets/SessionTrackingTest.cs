using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
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
            
            Assert.IsTrue(sessionCookie.Contains("Path=/"));
            Assert.IsTrue(sessionCookie.Contains("HttpOnly"));
            Assert.IsTrue(sessionCookie.Contains("Expires="));
            Assert.IsTrue(sessionCookie.Contains("Max-Age="));
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
            
            Assert.IsTrue(sessionCookie.Contains("Path=/"));
            Assert.IsTrue(sessionCookie.Contains("HttpOnly"));
            Assert.IsTrue(sessionCookie.Contains("Expires="));
            Assert.IsTrue(sessionCookie.Contains("Max-Age="));
        }
    }
}