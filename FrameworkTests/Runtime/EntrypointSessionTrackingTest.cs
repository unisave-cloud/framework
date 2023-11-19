using System;
using LightJson;
using NUnit.Framework;
using Unisave.Facets;
using Unisave.Sessions;

namespace FrameworkTests.Runtime
{
    [TestFixture]
    public class EntrypointSessionTrackingTest : EntrypointFixture
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

        public override void SetUpEntrypointContext()
        {
            SetEntrypointContext(new Type[] {
                typeof(MyFacet)
            });
        }
        
        [Test]
        public void ItStartsNewSession()
        {
            MyFacet.lastSessionId = null;
            EntrypointResponse response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray(),
                sessionId: null
            );
            response.AssertReturned(JsonValue.Null);
            Assert.IsNotNull(MyFacet.lastSessionId);
            response.AssertHasSpecial("sessionId", MyFacet.lastSessionId);
        }
        
        [Test]
        public void ItRefreshesSession()
        {
            MyFacet.lastSessionId = null;
            EntrypointResponse response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray(),
                sessionId: "123456789"
            );
            response.AssertReturned(JsonValue.Null);
            Assert.AreEqual("123456789", MyFacet.lastSessionId);
            response.AssertHasSpecial("sessionId", MyFacet.lastSessionId);
        }
    }
}