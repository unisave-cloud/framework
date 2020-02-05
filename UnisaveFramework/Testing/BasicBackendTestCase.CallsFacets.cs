using System;
using LightJson;
using Unisave.Facets;
using Unisave.Foundation;
using Unisave.Runtime;
using Unisave.Runtime.Kernels;
using Unisave.Serialization;
using Unisave.Utils;

namespace Unisave.Testing
{
    public partial class BasicBackendTestCase
    {
        /// <summary>
        /// Session ID used for facet calling
        /// </summary>
        protected string SessionId { get; set; }
        
        /// <summary>
        /// Begins a facet call
        /// </summary>
        protected FacetCallQuery OnFacet<TFacet>() where TFacet : Facet
        {
            return OnFacet(typeof(TFacet).Name);
        }

        /// <summary>
        /// Begins a facet call
        /// </summary>
        protected FacetCallQuery OnFacet(string facetName)
        {
            return new FacetCallQuery(
                facetName,
                App,
                SessionId,
                s => SessionId = s
            );
        }
        
        public class FacetCallQuery
        {
            private string facetName;
            private Application app;
            private string sessionId;
            private Action<string> setSessionId;
            
            public FacetCallQuery(
                string facetName,
                Application app,
                string sessionId,
                Action<string> setSessionId
            )
            {
                this.facetName = facetName;
                this.app = app;
                this.sessionId = sessionId;
                this.setSessionId = setSessionId;
            }

            /// <summary>
            /// Calls synchronously a facet method with return value
            /// </summary>
            /// <param name="methodName">Method to call</param>
            /// <param name="arguments">Arguments for the method</param>
            /// <typeparam name="TReturn">Method return type</typeparam>
            /// <returns></returns>
            public TReturn CallSync<TReturn>(
                string methodName,
                params object[] arguments
            )
            {
                var methodParameters = new FacetCallKernel.MethodParameters(
                    facetName,
                    methodName,
                    ExecutionHelper.SerializeArguments(arguments),
                    sessionId
                );
                
                var kernel = app.Resolve<FacetCallKernel>();
                
                var returnedJson = kernel.Handle(methodParameters);

                var specialValues = app.Resolve<SpecialValues>();
                setSessionId.Invoke(specialValues.Read("sessionId").AsString);

                return Serializer.FromJson<TReturn>(returnedJson);
            }
            
            /// <summary>
            /// Calls synchronously a facet method with void return type
            /// </summary>
            /// <param name="methodName">Method to call</param>
            /// <param name="arguments">Arguments for the method</param>
            public void CallSync(
                string methodName,
                params object[] arguments
            )
            {
                CallSync<JsonValue>(methodName, arguments);
            }
        }
    }
}