using System;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization
{
    /// <summary>
    /// Something that can serialize a given type
    /// The type being serialized is not specified here however
    /// It might also be used for multiple types
    /// 
    /// For more info see Serializer implementation
    /// </summary>
    public interface ITypeSerializer
    {
        /// <summary>
        /// Serialize a value
        /// </summary>
        JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        );

        /// <summary>
        /// Deserialize a value
        /// </summary>
        object FromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context
        );
    }
}
