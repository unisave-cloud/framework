using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization
{
    public interface IUnisaveSerializable
    {
        /*
         * It cannot be required by an interface
         * but any type implementing this should
         * also provide a constructor for deserialization:
         * Ctor(JsonValue, DeserializationContext)
         */
        
        /// <summary>
        /// Returns the JSON representation of the object
        /// </summary>
        /// <param name="context">Context of the serialization</param>
        /// <returns>JSON representation</returns>
        JsonValue ToJson(SerializationContext context);
    }
}