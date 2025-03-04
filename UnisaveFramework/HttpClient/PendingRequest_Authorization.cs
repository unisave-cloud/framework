using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Unisave.HttpClient
{
    public partial class PendingRequest
    {
        /// <summary>
        /// Authentication parameters
        /// When null: no authentication
        /// Else:
        /// ["_scheme"] = "Bearer" / "Digest" / "Basic" / ...
        /// ["username"], ["password"], ["token"], ...
        /// </summary>
        private Dictionary<string, string> auth;
        
        /// <summary>
        /// Adds authentication data to the request
        /// (basic authentication from RFC 7617)
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithBasicAuth(string username, string password)
        {
            auth = new Dictionary<string, string> {
                ["_scheme"] = "Basic",
                ["username"] = username,
                ["password"] = password
            };
            
            return this;
        }
        
        /// <summary>
        /// Adds authentication data to the request
        /// (bearer authentication of OAuth 2.0 from RFC 6750)
        /// </summary>
        /// <param name="bearerToken"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithToken(string bearerToken)
        {
            auth = new Dictionary<string, string> {
                ["_scheme"] = "Bearer",
                ["token"] = bearerToken
            };
            
            return this;
        }
        
        private void AddAuthorizationHeader(HttpRequestMessage request)
        {
            if (auth == null)
                return;

            string scheme = auth["_scheme"];
            string parameter;

            switch (scheme)
            {
                case "Basic":
                    parameter = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            auth["username"] + ":" + auth["password"]
                        )
                    );
                    break;
                
                case "Bearer":
                    parameter = auth["token"];
                    break;
                
                default:
                    throw new InvalidOperationException(
                        "Unknown authorization scheme: " + scheme
                    );
            }
            
            request.Headers.Authorization = new AuthenticationHeaderValue(
                scheme, parameter
            );
        }
    }
}