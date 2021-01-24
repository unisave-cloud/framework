using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Collections
{
    public class CollectionSerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Type type = subject.GetType();
            Type itemType = type.GetGenericArguments()[0];
            
            JsonArray jsonArray = new JsonArray();
            
            foreach (object item in (IEnumerable) subject)
                jsonArray.Add(Serializer.ToJson(item, itemType, context));
            
            return jsonArray;
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            var constructor = deserializationType.GetConstructor(new Type[] { });
            
            if (constructor == null)
                throw new ArgumentException(
                    $"Given type {deserializationType} doesn't implement " +
                    $"the default constructor."
                );

            if (!json.IsJsonArray)
                return null;
            
            Type itemType = deserializationType.GetGenericArguments()[0];
            Type genericType = deserializationType.GetGenericTypeDefinition();
            
            MethodInfo addMethod = GetAddMethod(
                    deserializationType, genericType, itemType
                ) ?? throw new NullReferenceException(
                    "No adequate 'add item' method was found."
                );
            
            object instance = constructor.Invoke(new object[] {});
            
            JsonArray jsonArray = json.AsJsonArray;
            
            // reverse stacks
            if (genericType == typeof(Stack<>))
                jsonArray = new JsonArray(jsonArray.Reverse().ToArray());

            foreach (JsonValue item in jsonArray)
            {
                addMethod.Invoke(instance, new object[] {
                    Serializer.FromJson(item, itemType, context)
                });
            }

            return instance;
        }

        private static MethodInfo GetAddMethod(
            Type deserializationType,
            Type genericType,
            Type itemType
        )
        {
            if (genericType == typeof(Stack<>))
                return deserializationType.GetMethod("Push", new[] {itemType});
            
            if (genericType == typeof(Queue<>))
                return deserializationType.GetMethod("Enqueue", new[] {itemType});
            
            if (genericType == typeof(LinkedList<>))
                return deserializationType.GetMethod("AddLast", new[] {itemType});
            
            return deserializationType.GetMethod("Add", new[] {itemType});
        }
    }
}