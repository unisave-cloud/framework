using System.Reflection;
using System.Runtime.ExceptionServices;
using LightJson;
using Unisave.Facets;
using Unisave.Foundation;
using Unisave.Sessions;
using Unisave.Utils;

namespace Unisave.Runtime.Kernels
{
    /// <summary>
    /// Handles the facet-call requests
    /// </summary>
    public class FacetCallKernel
    {
        private readonly Application app;
        private readonly SpecialValues specialValues;
        
        public FacetCallKernel(Application app, SpecialValues specialValues)
        {
            this.app = app;
            this.specialValues = specialValues;
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
                app.GameAssemblyTypes
            );

            MiddlewareAttribute[] globalMiddleware = {
                new MiddlewareAttribute(
                    typeof(SessionFacetMiddleware),
                    sessionId
                )
            };

            var response = FacetMiddleware.ExecuteMiddlewareStack(
                app,
                globalMiddleware,
                request,
                rq => {
                    object returnedValue = ExecutionHelper.InvokeMethodInfo(
                        rq.Method,
                        rq.Facet,
                        rq.Arguments
                    );

                    return FacetResponse.CreateFrom(
                        returnedValue,
                        request.Method
                    );
                }
            );

            return response.ReturnedJson;
        }

        /// <summary>
        /// Generate session id if needed and
        /// sends it back to the client. 
        /// </summary>
        private string ProcessSessionId(MethodParameters methodParameters)
        {
            string sessionId = methodParameters.SessionId
                ?? GenerateSessionId();
            
            specialValues.Add("sessionId", sessionId);

            return sessionId;
        }
        
        /// <summary>
        /// Generates new random session id
        /// </summary>
        public static string GenerateSessionId()
        {
            return Str.Random(16);
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
        }
    }
}