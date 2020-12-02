using System;
using System.Collections.Generic;
using System.Net.Http;
using LightJson;

namespace Unisave.HttpClient
{
    public class ResponseSequence
    {
        /// <summary>
        /// Should the sequence throw an exception when empty
        /// </summary>
        protected bool failWhenEmpty = true;
        
        /// <summary>
        /// Responses to return
        /// </summary>
        protected Queue<Response> responses = new Queue<Response>();

        /// <summary>
        /// Response to return when empty
        /// </summary>
        protected Response emptySequenceResponse;

        /// <summary>
        /// Returns the next response in the sequence
        /// </summary>
        /// <returns></returns>
        public Response Next()
        {
            if (responses.Count == 0)
            {
                if (failWhenEmpty)
                    throw new InvalidOperationException(
                        "Response sequence is empty, " +
                        "yet one more request was made."
                    );

                return emptySequenceResponse;
            }
            
            return responses.Dequeue();
        }

        /// <summary>
        /// Adds a JSON response to the sequence
        /// </summary>
        /// <param name="json">Json response body</param>
        /// <param name="status">HTTP status</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns></returns>
        public ResponseSequence Push(
            JsonObject json,
            int status = 200,
            Dictionary<string, string> headers = null
        )
            => Push(Response.Create(json, status, headers));
        
        /// <summary>
        /// Adds a text response to the sequence
        /// </summary>
        /// <param name="body">String response body</param>
        /// <param name="contentType">Text MIME type</param>
        /// <param name="status">HTTP status</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns></returns>
        public ResponseSequence Push(
            string body,
            string contentType = "text/plain",
            int status = 200,
            Dictionary<string, string> headers = null
        )
            => Push(Response.Create(body, contentType, status, headers));
        
        /// <summary>
        /// Adds a response to the sequence
        /// </summary>
        /// <param name="body">.NET Response body</param>
        /// <param name="status">HTTP status</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns></returns>
        public ResponseSequence Push(
            HttpContent body = null,
            int status = 200,
            Dictionary<string, string> headers = null
        )
            => Push(Response.Create(body, status, headers));
        
        /// <summary>
        /// Adds an empty response with given HTTP status code to the sequence
        /// </summary>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns></returns>
        public ResponseSequence PushStatus(
            int status,
            Dictionary<string, string> headers = null
        )
            => Push(Response.Create((HttpContent)null, status, headers));
        
        /// <summary>
        /// Adds a response to the sequence
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ResponseSequence Push(Response response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            
            responses.Enqueue(response);
            
            return this;
        }

        /// <summary>
        /// What response to return when the sequence gets empty
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ResponseSequence WhenEmpty(Response response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            emptySequenceResponse = response;
            failWhenEmpty = false;
            
            return this;
        }

        /// <summary>
        /// When the sequence gets empty, it starts returning
        /// empty 200 OK responses
        /// </summary>
        /// <returns></returns>
        public ResponseSequence DontFailWhenEmpty()
        {
            return WhenEmpty(Response.Create());
        }
    }
}