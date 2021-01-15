using System;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LightJson;
using Unisave.Serialization.Composites;
using Unisave.Serialization.Context;

namespace Unisave.Serialization.Exceptions
{
    /// <summary>
    /// Serializes exceptions
    /// </summary>
    public class ExceptionSerializer : ITypeSerializer
    {
        public JsonValue ToJson(
            object subject,
            Type typeScope,
            SerializationContext context
        )
        {
            if (!(subject is Exception exception))
                throw new ArgumentException(
                    "Provided instance is not an exception."
                );

            try
            {
                // every exception should be ISerializable
                return SerializableTypeSerializer.ToJson(
                    subject,
                    typeScope,
                    context
                );
            }
            catch
            {
                // it doesn't want to be serialized,
                // so serialize at lest what can be pulled out manually
                // (we want to get at least some information to the client)
                return new JsonObject()
                    .Add("ClassName", exception.GetType().FullName)
                    .Add("HResult", exception.HResult)
                    .Add("Message", exception.Message)
                    .Add("StackTraceString", exception.StackTrace);
            }
        }

        public object FromJson(
            JsonValue json,
            Type deserializationType,
            DeserializationContext context
        )
        {
            // NOTE: deserialization shouldn't fail no matter what the JSON is.
            // Instead a SerializedException instance should be returned.
            // The only exception is improper usage of this method.
            
            if (!typeof(Exception).IsAssignableFrom(deserializationType))
                throw new ArgumentException(
                    "Provided type is not an exception."
                );

            if (json.IsNull)
                return null;
            
            try
            {
                if (json.IsString)
                    return LegacyFromJson(json);
            }
            catch (Exception e)
            {
                // RETURN, not throw!
                return new SerializedException(json, e);
            }
            
            JsonObject obj = json.AsJsonObject;

            if (obj == null)
                return null;

            try
            {
                // deserialize the thing
                Type actualType = FindExceptionType(obj["ClassName"].AsString);
                SerializationInfo defaultInfo = GetDefaultSerializationInfo(actualType);
                ExtendJsonWithDefaultSerializationInfo(obj, defaultInfo);
                var e = SerializableTypeSerializer.FromJson(obj, actualType, context);
                PreserveStackTrace((Exception) e);
                return e;
            }
            catch (Exception e)
            {
                // RETURN, not throw!
                return new SerializedException(json, e);
            }
        }
        
        private Type FindExceptionType(string name)
        {
            if (name == null)
                throw new UnisaveSerializationException(
                    "Exception of type null makes no sense."
                );

            Type type = FindType(name);
            
            if (!typeof(Exception).IsAssignableFrom(type))
                throw new UnisaveSerializationException(
                    $"Type {name} is not an exception."
                );

            return type;
        }

        private Type FindType(string name)
        {
            if (name == null)
                throw new UnisaveSerializationException(
                    "Type name null makes no sense."
                );
            
            Type type = null;
            
            // first, try to use the default method
            // (for example because the following code
            // does not handle generics and arrays)
            type = Type.GetType(name);
            if (type != null)
                return type;

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in a.GetTypes())
                {
                    if (t.FullName == name)
                    {
                        type = t;
                        break;
                    }
                }

                if (type != null)
                    break;
            }
            
            if (type == null)
                throw new UnisaveSerializationException(
                    $"Type {name} wasn't found."
                );

            return type;
        }
        
        private SerializationInfo GetDefaultSerializationInfo(Type type)
        {
            var defaultConstructor = type.GetConstructor(new Type[] { });

            // no default constructor -> return empty info, since we don't
            // know anything and we have to hope that all the needed data
            // will be present in the JSON
            if (defaultConstructor == null)
                return new SerializationInfo(type, new FormatterConverter());

            Exception instance;
            try
            {
                instance = (Exception) defaultConstructor.Invoke(new object[] { });
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                
                throw;
            }
            
            var context = new StreamingContext(StreamingContextStates.All);
            var info = new SerializationInfo(type, new FormatterConverter());
            instance.GetObjectData(info, context);
            return info;
        }

        private void ExtendJsonWithDefaultSerializationInfo(
            JsonObject json,
            SerializationInfo info
        )
        {
            foreach (var entry in info)
            {
                if (json.ContainsKey(entry.Name))
                    continue;

                json[entry.Name] = Serializer.ToJson(
                    entry.Value,
                    entry.ObjectType
                );
            }
        }
        
        // magic
        // https://stackoverflow.com/a/2085377
        private static void PreserveStackTrace(Exception e)
        {
            var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
            var mgr = new ObjectManager(null, ctx);
            var si = new SerializationInfo(e.GetType(), new FormatterConverter());

            e.GetObjectData(si, ctx);
            mgr.RegisterObject(e, 1, si); // prepare for SetObjectData
            mgr.DoFixups(); // ObjectManager calls SetObjectData

            // voila, e is unmodified save for _remoteStackTraceString
        }
        
        #region "Legacy serialization (binary)"
        
        internal static JsonValue LegacyToJson(object subject)
        {
            if (!(subject is Exception))
                throw new ArgumentException(
                    "Provided instance is not an exception."
                );

            IFormatter formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, subject);
                
                return System.Convert.ToBase64String(stream.ToArray());
            }
        }

        internal static object LegacyFromJson(JsonValue json)
        {
            byte[] bytes = System.Convert.FromBase64String(json.AsString);

            IFormatter formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                var obj = formatter.Deserialize(stream);
                return obj;
            }
        }
        
        #endregion
    }
}
