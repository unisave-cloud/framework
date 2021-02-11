using System;
using LightJson;
using Unisave.Serialization.Collections;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Primitives
{
    public static class BinarySerializer
    {
        public static JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            // force typical array format
            if (context.serializeBinaryAsByteArray)
                return ArraySerializer.ToJson(subject, typeScope, context);
            
            byte[] data = (byte[]) subject;

            return "base64:" + Convert.ToBase64String(data);
        }

        public static object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            // typical array format
            if (json.IsJsonArray)
                return ArraySerializer.FromJson(
                    json, deserializationType, context
                );

            string encoded = json.AsString;
            
            if (encoded == null || !encoded.StartsWith("base64:"))
                throw new UnisaveSerializationException(
                    "Given JSON value is not a base64 encoded binary sequence."
                );

            try
            {
                return Convert.FromBase64String(encoded.Substring(7));
            }
            catch (FormatException e)
            {
                throw new UnisaveSerializationException(
                    "Given JSON value is not a base64 encoded binary sequence.",
                    e
                );
            }
        }
    }
}