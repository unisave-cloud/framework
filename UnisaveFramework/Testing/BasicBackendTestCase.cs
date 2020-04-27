using System;
using Unisave.Facades;
using Unisave.Foundation;
using Unisave.Runtime;

namespace Unisave.Testing
{
    /// <summary>
    /// Main base class for testing functionality
    /// of code written using the framework.
    ///
    /// This class is cut off from NUnit framework and needs
    /// to have all abstract methods implemented to be usable.
    ///
    /// Also you need to override and tag SetUp and TearDown methods.
    /// </summary>
    public abstract partial class BasicBackendTestCase
    {
        /// <summary>
        /// The application that we test requests against
        /// </summary>
        protected Application App { get; set; }

        /// <summary>
        /// Sets up the application
        /// </summary>
        /// <param name="gameAssemblyTypes">Types to use for searching</param>
        /// <param name="env">What environment variables to use</param>
        public virtual void SetUp(Type[] gameAssemblyTypes, Env env)
        {
            App = Bootstrap.Boot(
                gameAssemblyTypes,
                env,
                new SpecialValues()
            );
            
            Facade.SetApplication(App);

            SessionId = null;

            // NOTE: if you need to specify a concrete instance to be used
            // by the application (so Env is not enough), you can perform
            // some container surgery here:
            //
            // App.Instance<IArango>(mySpecialInstance);
        }

        /// <summary>
        /// Disposes the application
        /// </summary>
        public virtual void TearDown()
        {
            Facade.SetApplication(null);
            
            App.Dispose();
        }
    }
}