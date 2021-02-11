using System;
using LightJson;
using Unisave.Serialization.Context;
using UnityEngine;

namespace Unisave.Serialization.Unity
{
    public class Vector3IntSerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Vector3Int v = (Vector3Int) subject;
            
            return new JsonObject() {
                ["x"] = v.x,
                ["y"] = v.y,
                ["z"] = v.z
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
                return default(Vector3Int);

            return new Vector3Int(
                o["x"].AsInteger,
                o["y"].AsInteger,
                o["z"].AsInteger
            );
        }
    }
}