using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Composites
{
    /// <summary>
    /// Serializes types that are not in any way marked for serialization
    /// </summary>
    public static class DefaultSerializer
    {
        private const string AutoPropPrefix = "<";
        private const string AutoPropSuffix = ">k__BackingField";
        
        public static JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            Type type = subject.GetType();
            
            JsonObject json = new JsonObject();

            foreach ((FieldInfo fi, string name) in EnumerateFields(
                type, context.securityDomainCrossing
            ))
            {
                json.Add(
                    name,
                    Serializer.ToJson(
                        fi.GetValue(subject),
                        fi.FieldType,
                        context
                    )
                );
            }

            return json;
        }

        public static object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            JsonObject jsonObject = json.AsJsonObject;
            
            if (jsonObject == null)
                return null;

            object instance = CreateInstance(deserializationType);

            PopulateInstance(instance, jsonObject, context);

            return instance;
        }

        private static object CreateInstance(Type type)
        {
            // try to get an instance via the default constructor
            ConstructorInfo ci = type.GetConstructor(new Type[] { });
            
            if (ci != null)
                return ci.Invoke(new object[] { });

            // fall back to creating an uninitialized object
            // DOWN SIDE: missing fields won't have default values
            return FormatterServices.GetUninitializedObject(
                type
            );
        }

        /// <summary>
        /// Populate an instance with values in a given JSON object
        /// </summary>
        public static void PopulateInstance(
            object instance,
            JsonObject jsonObject,
            DeserializationContext context
        )
        {
            foreach ((FieldInfo fi, string name) in EnumerateFields(
                instance.GetType(), context.securityDomainCrossing
            ))
            {
                // skip fields missing in JSON
                if (!jsonObject.ContainsKey(name))
                    continue;
                
                fi.SetValue(
                    instance,
                    Serializer.FromJson(
                        jsonObject[name],
                        fi.FieldType,
                        context
                    )
                );
            }
        }

        private static IEnumerable<(FieldInfo, string)> EnumerateFields(
            Type type,
            SecurityDomainCrossing security
        )
        {
            var flags = BindingFlags.Instance
                        | BindingFlags.DeclaredOnly
                        | BindingFlags.Public
                        | BindingFlags.NonPublic;
            
            // climb up the inheritance hierarchy and collect fields
            while (type != null && type != typeof(object))
            {
                // get all fields in this type
                foreach (FieldInfo fi in type.GetFields(flags))
                {
                    // skip static fields
                    if (fi.IsStatic)
                        continue;

                    // extract data for plain fields
                    string name = fi.Name; // name to serialize as
                    MemberInfo attrMi = fi; // member with attributes

                    // extract data for auto-prop backing fields
                    if (IsAutoPropField(fi.Name))
                    {
                        name = SanitizeAutoPropFieldName(fi.Name);
                        attrMi = type.GetProperty(name, flags)
                            ?? throw new UnisaveSerializationException(
                                $"Cannot serialize field {fi.Name} of type " +
                                $"{type}. The auto-property cannot be found."
                            );
                    }
                    
                    // skip fields that shouldn't be serialized
                    if (attrMi.GetCustomAttribute<DontSerializeAttribute>() != null)
                        continue;

                    // skip fields that shouldn't leave the server
                    // (when we're actually leaving the server)
                    if (security == SecurityDomainCrossing.LeavingServer
                        && attrMi.GetCustomAttribute<DontLeaveServerAttribute>() != null)
                        continue; 
                    
                    // handle serialization renaming
                    var serializeAsAttr = attrMi.GetCustomAttribute<SerializeAsAttribute>();
                    if (serializeAsAttr != null)
                        name = serializeAsAttr.SerializedName;

                    yield return (fi, name);
                }
                
                type = type.BaseType;
            }
        }

        private static bool IsAutoPropField(string name)
        {
            return name.StartsWith(AutoPropPrefix)
                   && name.EndsWith(AutoPropSuffix);
        }
        
        private static string SanitizeAutoPropFieldName(string name)
        {
            if (IsAutoPropField(name))
            {
                return name.Substring(
                    AutoPropPrefix.Length,
                    name.Length - AutoPropPrefix.Length - AutoPropSuffix.Length
                );
            }

            return name;
        }
    }
}