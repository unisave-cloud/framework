using Unisave.Authentication;
using Unisave.Entities;

namespace Unisave.Testing
{
    public partial class BasicBackendTestCase
    {
        /// <summary>
        /// Make the given player the authenticated player
        /// </summary>
        protected BasicBackendTestCase ActingAs(Entity player)
        {
            var manager = App.Resolve<AuthenticationManager>();
            manager.SetPlayer(player);
            return this;
        }
    }
}