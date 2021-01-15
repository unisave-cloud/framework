using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using LightJson;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Composites
{
    public static class UnisaveSerializableTypeSerializer
    {
        public static JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            if (!(subject is IUnisaveSerializable))
                throw new ArgumentException(
                    $"Provided instance is not an {typeof(IUnisaveSerializable)}."
                );

            return ((IUnisaveSerializable) subject).ToJson(context);
        }

        public static object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            ConstructorInfo constructor = deserializationType.GetConstructor(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(JsonValue), typeof(DeserializationContext) },
                null
            );
            
            if (constructor == null)
                throw new UnisaveSerializationException(
                    $"The type {deserializationType} implements " +
                    $"{typeof(IUnisaveSerializable)} but lacks the deserialization " +
                    $"constructor."
                );
            
            try
            {
                return constructor.Invoke(
                    new object[] {json, context}
                );
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                
                throw;
            }
        }
    }
}