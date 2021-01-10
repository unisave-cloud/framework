using System;
using System.Collections;
using System.Collections.Generic;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Collections
{
    internal static class DictionarySerializer
    {
        public static JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Type[] typeArguments = typeScope.GetGenericArguments();
            Type keyType = typeArguments[0];
            Type valueType = typeArguments[1];

            if (keyType != typeof(string))
                return NonStringDictionaryToJson(subject, typeScope, context);

            JsonObject jsonObject = new JsonObject();
            IDictionary dictionary = (IDictionary)subject;

            foreach (DictionaryEntry entry in dictionary)
                jsonObject.Add((string)entry.Key, Serializer.ToJson(entry.Value, valueType, context));

            return jsonObject;
        }

        private static JsonValue NonStringDictionaryToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Type itemType = typeScope.GetGenericArguments()[1];
            
            JsonArray pairs = new JsonArray();

            foreach (DictionaryEntry entry in (IDictionary)subject)
            {
                pairs.Add(new JsonArray(
                    Serializer.ToJson(entry.Key, itemType, context),
                    Serializer.ToJson(entry.Value, itemType, context)
                ));
            }

            return pairs;
        }

        public static object FromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context
        )
        {
            Type[] typeArguments = typeScope.GetGenericArguments();
            Type keyType = typeArguments[0];
            Type valueType = typeArguments[1];
            
            if (keyType != typeof(string))
                return NonStringDictionaryFromJson(json, typeScope, context);

            object dictionary = typeScope.GetConstructor(new Type[] {}).Invoke(new object[] {});

            // handle PHP mangled empty JSON objects
            if (json.IsJsonArray && json.AsJsonArray.Count == 0)
                json = new JsonObject();
            
            JsonObject jsonObject = json.AsJsonObject;
            if (jsonObject == null)
                return null;

            foreach (KeyValuePair<string, JsonValue> item in jsonObject)
                typeScope.GetMethod("Add").Invoke(dictionary, new object[] {
                    item.Key,
                    Serializer.FromJson(item.Value, valueType, context)
                });

            return dictionary;
        }

        private static object NonStringDictionaryFromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context
        )
        {
            Type[] typeArguments = typeScope.GetGenericArguments();
            Type keyType = typeArguments[0];
            Type valueType = typeArguments[1];

            object dictionary = typeScope.GetConstructor(new Type[] {}).Invoke(new object[] {});

            foreach (JsonValue pair in json.AsJsonArray)
            {
                typeScope.GetMethod("Add").Invoke(dictionary, new object[] {
                    Serializer.FromJson(pair.AsJsonArray[0], keyType, context),
                    Serializer.FromJson(pair.AsJsonArray[1], valueType, context)
                });
            }

            return dictionary;
        }
    }
}