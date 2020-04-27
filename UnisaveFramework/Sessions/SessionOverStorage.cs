using LightJson;
using LightJson.Serialization;
using Unisave.Contracts;
using Unisave.Serialization;

namespace Unisave.Sessions
{
    /// <summary>
    /// Implements session interface, keeping session data in memory
    /// and when asked, storing the data to some session storage
    /// </summary>
    public class SessionOverStorage : ISession
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
        public SessionOverStorage(ISessionStorage storage, int sessionLifetime)
        {
            Storage = storage;
            SessionLifetime = sessionLifetime;
        }

        public void LoadSession(string sessionId)
        {
            if (Storage == null)
            {
                data = new JsonObject();
                return;
            }

            data = Storage.Load(sessionId);
        }

        public void StoreSession(string sessionId)
        {
            if (Storage == null)
                return;
            
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