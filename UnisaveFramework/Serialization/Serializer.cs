using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using LightJson;
using LightJson.Serialization;
using Unisave.Facades;
using Unisave.Serialization.Collections;
using Unisave.Serialization.Composites;
using Unisave.Serialization.Context;
using Unisave.Serialization.Primitives;

namespace Unisave.Serialization
{
    // TODO: serialize nullable types
    // TODO: serialize HashSet, Stack, Queue
    // TODO: attribute and class renaming (FormerlySerializedAs)
    
    /// <summary>
    /// Handles Unisave JSON serialization
    /// </summary>
    public static class Serializer
    {
        #region "Custom serializers"
        
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
            DefaultTypeSerializers.RegisterAllSerializers();
        }

        /// <summary>
        /// Override serialization of a given type
        /// Primitives, enums, arrays, lists and such cannot be overriden
        /// </summary>
        public static void SetExactTypeSerializer(Type type, ITypeSerializer serializer)
            => exactTypeSerializers[type] = serializer;

        /// <summary>
        /// Override serialization of any type, that is assignable to a provided general type
        /// This override has lower priority than exact type override
        /// Primitives, enums, arrays, lists and such cannot be overriden
        /// </summary>
        public static void SetAssignableTypeSerializer(Type generalType, ITypeSerializer serializer)
        {
            foreach (var pair in assignableTypeSerializers)
            {
                if (pair.Key.IsAssignableFrom(generalType) || generalType.IsAssignableFrom(pair.Key))
                {
                    throw new ArgumentException(
                        $"Cannot add assignable serializer for {generalType} because there's already a "
                        + $"serializer for type {pair.Key} and those two types are in relationship.\n"
                        + "Technically Unisave could just pick the least general parent and use it, "
                        + "but it seemed like a rare situation so I haven't implemented it yet."
                    );
                }
            }

            assignableTypeSerializers[generalType] = serializer;
        }

        /// <summary>
        /// Determine, whether there is an explicit serializer for given type
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
            SerializationContext context = null
        ) => ToJson(subject, null, context).ToString();
        
        public static string ToJsonString<TTypeScope>(
            object subject,
            SerializationContext context = null
        ) => ToJson(subject, typeof(TTypeScope), context).ToString();
        
        public static JsonValue ToJson<TTypeScope>(
            object subject,
            SerializationContext context = null
        ) => ToJson(subject, typeof(TTypeScope), context);
        
        public static TTypeScope FromJsonString<TTypeScope>(
            string jsonString,
            DeserializationContext context = null
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
            DeserializationContext context = null
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
            SerializationContext context = null
        )
        {
            // === Handle null ===
            
            if (subject == null)
                return JsonValue.Null;

            // === Parse and validate arguments ===
            
            if (typeScope == null)
                typeScope = subject.GetType();
            
            if (context == null)
                context = SerializationContext.DefaultContext();
            
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
            
            // lists and dictionaries
            if (type.IsGenericType)
            {
                // lists
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                    return ListSerializer.ToJson(subject, typeScope, context);

                // dictionaries
                if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    return DictionarySerializer.ToJson(subject, typeScope, context);
            }
            
            // exact type serializers
            if (exactTypeSerializers.TryGetValue(type, out ITypeSerializer serializer))
                return serializer.ToJson(subject, typeScope, context);

            // assignable type serializers
            foreach (var pair in assignableTypeSerializers)
                if (pair.Key.IsAssignableFrom(type))
                    return pair.Value.ToJson(subject, typeScope, context);
            
            // serializable
            if (type.GetCustomAttribute<SerializableAttribute>(inherit: false) != null)
                throw new NotImplementedException(
                    "Serializable attribute marked types are not supported yet."
                );
            
            // validate type scope
            if (!typeScope.IsAssignableFrom(type))
                throw new ArgumentException(
                    $"Given subject is of type {type} that is not assignable " +
                    $"to the given type scope {typeScope}"
                );

            // other
            return UnknownTypeSerializer.ToJson(subject, typeScope, context);
        }

        public static object FromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context = null
        )
        {
            // === Handle null ===
            
            if (json.IsNull)
                return null;
            
            // === Parse and validate arguments ===
            
            if (typeScope == null)
                throw new ArgumentNullException(nameof(typeScope));
            
            if (context == null)
                context = DeserializationContext.DefaultContext();
            
            // === Determine deserialization type (the "$type" argument) ===
            
            Type deserializationType = GetDeserializationType(json, typeScope);

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

            // lists and dictionaries
            if (typeScope.IsGenericType)
            {
                // lists
                if (typeScope.GetGenericTypeDefinition() == typeof(List<>))
                    return ListSerializer.FromJson(json, typeScope, context);

                // dictionaries
                if (typeScope.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    return DictionarySerializer.FromJson(json, typeScope, context);
            }
            
            // exact type serializers
            if (exactTypeSerializers.TryGetValue(typeScope, out ITypeSerializer serializer))
                return serializer.FromJson(json, deserializationType, context);

            // assignable type serializers
            foreach (var pair in assignableTypeSerializers)
                if (pair.Key.IsAssignableFrom(typeScope))
                    return pair.Value.FromJson(json, deserializationType, context);
            
            // serializable
            if (deserializationType.GetCustomAttribute<SerializableAttribute>(inherit: false) != null)
                throw new NotImplementedException(
                    "Serializable attribute marked types are not supported yet."
                );

            // other
            return UnknownTypeSerializer.FromJson(
                json,
                deserializationType,
                context
            );
        }
        
        #endregion
        
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
    }
}
