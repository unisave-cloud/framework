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
            // TODO: create the subscription on the server-side
            // ergo -> adjust the message flow in the rabbit
            
            // TODO: get the session id
            
            return new ChannelSubscription(ChannelName, "TODO-SESSION-ID");
        }

        public void Send(BroadcastingMessage message)
        {
            // send a message through the channel
        }
    }
}