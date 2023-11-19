using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameworkTests.Testing;
using FrameworkTests.Testing.Facets;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Facets;
using Unisave.Sessions;

namespace FrameworkTests.Sessions
{
    [TestFixture]
    public class FacetsTrackSessionsTest : BackendApplicationFixture
    {
        #region "Backend definition"
        
        public class MyFacet : Facet
        {
            public static string lastSessionId = null;

            public MyFacet(ServerSessionIdRepository repo)
            {
                lastSessionId = repo.SessionId;
            }
            
            public void MyProcedure()
            {
                // nothing
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
        public async Task ItStartsNewSession()
        {
            MyFacet.lastSessionId = null;
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray(),
                request => {
                    request.Headers.Remove("Cookie");
                }
            );
            await response.AssertOkVoidResponse();
            Assert.IsNotNull(MyFacet.lastSessionId);

            IList<string> setCookies = response.Headers.GetValues("Set-Cookie");
            Assert.IsNotNull(setCookies);
            string sessionCookie = setCookies.FirstOrDefault(
                c => c.Contains("unisave_session_id")
            );
            Assert.IsNotNull(sessionCookie);
            Assert.IsTrue(sessionCookie.StartsWith(
                "unisave_session_id=" + Uri.EscapeDataString(MyFacet.lastSessionId)
            ));
            
            Assert.IsTrue(sessionCookie.ToLowerInvariant().Contains("path=/"));
            Assert.IsTrue(sessionCookie.ToLowerInvariant().Contains("httponly"));
            Assert.IsTrue(sessionCookie.ToLowerInvariant().Contains("expires="));
        }
        
        [Test]
        public async Task ItRefreshesSession()
        {
            MyFacet.lastSessionId = null;
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray(),
                request => {
                    var key = Uri.EscapeDataString("unisave_session_id");
                    var value = Uri.EscapeDataString("123456789");
                    request.Headers["Cookie"] = $"{key}={value};";
                }
            );
            await response.AssertOkVoidResponse();
            Assert.AreEqual("123456789", MyFacet.lastSessionId);
            
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
    }
}