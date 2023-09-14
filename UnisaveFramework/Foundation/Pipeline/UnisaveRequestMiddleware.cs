using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        private readonly AppFunc next;
        
        /// <summary>
        /// Initializes a new instance of the middleware class
        /// </summary>
        /// <param name="next">Next middleware</param>
        public UnisaveRequestMiddleware(AppFunc next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }
    }
}