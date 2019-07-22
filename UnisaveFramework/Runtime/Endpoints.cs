using System;
using Unisave.Database;

namespace Unisave.Runtime
{
    /// <summary>
    /// Collection of interfaces the framework uses to communicate
    /// with the outside world
    /// 
    /// Values have to be initialized during the bootstrapping process
    /// 
    /// These interfaces are replaced when emulating server locally
    /// </summary>
    public static class Endpoints
    {
        /// <summary>
        /// Handles all communication with the underlying database
        /// </summary>
        public static IDatabase Database => DatabaseResolver();

        /// <summary>
        /// Function that resolves the database endpoint
        /// Set this delegate to customize the database endpoint
        /// </summary>
        public static Func<IDatabase> DatabaseResolver;
    }
}
