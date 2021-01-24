using System;
using System.Runtime.Serialization;
using LightJson;

namespace Unisave.Serialization.Primitives
{
    internal static class DotNetPrimitivesSerializer
    {
        public static JsonValue ToJson(
            object subject,
            Type type
        )
        {
            if (type == typeof(long))
                return ((long)subject).ToString();
            if (type == typeof(ulong))
                return ((ulong)subject).ToString();
            if (type == typeof(int))
                return (int)subject;
            if (type == typeof(uint))
                return ((uint)subject).ToString();
            if (type == typeof(short))
                return (short)subject;
            if (type == typeof(ushort))
                return (ushort)subject;
            if (type == typeof(byte))
                return (byte)subject;
            if (type == typeof(sbyte))
                return (sbyte)subject;
            
            if (type == typeof(bool))
                return (bool)subject;

            if (type == typeof(double))
            {
                if (double.IsNaN((double) subject))
                    return "NaN";
                
                if (double.IsPositiveInfinity((double) subject))
                    return "Infinity";
                
                if (double.IsNegativeInfinity((double) subject))
                    return "-Infinity";
                
                return (double) subject;
            }


            if (type == typeof(float))
            {
                if (float.IsNaN((float) subject))
                    return "NaN";
                
                if (float.IsPositiveInfinity((float) subject))
                    return "Infinity";
                
                if (float.IsNegativeInfinity((float) subject))
                    return "-Infinity";
                
                return (float) subject;
            }

            if (type == typeof(char))
                return ((char)subject).ToString();
            
            throw new SerializationException(
                $"Unisave cannot serialize this primitive type: {type}"
            );
        }
        
        public static object FromJson(
            JsonValue json,
            Type typeScope
        )
        {
            if (typeScope == typeof(long))
                return long.Parse(json.AsString);
            if (typeScope == typeof(ulong))
                return ulong.Parse(json.AsString);
            if (typeScope == typeof(int))
                return json.AsInteger;
            if (typeScope == typeof(uint))
                return uint.Parse(json.AsString);
            if (typeScope == typeof(short))
                return (short)json.AsInteger;
            if (typeScope == typeof(ushort))
                return (ushort)json.AsInteger;
            if (typeScope == typeof(byte))
                return (byte)json.AsInteger;
            if (typeScope == typeof(sbyte))
                return (sbyte)json.AsInteger;
            
            if (typeScope == typeof(bool))
                return json.AsBoolean;

            if (typeScope == typeof(double))
            {
                if (json.IsString)
                {
                    switch (json.AsString)
                    {
                        case "NaN": return double.NaN;
                        case "Infinity": return double.PositiveInfinity;
                        case "-Infinity": return double.NegativeInfinity;
                    }
                }
                
                return (double) json.AsNumber;
            }

            if (typeScope == typeof(float))
            {
                if (json.IsString)
                {
                    switch (json.AsString)
                    {
                        case "NaN": return float.NaN;
                        case "Infinity": return float.PositiveInfinity;
                        case "-Infinity": return float.NegativeInfinity;
                    }
                }
                
                return (float) json.AsNumber;
            }

            if (typeScope == typeof(char))
            {
                var s = json.AsString ?? "";
                return s.Length != 0 ? s[0] : '\0';
            }

            throw new SerializationException(
                $"Unisave cannot deserialize this primitive type: {typeScope}"
            );
        }
    }
}