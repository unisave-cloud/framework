using System;
using LightJson;
using Unisave.HttpClient;
using Unisave.Serialization;
using Unisave.Serialization.Context;
using Unisave.Sessions;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// A broadcaster service implementation
    /// that broadcasts through the Unisave cloud
    /// </summary>
    public class UnisaveBroadcaster : IBroadcaster
    {
        private readonly ServerSessionIdRepository sessionIdRepository;
        private readonly Factory http;
        
        private readonly Uri serverUri;
        private readonly string broadcastingKey;
        private readonly string environmentId;
        
        public UnisaveBroadcaster(
            ServerSessionIdRepository sessionIdRepository,
            Factory http,
            string serverUrl,
            string broadcastingKey,
            string environmentId
        )
        {
            this.sessionIdRepository = sessionIdRepository;
            this.http = http;
            
            serverUri = new Uri(serverUrl);
            this.broadcastingKey = broadcastingKey;
            this.environmentId = environmentId;
        }
        
        /// <summary>
        /// Creates a new subscription of this session to a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public ChannelSubscription CreateSubscription(SpecificChannel channel)
        {
            string sessionId = sessionIdRepository.SessionId;
            string url = new Uri(serverUri, "subscribe").ToString();

            var subscription = new ChannelSubscription(
                channel.ChannelName,
                sessionId
            );

            var serializedSubscription = Serializer.ToJson<ChannelSubscription>(
                subscription,
                SerializationContext.ServerToClient
            );
            
            Response response = http.PendingRequest().Post(url, new JsonObject {
                ["environmentId"] = environmentId,
                ["broadcastingKey"] = broadcastingKey,
                ["channel"] = channel.ChannelName,
                ["sessionId"] = sessionId,
                ["subscription"] = serializedSubscription
            });

            response.Throw();
            
            return subscription;
        }
        
        /// <summary>
        /// Sends a message into a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public void Send(SpecificChannel channel, BroadcastingMessage message)
        {
            string url = new Uri(serverUri, "send").ToString();

            JsonValue serializedMessage = Serializer.ToJson<BroadcastingMessage>(
                message,
                SerializationContext.ServerToClient
            );

            Response response = http.PendingRequest().Post(url, new JsonObject {
                ["environmentId"] = environmentId,
                ["broadcastingKey"] = broadcastingKey,
                ["channel"] = channel.ChannelName,
                ["message"] = serializedMessage
            });

            response.Throw();
        }
    }
}