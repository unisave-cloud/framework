using System;
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
using Unisave.Utils;

namespace Unisave.Runtime.Kernels
{
    /// <summary>
    /// Handles the facet-call requests
    /// </summary>
    public class FacetCallKernel
    {
        private readonly BackendApplication app;
        private readonly SpecialValues specialValues;
        private readonly ServerSessionIdRepository sessionIdRepository;
        
        public FacetCallKernel(BackendApplication app, SpecialValues specialValues)
        {
            this.app = app;
            this.specialValues = specialValues;
            sessionIdRepository = app.Services.Resolve<ServerSessionIdRepository>();
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
                app.BackendTypes
            );

            MiddlewareAttribute[] globalMiddleware = {
                new MiddlewareAttribute(
                    1,
                    typeof(StartSession),
                    sessionId
                )
            };

            var response = FacetMiddleware.ExecuteMiddlewareStack(
                app,
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
            specialValues.Add("sessionId", sessionId);
            
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