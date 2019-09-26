using System;
using System.Text;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Runtime
{
    /// <summary>
    /// Represents a single channel in the sandbox api
    /// </summary>
    public class ApiChannel
    {
        /// <summary>
        /// Name of the channel
        /// </summary>
        public string Name { get; }
        
        public ApiChannel(string name)
        {
            Name = name ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Sends a JSON message through the channel
        /// </summary>
        public virtual JsonObject SendJsonMessage(int messageType, JsonObject message)
        {
            byte[] response = SandboxApi.SendMessage(
                Name,
                messageType,
                Encoding.UTF8.GetBytes(message.ToString())
            );

            return JsonReader.Parse(
                Encoding.UTF8.GetString(response)
            ).AsJsonObject;
        }
    }
}