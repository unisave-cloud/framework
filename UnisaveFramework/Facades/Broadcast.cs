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
        /// Access broadcasting on a specific channel
        /// </summary>
        /// <param name="parameters">Parameters of the channel</param>
        /// <typeparam name="TChannel">Channel type</typeparam>
        /// <returns>A handle on which you can call .Send()</returns>
        public static ChannelHandle<TChannel> Channel<TChannel>(
            params string[] parameters
        ) where TChannel : BroadcastingChannel, new()
        {
            BroadcastingChannel.ValidateParameters<TChannel>(parameters);
            
            return new ChannelHandle<TChannel>(parameters);
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