using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.ExceptionServices;
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
            if (!(subject is ISerializable))
                throw new ArgumentException(
                    $"Provided instance is not an {typeof(ISerializable)}."
                );
            
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

            ConstructorInfo constructor = deserializationType.GetConstructor(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(SerializationInfo), typeof(StreamingContext) },
                null
            );
            
            if (constructor == null)
                throw new UnisaveSerializationException(
                    $"The type {deserializationType} implements " +
                    $"{typeof(ISerializable)} but lacks the deserialization " +
                    $"constructor."
                );

            try
            {
                return constructor.Invoke(
                    new object[] {info, context.GetStreamingContext()}
                );
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                
                throw;
            }
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
        /// (basically performs deserialization)
        /// </summary>
        private class JsonDeserializationConverter : IFormatterConverter
        {
            private readonly DeserializationContext context;
            
            public JsonDeserializationConverter(DeserializationContext context)
            {
                this.context = context;
            }

            public object Convert(object value, Type type)
                => Serializer.FromJson(CastValue(value), type, context);

            private T Convert<T>(object value)
                => (T) Convert(value, typeof(T));

            private JsonValue CastValue(object value)
            {
                if (value == null)
                    throw new InvalidOperationException(
                        $"{typeof(JsonDeserializationConverter)} can only be " +
                        $"used on JSON data but plain null given."
                    );
                
                if (!(value is JsonValue))
                    throw new InvalidOperationException(
                        $"{typeof(JsonDeserializationConverter)} can only be " +
                        $"used on JSON data but {value.GetType()} given."
                    );
                    
                return (JsonValue) value;
            }

            public object Convert(object value, TypeCode typeCode)
            {
                switch (typeCode)
                {
                    case TypeCode.Boolean: return Convert(value, typeof(bool));
                    case TypeCode.Byte: return Convert(value, typeof(byte));
                    case TypeCode.Char: return Convert(value, typeof(char));
                    case TypeCode.Decimal: return Convert(value, typeof(decimal));
                    case TypeCode.Double: return Convert(value, typeof(double));
                    case TypeCode.Empty: return null;
                    case TypeCode.Int16: return Convert(value, typeof(short));
                    case TypeCode.Int32: return Convert(value, typeof(int));
                    case TypeCode.Int64: return Convert(value, typeof(long));
                    case TypeCode.Single: return Convert(value, typeof(float));
                    case TypeCode.String: return Convert(value, typeof(string));
                    case TypeCode.DateTime: return Convert(value, typeof(DateTime));
                    case TypeCode.SByte: return Convert(value, typeof(sbyte));
                    case TypeCode.UInt16: return Convert(value, typeof(ushort));
                    case TypeCode.UInt32: return Convert(value, typeof(uint));
                    case TypeCode.UInt64: return Convert(value, typeof(ulong));
                    case TypeCode.DBNull: return null;
                    
                    // TypeCode.Object is not supported
                }
                
                throw new InvalidEnumArgumentException(
                    $"Type code {typeCode} is not supported."
                );
            }

            public bool ToBoolean(object value) => Convert<bool>(value);
            public char ToChar(object value) => Convert<char>(value);
            public sbyte ToSByte(object value) => Convert<sbyte>(value);
            public byte ToByte(object value) => Convert<byte>(value);
            public short ToInt16(object value) => Convert<short>(value);
            public ushort ToUInt16(object value) => Convert<ushort>(value);
            public int ToInt32(object value) => Convert<int>(value);
            public uint ToUInt32(object value) => Convert<uint>(value);
            public long ToInt64(object value) => Convert<long>(value);
            public ulong ToUInt64(object value) => Convert<ulong>(value);
            public float ToSingle(object value) => Convert<float>(value);
            public double ToDouble(object value) => Convert<double>(value);
            public Decimal ToDecimal(object value) => Convert<Decimal>(value);
            public DateTime ToDateTime(object value) => Convert<DateTime>(value);
            public string ToString(object value) => Convert<string>(value);
        }
    }
}