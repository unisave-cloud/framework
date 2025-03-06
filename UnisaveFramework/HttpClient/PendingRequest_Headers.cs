#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Unisave.HttpClient
{
    public partial class PendingRequest
    {
        /// <summary>
        /// Header to add to the request
        /// </summary>
        private Dictionary<string, string>? headers;
        
        /// <summary>
        /// Sets additional request headers. When invoked multiple times,
        /// the previous values are forgotten.
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithHeaders(
            Dictionary<string, string> requestHeaders
        )
        {
            headers = requestHeaders;
            return this;
        }
        
        private void AddRequestHeaders(HttpRequestMessage request)
        {
            if (headers == null)
                return;
            
            foreach (var pair in headers)
            {
                if (!request.Headers.TryAddWithoutValidation(pair.Key, pair.Value))
                {
                    throw new ArgumentException(
                        $"The header {pair.Key} cannot be specified via the " +
                        $"method {nameof(WithHeaders)}. The header probably " +
                        $"refers to the content and will be determined " +
                        $"automatically. In other cases use the " +
                        $"{nameof(WithBody)} method to specify content headers."
                    );
                }
            }
        }
    }
}