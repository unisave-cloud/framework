using System;
using System.Collections;
using System.Collections.Generic;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Collections
{
    public class DictionarySerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Type[] typeArguments = typeScope.GetGenericArguments();
            Type keyType = typeArguments[0];
            Type valueType = typeArguments[1];

            if (!IsTypeSuitableAsKey(keyType))
                return NonStringDictionaryToJson(subject, typeScope, context);
            
            JsonObject jsonObject = new JsonObject();
            IDictionary dictionary = (IDictionary)subject;

            foreach (DictionaryEntry entry in dictionary)
            {
                jsonObject.Add(
                    Serializer.ToJson(entry.Key, keyType, context).AsString,
                    Serializer.ToJson(entry.Value, valueType, context)
                );
            }

            return jsonObject;
        }

        private static JsonValue NonStringDictionaryToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Type[] typeArguments = typeScope.GetGenericArguments();
            Type keyType = typeArguments[0];
            Type valueType = typeArguments[1];
            
            JsonArray pairs = new JsonArray();

            foreach (DictionaryEntry entry in (IDictionary)subject)
            {
                pairs.Add(new JsonArray(
                    Serializer.ToJson(entry.Key, keyType, context),
                    Serializer.ToJson(entry.Value, valueType, context)
                ));
            }

            return pairs;
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            // handle PHP mangled empty JSON objects
            if (json.IsJsonArray && json.AsJsonArray.Count == 0)
                json = new JsonObject();
            
            if (json.IsJsonArray)
                return NonStringDictionaryFromJson(json, deserializationType, context);
            
            if (!json.IsJsonObject) // not array, nor object
                throw new UnisaveSerializationException(
                    "Cannot deserialize given json as a dictionary."
                );
            
            Type[] typeArguments = deserializationType.GetGenericArguments();
            Type keyType = typeArguments[0];
            Type valueType = typeArguments[1];

            IDictionary dictionary = CreateDictionaryInstance(deserializationType);

            foreach (KeyValuePair<string, JsonValue> item in json.AsJsonObject)
            {
                dictionary.Add(
                    Serializer.FromJson(item.Key, keyType, context),
                    Serializer.FromJson(item.Value, valueType, context)
                );
            }

            return dictionary;
        }

        private static object NonStringDictionaryFromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            Type[] typeArguments = deserializationType.GetGenericArguments();
            Type keyType = typeArguments[0];
            Type valueType = typeArguments[1];

            IDictionary dictionary = CreateDictionaryInstance(deserializationType);

            foreach (JsonValue pair in json.AsJsonArray)
            {
                dictionary.Add(
                    Serializer.FromJson(pair.AsJsonArray[0], keyType, context),
                    Serializer.FromJson(pair.AsJsonArray[1], valueType, context)
                );
            }

            return dictionary;
        }

        private static IDictionary CreateDictionaryInstance(
            Type deserializationType
        )
        {
            if (!typeof(IDictionary).IsAssignableFrom(deserializationType))
                throw new ArgumentException(
                    $"Given type {deserializationType} doesn't inherit from " +
                    $"the {nameof(IDictionary)} interface."
                );
            
            var constructor = deserializationType.GetConstructor(new Type[] { });
            
            if (constructor == null)
                throw new ArgumentException(
                    $"Given type {deserializationType} doesn't implement " +
                    $"the default constructor."
                );
            
            return (IDictionary) constructor.Invoke(new object[] {});
        }

        /// <summary>
        /// Returns true for types that can be used as string keys
        /// in a regular JSON object
        /// </summary>
        private static bool IsTypeSuitableAsKey(Type type)
        {
            if (type == typeof(string))
                return true;
            
            if (type.IsPrimitive)
                return true;

            if (type.IsEnum)
                return true;

            if (type == typeof(decimal))
                return true;

            return false;
        }
    }
}