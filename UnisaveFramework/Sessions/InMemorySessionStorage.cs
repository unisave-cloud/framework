using LightJson;

namespace Unisave.Sessions
{
    /// <summary>
    /// Stores sessions in memory
    /// </summary>
    public class InMemorySessionStorage : ISessionStorage
    {
        // HACK: why not use an already existing in-memory storage?
        protected readonly SessionOverStorage memory
            = new SessionOverStorage(null, 0);
        
        public JsonObject Load(string sessionId)
        {
            if (!memory.Has(sessionId))
                return new JsonObject();
            
            return memory.Get<JsonObject>(sessionId);
        }

        public void Store(
            string sessionId,
            JsonObject sessionData,
            int lifetime
        )
        {
            // NOTE: lifetime is ignored when storing into memory
            
            memory.Set(sessionId, sessionData);
        }
    }
}