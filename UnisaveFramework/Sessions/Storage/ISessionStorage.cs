using LightJson;

namespace Unisave.Sessions.Storage
{
    public interface ISessionStorage
    {
        /// <summary>
        /// Loads the session data
        ///
        /// Given unknown session id, a new empty session should be returned.
        /// </summary>
        /// <param name="sessionId">Identifier for this session</param>
        JsonObject Load(string sessionId);

        /// <summary>
        /// Persists session data
        /// </summary>
        /// <param name="sessionId">Identifier for this session</param>
        /// <param name="sessionData">Data to be stored</param>
        /// <param name="lifetime">Lifetime for the session in seconds</param>
        void Store(string sessionId, JsonObject sessionData, int lifetime);
    }
}