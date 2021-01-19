using System;
using System.Collections;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Collections
{
    public class ListSerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Type itemType = typeScope.GetGenericArguments()[0];
            
            JsonArray jsonArray = new JsonArray();
            IList list = (IList)subject;

            foreach (object item in list)
                jsonArray.Add(Serializer.ToJson(item, itemType, context));

            return jsonArray;
        }

        public object FromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context
        )
        {
            Type itemType = typeScope.GetGenericArguments()[0];

            IList list = (IList) typeScope.GetConstructor(new Type[] {})
                ?.Invoke(new object[] {});
            
            if (list == null)
                throw new ArgumentException(
                    $"Given type {typeScope} cannot be instantiated."
                );

            JsonArray jsonArray = json.AsJsonArray;
            if (jsonArray == null)
                return null;

            foreach (JsonValue item in jsonArray)
                list.Add(Serializer.FromJson(item, itemType, context));

            return list;
        }
    }
}