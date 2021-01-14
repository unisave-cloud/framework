using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Composites
{
    public static class SerializableTypeSerializer
    {
        public static JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            SerializationInfo info = ExtractSerializationInfo(subject, context);
            
            JsonObject json = new JsonObject();

            foreach (SerializationEntry entry in info)
            {
                json.Add(
                    entry.Name,
                    Serializer.ToJson(entry.Value, entry.ObjectType, context)
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

            var info = new SerializationInfo(
                deserializationType,
                new JsonDeserializationConverter(context)
            );

            foreach (var pair in jsonObject)
                info.AddValue(pair.Key, (object) pair.Value);

            var instance = Activator.CreateInstance(
                deserializationType,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object[] { info, context.GetStreamingContext() },
                CultureInfo.InvariantCulture
            );

            return instance;
        }

        private static SerializationInfo ExtractSerializationInfo(
            object subject,
            SerializationContext context
        )
        {
            SerializationInfo info = new SerializationInfo(
                subject.GetType(),
                new FormatterConverter()
            );
            
            ((ISerializable) subject).GetObjectData(
                info,
                context.GetStreamingContext()
            );

            return info;
        }

        /// <summary>
        /// Converts JSON to the target type at the moment the value
        /// is requested because that's the moment we know the target type
        /// </summary>
        private class JsonDeserializationConverter : IFormatterConverter
        {
            private readonly FormatterConverter defaultConverter
                = new FormatterConverter();

            private readonly DeserializationContext context;
            
            public JsonDeserializationConverter(DeserializationContext context)
            {
                this.context = context;
            }

            public object Convert(object value, Type type)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                // deserialize the value
                if (value is JsonValue jsonValue)
                    return Serializer.FromJson(jsonValue, type, context);
                
                return defaultConverter.Convert(value, type);
            }

            public object Convert(object value, TypeCode typeCode)
                => defaultConverter.Convert(value, typeCode);
            
            public bool ToBoolean(object value)
                => defaultConverter.ToBoolean(value);
            
            public char ToChar(object value)
                => defaultConverter.ToChar(value);
            
            public sbyte ToSByte(object value)
                => defaultConverter.ToSByte(value);
            
            public byte ToByte(object value)
                => defaultConverter.ToByte(value);
            
            public short ToInt16(object value)
                => defaultConverter.ToInt16(value);
            
            public ushort ToUInt16(object value)
                => defaultConverter.ToUInt16(value);
            
            public int ToInt32(object value)
                => defaultConverter.ToInt32(value);
            
            public uint ToUInt32(object value)
                => defaultConverter.ToUInt32(value);
            
            public long ToInt64(object value)
                => defaultConverter.ToInt64(value);
            
            public ulong ToUInt64(object value)
                => defaultConverter.ToUInt64(value);
            
            public float ToSingle(object value)
                => defaultConverter.ToSingle(value);
            
            public double ToDouble(object value)
                => defaultConverter.ToDouble(value);
            
            public Decimal ToDecimal(object value)
                => defaultConverter.ToDecimal(value);
            
            public DateTime ToDateTime(object value)
                => defaultConverter.ToDateTime(value);
            
            public string ToString(object value)
                => defaultConverter.ToString(value);
        }
    }
}