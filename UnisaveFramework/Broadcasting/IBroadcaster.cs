namespace Unisave.Broadcasting
{
    public interface IBroadcaster
    {
        /// <summary>
        /// Creates a new subscription of this session to a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        ChannelSubscription CreateSubscription(SpecificChannel channel);

        /// <summary>
        /// Sends a message into a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        void Send(SpecificChannel channel, BroadcastingMessage message);
    }
}