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
using Unisave.Serialization.Composites;
using Unisave.Serialization.Context;

namespace Unisave.Serialization
{
    // TODO: serialize nullable types
    // TODO: serialize HashSet, Stack, Queue
    // TODO: attribute and class renaming (FormerlySerializedAs)
    
    // TODO: use proper context in facet serialization and entity serialization
    
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
            
            // primitives
            if (type == typeof(long))
                return ((long)subject).ToString();
            else if (type == typeof(ulong))
                return ((ulong)subject).ToString();
            else if (type == typeof(int))
                return (int)subject;
            else if (type == typeof(uint))
                return ((uint)subject).ToString();
            else if (type == typeof(short))
                return (short)subject;
            else if (type == typeof(ushort))
                return (ushort)subject;
            else if (type == typeof(byte))
                return (byte)subject;
            else if (type == typeof(bool))
                return (bool)subject;
            else if (type == typeof(double))
                return (double)subject;
            else if (type == typeof(float))
                return (float)subject;
            else if (type == typeof(string))
                return (string)subject;

            // enums
            if (type.IsEnum)
                return EnumToJson(subject);

            // arrays
            if (type.IsArray)
                return ArrayToJson(subject, typeScope, context);
            
            // lists and dictionaries
            if (type.IsGenericType)
            {
                // lists
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                    return ListToJson(subject, typeScope, context);

                // dictionaries
                if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    return DictionaryToJson(subject, typeScope, context);
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
            
            // primitives
            if (typeScope == typeof(long))
                return long.Parse(json.AsString);
            else if (typeScope == typeof(ulong))
                return ulong.Parse(json.AsString);
            else if (typeScope == typeof(int))
                return json.AsInteger;
            else if (typeScope == typeof(uint))
                return uint.Parse(json.AsString);
            else if (typeScope == typeof(short))
                return (short)json.AsInteger;
            else if (typeScope == typeof(ushort))
                return (ushort)json.AsInteger;
            else if (typeScope == typeof(byte))
                return (byte)json.AsInteger;
            else if (typeScope == typeof(bool))
                return json.AsBoolean;
            else if (typeScope == typeof(double))
                return json.AsNumber;
            else if (typeScope == typeof(float))
                return (float)json.AsNumber;
            else if (typeScope == typeof(string))
                return json.AsString;

            // enums
            if (typeScope.IsEnum)
                return EnumFromJson(json, typeScope);

            // arrays
            if (typeScope.IsArray)
                return ArrayFromJson(json, typeScope, context);

            // lists and dictionaries
            if (typeScope.IsGenericType)
            {
                // lists
                if (typeScope.GetGenericTypeDefinition() == typeof(List<>))
                    return ListFromJson(json, typeScope, context);

                // dictionaries
                if (typeScope.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    return DictionaryFromJson(json, typeScope, context);
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
        
        #region "Implementation"

        private static JsonValue EnumToJson(object subject)
        {
            // NOTE: Enums used to be serialized as strings "Name=42",
            // but that caused problems in linq expression trees
            // and enum arithmetic: "Foo.A | Foo.B"

            return (int)subject;
            
            // Legacy serialization:
            // return subject.ToString() + "=" + ((int)subject).ToString();
        }

        private static object EnumFromJson(JsonValue json, Type typeScope)
        {
            if (json.IsInteger)
                return json.AsInteger;

            return LegacyEnumFromJson(json, typeScope);
        }

        private static object LegacyEnumFromJson(JsonValue json, Type typeScope)
        {
            // NOTE: String part is for "What the f**k does value 5 mean?"
            //       Integer part is used for the actual deserialization

            string value = json.AsString;

            if (value == null)
            {
                Log.Warning(
                    $"Loading enum {typeScope} failed because the provided " +
                    $"value was invalid: {json.ToString()}"
                );
                return 0;
            }

            string[] parts = value.Split('=');

            if (parts.Length != 2)
            {
                Log.Warning(
                    $"Loading enum {typeScope} failed because the provided " +
                    $"value was invalid: {json.ToString()}"
                );
                return 0;
            }

            if (!int.TryParse(parts[1], out int intValue))
            {
                Log.Warning(
                    $"Loading enum {typeScope} failed because the provided " +
                    $"value was invalid: {json.ToString()}"
                );
                return 0;
            }

            return intValue;
        }

        private static JsonValue ArrayToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            JsonArray jsonArray = new JsonArray();
            Array array = (Array)subject;

            foreach (object item in array)
                jsonArray.Add(ToJson(item, typeScope, context));

            return jsonArray;
        }

        private static object ArrayFromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context
        )
        {
            JsonArray jsonArray = json.AsJsonArray;
            if (jsonArray == null)
                return null;

            Type elementType = typeScope.GetElementType();
            Array array = Array.CreateInstance(elementType, jsonArray.Count);

            for (int i = 0; i < jsonArray.Count; i++)
                array.SetValue(FromJson(jsonArray[i], elementType, context), i);

            return array;
        }

        private static JsonValue ListToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Type itemType = typeScope.GetGenericArguments()[0];
            
            JsonArray jsonArray = new JsonArray();
            IList list = (IList)subject;

            foreach (object item in list)
                jsonArray.Add(ToJson(item, itemType, context));

            return jsonArray;
        }

        private static object ListFromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context
        )
        {
            Type itemType = typeScope.GetGenericArguments()[0];

            object list = typeScope.GetConstructor(new Type[] {}).Invoke(new object[] {});

            JsonArray jsonArray = json.AsJsonArray;
            if (jsonArray == null)
                return null;

            foreach (JsonValue item in jsonArray)
                typeScope.GetMethod("Add").Invoke(list, new object[] {
                    FromJson(item, itemType, context)
                });

            return list;
        }

        private static JsonValue DictionaryToJson(
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
                jsonObject.Add((string)entry.Key, ToJson(entry.Value, valueType, context));

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

        private static object DictionaryFromJson(
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

            JsonObject jsonObject = json.AsJsonObject;
            if (jsonObject == null)
                return null;

            foreach (KeyValuePair<string, JsonValue> item in jsonObject)
                typeScope.GetMethod("Add").Invoke(dictionary, new object[] {
                    item.Key,
                    FromJson(item.Value, valueType, context)
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
                    FromJson(pair.AsJsonArray[0], keyType, context),
                    FromJson(pair.AsJsonArray[1], valueType, context)
                });
            }

            return dictionary;
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
