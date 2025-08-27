using System;
using System.Globalization;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Primitives
{
    public class DecimalSerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            return ((decimal) subject).ToString(CultureInfo.InvariantCulture);
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            return decimal.Parse(json.AsString, CultureInfo.InvariantCulture);
        }
    }
}