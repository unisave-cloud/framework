using System;
using Unisave.Foundation;

namespace Unisave.Facades
{
    /// <summary>
    /// Facade for accessing environment variables
    /// </summary>
    public static class Env
    {
        private static EnvStore GetStore()
        {
            if (!Facade.HasApp)
                throw new InvalidOperationException(
                    "You cannot access env variables from the client side."
                );
            
            return Facade.App.Resolve<EnvStore>();
        }

        /// <summary>
        /// Returns true if a key is known
        /// </summary>
        public static bool Has(string key)
            => GetStore().Has(key);

        /// <summary>
        /// Gets value with a default specified
        /// </summary>
        public static string GetString(string key, string defaultValue = null)
            => GetStore().GetString(key, defaultValue);

        /// <summary>
        /// Sets a string value
        /// </summary>
        public static void Set(string key, string value)
            => GetStore().Set(key, value);

        /// <summary>
        /// Gets value, converted to integer with default specified
        /// </summary>
        public static int GetInt(string key, int defaultValue = 0)
            => GetStore().GetInt(key, defaultValue);
    }
}