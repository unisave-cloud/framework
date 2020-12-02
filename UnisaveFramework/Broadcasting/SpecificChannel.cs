using Unisave.Facades;

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
        
        /// <summary>
        /// Subscribes the current session to this channel
        /// </summary>
        /// <returns>
        /// The subscription object that should be sent
        /// to the client and listened on
        /// </returns>
        public ChannelSubscription CreateSubscription()
        {
            return Broadcast.GetBroadcaster().CreateSubscription(this);
        }

        /// <summary>
        /// Sends a message into this channel
        /// </summary>
        /// <param name="message"></param>
        public void Send(BroadcastingMessage message)
        {
            Broadcast.GetBroadcaster().Send(this, message);
        }
    }
}