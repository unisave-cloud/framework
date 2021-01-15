using System;
using LightJson;
using Unisave.Entities;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Unisave
{
    public class EntitySerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            return ((Entity) subject).GetAttributes();
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            Entity entity = EntityUtils.CreateInstance(deserializationType);
            entity.SetAttributes(json);
            return entity;
        }
    }
}