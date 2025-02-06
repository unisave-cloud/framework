using System;
using LightJson;
using Unisave.Facades;

namespace Unisave.Serialization.Primitives
{
    internal static class EnumSerializer
    {
        public static JsonValue ToJson(object subject)
        {
            // NOTE: Enums used to be serialized as strings "Name=42",
            // but that caused problems in linq expression trees
            // and enum arithmetic: "Foo.A | Foo.B"

            return (int)subject;
            
            // Legacy serialization:
            // return subject.ToString() + "=" + ((int)subject).ToString();
        }

        public static object FromJson(JsonValue json, Type typeScope)
        {
            if (json.IsInteger)
                return Enum.ToObject(typeScope, json.AsInteger);

            // can be a legacy enum, or a string integer (in dictionary keys) 
            if (json.IsString)
            {
                if (!json.AsString.Contains("=")) // not a legacy enum
                    return Enum.ToObject(typeScope, json.AsInteger);
            }

            return Enum.ToObject(
                typeScope,
                FromLegacyJson(json, typeScope)
            );
        }

        private static int FromLegacyJson(JsonValue json, Type typeScope)
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
    }
}