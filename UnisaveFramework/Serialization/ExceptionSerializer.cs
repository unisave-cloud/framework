using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LightJson;
using Unisave.Exceptions;

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
            if (!typeof(Exception).IsAssignableFrom(subject.GetType()))
                throw new ArgumentException("Provided instance is not an exception.");

            IFormatter formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, subject);
                
                return System.Convert.ToBase64String(stream.ToArray());
            }
        }

        public object FromJson(JsonValue json, Type type)
        {
            if (!typeof(Exception).IsAssignableFrom(type))
                throw new ArgumentException("Provided type is not an exception.");

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
    }
}
