using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using LightJson;
using LightJson.Serialization;
using Unisave.Entities;
using Unisave.Facades;
using Unisave.Serialization.Collections;
using Unisave.Serialization.Composites;
using Unisave.Serialization.Context;
using Unisave.Serialization.Exceptions;
using Unisave.Serialization.Primitives;
using Unisave.Serialization.Unisave;
using Unisave.Serialization.Unity;
using UnityEngine;

namespace Unisave.Serialization
{
    /// <summary>
    /// Handles Unisave JSON serialization
    /// </summary>
    public static class Serializer
    {
        #region "ITypeSerializer logic"
        
        /// <summary>
        /// When the dictionary key type matches exactly the requested type,
        /// the assigned serializer will be used for serialization.
        /// </summary>
        private static Dictionary<Type, ITypeSerializer> exactTypeSerializers
            = new Dictionary<Type, ITypeSerializer>();

        /// <summary>
        /// When the dictionary key type is assignable from the requested type,
        /// the assigned serializer will be used for serialization.
        /// </summary>
        private static Dictionary<Type, ITypeSerializer> assignableTypeSerializers
            = new Dictionary<Type, ITypeSerializer>();

        static Serializer()
        {
            SetSerializer<JsonValue>(new LightJsonSerializer());
            SetSerializer<JsonArray>(new LightJsonSerializer());
            SetSerializer<JsonObject>(new LightJsonSerializer());
            
            SetSerializer<DateTime>(new DateTimeSerializer());
            SetSerializer<decimal>(new DecimalSerializer());
            
            SetPolymorphicSerializer(typeof(Exception), new ExceptionSerializer());
            
            SetSerializer(typeof(List<>), new CollectionSerializer());
            SetSerializer(typeof(LinkedList<>), new CollectionSerializer());
            SetSerializer(typeof(Stack<>), new CollectionSerializer());
            SetSerializer(typeof(Queue<>), new CollectionSerializer());
            SetSerializer(typeof(HashSet<>), new CollectionSerializer());
            SetSerializer(typeof(SortedSet<>), new CollectionSerializer());
            
            SetSerializer(typeof(Dictionary<,>), new DictionarySerializer());
            SetSerializer(typeof(SortedDictionary<,>), new DictionarySerializer());
            SetSerializer(typeof(SortedList<,>), new DictionarySerializer());
            
            SetSerializer(typeof(Nullable<>), new NullableSerializer());
            
            SetSerializer(typeof(Vector2Int), new Vector2IntSerializer());
            SetSerializer(typeof(Vector3Int), new Vector3IntSerializer());
            
            SetPolymorphicSerializer(typeof(Entity), new EntitySerializer());
        }

        /// <summary>
        /// Override serialization of a given type
        /// Primitives, enums, arrays and such cannot be overriden
        /// </summary>
        public static void SetSerializer<T>(ITypeSerializer serializer)
            => SetSerializer(typeof(T), serializer);
        
        /// <summary>
        /// Override serialization of a given type
        /// Primitives, enums, arrays and such cannot be overriden
        /// </summary>
        public static void SetSerializer(Type type, ITypeSerializer serializer)
            => exactTypeSerializers[type] = serializer;

        /// <summary>
        /// Override serialization of any type, that is assignable to the provided type
        /// This override has lower priority than exact type override
        /// Primitives, enums, arrays and such cannot be overriden
        /// </summary>
        public static void SetPolymorphicSerializer<T>(ITypeSerializer serializer)
            => SetPolymorphicSerializer(typeof(T), serializer);
        
        /// <summary>
        /// Override serialization of any type, that is assignable to the provided type
        /// This override has lower priority than exact type override
        /// Primitives, enums, arrays and such cannot be overriden
        /// </summary>
        public static void SetPolymorphicSerializer(
            Type generalType,
            ITypeSerializer serializer
        )
        {
            foreach (var pair in assignableTypeSerializers)
            {
                if (pair.Key.IsAssignableFrom(generalType)
                    || generalType.IsAssignableFrom(pair.Key))
                {
                    throw new ArgumentException(
                        $"Cannot add polymorphic serializer for {generalType} " +
                        $"because there's already a polymorphic serializer for " +
                        $"type {pair.Key} and those two types are in relationship.\n"
                    );
                }
            }

            assignableTypeSerializers[generalType] = serializer;
        }

        /// <summary>
        /// Determine, whether there is an explicit serializer for given type
        /// (either exact, or polymorphic)
        /// </summary>
        public static bool HasSerializerFor(Type type)
        {
            if (exactTypeSerializers.ContainsKey(type))
                return true;

            foreach (var pair in assignableTypeSerializers)
                if (pair.Key.IsAssignableFrom(type))
                    return true;

            return false;
        }
        
        #endregion
        
        #region "High-level API"
        
        public static string ToJsonString(
            object subject,
            SerializationContext context = default(SerializationContext)
        ) => ToJson(subject, null, context).ToString();
        
        public static string ToJsonString<TTypeScope>(
            object subject,
            SerializationContext context = default(SerializationContext)
        ) => ToJson(subject, typeof(TTypeScope), context).ToString();
        
        public static JsonValue ToJson<TTypeScope>(
            object subject,
            SerializationContext context = default(SerializationContext)
        ) => ToJson(subject, typeof(TTypeScope), context);
        
        public static JsonValue ToJson(
            object subject,
            SerializationContext context
        ) => ToJson(subject, subject?.GetType(), context);
        
        public static TTypeScope FromJsonString<TTypeScope>(
            string jsonString,
            DeserializationContext context = default(DeserializationContext)
        )
        {
            JsonValue json = JsonReader.Parse(jsonString);
            
            return (TTypeScope) (
                FromJson(json, typeof(TTypeScope), context)
                    ?? default(TTypeScope)
            );
        }

        public static TTypeScope FromJson<TTypeScope>(
            JsonValue json,
            DeserializationContext context = default(DeserializationContext)
        )
        {
            return (TTypeScope)(
                FromJson(json, typeof(TTypeScope), context)
                    ?? default(TTypeScope)
            );
        }
        
        #endregion
        
        #region "Low-level API"

        public static JsonValue ToJson(
            object subject,
            Type typeScope = null,
            SerializationContext context = default(SerializationContext)
        )
        {
            // === Handle null ===
            
            if (subject == null)
                return JsonValue.Null;

            // === Parse and validate arguments ===
            
            if (typeScope == null)
                typeScope = subject.GetType();
            
            // context cannot be null, no need to check
            
            // === Serialize ===

            JsonValue json = Serialize(subject, typeScope, context);
            
            // === Add the "$type" attribute ===
            
            if (json.IsJsonObject)
            {
                if (json.AsJsonObject.ContainsKey("$type"))
                    throw new UnisaveSerializationException(
                        "You shouldn't produce the '$type' attribute when " +
                        "serializing your data. The attribute is reserved by " +
                        "Unisave."
                    );
                
                if (ShouldStoreType(subject, typeScope, context))
                    json["$type"] = subject.GetType().FullName;
            }

            return json;
        }

        private static JsonValue Serialize(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Type type = subject.GetType();
            
            // .NET primitives
            if (type.IsPrimitive)
                return DotNetPrimitivesSerializer.ToJson(subject, type);
            
            // string
            if (type == typeof(string))
                return (string)subject;

            // enums
            if (type.IsEnum)
                return EnumSerializer.ToJson(subject);

            // arrays
            if (type.IsArray)
                return ArraySerializer.ToJson(subject, typeScope, context);
            
            // by what type value to search through ITypeSerializers
            var searchType = type.IsGenericType
                ? type.GetGenericTypeDefinition()
                : type;
            
            // exact type serializers
            if (exactTypeSerializers.TryGetValue(searchType, out ITypeSerializer serializer))
                return serializer.ToJson(subject, typeScope, context);

            // assignable type serializers
            foreach (var pair in assignableTypeSerializers)
                if (pair.Key.IsAssignableFrom(searchType))
                    return pair.Value.ToJson(subject, typeScope, context);
            
            // unisave serializable
            if (typeof(IUnisaveSerializable).IsAssignableFrom(type))
                return UnisaveSerializableTypeSerializer.ToJson(
                    subject, typeScope, context
                );
            
            // serializable
            if (typeof(ISerializable).IsAssignableFrom(type))
                return SerializableTypeSerializer.ToJson(
                    subject, typeScope, context
                );
            
            // validate type scope
            if (!typeScope.IsAssignableFrom(type))
                throw new ArgumentException(
                    $"Given subject is of type {type} that is not assignable " +
                    $"to the given type scope {typeScope}"
                );

            // other
            return DefaultSerializer.ToJson(subject, typeScope, context);
        }

        public static object FromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context = default(DeserializationContext)
        )
        {
            // === Handle null ===
            
            if (json.IsNull)
                return null;
            
            // === Parse and validate arguments ===
            
            if (typeScope == null)
                throw new ArgumentNullException(nameof(typeScope));
            
            // context cannot be null, no need to check
            
            // === Determine deserialization type (the "$type" argument) ===
            
            Type deserializationType = GetDeserializationType(json, typeScope);
            
            // === Call static constructor of the deserialized type ===
            
            RuntimeHelpers.RunClassConstructor(deserializationType.TypeHandle);

            // === Deserialize ===
            
            // .NET primitives
            if (typeScope.IsPrimitive)
                return DotNetPrimitivesSerializer.FromJson(json, typeScope);
            
            // string
            if (typeScope == typeof(string))
                return json.AsString;

            // enums
            if (typeScope.IsEnum)
                return EnumSerializer.FromJson(json, typeScope);

            // arrays
            if (typeScope.IsArray)
                return ArraySerializer.FromJson(json, typeScope, context);

            // by what type value to search through ITypeSerializers
            var searchType = typeScope.IsGenericType
                ? typeScope.GetGenericTypeDefinition()
                : typeScope;
            
            // exact type serializers
            if (exactTypeSerializers.TryGetValue(searchType, out ITypeSerializer serializer))
                return serializer.FromJson(json, deserializationType, context);

            // assignable type serializers
            foreach (var pair in assignableTypeSerializers)
                if (pair.Key.IsAssignableFrom(searchType))
                    return pair.Value.FromJson(json, deserializationType, context);
            
            // unisave serializable
            if (typeof(IUnisaveSerializable).IsAssignableFrom(deserializationType))
                return UnisaveSerializableTypeSerializer.FromJson(
                    json, deserializationType, context
                );
            
            // serializable
            if (typeof(ISerializable).IsAssignableFrom(deserializationType))
                return SerializableTypeSerializer.FromJson(
                    json, deserializationType, context
                );

            // other
            return DefaultSerializer.FromJson(
                json,
                deserializationType,
                context
            );
        }
        
        #endregion
        
        #region "Helpers"
        
        private static bool ShouldStoreType(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            switch (context.typeSerialization)
            {
                case TypeSerialization.Always:
                    return true;
                
                case TypeSerialization.Never:
                    return false;
                
                case TypeSerialization.DuringPolymorphism:
                    return ! typeScope.IsEquivalentTo(subject.GetType())
                           && typeScope.IsInstanceOfType(subject);
            }
            
            throw new InvalidEnumArgumentException();
        }
        
        private static Type GetDeserializationType(
            JsonValue json,
            Type typeScope
        )
        {
            if (!json.IsJsonObject)
                return typeScope;

            JsonObject jsonObject = json.AsJsonObject;
            
            if (!jsonObject.ContainsKey("$type"))
                return typeScope;

            // slight optimization
            if (jsonObject["$type"].AsString == typeScope.FullName)
                return typeScope;
            
            Type type = FindType(jsonObject["$type"].AsString);
            
            if (type == null)
                throw new UnisaveSerializationException(
                    $"Type {jsonObject["$type"].AsString} couldn't be " +
                    $"deserialized, because it doesn't exist in the assembly."
                );
            
            if (!typeScope.IsAssignableFrom(type))
                throw new UnisaveSerializationException(
                    $"Type {jsonObject["$type"].AsString} couldn't be " +
                    $"deserialized as {typeScope} because it isn't assignable."
                );

            return type;
        }

        private static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (Type type in assembly.GetTypes())
            {
                if (type.FullName == fullName)
                    return type;
            }

            return null;
        }
        
        #endregion
    }
}
