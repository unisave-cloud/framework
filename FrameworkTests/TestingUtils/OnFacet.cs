using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using LightJson;
using LightJson.Serialization;
using Unisave;
using Unisave.Exceptions;
using Unisave.Runtime;
using Unisave.Serialization;

namespace FrameworkTests.TestingUtils
{
    /// <summary>
    /// Simulates the "OnFacet" facade available on the client
    ///
    /// But it has removed restrictions on types and return values so that
    /// they can be tested.
    /// </summary>
    public class OnFacet<TFacet>
    {
        // OnFacet<Facet>.AsPlayer(player).WithTypes(typeof(Facet)).Call(...)

        private UnisavePlayer caller;
        private Type[] types;
        
        /// <summary>
        /// Start chaining by providing the calling player
        /// </summary>
        public static OnFacet<TFacet> AsPlayer(UnisavePlayer caller)
        {
            return new OnFacet<TFacet> {
                caller = caller
            };
        }

        /// <summary>
        /// Start chaining by specifying some player as the caller
        /// </summary>
        public static OnFacet<TFacet> AsSomePlayer()
        {
            return new OnFacet<TFacet> {
                caller = new UnisavePlayer("some-player")
            };
        }

        /// <summary>
        /// Optional specification of types used to search for the facet
        /// </summary>
        public OnFacet<TFacet> WithTypes(params Type[] types)
        {
            this.types = types;
            return this;
        }

        private Type[] ResolveTypes()
        {
            if (types != null)
                return types;

            // all types from the testing project
            return typeof(OnFacet<>).Assembly.GetTypes();
        }

        /// <summary>
        /// Performs a facet call just the same way an actual
        /// facet call would be performed
        /// </summary>
        public void Call(string methodName, params object[] arguments)
        {
            Call<bool>(methodName, arguments);
        }
        
        /// <summary>
        /// Performs a facet call just the same way an actual
        /// facet call would be performed
        /// </summary>
        public TReturn Call<TReturn>(
            string methodName,
            params object[] arguments
        )
        {
            var methodParameters = new JsonObject();
            methodParameters.Add("facetName", typeof(TFacet).Name);
            methodParameters.Add("methodName", methodName);
            methodParameters.Add(
                "arguments",
                ExecutionHelper.SerializeArguments(arguments)
            );
            methodParameters.Add("callerId", caller.Id);

            var executionParameters = new JsonObject();
            executionParameters.Add("executionMethod", "facet");
            executionParameters.Add("methodParameters", methodParameters);
            
            string result = Entrypoint.Start(
                executionParameters.ToString(),
                ResolveTypes()
            );

            var response = JsonReader.Parse(result);
             
            switch (response["result"].AsString)
            {
                case "ok":
                    if (response["methodResponse"]["hasReturnValue"].AsBoolean)
                        return Serializer.FromJson<TReturn>(
                            response["methodResponse"]["returnValue"]
                        );
                    else
                        return default(TReturn);

                case "exception":
                    var e = Serializer.FromJson<Exception>(
                        response["exception"]
                    );
                    PreserveStackTrace(e);
                    throw e;

                case "error":
                    throw new UnisaveException(
                        "Facet call error:\n" + response["message"]
                    );
            }

            throw new Exception("Invalid result value: " + response.ToString());
        }
        
        // magic
        // https://stackoverflow.com/a/2085377
        private static void PreserveStackTrace (Exception e)
        {
            var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
            var mgr = new ObjectManager(null, ctx);
            var si = new SerializationInfo(e.GetType(), new FormatterConverter());

            e.GetObjectData(si, ctx);
            mgr.RegisterObject(e, 1, si); // prepare for SetObjectData
            mgr.DoFixups(); // ObjectManager calls SetObjectData

            // voila, e is unmodified save for _remoteStackTraceString
        }
    }
}