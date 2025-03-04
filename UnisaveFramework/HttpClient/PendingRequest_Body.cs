using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using LightJson;

namespace Unisave.HttpClient
{
    public partial class PendingRequest
    {
        /// <summary>
        /// Content (body / payload) of the request, as is currently built up
        /// </summary>
        private HttpContent content;
        
        /// <summary>
        /// Specifies the request body via the .NET HttpContent class.
        /// Null represent no body.
        /// </summary>
        /// <param name="body"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithBody(HttpContent body)
        {
            content = body;
            return this;
        }

        /// <summary>
        /// Specifies the request body as a form url encoded content
        /// </summary>
        /// <param name="form">Content</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithFormBody(Dictionary<string, string> form)
        {
            if (form == null)
                form = new Dictionary<string, string>();
            
            content = new FormUrlEncodedContent(form);
            
            return this;
        }

        /// <summary>
        /// Specifies the request body as a JSON object
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithJsonBody(JsonObject json)
        {
            if (json == null)
                json = new JsonObject();

            content = new StringContent(
                json.ToString(),
                Encoding.UTF8,
                "application/json"
            );
            
            return this;
        }

        /// <summary>
        /// Attaches a JSON part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="jsonPart">Json content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest Attach(
            string name,
            JsonObject jsonPart,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        )
        {
            return Attach(
                name,
                new StringContent(
                    jsonPart.ToString(),
                    Encoding.UTF8,
                    "application/json"
                ),
                fileName,
                contentHeaders
            );
        }

        /// <summary>
        /// Attaches a part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="part">Part content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest Attach(
            string name,
            HttpContent part,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        )
        {
            var mpContent = content as MultipartFormDataContent;
            
            if (mpContent == null)
                content = mpContent = new MultipartFormDataContent();
            
            AddContentHeaders(part.Headers, contentHeaders);
            
            if (fileName == null)
                mpContent.Add(part, name);
            else
                mpContent.Add(part, name, fileName);
            
            return this;
        }
        
        private void AddContentHeaders(
            HttpContentHeaders dotNetHeaders,
            Dictionary<string, string> contentHeaders
        )
        {
            if (contentHeaders == null)
                return;
            
            foreach (KeyValuePair<string, string> pair in contentHeaders)
            {
                if (!dotNetHeaders.TryAddWithoutValidation(pair.Key, pair.Value))
                {
                    throw new ArgumentException(
                        $"The header {pair.Key} cannot be specified via the " +
                        $"method {nameof(Attach)}. The header probably " +
                        $"refers to the whole request and so cannot be " +
                        $"attached to a content part."
                    );
                }
            }
        }
    }
}