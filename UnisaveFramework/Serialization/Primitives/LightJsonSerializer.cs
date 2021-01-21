using System;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Primitives
{
    public class LightJsonSerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            if (subject == null)
                return JsonValue.Null;

            switch (subject)
            {
                case JsonArray a: return new JsonValue(a);
                case JsonObject o: return new JsonValue(o);
            }
            
            return (JsonValue) subject;
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            if (deserializationType == typeof(JsonObject))
                return json.AsJsonObject;
            
            if (deserializationType == typeof(JsonArray))
                return json.AsJsonArray;
            
            return json;
        }
    }
}