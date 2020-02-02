using LightJson;
using Unisave.Contracts;

namespace Unisave.Sessions
{
    /// <summary>
    /// Stores session data in memory
    /// </summary>
    public class InMemorySession : ISession
    {
        public void LoadSession(string sessionId)
        {
            throw new System.NotImplementedException();
        }

        public void StoreSession(string sessionId)
        {
            throw new System.NotImplementedException();
        }

        public T Get<T>(string key, T defaultValue = default(T))
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public JsonObject All()
        {
            throw new System.NotImplementedException();
        }

        public void Set(string key, object value)
        {
            throw new System.NotImplementedException();
        }

        public void Put(string key, object value)
        {
            throw new System.NotImplementedException();
        }

        public void Forget(string key)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }
    }
}