using System;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization
{
    /// <summary>
    /// Type serializer that uses user-defined lambda functions for serialization
    /// </summary>
    public class LambdaTypeSerializer : ITypeSerializer
    {
        private Func<object, JsonValue> toJson;
        private Func<JsonValue, Type, object> fromJson;

        public LambdaTypeSerializer() { }

        /// <summary>
        /// Defines the ToJson lambda function
        /// </summary>
        public LambdaTypeSerializer ToJson(Func<object, JsonValue> lambda)
        {
            toJson = lambda;
            return this;
        }

        /// <summary>
        /// Defines the FromJson lambda function
        /// </summary>
        public LambdaTypeSerializer FromJson(Func<JsonValue, Type, object> lambda)
        {
            fromJson = lambda;
            return this;
        }

        /// <inheritdoc/>
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            if (toJson == null)
                throw new InvalidOperationException("Corresponding lambda has not been defined yet.");

            return toJson(subject);
        }

        /// <inheritdoc/>
        public object FromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context
        )
        {
            if (fromJson == null)
                throw new InvalidOperationException("Corresponding lambda has not been defined yet.");

            return fromJson(json, typeScope);
        }
    }
}
