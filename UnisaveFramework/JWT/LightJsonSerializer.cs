using System;
using System.Collections.Generic;
using System.Linq;
using JWT;
using JWT.Builder;
using LightJson;
using LightJson.Serialization;
using Unisave.Serialization;

namespace Unisave.JWT
{
    public class LightJsonSerializer : IJsonSerializer
    {
        public string Serialize(object obj)
        {
            if (obj == null)
                return "null";
            
            if (obj is JwtHeader header)
            {
                var json = new JsonObject
                {
                    ["typ"] = header.Type,
                    ["cty"] = header.ContentType,
                    ["alg"] = header.Algorithm,
                    ["kid"] = header.KeyId,
                    ["x5u"] = header.X5u,
                    ["x5t"] = header.X5t
                };

                if (header.X5c != null)
                {
                    json["x5c"] = new JsonArray(
                        header.X5c
                            .Select(x => (JsonValue)x)
                            .ToArray()
                    );
                }
                
                // remove nulls
                List<string> keysToRemove = new List<string>(16);
                foreach (var pair in json)
                    if (pair.Value.IsNull)
                        keysToRemove.Add(pair.Key);
                foreach (string key in keysToRemove)
                    json.Remove(key);

                return json.ToString();
            }
            
            return Serializer.ToJsonString(obj);
        }

        public object Deserialize(Type type, string json)
        {
            JsonValue jsonValue = JsonReader.Parse(json);

            if (type == typeof(Dictionary<string, object>))
                return ToDictJson(jsonValue);

            if (type == typeof(JwtHeader))
            {
                return new JwtHeader() {
                    Type = jsonValue["typ"],
                    ContentType = jsonValue["cty"],
                    Algorithm = jsonValue["alg"],
                    KeyId = jsonValue["kid"],
                    X5u = jsonValue["x5u"],
                    X5c = jsonValue["x5c"].AsJsonArray
                        ?.Select(x => x.AsString)
                        ?.ToArray(),
                    X5t = jsonValue["x5t"]
                };
            }
            
            return Serializer.FromJson(jsonValue, type);
        }

        private object ToDictJson(JsonValue value)
        {
            switch (value.Type)
            {
                case JsonValueType.Null: return null;
                case JsonValueType.Boolean: return value.AsBoolean;
                case JsonValueType.Number: return value.AsNumber;
                case JsonValueType.String: return value.AsString;
                
                case JsonValueType.Array:
                    return value.AsJsonArray.Select(ToDictJson).ToArray();
                
                case JsonValueType.Object:
                    var obj = value.AsJsonObject;
                    var dict = new Dictionary<string, object>();
                    foreach (var pair in obj)
                        dict[pair.Key] = ToDictJson(pair.Value);
                    return dict;
            }

            return null;
        }
    }
}