using System;
using LightJson;
using Unisave.Arango;
using Unisave.Entities;
using Unisave.Serialization.Composites;
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
            return GetAttributes((Entity) subject, context);
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            Entity entity = EntityUtils.CreateInstance(deserializationType);
            SetAttributes(entity, json.AsJsonObject, context);
            return entity;
        }

        /// <summary>
        /// Sets entity attributes according to a given attributes JSON object
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="newAttributes"></param>
        /// <param name="context"></param>
        public static void SetAttributes(
            Entity entity,
            JsonObject newAttributes,
            DeserializationContext context
        )
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            if (newAttributes == null)
                throw new ArgumentNullException(nameof(newAttributes));
            
            DefaultSerializer.PopulateInstance(
                entity,
                newAttributes, 
                context
            );
        }

        /// <summary>
        /// Gets the attributes JSON object that represents an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="context"></param>
        public static JsonObject GetAttributes(
            Entity entity,
            SerializationContext context
        )
        {
            JsonObject attributes = DefaultSerializer.ToJson(
                entity,
                entity.GetType(),
                context
            );
            
            // add "_key" attribute
            attributes["_key"] = DocumentId.Parse(
                attributes["_id"].AsString
            ).Key;

            return attributes;
        }
    }
}