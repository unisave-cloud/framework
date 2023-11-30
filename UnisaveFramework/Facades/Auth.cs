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
        /// Logout the player/user
        /// </summary>
        public static void Logout()
            => GetManager().Logout();
        
        /// <summary>
        /// Login a player/user represented by an entity
        /// </summary>
        public static void Login(Entity entity)
            => GetManager().Login(entity);
        
        /// <summary>
        /// Login a player/user represented by a document ID
        /// </summary>
        public static void Login(string documentId)
            => GetManager().Login(documentId);

        /// <summary>
        /// Returns true if someone is authenticated
        /// </summary>
        public static bool Check()
            => GetManager().Check();

        /// <summary>
        /// Get the ID of the authenticated player/user or null
        /// </summary>
        public static string Id()
            => GetManager().Id();

        /// <summary>
        /// Get the authorized player/user instance or null
        /// </summary>
        public static T GetPlayer<T>() where T : class
            => GetManager().GetPlayer<T>();
    }
}