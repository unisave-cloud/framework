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
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-dd H:mm:ss";
    }
}
