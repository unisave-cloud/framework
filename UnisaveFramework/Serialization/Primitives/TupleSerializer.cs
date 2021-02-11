using System;
using System.Collections.Generic;
using System.Reflection;
using LightJson;
using Unisave.Serialization.Composites;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Primitives
{
    public class TupleSerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            JsonArray json = new JsonArray();

            List<Tuple<Type, object>> items = GetTupleItems(subject);

            foreach (var item in items)
                json.Add(Serializer.ToJson(item.Item2, item.Item1, context));

            return json;
        }

        // using tuples when serializing tuples, lol
        private List<Tuple<Type, object>> GetTupleItems(object subject)
        {
            Type subjectType = subject.GetType();
            Type genericSubjectType = subjectType.GetGenericTypeDefinition();
            Type[] typeArgs = subjectType.GenericTypeArguments;
            
            List<Tuple<Type, object>> items = new List<Tuple<Type, object>>();
            
            if (IsValueTuple(genericSubjectType))
            {
                foreach (FieldInfo fi in subjectType.GetFields())
                {
                    items.Add(new Tuple<Type, object>(
                        fi.FieldType,
                        fi.GetValue(subject)
                    ));
                }
            }
            else if (IsRegularTuple(genericSubjectType))
            {
                foreach (PropertyInfo pi in subjectType.GetProperties())
                {
                    items.Add(new Tuple<Type, object>(
                        pi.PropertyType,
                        pi.GetMethod.Invoke(subject, new object[0])
                    ));
                }
            }
            else
            {
                throw new UnisaveSerializationException(
                    $"Unknown tuple type {subjectType}"
                );
            }
            
            // handle long tuples (recursively)
            if (items.Count == 8
                && typeArgs[7].IsGenericType
                && IsTupleType(typeArgs[7].GetGenericTypeDefinition()))
            {
                items.AddRange(GetTupleItems(items[7].Item2));
                items.RemoveAt(7);
            }

            return items;
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            // legacy format
            if (json.IsJsonObject)
                return DefaultSerializer.FromJson(
                    json, deserializationType, context
                );
            
            JsonArray jsonArray = json.AsJsonArray;
            
            if (jsonArray == null)
                throw new UnisaveSerializationException(
                    "Given JSON cannot be deserialized as a Tuple."
                );
            
            if (jsonArray.Count == 0)
                throw new UnisaveSerializationException(
                    "Empty JSON array cannot be deserialized as a Tuple."
                );

            Type[] typeArgs = deserializationType.GenericTypeArguments;
            
            ConstructorInfo ci = deserializationType.GetConstructor(typeArgs);
            
            if (ci == null)
                throw new UnisaveSerializationException(
                    $"Cannot construct type {deserializationType}"
                );
            
            object[] items = new object[typeArgs.Length];
            
            for (int i = 0; i < items.Length; i++)
            {
                // handle long tuples (recursively)
                if (i == 7)
                {
                    if (!typeArgs[i].IsGenericType
                        || !IsTupleType(typeArgs[i].GetGenericTypeDefinition()))
                        throw new UnisaveSerializationException(
                            "Eighth tuple value has to be another tuple."
                        );
                    
                    items[7] = FromJson(jsonArray, typeArgs[7], context);
                    break;
                }
                
                items[i] = Serializer.FromJson(jsonArray[0], typeArgs[i], context);
                jsonArray.Remove(0);
            }
            

            return ci.Invoke(items);
        }
        
        private bool IsTupleType(Type genericType)
            => IsRegularTuple(genericType) || IsValueTuple(genericType);

        private bool IsValueTuple(Type genericType)
        {
            return genericType == typeof(ValueTuple<>)
                   || genericType == typeof(ValueTuple<,>)
                   || genericType == typeof(ValueTuple<,,>)
                   || genericType == typeof(ValueTuple<,,,>)
                   || genericType == typeof(ValueTuple<,,,,>)
                   || genericType == typeof(ValueTuple<,,,,,>)
                   || genericType == typeof(ValueTuple<,,,,,,>)
                   || genericType == typeof(ValueTuple<,,,,,,,>);
        }
        
        private bool IsRegularTuple(Type genericType)
        {
            return genericType == typeof(Tuple<>)
                   || genericType == typeof(Tuple<,>)
                   || genericType == typeof(Tuple<,,>)
                   || genericType == typeof(Tuple<,,,>)
                   || genericType == typeof(Tuple<,,,,>)
                   || genericType == typeof(Tuple<,,,,,>)
                   || genericType == typeof(Tuple<,,,,,,>)
                   || genericType == typeof(Tuple<,,,,,,,>);
        }
    }
}