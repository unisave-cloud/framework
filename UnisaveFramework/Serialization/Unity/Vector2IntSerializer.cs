using System;
using LightJson;
using Unisave.Serialization.Context;
using UnityEngine;

namespace Unisave.Serialization.Unity
{
    public class Vector2IntSerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Vector2Int v = (Vector2Int) subject;
            
            return new JsonObject() {
                ["x"] = v.x,
                ["y"] = v.y
            };
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            JsonObject o = json.AsJsonObject;

            if (o == null)
                return default(Vector2Int);

            return new Vector2Int(
                o["x"].AsInteger,
                o["y"].AsInteger
            );
        }
    }
}