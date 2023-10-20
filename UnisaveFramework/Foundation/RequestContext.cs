using System;
using System.Threading;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Unisave.Foundation
{
    /// <summary>
    /// Represents the context of an HTTP request within the Unisave Framework.
    /// Analogous to the <see cref="BackendApplication"/>,
    /// but scoped only to a single request.
    /// </summary>
    public class RequestContext : IDisposable
    {
        private static readonly AsyncLocal<RequestContext> current
            = new AsyncLocal<RequestContext>();

        /// <summary>
        /// Returns the context of the currently processed request
        /// (even concurrently running requests will receive their proper contexts)
        /// </summary>
        public static RequestContext Current => current.Value;

        /// <summary>
        /// IoC container for the HTTP request. Falls back onto the
        /// application-level container when a type is not defined.
        /// </summary>
        public IContainer Services { get; }

        /// <summary>
        /// OWIN HTTP request context
        /// </summary>
        public IOwinContext OwinContext { get; }
        
        /// <summary>
        /// OWIN HTTP request wrapper
        /// </summary>
        public IOwinRequest OwinRequest { get; }
        
        /// <summary>
        /// OWIN HTTP response wrapper
        /// </summary>
        public IOwinResponse OwinResponse { get; }

        private bool disposed = false;
        
        public RequestContext(IContainer applicationServices, IOwinContext owinContext)
        {
            OwinContext = owinContext;
            OwinRequest = owinContext.Request;
            OwinResponse = owinContext.Response;
            
            AttachToCurrentExecutionContext();

            Services = applicationServices.CreateChildContainer();
            
            // register instances
            Services.RegisterInstance<IOwinContext>(
                owinContext, transferOwnership: false
            );
            Services.RegisterInstance<IOwinRequest>(
                owinContext.Request, transferOwnership: false
            );
            Services.RegisterInstance<IOwinResponse>(
                owinContext.Response, transferOwnership: false
            );
            Services.RegisterInstance<IAuthenticationManager>(
                owinContext.Authentication, transferOwnership: false
            );
            
            // attach to the OWIN environment dictionary
            owinContext.Environment["unisave.RequestContext"] = this;
            owinContext.Environment["unisave.RequestServices"] = Services;
        }

        private void AttachToCurrentExecutionContext()
        {
            current.Value = this;
        }

        private void DetachFromCurrentExecutionContext()
        {
            if (current.Value == this)
                current.Value = null;
        }
        
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            
            Services.Dispose();
            
            DetachFromCurrentExecutionContext();
        }
    }
}