using System;
using System.Collections.Generic;
using LightJson;
using Unisave.Contracts;
using Unisave.Serialization;

namespace Unisave.Logging
{
    public class InMemoryLog : ILog
    {
        private readonly List<JsonObject> records = new List<JsonObject>();

        private readonly int maxRecordCount;
        private readonly int maxRecordSize;

        private bool maxRecordCountExceeded;
        
        /// <summary>
        /// Creates new log that keeps records in memory
        /// and can be then exported to json
        /// </summary>
        /// <param name="maxRecordCount">Limit on the number of records</param>
        /// <param name="maxRecordSize">
        /// Limit on the size of a single record
        /// in the number of characters of serialized JSON
        /// </param>
        public InMemoryLog(int maxRecordCount = -1, int maxRecordSize = -1)
        {
            this.maxRecordCount = maxRecordCount;
            this.maxRecordSize = maxRecordSize;
        }
        
        public void Critical(string message, object context = null)
            => LogMessage(LogLevel.Critical, message, context);
        
        public void Error(string message, object context = null)
            => LogMessage(LogLevel.Error, message, context);
        
        public void Warning(string message, object context = null)
            => LogMessage(LogLevel.Warning, message, context);
        
        public void Info(string message, object context = null)
            => LogMessage(LogLevel.Info, message, context);
        
        public void LogMessage(LogLevel level, string message, object context = null)
        {
            if (message == null)
                message = "";
            
            if (maxRecordCountExceeded)
                return;
            
            // check max record count
            if (maxRecordCount >= 0 && records.Count >= maxRecordCount)
            {
                maxRecordCountExceeded = true;
                records.Add(
                    new JsonObject()
                        .Add("level", SerializeLogLevel(LogLevel.Warning))
                        .Add("time", DateTime.UtcNow)
                        .Add(
                            "message",
                            $"[Unisave] Maximum log message count of " +
                            $"{maxRecordCount} was exceeded. " +
                            $"Further logging will be ignored."
                        )
                        .Add("context", JsonValue.Null)
                );
                return;
            }
            
            // handle the record
            JsonValue serializedContext = Serializer.ToJson(context);
            
            JsonObject record = new JsonObject()
                .Add("level", SerializeLogLevel(level))
                .Add("time", DateTime.UtcNow)
                .Add("message", message)
                .Add("context", serializedContext);

            // check max record size
            if (maxRecordSize != -1)
            {
                int size = record.ToString().Length;
                if (size > maxRecordSize)
                {
                    var choppedMessage = message;
                    if (choppedMessage.Length > 100)
                        choppedMessage = choppedMessage.Substring(0, 100);
                    
                    records.Add(
                        new JsonObject()
                            .Add("level", SerializeLogLevel(LogLevel.Warning))
                            .Add("time", DateTime.UtcNow)
                            .Add(
                                "message",
                                $"[Unisave] Maximum log message size of " +
                                $"{maxRecordSize} characters was exceeded " +
                                $"(including context object). " +
                                $"The message was:\n{choppedMessage}..."
                            )
                            .Add("context", JsonValue.Null)
                    );
                    return;
                }
            }
            
            // finally everything ok
            records.Add(record);
        }

        private string SerializeLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    return "info";
                
                case LogLevel.Warning:
                    return "warning";
                
                case LogLevel.Error:
                    return "error";
                
                case LogLevel.Critical:
                    return "critical";
                
                default:
                    throw new ArgumentException(
                        $"Given log level {level} cannot be serialized."
                    );
            }
        }

        /// <summary>
        /// Export the log into a json object
        /// </summary>
        public JsonArray ExportLog()
        {
            var export = new JsonArray();

            foreach (var record in records)
                export.Add(record);

            return export;
        }
    }
}