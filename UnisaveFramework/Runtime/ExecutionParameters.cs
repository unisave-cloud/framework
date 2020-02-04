using LightJson;
using LightJson.Serialization;

namespace Unisave.Runtime
{
    /// <summary>
    /// Holds parsed execution parameters given to the framework entrypoint
    /// </summary>
    public class ExecutionParameters
    {
        /// <summary>
        /// Name of the method to execute
        /// </summary>
        public string Method { get; }
        
        /// <summary>
        /// Parameters for the method
        /// </summary>
        public JsonValue MethodParameters { get; }
        
        /// <summary>
        /// Source text for env config
        /// </summary>
        public string EnvSource { get; }
        
        public ExecutionParameters(
            string method,
            JsonValue methodParameters,
            string envSource
        )
        {
            Method = method;
            MethodParameters = methodParameters;
            EnvSource = envSource;
        }

        /// <summary>
        /// Parse out execution parameters from a json string
        /// </summary>
        public static ExecutionParameters Parse(string executionParameters)
        {
            JsonObject ep = JsonReader.Parse(executionParameters);

            return new ExecutionParameters(
                ep["method"].AsString,
                ep["methodParameters"],
                ep["env"]
            );
        }
    }
}