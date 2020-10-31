using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Composites
{
    /// <summary>
    /// Serializes types that are not in any way marked for serialization
    /// </summary>
    internal static class UnknownTypeSerializer
    {
        public static JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            JsonObject json = new JsonObject();

            foreach ((FieldInfo fi, string name) in EnumerateFields(typeScope))
            {
                json.Add(
                    name,
                    Serializer.ToJson(fi.GetValue(subject), fi.FieldType, context)
                );
            }

            return json;
        }

        public static object FromJson(
            JsonValue json,
            Type typeScope,
            DeserializationContext context
        )
        {
            JsonObject jsonObject = json.AsJsonObject;
            if (jsonObject == null)
                return null;

            object instance = FormatterServices.GetUninitializedObject(typeScope);

            foreach ((FieldInfo fi, string name) in EnumerateFields(typeScope))
            {
                fi.SetValue(
                    instance,
                    Serializer.FromJson(jsonObject[name], fi.FieldType, context)
                );
            }

            return instance;
        }

        private static IEnumerable<(FieldInfo, string)> EnumerateFields(Type type)
        {
            var flags = BindingFlags.Instance
                        | BindingFlags.DeclaredOnly
                        | BindingFlags.Public
                        | BindingFlags.NonPublic;
            
            while (type != null && type != typeof(object))
            {
                foreach (FieldInfo fi in type.GetFields(flags))
                {
                    if (fi.IsStatic)
                        continue;

                    yield return (fi, SanitizeAutopropBackingFields(fi.Name));
                }
                
                type = type.BaseType;
            }
        }

        private static string SanitizeAutopropBackingFields(string name)
        {
            const string prefix = "<";
            const string suffix = ">k__BackingField";
            
            if (name.StartsWith(prefix) && name.EndsWith(suffix))
            {
                return name.Substring(
                    prefix.Length,
                    name.Length - prefix.Length - suffix.Length
                );
            }

            return name;
        }
    }
}