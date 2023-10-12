using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Unisave.Foundation.Pipeline
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    /// <summary>
    /// This middleware sits at the beginning of the OWIN middleware pipeline
    /// and it captures and routes unisave requests. Unisave request is an
    /// HTTP request with the header X-Unisave-Request (e.g. a facet call)
    /// </summary>
    public class UnisaveRequestMiddleware
    {
        /// <summary>
        /// Name of the Unisave request routing header
        /// </summary>
        public const string UnisaveRequestHeaderName = "X-Unisave-Request";
        
        private readonly AppFunc next;
        private readonly Dictionary<string, AppFunc> branches;
        
        /// <summary>
        /// Initializes a new instance of the middleware class
        /// </summary>
        /// <param name="next">Next middleware</param>
        /// <param name="branches">Compiled branches for given request kinds</param>
        public UnisaveRequestMiddleware(
            AppFunc next,
            Dictionary<string, AppFunc> branches
        )
        {
            this.next = next
                ?? throw new ArgumentNullException(nameof(next));
            
            this.branches = branches
                ?? throw new ArgumentNullException(nameof(branches));
        }
        
        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public async Task Invoke(IDictionary<string, object> environment)
        {
            IOwinContext ctx = new OwinContext(environment);

            if (IsUnisaveRequest(ctx))
            {
                string requestKind = ctx.Request.Headers[UnisaveRequestHeaderName];

                if (branches.TryGetValue(requestKind, out AppFunc branch))
                {
                    await branch.Invoke(ctx.Environment);
                }
                else
                {
                    ctx.Response.StatusCode = 422;
                    ctx.Response.ReasonPhrase = "Unknown unisave request kind";
                }
            }
            else
            {
                await next(environment);
            }
        }
        
        private static bool IsUnisaveRequest(IOwinContext ctx)
        {
            return ctx.Request.Headers.ContainsKey(UnisaveRequestHeaderName);
        }
    }
}