using System.Collections.Concurrent;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Sessions.Storage
{
    /// <summary>
    /// Stores sessions in memory,
    /// is thread safe
    /// </summary>
    public class InMemorySessionStorage : ISessionStorage
    {
        private readonly ConcurrentDictionary<string, string> sessions
            = new ConcurrentDictionary<string, string>();
        
        public JsonObject Load(string sessionId)
        {
            if (!sessions.TryGetValue(sessionId, out string sessionData))
                return new JsonObject();

            return JsonReader.Parse(sessionData).AsJsonObject;
        }

        public void Store(
            string sessionId,
            JsonObject sessionData,
            int lifetime
        )
        {
            // NOTE: lifetime is ignored when storing into memory
            
            sessions[sessionId] = sessionData.ToString();
        }
    }
}