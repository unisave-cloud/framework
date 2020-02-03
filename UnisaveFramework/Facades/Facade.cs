using System;
using Unisave.Foundation;

namespace Unisave
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
        /// Sets the application instance to be used by facades
        /// </summary>
        public static void SetApplication(Application newApp)
        {
            app = newApp;
        }
    }
}