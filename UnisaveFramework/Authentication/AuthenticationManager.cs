using System;
using System.Collections.Generic;
using Unisave.Arango;
using Unisave.Contracts;
using Unisave.Entities;

namespace Unisave.Authentication
{
    /// <summary>
    /// Remembers the player, that is authenticated within
    /// the request context (within a session context).
    /// </summary>
    public class AuthenticationManager
    {
        public const string SessionKey = "authenticatedPlayerId";
        
        // used services
        private readonly ISession session;
        private readonly IArango arango;

        // caches returned player/user instance
        // (the deserialized documents)
        private readonly Dictionary<Type, object> instanceCache
            = new Dictionary<Type, object>();

        public AuthenticationManager(
            ISession session,
            IArango arango
        )
        {
            this.session = session;
            this.arango = arango;
        }

        /// <summary>
        /// Returns the authenticated player/user or null.
        /// The provided type is the used deserialization type
        /// for the database document.
        /// </summary>
        /// <param name="fresh">
        /// Sidestep the cache and get a fresh instance from the database
        /// </param>
        public T GetPlayer<T>(bool fresh = false) where T : class
        {
            // nobody is logged in
            if (!Check())
                return null;
            
            // try the cache
            if (!fresh)
            {
                if (instanceCache.TryGetValue(typeof(T), out object instance))
                    return (T) instance;
            }
            
            // fetch from the database
            T freshInstance = new RawAqlQuery(
                arango,
                @"RETURN DOCUMENT(@id)"
            )
                .Bind("id", Id())
                .FirstAs<T>();

            if (freshInstance == null)
                throw new InvalidOperationException(
                    "Getting authenticated player instance failed, " +
                    "there is no such document in the database."
                );

            // cache and return
            instanceCache[typeof(T)] = freshInstance;
            return freshInstance;
        }

        /// <summary>
        /// Logout the player/user
        /// </summary>
        public void Logout()
        {
            // clear from session
            session.Forget(SessionKey);
            
            // clear the instance cache
            instanceCache.Clear();
        }

        /// <summary>
        /// Login a player/user represented by an entity
        /// </summary>
        public void Login(Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(
                    nameof(entity),
                    "Cannot call login with a null. To logout use " +
                    "the logout method instead."
                );
            
            if (entity.EntityId == null)
                throw new ArgumentNullException(
                    nameof(entity),
                    "Cannot log in with an entity that has not been written " +
                    "to the database yet."
                );
            
            // login via ID
            Login(entity.EntityId);
            
            // cache the entity instance
            instanceCache[entity.GetType()] = entity;
        }

        /// <summary>
        /// Login a player/user represented by a document ID
        /// </summary>
        public void Login(string documentId)
        {
            if (documentId == null)
            {
                throw new ArgumentNullException(
                    nameof(documentId),
                    "Cannot call login with a null. To logout use " +
                    "the logout method instead."
                );
            }
            
            // validate ID
            try
            {
                DocumentId.Parse(documentId);
            }
            catch (ArangoException)
            {
                throw new ArgumentException(
                    nameof(documentId),
                    $"Given string '{documentId}' is not a valid document ID"
                );
            }
            
            // store the ID in the session
            session.Set(SessionKey, documentId);
        }

        /// <summary>
        /// Returns true if someone is authenticated
        /// </summary>
        public bool Check() => Id() != null;

        /// <summary>
        /// Get the ID of the authenticated player/user or null
        /// </summary>
        public string Id() => session.Get<string>(SessionKey);
    }
}