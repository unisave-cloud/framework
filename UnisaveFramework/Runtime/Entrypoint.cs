using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LightJson;
using LightJson.Serialization;
using Microsoft.Owin;
using Unisave.Exceptions;
using Unisave.Foundation;

namespace Unisave.Runtime
{
    /// <summary>
    /// Here is where all execution begins
    /// </summary>
    public static class Entrypoint
    {
        /// <summary>
        /// Used to detect recursion in framework executions
        /// </summary>
        private static bool someExecutionIsRunning;
        
        /// <summary>
        /// Main entrypoint to the framework execution
        /// Given executionParameters it starts some action which results.
        /// in a returned value or an exception.
        /// 
        /// Input and output are serialized as JSON strings.
        /// </summary>
        public static string Start(
            string executionParametersAsJson,
            Type[] gameAssemblyTypes
        )
        {
            if (someExecutionIsRunning)
                throw new InvalidOperationException(
                    "You cannot start the framework from within the framework."
                );
            
            someExecutionIsRunning = true;

            var executionStopwatch = Stopwatch.StartNew();

            try
            {
                PrintGreeting();
                
                // parse execution parameters
                JsonObject executionParameters = JsonReader.Parse(
                    executionParametersAsJson
                ).AsJsonObject;
                
                // assert it's a facet call
                if (executionParameters["method"].AsString != "facet-call")
                    throw new UnisaveException(
                        "UnisaveFramework: Unknown execution method: "
                        + executionParameters["method"].AsString
                    );
                
                // parse method parameters
                JsonObject methodParameters
                    = executionParameters["methodParameters"].AsJsonObject;
                string facetName = methodParameters["facetName"].AsString;
                string methodName = methodParameters["methodName"].AsString;
                JsonArray arguments = methodParameters["arguments"].AsJsonArray;
                string sessionId = methodParameters["sessionId"].AsString;
                
                // parse env variables
                EnvStore envStore = EnvStore.Parse(
                    executionParameters["env"].AsString
                );
                
                // prepare backend types
                Type[] backendTypes = gameAssemblyTypes.Concat(
                    typeof(Entrypoint).Assembly.GetTypes()
                ).ToArray();

                JsonObject result;
                
                using (var app = BackendApplication.Start(backendTypes, envStore))
                {
                    var ctx = new OwinContext();

                    // prepare request
                    ctx.Request.Path = new PathString($"/{facetName}/{methodName}");
                    ctx.Request.Headers["X-Unisave-Request"] = "Facet";
                    ctx.Request.Headers["Content-Type"] = "application/json";
                    if (sessionId != null)
                    {
                        ctx.Request.Headers["Cookie"] = "unisave_session_id=" +
                            Uri.EscapeDataString(sessionId) + ";";
                    }

                    JsonObject requestBody = new JsonObject() {
                        ["arguments"] = arguments
                    };
                    byte[] requestBodyBytes = Encoding.UTF8.GetBytes(requestBody.ToString());
                    ctx.Request.Body = new MemoryStream(requestBodyBytes, writable: false);

                    // prepare response stream for writing
                    var responseStream = new MemoryStream(10 * 1024); // 10 KB, grows
                    ctx.Response.Body = responseStream;
    
                    // run the app delegate synchronously
                    app.Invoke(ctx)
                        .GetAwaiter()
                        .GetResult();
    
                    // process the HTTP response
                    if (ctx.Response.StatusCode != 200)
                        throw new UnisaveException(
                            "OWIN response did not have 200 status"
                        );
                    int receivedBytes = int.Parse(
                        ctx.Response.Headers["Content-Length"]
                    );
                    string newSessionId = ExtractSessionIdFromCookies(ctx.Response);
                    JsonObject owinResponse = JsonReader.Parse(
                        new StreamReader(
                            new MemoryStream(
                                responseStream.GetBuffer(), 0,
                                receivedBytes, writable: false
                            )
                        )
                    ).AsJsonObject;
                    
                    // convert response to entrypoint result
                    result = new JsonObject() {
                        ["result"] = owinResponse["status"],
                        ["special"] = new JsonObject() {
                            ["sessionId"] = newSessionId,
                            ["logs"] = new JsonArray() // TODO: pass logs along
                        }
                    };
                    
                    if (result["result"].AsString == "ok")
                        result["returned"] = owinResponse["returned"];
                    
                    if (result["result"].AsString == "exception")
                        result["exception"] = owinResponse["exception"];
                }

                executionStopwatch.Stop();
                result["special"].AsJsonObject["executionDuration"]
                    = executionStopwatch.ElapsedMilliseconds / 1000.0;

                return result.ToString();
            }
            finally
            {
                someExecutionIsRunning = false;
            }
        }

        /// <summary>
        /// Prints framework greeting to the commandline
        /// </summary>
        private static void PrintGreeting()
        {
            Console.WriteLine(
                "Starting Unisave framework " + FrameworkMeta.Version
            );
        }

        /// <summary>
        /// Extracts session ID from Set-Cookie headers and
        /// returns null if that fails.
        /// </summary>
        private static string ExtractSessionIdFromCookies(IOwinResponse response)
        {
            const string prefix = "unisave_session_id=";
            
            IList<string> setCookies = response.Headers.GetValues("Set-Cookie");

            string sessionCookie = setCookies?.FirstOrDefault(
                c => c.Contains(prefix)
            );

            sessionCookie = sessionCookie?.Split(';')?.FirstOrDefault(
                c => c.StartsWith(prefix)
            );

            string sessionId = sessionCookie?.Substring(prefix.Length);

            if (sessionId == null)
                return null;
            
            return Uri.UnescapeDataString(sessionId);
        }
    }
}
