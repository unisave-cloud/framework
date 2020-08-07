using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Http.Client
{
    public class Request : HttpContentHolder
    {
        /// <summary>
        /// The original request message from the .NET framework
        /// </summary>
        public HttpRequestMessage Original { get; }

        // implements the SPI
        protected override HttpContent Content => Original.Content;

        /// <summary>
        /// URL of the request
        /// </summary>
        public string Url => Original.RequestUri.ToString();
        
        public Request(HttpRequestMessage original)
        {
            Original = original;
        }
        
        /// <summary>
        /// Get value of a header or null if header not present
        /// </summary>
        /// <param name="name">Header name</param>
        /// <returns></returns>
        public string Header(string name)
        {
            IEnumerable<string> values;
        
            if (Original.Headers.TryGetValues(name, out values))
                return values.FirstOrDefault();
            
            if (Original.Content != null &&
                Original.Content.Headers.TryGetValues(name, out values))
                return values.FirstOrDefault();

            return null;
        }

        /// <summary>
        /// Returns true if a given header is present
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasHeader(string name)
            => Header(name) != null;
        
        /// <summary>
        /// Returns true if a given header has the given value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool HasHeader(string name, string value)
            => Header(name) == value;
    }
}