using LightJson;

namespace Unisave.Sessions.Storage
{
    /// <summary>
    /// Session storage that does not store anything
    /// </summary>
    public class NullSessionStorage : ISessionStorage
    {
        public JsonObject Load(string sessionId)
        {
            return new JsonObject();
        }

        public void Store(string sessionId, JsonObject sessionData, int lifetime)
        {
            // do nothing
        }
    }
}