using System;
using LightJson;
using UnityEngine;

namespace Unisave.Serialization
{
    /// <summary>
    /// Collection of default type serializers for the Unisave serializer
    /// </summary>
    public static class DefaultTypeSerializers
    {
        public static void RegisterAllSerializers()
        {
            Json();
            UnityMath();

            Serializer.SetExactTypeSerializer(typeof(DateTime), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    return (JsonValue)((DateTime)subject).ToString(SerializationParams.DateTimeFormat);
                })
                .FromJson((json, type) => {
                    return DateTime.Parse(json.AsString);
                })
            );

            Serializer.SetAssignableTypeSerializer(
                typeof(Exception),
                new ExceptionSerializer()
            );

            Serializer.SetExactTypeSerializer(typeof(UnisavePlayer), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    return (JsonValue)((UnisavePlayer)subject).Id;
                })
                .FromJson((json, type) => {
                    return new UnisavePlayer(json.AsString);
                })
            );

            Serializer.SetAssignableTypeSerializer(typeof(Entity), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    return ((Entity)subject).ToJson();
                })
                .FromJson((json, type) => {
                    return Entity.FromJson(json, type);
                })
            );
        }

        /// <summary>
        /// Makes sure that JsonValue, JsonArray and JsonObject instances are passed as is
        /// </summary>
        private static void Json()
        {
            Serializer.SetExactTypeSerializer(typeof(JsonValue), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    return (JsonValue)subject;
                })
                .FromJson((json, type) => {
                    return json;
                })
            );

            Serializer.SetExactTypeSerializer(typeof(JsonArray), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    return new JsonValue((JsonArray)subject);
                })
                .FromJson((json, type) => {
                    return json.AsJsonArray;
                })
            );

            Serializer.SetExactTypeSerializer(typeof(JsonObject), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    return new JsonValue((JsonObject)subject);
                })
                .FromJson((json, type) => {
                    return json.AsJsonObject;
                })
            );
        }

        /// <summary>
        /// Handles serialization of Unity math types, like Vector3
        /// </summary>
        private static void UnityMath()
        {
            Serializer.SetExactTypeSerializer(typeof(Vector4), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    Vector4 vector = (Vector4)subject;
                    return new JsonObject()
                        .Add("x", vector.x)
                        .Add("y", vector.y)
                        .Add("z", vector.z)
                        .Add("w", vector.w);
                })
                .FromJson((json, type) => {
                    JsonObject o = json.AsJsonObject;
                    return new Vector4(
                        (float)o["x"].AsNumber,
                        (float)o["y"].AsNumber,
                        (float)o["z"].AsNumber,
                        (float)o["w"].AsNumber
                    );
                })
            );

            Serializer.SetExactTypeSerializer(typeof(Vector3), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    Vector3 vector = (Vector3)subject;
                    return new JsonObject()
                        .Add("x", vector.x)
                        .Add("y", vector.y)
                        .Add("z", vector.z);
                })
                .FromJson((json, type) => {
                    JsonObject o = json.AsJsonObject;
                    return new Vector3(
                        (float)o["x"].AsNumber,
                        (float)o["y"].AsNumber,
                        (float)o["z"].AsNumber
                    );
                })
            );

            Serializer.SetExactTypeSerializer(typeof(Vector2), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    Vector2 vector = (Vector2)subject;
                    return new JsonObject()
                        .Add("x", vector.x)
                        .Add("y", vector.y);
                })
                .FromJson((json, type) => {
                    JsonObject o = json.AsJsonObject;
                    return new Vector2(
                        (float)o["x"].AsNumber,
                        (float)o["y"].AsNumber
                    );
                })
            );

            Serializer.SetExactTypeSerializer(typeof(Vector3Int), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    Vector3Int vector = (Vector3Int)subject;
                    return new JsonObject()
                        .Add("x", vector.x)
                        .Add("y", vector.y)
                        .Add("z", vector.z);
                })
                .FromJson((json, type) => {
                    JsonObject o = json.AsJsonObject;
                    return new Vector3Int(
                        o["x"].AsInteger,
                        o["y"].AsInteger,
                        o["z"].AsInteger
                    );
                })
            );

            Serializer.SetExactTypeSerializer(typeof(Vector2Int), new LambdaTypeSerializer()
                .ToJson((subject) => {
                    Vector2Int vector = (Vector2Int)subject;
                    return new JsonObject()
                        .Add("x", vector.x)
                        .Add("y", vector.y);
                })
                .FromJson((json, type) => {
                    JsonObject o = json.AsJsonObject;
                    return new Vector2Int(
                        o["x"].AsInteger,
                        o["y"].AsInteger
                    );
                })
            );
        }
    }
}
