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
        /// Application instance that should be used by facades
        /// </summary>
        public static Application App => app
            ?? throw new InvalidOperationException(
                "Trying to use a facade, but facades have not been "
                + "initialized to an application instance."
            );

        private static Application app;

        /// <summary>
        /// True if an application instance is set and can be used
        /// </summary>
        public static bool HasApp => app != null;
        
        /// <summary>
        /// Sets the application instance to be used by facades
        /// </summary>
        public static void SetApplication(Application newApp)
        {
            app = newApp;
        }
    }
}