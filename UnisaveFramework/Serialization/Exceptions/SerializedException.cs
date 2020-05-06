using System;
using System.Runtime.Serialization;
using System.Text;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Serialization.Exceptions
{
    /// <summary>
    /// Represents an exception that couldn't be deserialized for some reason
    /// so it has been wrapped and is being propagated in it's serialized form.
    ///
    /// When inner exception is set, it is the reason the deserialization failed.
    /// </summary>
    [Serializable]
    public class SerializedException : Exception
    {
        /// <summary>
        /// The serialized JSON value representing the exception
        /// </summary>
        public JsonValue SerializedValue { private set; get; }
        
        public SerializedException(JsonValue json) : base(BuildMessage(json))
        {
            SerializedValue = json;
        }
        
        public SerializedException(
            JsonValue json,
            Exception failureCause
        ) : base(BuildMessage(json), failureCause)
        {
            SerializedValue = json;
        }

        protected SerializedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            try
            {
                SerializedValue = JsonReader.Parse(
                    info.GetString("SerializedValue")
                );
            }
            catch
            {
                SerializedValue = JsonValue.Null;
            }
        }

        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context
        )
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            
            info.AddValue(
                "SerializedValue",
                SerializedValue.ToString(),
                typeof(string)
            );
            
            base.GetObjectData(info, context);
        }
        
        private static string BuildMessage(JsonValue json)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Exception couldn't be deserialized, so here is some info:");
            sb.AppendLine("What could be extracted:");
            ExtractData(json, sb);
            sb.AppendLine();
            
            sb.AppendLine("Raw JSON serialized exception:");
            sb.AppendLine(json.ToString());
            sb.AppendLine();
            
            return sb.ToString();
        }

        private static void ExtractData(JsonValue json, StringBuilder sb)
        {
            if (!json.IsJsonObject)
            {
                sb.AppendLine("  Nothing, exception is binary, base64 encoded.");
                return;
            }

            JsonObject obj = json.AsJsonObject;

            if (obj == null)
                return;
            
            // class name
            if (obj.ContainsKey("ClassName"))
                sb.AppendLine("  ClassName: " + obj["ClassName"].ToString());
            
            // message
            if (obj.ContainsKey("Message"))
                sb.AppendLine("  Message: " + obj["Message"].ToString());
            
            // inner exception
            if (obj.ContainsKey("InnerException"))
            {
                sb.AppendLine("  InnerException: not null, see the JSON below");
            }
            
            // stack trace
            if (obj.ContainsKey("StackTraceString"))
            {
                sb.AppendLine("  StackTraceString:");
                sb.AppendLine(obj["StackTraceString"].AsString);
            }
        }
    }
}