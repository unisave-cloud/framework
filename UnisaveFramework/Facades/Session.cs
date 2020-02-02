using LightJson;
using Unisave.Contracts;
using Unisave.Foundation;
using Unisave.Services;

namespace Unisave
{
    /// <summary>
    /// Facade for working with the session storage
    /// </summary>
    public static class Session
    {
        private static ISession GetSession()
        {
            return Application.Default.Resolve<ISession>();
        }
        
        /// <summary>
        /// Get value of a session object
        /// 
        /// Object's type has to be provided explicitly and match the actual
        /// type, otherwise the deserialization might mangle up the object.
        /// </summary>
        /// <param name="key">Key under which it's stored</param>
        /// <param name="defaultValue">What to return if not present</param>
        /// <typeparam name="T">Type of the stored object</typeparam>
        public static T Get<T>(string key, T defaultValue = default(T))
        {
            return GetSession().Get(key, defaultValue);
        }

        /// <summary>
        /// Returns true if a given session key is occupied and is non-null
        /// </summary>
        public static bool Has(string key)
        {
            return GetSession().Has(key);
        }

        /// <summary>
        /// Returns true if a given key exists in the session object
        /// Returns true even if the key value is null
        /// </summary>
        public static bool Exists(string key)
        {
            return GetSession().Exists(key);
        }

        /// <summary>
        /// Returns all the data stored inside the session
        /// </summary>
        public static JsonObject All()
        {
            return GetSession().All();
        }
        
        /// <summary>
        /// Set the value to be stored under a session key
        /// </summary>
        /// <param name="key">Session key</param>
        /// <param name="value">Value to store</param>
        public static void Set(string key, object value)
        {
            GetSession().Set(key, value);
        }

        /// <summary>
        /// Alias for Set method
        /// </summary>
        public static void Put(string key, object value)
            => Set(key, value);

        /// <summary>
        /// Forget value under a single key
        /// </summary>
        public static void Forget(string key)
        {
            GetSession().Forget(key);
        }

        /// <summary>
        /// Forget all the stored values
        /// </summary>
        public static void Clear()
        {
            GetSession().Clear();
        }
    }
}