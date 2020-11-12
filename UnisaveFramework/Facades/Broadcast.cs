using System;
using Unisave.Broadcasting;

namespace Unisave.Facades
{
    /// <summary>
    /// Facade for performing message broadcasts
    /// </summary>
    public static class Broadcast
    {
        private static IBroadcaster GetBroadcaster()
        {
            if (!Facade.HasApp)
                throw new InvalidOperationException(
                    "You cannot broadcast messages from the client side."
                );
            
            return Facade.App.Resolve<IBroadcaster>();
        }
        
        /// <summary>
        /// Access broadcasting on a channel
        /// </summary>
        /// <typeparam name="TChannel">Channel type</typeparam>
        /// <returns>Instance of the given channel type</returns>
        public static TChannel Channel<TChannel>()
            where TChannel : BroadcastingChannel, new()
        {
            return new TChannel();
            
            //BroadcastingChannel.ValidateParameters<TChannel>(parameters);
            //return new ChannelHandle<TChannel>(parameters);
        }

        /// <summary>
        /// Helper class to allow for method chaining
        /// </summary>
        public class ChannelHandle<TChannel>
            where TChannel : BroadcastingChannel, new()
        {
            private readonly string[] parameters;
            
            public ChannelHandle(string[] parameters)
            {
                this.parameters = parameters;
            }
            
            /// <summary>
            /// Sends a message to the channel
            /// </summary>
            /// <param name="message">The message to be sent</param>
            public void Send(BroadcastingMessage message)
            {
                Broadcast
                    .GetBroadcaster()
                    .BroadcastMessage<TChannel>(parameters, message);
            }
        }
    }
}