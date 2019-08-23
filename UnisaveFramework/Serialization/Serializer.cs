using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Serialization
{
    /// <summary>
    /// Handles unisave json serialization
    /// </summary>
    public static class Serializer
    {
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

        /// <summary>
        /// Logs a warning message
        /// </summary>
        private static void Warning(string message)
        {
            message = "Unisave serializer: " + message;

            #if UNITY_EDITOR
                UnityEngine.Debug.LogWarning(message);
            #else
                Console.WriteLine(message);
            #endif
        }

        /// <summary>
        /// Serializes given object into JSON
        /// </summary>
        public static JsonValue ToJson(object subject)
        {
            if (subject == null)
                return JsonValue.Null;

            Type type = subject.GetType();

            // primitives
            if (type == typeof(long))
                return (int)subject;
            else if (type == typeof(int))
                return (int)subject;
            else if (type == typeof(short))
                return (int)subject;
            else if (type == typeof(byte))
                return (int)subject;
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
                return EnumToJson(subject, type);

            // arrays
            if (type.IsArray)
                return ArrayToJson(subject, type);

            // lists and dictionaries
            if (type.IsGenericType)
            {
                // lists
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                    return ListToJson(subject);

                // dictionaries
                if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    return DictionaryToJson(subject, type);
            }

            // exact type serializers
            if (exactTypeSerializers.TryGetValue(type, out ITypeSerializer serializer))
                return serializer.ToJson(subject);

            // assignable type serializers
            foreach (var pair in assignableTypeSerializers)
                if (pair.Key.IsAssignableFrom(type))
                    return pair.Value.ToJson(subject);

            // other
            return UnknownTypeToJson(subject, type);
        }

        // alias
        public static T FromJson<T>(string json)
            => (T)(FromJson(json, typeof(T)) ?? default(T));

        // alias
        public static object FromJson(string json, Type type)
            => FromJson(JsonReader.Parse(json), type);

        /// <summary>
        /// Deserializes an object from JSON
        /// </summary>
        public static object FromJson(JsonValue json, Type type)
        {
            // primitives
            if (type == typeof(long))
                return (long)json.AsInteger;
            else if (type == typeof(int))
                return json.AsInteger;
            else if (type == typeof(short))
                return (short)json.AsInteger;
            else if (type == typeof(byte))
                return (byte)json.AsInteger;
            else if (type == typeof(bool))
                return json.AsBoolean;
            else if (type == typeof(double))
                return json.AsNumber;
            else if (type == typeof(float))
                return (float)json.AsNumber;
            else if (type == typeof(string))
                return json.AsString;

            // not a primitive, may be null
            if (json.IsNull)
                return null;

            // enums
            if (type.IsEnum)
                return EnumFromJson(json, type);

            // arrays
            if (type.IsArray)
                return ArrayFromJson(json, type);

            // lists and dictionaries
            if (type.IsGenericType)
            {
                // lists
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                    return ListFromJson(json, type);

                // dictionaries
                if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    return DictionaryFromJson(json, type);
            }

            // exact type serializers
            if (exactTypeSerializers.TryGetValue(type, out ITypeSerializer serializer))
                return serializer.FromJson(json, type);

            // assignable type serializers
            foreach (var pair in assignableTypeSerializers)
                if (pair.Key.IsAssignableFrom(type))
                    return pair.Value.FromJson(json, type);

            // other
            return UnknownTypeFromJson(json, type);
        }

        private static JsonValue EnumToJson(object subject, Type type)
        {
            // NOTE: String part is for "What the f**k does value 5 mean?"
            //       Integer part is used for the actual deserialization

            return subject.ToString() + "=" + ((int)subject).ToString();
        }

        private static object EnumFromJson(JsonValue json, Type type)
        {
            // NOTE: String part is for "What the f**k does value 5 mean?"
            //       Integer part is used for the actual deserialization

            string value = json.AsString;

            if (value == null)
            {
                Warning($"Loading enum {type} failed because the provided value was invalid: {json.ToString()}");
                return 0;
            }

            string[] parts = value.Split('=');

            if (parts.Length != 2)
            {
                Warning($"Loading enum {type} failed because the provided value was invalid: {json.ToString()}");
                return 0;
            }

            if (!int.TryParse(parts[1], out int intValue))
            {
                Warning($"Loading enum {type} failed because the provided value was invalid: {json.ToString()}");
                return 0;
            }

            return intValue;
        }

        private static JsonValue ArrayToJson(object subject, Type type)
        {
            JsonArray jsonArray = new JsonArray();
            Array array = (Array)subject;

            foreach (object item in array)
                jsonArray.Add(ToJson(item));

            return jsonArray;
        }

        private static object ArrayFromJson(JsonValue json, Type type)
        {
            JsonArray jsonArray = json.AsJsonArray;
            if (jsonArray == null)
                return null;

            Type elementType = type.GetElementType();
            Array array = Array.CreateInstance(elementType, jsonArray.Count);

            for (int i = 0; i < jsonArray.Count; i++)
                array.SetValue(FromJson(jsonArray[i], elementType), i);

            return array;
        }

        private static JsonValue ListToJson(object subject)
        {
            JsonArray jsonArray = new JsonArray();
            IList list = (IList)subject;

            foreach (object item in list)
                jsonArray.Add(ToJson(item));

            return jsonArray;
        }

        private static object ListFromJson(JsonValue json, Type type)
        {
            Type itemType = type.GetGenericArguments()[0];

            object list = type.GetConstructor(new Type[] {}).Invoke(new object[] {});

            JsonArray jsonArray = json.AsJsonArray;
            if (jsonArray == null)
                return null;

            foreach (JsonValue item in jsonArray)
                type.GetMethod("Add").Invoke(list, new object[] {
                    FromJson(item, itemType)
                });

            return list;
        }

        private static JsonValue DictionaryToJson(object subject, Type type)
        {
            Type keyType = type.GetGenericArguments()[0];

            if (keyType != typeof(string))
                throw new Exception("Unisave serialization: Non-string key dictionaries not supported.");

            JsonObject jsonObject = new JsonObject();
            IDictionary dictionary = (IDictionary)subject;

            foreach (DictionaryEntry entry in dictionary)
                jsonObject.Add((string)entry.Key, ToJson(entry.Value));

            return jsonObject;
        }

        private static object DictionaryFromJson(JsonValue json, Type type)
        {
            Type[] typeArguments = type.GetGenericArguments();
            Type keyType = typeArguments[0];
            Type valueType = typeArguments[1];
            
            if (keyType != typeof(string))
                throw new Exception("Unisave serialization: Dictionaries with non-string keys are not supported.");

            object dictionary = type.GetConstructor(new Type[] {}).Invoke(new object[] {});

            JsonObject jsonObject = json.AsJsonObject;
            if (jsonObject == null)
                return null;

            foreach (KeyValuePair<string, JsonValue> item in jsonObject)
                type.GetMethod("Add").Invoke(dictionary, new object[] {
                    item.Key,
                    FromJson(item.Value, valueType)
                });

            return dictionary;
        }

        private static JsonValue UnknownTypeToJson(object subject, Type type)
        {
            JsonObject jsonObject = new JsonObject();

            // get public non-static fields
            foreach (FieldInfo fi in type.GetFields())
            {
                if (fi.IsPublic && !fi.IsStatic)
                {
                    jsonObject.Add(fi.Name, ToJson(fi.GetValue(subject)));
                }
            }

            return jsonObject;
        }

        private static object UnknownTypeFromJson(JsonValue json, Type type)
        {
            JsonObject jsonObject = json.AsJsonObject;
            if (jsonObject == null)
                return null;

            ConstructorInfo ci = type.GetConstructor(new Type[] {});
            if (ci == null)
            {
                Warning($"Trying to load unknown type {type}, but it lacks public parameterless constructor.");
                return null;
            }

            object instance = ci.Invoke(new object[] {});

            // set public non-static fields
            foreach (FieldInfo fi in type.GetFields())
            {
                if (fi.IsPublic && !fi.IsStatic)
                {
                    fi.SetValue(instance, FromJson(jsonObject[fi.Name], fi.FieldType));
                }
            }

            return instance;
        }
    }
}
