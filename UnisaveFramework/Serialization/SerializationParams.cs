using System;

namespace Unisave.Serialization
{
    /// <summary>
    /// Parameters regarding data serialization
    /// </summary>
    public static class SerializationParams
    {
        /// <summary>
        /// Format for serializing datetime values
        /// The value is always assumed to be in UTC
        /// Format is taken from the ArangoDB DATE_ISO8601(...) function,
        /// another words it's the ISO8601 format with milliseconds and "T", "Z"
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-dd\\THH:mm:ss.fff\\Z";
    }
}
