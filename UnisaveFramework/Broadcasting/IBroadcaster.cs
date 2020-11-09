namespace Unisave.Broadcasting
{
    public interface IBroadcaster
    {
        /// <summary>
        /// Broadcasts a message into a channel
        /// </summary>
        /// <param name="parameters">Channel parameters</param>
        /// <param name="message">The message</param>
        /// <typeparam name="TChannel">Channel type</typeparam>
        void BroadcastMessage<TChannel>(
            string[] parameters,
            BroadcastingMessage message
        ) where TChannel : BroadcastingChannel, new();
    }
}