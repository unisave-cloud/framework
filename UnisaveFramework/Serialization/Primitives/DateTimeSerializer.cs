using System;
using System.Globalization;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Primitives
{
    public class DateTimeSerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            var datetime = (DateTime) subject;
            
            return (JsonValue) datetime.ToString(
                SerializationParams.DateTimeFormat
            );
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            bool done = DateTime.TryParseExact(
                json.AsString,
                SerializationParams.DateTimeFormat,
                null,
                DateTimeStyles.None,
                out DateTime parsed
            );

            if (done)
                return parsed;

            return DateTime.Parse(json.AsString);
        }
    }
}