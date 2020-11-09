using LightJson;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// A broadcaster service implementation
    /// that broadcasts through the Unisave cloud
    /// </summary>
    public class UnisaveBroadcaster : IBroadcaster
    {
        public void BroadcastMessage<TChannel>(
            string[] parameters,
            BroadcastingMessage message
        ) where TChannel : BroadcastingChannel, new()
        {
            // TODO: pass authentication information from env variables
            // TODO: pull target URL from env variables

            JsonValue serializedMessage = Serializer.ToJson(
                message,
                typeof(BroadcastingMessage),
                SerializationContext.BroadcastingContext()
            );

            string channelName = BroadcastingChannel.GetStringName<TChannel>(
                parameters
            );
            
            Facades.Http
                .WithBasicAuth("<game-id>", "<game-broadcast-key>")
                .Post(
                    "https://unisave.cloud/_broadcasting/broadcast",
                    new JsonObject {
                        ["channel"] = channelName, 
                        ["message"] = serializedMessage
                    }
                );
        }
    }
}