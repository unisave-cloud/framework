using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Foundation;

namespace FrameworkTests.Testing
{
    /// <summary>
    /// Base test suite class that lets you test code that relies on a
    /// request context. Ideal for testing facades and contracts.
    /// This test fixture does not create the whole application,
    /// just the request context and its service container,
    /// where you can place your own mocks.
    /// </summary>
    public abstract class RequestContextFixture
    {
        /// <summary>
        /// Instance of the request context that is created and teared
        /// down with each test.
        /// </summary>
        protected RequestContext ctx;

        [SetUp]
        public virtual void SetUpRequestContext()
        {
            ctx = new RequestContext(
                new TinyIoCAdapter(),
                new OwinContext()
            );
        }
        
        [TearDown]
        public virtual void TearDownRequestContext()
        {
            ctx?.Dispose();
            ctx = null;
        }
    }
}