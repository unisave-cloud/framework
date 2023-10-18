using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using Unisave.Facets;
using Unisave.Foundation;
using Unisave.Serialization;
using Unisave.Sessions;
using Unisave.Sessions.Middleware;

namespace Unisave.Runtime.Kernels
{
    /// <summary>
    /// Handles the facet-call requests
    /// </summary>
    public class FacetCallKernel
    {
        private readonly IOwinContext owinContext;
        private readonly IContainer services;
        private readonly BackendTypes backendTypes;
        private readonly ServerSessionIdRepository sessionIdRepository;
        
        public FacetCallKernel(
            IOwinContext owinContext,
            IContainer services,
            BackendTypes backendTypes,
            ServerSessionIdRepository sessionIdRepository
        )
        {
            this.owinContext = owinContext;
            this.services = services;
            this.backendTypes = backendTypes;
            this.sessionIdRepository = sessionIdRepository;
        }
        
        /// <summary>
        /// Handle the facet call request
        /// </summary>
        public JsonValue Handle(MethodParameters methodParameters)
        {
            string sessionId = ProcessSessionId(methodParameters);

            FacetRequest request = FacetRequest.CreateFrom(
                methodParameters.FacetName,
                methodParameters.MethodName,
                methodParameters.Arguments,
                backendTypes,
                services
            );

            MiddlewareAttribute[] globalMiddleware = {
                new MiddlewareAttribute(
                    1,
                    typeof(StartSession),
                    sessionId
                )
            };

            var response = FacetMiddleware.ExecuteMiddlewareStack(
                services,
                globalMiddleware,
                request,
                rq => {
                    object returnedValue = null;
                    
                    UnwrapTargetInvocationException(() => {
                        returnedValue = rq.Method.Invoke(
                            rq.Facet,
                            rq.Arguments
                        );
                    });

                    return FacetResponse.CreateFrom(
                        returnedValue,
                        request.Method
                    );
                }
            );

            return response.ReturnedJson;
        }
        
        /// <summary>
        /// Invokes method via method info with transparent
        /// exception propagation
        /// </summary>
        private static void UnwrapTargetInvocationException(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException == null)
                    throw;
                    
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                throw;
            }
        }

        /// <summary>
        /// Generate session id if needed and
        /// sends it back to the client. 
        /// </summary>
        private string ProcessSessionId(MethodParameters methodParameters)
        {
            string sessionId = methodParameters.SessionId
                ?? ServerSessionIdRepository.GenerateSessionId();

            sessionIdRepository.SessionId = sessionId;
            
            owinContext.Response.Cookies.Append(
                "unisave_session_id",
                sessionId,
                new CookieOptions() {
                    HttpOnly = true,
                    Path = "/",
                    // TODO: load expiration time from session configuration
                    Expires = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                }
            );
            
            return sessionId;
        }
        
        /// <summary>
        /// Method parameters for session call
        /// </summary>
        public class MethodParameters
        {
            public string FacetName { get; }
            public string MethodName { get; }
            public JsonArray Arguments { get; }
            public string SessionId { get; }
            
            public MethodParameters(
                string facetName,
                string methodName,
                JsonArray arguments,
                string sessionId
            )
            {
                FacetName = facetName;
                MethodName = methodName;
                Arguments = arguments;
                SessionId = sessionId;
            }
            
            public static MethodParameters Parse(JsonObject methodParameters)
            {
                return new MethodParameters(
                    methodParameters["facetName"],
                    methodParameters["methodName"],
                    methodParameters["arguments"],
                    methodParameters["sessionId"]
                );
            }

            public static async Task<MethodParameters> Parse(IOwinRequest request)
            {
                string[] segments = request.Path.Value.Split('/');

                // TODO: handle invalid number of segments
                
                string sessionId = request.Cookies["unisave_session_id"];
                
                using (var sr = new StreamReader(request.Body, Encoding.UTF8))
                {
                    string json = await sr.ReadToEndAsync();

                    JsonObject body = Serializer.FromJsonString<JsonObject>(json);

                    return new MethodParameters(
                        facetName: segments[1],
                        methodName: segments[2],
                        arguments: body["arguments"].AsJsonArray,
                        sessionId: sessionId
                    );
                }
            }
        }
    }
}