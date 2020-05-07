using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LightJson;
using Unisave.Exceptions;
using Unisave.Serialization.Exceptions;

namespace Unisave.Serialization
{
    /// <summary>
    /// Serializes exceptions
    /// 
    /// Exception serialization is special, because the type information
    /// is stored together with the exception. This is not the case for
    /// the rest of types. This is ok though, because the serialized
    /// exception is only stored for a short amount of time (during
    /// the transfer between server and client).
    /// 
    /// The serialization for other types has to remain type-less
    /// because these types may be stored inside the database.
    /// This allows for type renaming or even joining or
    /// splitting types. Simply the type is not glued to the data.
    /// </summary>
    public class ExceptionSerializer : ITypeSerializer
    {
        public JsonValue ToJson(object subject)
        {
            if (!(subject is Exception exception))
                throw new ArgumentException(
                    "Provided instance is not an exception."
                );
            
            // instance to SerializationInfo
            var context = new StreamingContext(
                // could be anything, but should be the same as below in
                // deserialization. And since there has to be CrossAppDomain,
                // it will be here as well.
                StreamingContextStates.CrossAppDomain
            );
            var converter = new FormatterConverter();
            var info = new SerializationInfo(exception.GetType(), converter);

            try
            {
                exception.GetObjectData(info, context);
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

            // SerializationInfo to JSON
            var json = new JsonObject();
            foreach (SerializationEntry entry in info)
                AddSerializationEntryToJsonObject(entry, json, converter);
            return json;
        }

        private void AddSerializationEntryToJsonObject(
            SerializationEntry entry,
            JsonObject json,
            FormatterConverter converter
        )
        {
            if (entry.ObjectType == typeof(string))
            {
                if (entry.Value == null)
                {
                    json[entry.Name] = JsonValue.Null;
                    return;
                }
                
                if (entry.ObjectType != entry.Value.GetType())
                    json[entry.Name] = (string) converter
                        .Convert(entry.Value, typeof(string));
                else
                    json[entry.Name] = (string) entry.Value;

                return;
            }
            
            if (entry.ObjectType == typeof(int))
            {
                if (entry.ObjectType != entry.Value.GetType())
                    json[entry.Name] = (int) (converter
                        .Convert(entry.Value, typeof(int)) ?? 0);
                else
                    json[entry.Name] = (int) entry.Value;

                return;
            }
            
            if (entry.ObjectType == typeof(bool))
            {
                if (entry.ObjectType != entry.Value.GetType())
                    json[entry.Name] = (bool) (converter
                        .Convert(entry.Value, typeof(bool)) ?? false);
                else
                    json[entry.Name] = (bool) entry.Value;

                return;
            }
            
            // NOTE: maybe serialize as entry.ObjectType,
            // not as entry.Value.GetType(), not sure though...
            json[entry.Name] = Serializer.ToJson(entry.Value);
            json["typeof:" + entry.Name] = entry.ObjectType.FullName;
        }

        public object FromJson(JsonValue json, Type outputType)
        {
            // NOTE: deserialization shouldn't fail no matter what the JSON is.
            // Instead a SerializedException instance should be returned.
            // The only exception is improper usage of this method.
            
            if (!typeof(Exception).IsAssignableFrom(outputType))
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
                var defaultInfo = GetDefaultSerializationInfo(actualType);
                var updatedInfo = UpdateSerializationInfoWithJson(defaultInfo, obj);
                return CreateExceptionViaSerializationConstructor(
                    actualType,
                    updatedInfo,
                    new StreamingContext(
                        // has to be EXACTLY this, because then the exception can
                        // be rethrown without loosing the stack trace
                        StreamingContextStates.CrossAppDomain
                    )
                );
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
                throw new SerializationException(
                    "Exception of type null makes no sense."
                );

            Type type = FindType(name);
            
            if (!typeof(Exception).IsAssignableFrom(type))
                throw new SerializationException(
                    $"Type {name} is not an exception."
                );

            return type;
        }

        private Type FindType(string name)
        {
            if (name == null)
                throw new SerializationException(
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
                throw new SerializationException(
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
                ExceptionDispatchInfo.Capture(e).Throw();
                throw; // should not be called
            }
            
            var context = new StreamingContext(StreamingContextStates.All);
            var info = new SerializationInfo(type, new FormatterConverter());
            instance.GetObjectData(info, context);
            return info;
        }

        private SerializationInfo UpdateSerializationInfoWithJson(
            SerializationInfo defaultInfo,
            JsonObject json
        )
        {
            var info = new SerializationInfo(
                defaultInfo.ObjectType,
                new FormatterConverter()
            );
            
            // go through the keys in default info and copy or replace
            // them with the values in the serialized JSON
            HashSet<string> writtenKeys = new HashSet<string>();
            foreach (SerializationEntry entry in defaultInfo)
            {
                if (json.ContainsKey(entry.Name))
                    AddJsonValueToSerializationInfo(
                        info,
                        entry.Name,
                        json[entry.Name],
                        json["typeof:" + entry.Name].AsString
                    );
                else
                    info.AddValue(entry.Name, entry.Value, entry.ObjectType);

                writtenKeys.Add(entry.Name);
            }
            
            // add all the remaining data in the JSON that hasn't been used up
            // (this might happen when the default info is empty)
            foreach (var pair in json)
            {
                if (pair.Key.StartsWith("typeof:"))
                    continue;
                
                if (writtenKeys.Contains(pair.Key))
                    continue;
                
                AddJsonValueToSerializationInfo(
                    info,
                    pair.Key,
                    json[pair.Key],
                    json["typeof:" + pair.Key].AsString
                );
            }

            return info;
        }

        private void AddJsonValueToSerializationInfo(
            SerializationInfo info,
            string key,
            JsonValue json,
            string typeString
        )
        {
            // type information is specified
            if (typeString != null)
            {
                Type type = FindType(typeString);
                info.AddValue(key, Serializer.FromJson(json, type));
                return;
            }
            
            // type information is not specified, and so should be inferred
            if (json.IsNull)
                info.AddValue(key, null);
            else if (json.IsString)
                info.AddValue(key, json.AsString);
            else if (json.IsInteger)
                info.AddValue(key, json.AsInteger);
            else if (json.IsBoolean)
                info.AddValue(key, json.AsBoolean);
            else
                throw new SerializationException(
                    $"Cannot deserialize key '{key}' when type not " +
                    $"serialized and cannot be inferred: {json.ToString()}"
                );
        }

        private object CreateExceptionViaSerializationConstructor(
            Type type,
            SerializationInfo info,
            StreamingContext context
        )
        {
            var serializationConstructor = type.GetConstructor(
                (BindingFlags)(-1),
                null,
                new Type[] {
                    typeof(SerializationInfo),
                    typeof(StreamingContext)
                },
                null
            );
            
            if (serializationConstructor == null)
                throw new SerializationException(
                    $"Exception {type} cannot be deserialized, " +
                    "because it has no serialization constructor."
                );

            Exception instance;
            try
            {
                instance = (Exception) serializationConstructor.Invoke(
                    new object[] {
                        info,
                        context
                    }
                );
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
                throw; // should not be called
            }

            return instance;
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
