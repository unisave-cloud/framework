using System;
using System.Reflection;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Primitives
{
    internal static class NullableSerializer
    {
        public static JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            // the value is not null, because nulls are caught at the beginning
            // of the serializer
            
            Type itemType = typeScope.GetGenericArguments()[0];

            PropertyInfo pi = typeScope.GetProperty("Value");
            
            return Serializer.ToJson(
                pi.GetValue(subject),
                itemType,
                context
            );
        }

        public static object FromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context = null
        )
        {
            // the value is not null, because nulls are caught at the beginning
            // of the serializer
            
            Type itemType = typeScope.GetGenericArguments()[0];
            
            object item = Serializer.FromJson(json, itemType, context);

            return typeScope
                .GetConstructor(new Type[] { itemType })
                .Invoke(new object[] { item });
        }
    }
}