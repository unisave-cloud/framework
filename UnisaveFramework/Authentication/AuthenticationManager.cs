using System;
using System.ComponentModel;
using Unisave.Contracts;
using Unisave.Entities;

namespace Unisave.Authentication
{
    /// <summary>
    /// Remembers the player, that is authenticated during
    /// the application lifetime. Further persistence (session) has to
    /// be performed in appropriate middleware.
    /// </summary>
    public class AuthenticationManager
    {
        public const string SessionKey = "authenticatedPlayerId";
        
        /// <summary>
        /// The currently authenticated player, null is ok
        /// When not initialized, this field has no meaning
        /// </summary>
        private Entity authenticatedPlayer;
        
        /// <summary>
        /// Whether is this instance initialized or not
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// Reference to a session store
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Reference to an entity manager
        /// </summary>
        private readonly EntityManager entityManager;

        public AuthenticationManager(
            ISession session,
            EntityManager entityManager
        )
        {
            this.session = session;
            this.entityManager = entityManager;
        }
        
        /// <summary>
        /// Set the player that is currently authenticated
        /// </summary>
        public void SetPlayer(Entity player)
        {
            // NOTE: null is ok, acts like logout
            
            session.Set(SessionKey, player?.EntityId);
            authenticatedPlayer = player;
            initialized = true;
        }

        /// <summary>
        /// Returns the authenticated player or null.
        /// Make sure the type T matches, otherwise an exception gets thrown.
        /// </summary>
        public T GetPlayer<T>() where T : Entity
        {
            if (!initialized)
                Initialize<T>();
            
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
        /// Initializes the manager by pulling the authenticated player
        /// from session and database
        /// </summary>
        /// <typeparam name="T">Type of the requested entity</typeparam>
        private void Initialize<T>() where T : Entity
        {
            string id = Id();
            
            if (id == null)
            {
                authenticatedPlayer = null;
            }
            else
            {
                authenticatedPlayer = entityManager.Find<T>(id);
            }
            
            initialized = true;
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
        public bool Check() => Id() != null;

        /// <summary>
        /// Get the ID of the authenticated player or null
        /// </summary>
        public string Id() => session.Get<string>(SessionKey);
    }
}