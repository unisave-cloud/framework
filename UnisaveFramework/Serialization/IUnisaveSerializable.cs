using System;
using LightJson;

namespace Unisave.Serialization
{
    /// <summary>
    /// Allows a type to be serialized by the Unisave serializer
    /// Or rather it overrides the serialization process
    /// </summary>
    public interface IUnisaveSerializable
    {
        

        /// <summary>
        /// Serializes the instance to JSON
        /// </summary>
        JsonValue ToJson();
    }
}
