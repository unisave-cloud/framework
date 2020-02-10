using System;

namespace Unisave.Authentication
{
    /// <summary>
    /// Remembers the player, that is authenticated during
    /// the application lifetime. Further persistence (session) has to
    /// be performed in appropriate middleware.
    /// </summary>
    public class AuthenticationManager
    {
        private Entity authenticatedPlayer;
        
        /// <summary>
        /// Set the player that is currently authenticated
        /// </summary>
        public void SetPlayer(Entity player)
        {
            // NOTE: null is ok, acts like logout
            
            authenticatedPlayer = player;
        }

        /// <summary>
        /// Returns the authenticated player or null.
        /// Make sure the type T matches, otherwise an exception gets thrown.
        /// </summary>
        public T GetPlayer<T>() where T : Entity
        {
            if (authenticatedPlayer == null)
                return null;
            
            if (authenticatedPlayer.GetType() != typeof(T))
                throw new ArgumentException(
                    "Authenticated player is of type " +
                    $"{authenticatedPlayer.GetType()}, not {typeof(T)}"
                );

            return (T) authenticatedPlayer;
        }

        /// <summary>
        /// Logout the player
        /// </summary>
        public void Logout() => SetPlayer(null);
        
        /// <summary>
        /// Login a player
        /// </summary>
        public void Login(Entity player) => SetPlayer(player);

        /// <summary>
        /// Returns true if someone is authenticated
        /// </summary>
        public bool Check() => authenticatedPlayer != null;

        /// <summary>
        /// Get the ID of the authenticated player or null
        /// </summary>
        public string Id() => authenticatedPlayer?.EntityId;
    }
}