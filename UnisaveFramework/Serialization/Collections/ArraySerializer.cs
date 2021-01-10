using System;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Collections
{
    internal static class ArraySerializer
    {
        public static JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            JsonArray jsonArray = new JsonArray();
            Array array = (Array)subject;
            
            Type elementTypeScope = typeScope.GetElementType();

            foreach (object item in array)
                jsonArray.Add(
                    Serializer.ToJson(item, elementTypeScope, context)
                );

            return jsonArray;
        }

        public static object FromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context
        )
        {
            JsonArray jsonArray = json.AsJsonArray;
            if (jsonArray == null)
                return null;

            Type elementType = typeScope.GetElementType();
            
            if (elementType == null)
                throw new ArgumentException(
                    $"Given type {typeScope} is not an array."
                );
            
            Array array = Array.CreateInstance(elementType, jsonArray.Count);

            for (int i = 0; i < jsonArray.Count; i++)
                array.SetValue(
                    Serializer.FromJson(jsonArray[i], elementType, context),
                    i
                );

            return array;
        }
    }
}