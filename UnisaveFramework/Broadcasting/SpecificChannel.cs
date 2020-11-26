using System;
using LightJson;
using Unisave.Facades;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// Represents a specific channel with given parameters
    /// </summary>
    public class SpecificChannel
    {
        /// <summary>
        /// Full name of the channel with all parameters
        /// </summary>
        public string ChannelName { get; }
        
        public SpecificChannel(string channelName)
        {
            ChannelName = channelName;
        }
        
        /// <summary>
        /// Creates a specific channel from a broadcasting channel
        /// and a set of string parameters
        /// </summary>
        /// <param name="parameters">List of parameters that specify the channel</param>
        /// <typeparam name="TChannel">The broadcasting channel</typeparam>
        /// <returns></returns>
        public static SpecificChannel From<TChannel>(params string[] parameters)
            where TChannel : BroadcastingChannel, new()
        {
            var name = BroadcastingChannel.GetStringName<TChannel>(parameters);
            
            return new SpecificChannel(name);
        }
        
        public ChannelSubscription CreateSubscription()
        {
            // TODO: get the session id
            string sessionId = "TODO-SESSION-ID";
        
            string url = new Uri(
                new Uri(Env.GetString("BROADCASTING_SERVER_URL")),
                "subscribe"
            ).ToString();

            Facades.Http.Post(url, new JsonObject {
                ["environmentId"] = Env.GetString("UNISAVE_ENVIRONMENT_ID"),
                ["broadcastingKey"] = Env.GetString("BROADCASTING_KEY"),
                ["channel"] = ChannelName,
                ["sessionId"] = sessionId
            });
            
            return new ChannelSubscription(ChannelName, sessionId);
        }

        public void Send(BroadcastingMessage message)
        {
            string url = new Uri(
                new Uri(Env.GetString("BROADCASTING_SERVER_URL")),
                "subscribe"
            ).ToString();

            JsonValue serializedMessage = Serializer.ToJson<BroadcastingMessage>(
                message,
                SerializationContext.BroadcastingContext()
            );

            Facades.Http.Post(url, new JsonObject {
                ["environmentId"] = Env.GetString("UNISAVE_ENVIRONMENT_ID"),
                ["broadcastingKey"] = Env.GetString("BROADCASTING_KEY"),
                ["channel"] = ChannelName,
                ["message"] = serializedMessage
            });
        }
    }
}