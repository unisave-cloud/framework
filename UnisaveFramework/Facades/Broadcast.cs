using System;
using Unisave.Broadcasting;

namespace Unisave.Facades
{
    /// <summary>
    /// Facade for performing message broadcasts
    /// </summary>
    public static class Broadcast
    {
        internal static IBroadcaster GetBroadcaster()
        {
            if (!Facade.HasApp)
                throw new InvalidOperationException(
                    "You cannot broadcasts from the client side, " +
                    "only listen to subscriptions."
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
        }
    }
}