using System;
using Unisave.Authentication;
using Unisave.Entities;

namespace Unisave.Facades
{
    /// <summary>
    /// Facade for accessing the authentication manager
    /// </summary>
    public static class Auth
    {
        private static AuthenticationManager GetManager()
        {
            if (!Facade.CanUse)
                throw new InvalidOperationException(
                    "You cannot access authentication logic from the client side."
                );
            
            return Facade.Services.Resolve<AuthenticationManager>();
        }
        
        /// <summary>
        /// Logout the player
        /// </summary>
        public static void Logout()
            => GetManager().Logout();
        
        /// <summary>
        /// Login a player
        /// </summary>
        public static void Login(Entity player)
            => GetManager().Login(player);

        /// <summary>
        /// Returns true if someone is authenticated
        /// </summary>
        public static bool Check()
            => GetManager().Check();

        /// <summary>
        /// Get the ID of the authenticated player or null
        /// </summary>
        public static string Id()
            => GetManager().Id();

        /// <summary>
        /// Get the authorized player entity or null
        /// </summary>
        public static T GetPlayer<T>() where T : Entity
            => GetManager().GetPlayer<T>();
    }
}