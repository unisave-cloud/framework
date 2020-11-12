using System;
using Unisave.Broadcasting;

namespace Unisave.Facades
{
    /// <summary>
    /// Facade for performing message broadcasts
    /// </summary>
    public static class Broadcast
    {
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