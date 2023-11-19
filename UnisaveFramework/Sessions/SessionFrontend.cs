using System;
using LightJson;
using LightJson.Serialization;
using Unisave.Contracts;
using Unisave.Serialization;
using Unisave.Sessions.Storage;

namespace Unisave.Sessions
{
    /// <summary>
    /// Implements the <see cref="ISession"/> contract and in turn
    /// the <see cref="Unisave.Facades.Session"/> facade.
    /// Has only per-request lifetime and it loads/stores the session data
    /// from some external <see cref="ISessionStorage"/> instance,
    /// which is either in-memory or in-database storage.
    /// This object holds and manipulates data of a single player session only.
    /// </summary>
    public class SessionFrontend : ISession
    {
        /// <summary>
        /// The underlying session storage
        /// </summary>
        protected ISessionStorage Storage { get; }
        
        /// <summary>
        /// Lifetime of sessions in seconds
        /// </summary>
        protected int SessionLifetime { get; }

        /// <summary>
        /// Data that the session currently holds
        /// </summary>
        protected JsonObject data = new JsonObject();
        
        /// <summary>
        /// Creates new instance of a session that uses
        /// some ISessionStorage implementation as it's backend
        /// </summary>
        /// <param name="storage">
        /// Underlying storage instance
        /// Can be null, then storing and loading is ignored
        /// </param>
        /// <param name="sessionLifetime">
        /// Lifetime of sessions in seconds
        /// </param>
        public SessionFrontend(ISessionStorage storage, int sessionLifetime)
        {
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            SessionLifetime = sessionLifetime;
        }

        public void LoadSession(string sessionId)
        {
            data = Storage.Load(sessionId);
        }

        public void StoreSession(string sessionId)
        {
            Storage.Store(sessionId, data, SessionLifetime);
        }

        public T Get<T>(string key, T defaultValue = default(T))
        {
            if (!Exists(key))
                return defaultValue;
            
            return Serializer.FromJson<T>(data[key]);
        }

        public bool Has(string key)
        {
            if (!Exists(key))
                return false;

            var value = Get<JsonValue>(key);

            if (value.IsNull)
                return false;
            
            return true;
        }

        public bool Exists(string key)
        {
            return data.ContainsKey(key);
        }

        public JsonObject All()
        {
            return JsonReader.Parse(data.ToString());
        }

        public void Set(string key, object value)
        {
            data[key] = Serializer.ToJson(value);
        }

        public void Put(string key, object value)
        {
            Set(key, value);
        }

        public void Forget(string key)
        {
            data.Remove(key);
        }

        public void Clear()
        {
            data = new JsonObject();
        }
    }
}