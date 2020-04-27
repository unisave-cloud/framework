using FrameworkTests.Authentication.Stubs;
using FrameworkTests.TestingUtils;
using NUnit.Framework;
using Unisave.Authentication.Middleware;
using Unisave.Facades;

namespace FrameworkTests.Authentication
{
    [TestFixture]
    public class AuthenticateSessionTest : BackendTestCase
    {
        [Test]
        public void ItLoadsNull()
        {
            var returned = OnFacet<AuthStubFacet>().CallSync<AuthStubPlayer>(
                nameof(AuthStubFacet.GetAuthenticatedPlayer)
            );
            
            Assert.IsNull(returned);
        }
        
        [Test]
        public void ItLoadsAuthenticatedPlayer()
        {
            var player = new AuthStubPlayer { Name = "John" };
            player.Save();

            Session.Set(AuthenticateSession.SessionKey, player.EntityId);
            
            // HACK:
            // I need to figure out how to properly merge test facade access
            // with middleware logic so that it does not interfere.
            ActingAs(player);
            
            var returned = OnFacet<AuthStubFacet>().CallSync<AuthStubPlayer>(
                nameof(AuthStubFacet.GetAuthenticatedPlayer)
            );
            
            Assert.AreEqual("John", returned?.Name);
        }

        [Test]
        public void ItStoresAuthenticatedPlayer()
        {
            var player = new AuthStubPlayer { Name = "John" };
            player.Save();
            
            Assert.IsNull(
                Session.Get<string>(AuthenticateSession.SessionKey)
            );
            
            OnFacet<AuthStubFacet>().CallSync<AuthStubPlayer>(
                nameof(AuthStubFacet.Login), "John"
            );
            
            Assert.AreEqual(
                player.EntityId,
                Session.Get<string>(AuthenticateSession.SessionKey)
            );
        }

        [Test]
        public void ItStoresLogout()
        {
            var player = new AuthStubPlayer { Name = "John" };
            player.Save();
            
            Session.Set(AuthenticateSession.SessionKey, player.EntityId);
            
            OnFacet<AuthStubFacet>().CallSync<AuthStubPlayer>(
                nameof(AuthStubFacet.Logout)
            );
            
            Assert.IsNull(
                Session.Get<string>(AuthenticateSession.SessionKey)
            );
        }

        [Test]
        public void ItPreservesAuthenticatedUserFromTestCase()
        {
            var player = new AuthStubPlayer { Name = "John" };
            player.Save();

            ActingAs(player);
            var returned = OnFacet<AuthStubFacet>().CallSync<AuthStubPlayer>(
                nameof(AuthStubFacet.GetAuthenticatedPlayer)
            );
            Assert.AreEqual(player.EntityId, returned?.EntityId);
            
            ActingAs(null);
            returned = OnFacet<AuthStubFacet>().CallSync<AuthStubPlayer>(
                nameof(AuthStubFacet.GetAuthenticatedPlayer)
            );
            Assert.IsNull(returned);
        }
    }
}