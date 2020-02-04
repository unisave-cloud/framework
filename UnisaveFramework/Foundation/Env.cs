using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Unisave.Foundation
{
    /// <summary>
    /// Holds environment configuration for the framework execution
    /// </summary>
    public class Env
    {
        /// <summary>
        /// Contains the environment values
        /// </summary>
        private readonly Dictionary<string, string> values
            = new Dictionary<string, string>();

        /// <summary>
        /// Indexer for getting and setting the values
        /// </summary>
        public string this[string key]
        {
            get => GetString(key);
            set => values[key] = value;
        }

        /// <summary>
        /// Returns true if a key is known
        /// </summary>
        public bool Has(string key)
        {
            return values.ContainsKey(key);
        }

        /// <summary>
        /// Gets value with a default specified
        /// </summary>
        public string GetString(string key, string defaultValue = null)
        {
            if (!Has(key))
                return defaultValue;

            return values[key];
        }

        /// <summary>
        /// Gets value, converted to integer with default specified
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            string s = GetString(key);

            if (s == null)
                return defaultValue;

            if (int.TryParse(s, out int i))
                return i;

            return defaultValue;
        }

        /// <summary>
        /// Parse out env config from a string
        /// </summary>
        public static Env Parse(string source)
        {
            var env = new Env();

            if (source == null)
                return env;
            
            string[] lines = Regex.Split(source, "\r\n|\r|\n");
            
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');

                if (parts.Length <= 1)
                    continue;

                string keyPart = parts[0];
                string valuePart = line.Substring(keyPart.Length + 1);
                string key = keyPart.Trim();
                string value = valuePart.Trim();

                if (key.StartsWith("#"))
                    continue;

                env[key] = value;
            }
            
            return env;
        }
    }
}