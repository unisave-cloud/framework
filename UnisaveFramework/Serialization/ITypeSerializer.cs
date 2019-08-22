using System;
using LightJson;

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
        /// Convert given instance to JSON
        /// Proper type of the instance is verified outside this method however
        /// </summary>
        JsonValue ToJson(object subject);

        /// <summary>
        /// Convert JSON into new instance of a given type
        /// </summary>
        object FromJson(JsonValue json, Type type);
    }
}
