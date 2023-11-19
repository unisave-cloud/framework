using System;
using Unisave.Foundation;

namespace Unisave.Facades
{
    /// <summary>
    /// Represents all facades
    /// </summary>
    public static class Facade
    {
        /// <summary>
        /// Service container scoped to the current backend request
        /// </summary>
        public static IContainer Services
        {
            get
            {
                var ctx = RequestContext.Current;
                
                if (ctx == null)
                    throw new InvalidOperationException(
                        "Facades can only be used in the " +
                        "context of a backend request."
                    );

                return ctx.Services;
            }
        }

        /// <summary>
        /// Returns true if facades can be used
        /// </summary>
        public static bool CanUse => RequestContext.Current != null;
    }
}