using System;
using LightJson;
using Unisave.Serialization.Composites;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Primitives
{
    public class TimeSpanSerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            TimeSpan ts = (TimeSpan) subject;

            return ts.TotalSeconds;
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            // legacy format
            if (json.IsJsonObject)
                return DefaultSerializer.FromJson(
                    json, deserializationType, context
                );

            // NOPE! this rounds to milliseconds:
            //return TimeSpan.FromSeconds(json.AsNumber);

            return new TimeSpan((long)(json.AsNumber * TimeSpan.TicksPerSecond));
        }
    }
}