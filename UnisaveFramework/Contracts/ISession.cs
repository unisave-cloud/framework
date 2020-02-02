using LightJson;

namespace Unisave.Contracts
{
    /// <summary>
    /// Interface for session storage used for resolving from the container
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Loads the session contents from some underlying storage system.
        ///
        /// Given unknown session id, a new empty session should be created.
        /// </summary>
        /// <param name="sessionId">Identifier for this session</param>
        void LoadSession(string sessionId);

        /// <summary>
        /// Stores the session content into some underlying storage system.
        /// </summary>
        /// <param name="sessionId">Identifier for this session</param>
        void StoreSession(string sessionId);
        
        /// <summary>
        /// Get value of a session object
        /// 
        /// Object's type has to be provided explicitly and match the actual
        /// type, otherwise the deserialization might mangle up the object.
        /// </summary>
        /// <param name="key">Key under which it's stored</param>
        /// <param name="defaultValue">What to return if not present</param>
        /// <typeparam name="T">Type of the stored object</typeparam>
        T Get<T>(string key, T defaultValue = default(T));

        /// <summary>
        /// Returns true if a given session key is occupied and is non-null
        /// </summary>
        bool Has(string key);

        /// <summary>
        /// Returns true if a given key exists in the session object
        /// Returns true even if the key value is null
        /// </summary>
        bool Exists(string key);

        /// <summary>
        /// Returns all the data stored inside the session
        /// </summary>
        JsonObject All();

        /// <summary>
        /// Set the value to be stored under a session key
        /// </summary>
        /// <param name="key">Session key</param>
        /// <param name="value">Value to store</param>
        void Set(string key, object value);

        /// <summary>
        /// Alias for Set method
        /// </summary>
        void Put(string key, object value);

        /// <summary>
        /// Forget value under a single key
        /// </summary>
        void Forget(string key);

        /// <summary>
        /// Forget all the stored values
        /// </summary>
        void Clear();
    }
}