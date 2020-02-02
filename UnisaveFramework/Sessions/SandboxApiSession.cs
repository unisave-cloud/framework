using System;
using LightJson;
using Unisave.Contracts;
using Unisave.Runtime;
using Unisave.Utils;

namespace Unisave.Sessions
{
    /// <summary>
    /// Communicates with the underlying session implementation via sandbox API
    /// </summary>
    public class SandboxApiSession : ISession
    {
        private readonly ApiChannel channel;
        private readonly InMemorySession session;
        
        public SandboxApiSession() : this(new ApiChannel("session")) { }
        
        public SandboxApiSession(ApiChannel channel)
        {
            this.channel = channel;
            session = new InMemorySession();
        }

        public void LoadSession(string sessionId)
        {
            // communicate through the channel
            
            throw new NotImplementedException();
        }

        public void StoreSession(string sessionId)
        {
            throw new NotImplementedException();
        }
        
        //////////////////////////////////////////////
        // Redirect all remaining interface methods //
        //////////////////////////////////////////////
        
        public T Get<T>(string key, T defaultValue = default(T))
        {
            return session.Get(key, defaultValue);
        }

        public bool Has(string key)
        {
            return session.Has(key);
        }

        public bool Exists(string key)
        {
            return session.Exists(key);
        }

        public JsonObject All()
        {
            return session.All();
        }

        public void Set(string key, object value)
        {
            session.Set(key, value);
        }

        public void Put(string key, object value)
        {
            session.Put(key, value);
        }

        public void Forget(string key)
        {
            session.Forget(key);
        }

        public void Clear()
        {
            session.Clear();
        }
    }
}