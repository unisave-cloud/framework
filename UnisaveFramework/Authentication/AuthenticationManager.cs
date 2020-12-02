using System;
using System.ComponentModel;
using Unisave.Arango;
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

            // couldn't be initialized with this type T
            if (!initialized)
                return null;
            
            if (authenticatedPlayer == null)
                return null;

            // the authenticated player is not of type T
            if (authenticatedPlayer.GetType() != typeof(T))
                return null;

            return (T) authenticatedPlayer;
        }

        /// <summary>
        /// Initializes the manager by pulling the authenticated player
        /// from session and database
        /// (may not perform initialization if the type does not match the id)
        /// </summary>
        /// <typeparam name="T">Type of the requested entity</typeparam>
        private void Initialize<T>() where T : Entity
        {
            string id = Id();
            
            if (id == null)
            {
                // there's no authenticated player
                authenticatedPlayer = null;
                initialized = true;
            }
            else
            {
                // if the id belongs to a different type,
                // do not initialize
                var docId = DocumentId.Parse(id);
                if (docId.Collection != EntityUtils.CollectionFromType(typeof(T)))
                    return;
                
                // here we have the authenticated player
                authenticatedPlayer = entityManager.Find<T>(id);
                initialized = true;
            }
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