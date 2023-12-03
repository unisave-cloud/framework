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
                
                // get the application
                // (create or resolve from cache)
                BackendApplication app = ResolveApplication(
                    gameAssemblyTypes,
                    executionParameters["env"].AsString
                );
                
                var ctx = new OwinContext();

                // prepare request
                ctx.Request.Method = "GET";
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
                JsonObject result = new JsonObject() {
                    ["result"] = owinResponse["status"],
                    ["special"] = new JsonObject() {
                        ["sessionId"] = newSessionId,
                        ["logs"] = owinResponse["logs"]
                    }
                };
                
                if (result["result"].AsString == "ok")
                    result["returned"] = owinResponse["returned"];
                
                if (result["result"].AsString == "exception")
                    result["exception"] = owinResponse["exception"];

                executionStopwatch.Stop();
                result["special"].AsJsonObject["executionDuration"]
                    = executionStopwatch.ElapsedMilliseconds / 1000.0;

                LogAccess(ctx, executionStopwatch.ElapsedMilliseconds);
                
                return result.ToString();
            }
            finally
            {
                someExecutionIsRunning = false;
            }
        }

        #region "BackendApplication resolution & caching"

        private static BackendApplication cachedApp = null;
        private static Type[] cachedGameAssemblyTypes = Type.EmptyTypes;
        private static string cachedEnvironmentVariables = "";
        
        private static BackendApplication ResolveApplication(
            Type[] gameAssemblyTypes,
            string environmentVariables
        )
        {
            // first request
            if (cachedApp == null)
                return CreateApplication(gameAssemblyTypes, environmentVariables);
            
            // next requests need to check
            if (
                environmentVariables == cachedEnvironmentVariables
                && AreSameTypes(gameAssemblyTypes, cachedGameAssemblyTypes)
            )
            {
                return cachedApp;
            }
            else
            {
                Console.WriteLine(
                    "Re-initializing backend application, because the " +
                    "environment variables or backend types have changed."
                );
                
                cachedApp.Dispose();
                cachedApp = null;

                return CreateApplication(gameAssemblyTypes, environmentVariables);
            }
            
            // The last app to be created is not disposed. Because this
            // is a temporary solution and there isn't much to dispose anyways.
            // With the Startup class API, there will be a proper disposal,
            // based on OWIN specification. This is just a temporary connector.
        }

        private static bool AreSameTypes(Type[] a, Type[] b)
        {
            if (a.Length != b.Length)
                return false;
            
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;

            return true;
        }

        private static BackendApplication CreateApplication(
            Type[] gameAssemblyTypes,
            string environmentVariables
        )
        {
            // parse env variables
            EnvStore envStore = EnvStore.Parse(
                environmentVariables
            );
                
            // prepare backend types
            Type[] backendTypes = gameAssemblyTypes.Concat(
                typeof(Entrypoint).Assembly.GetTypes()
            ).ToArray();
            
            // create the app
            Console.WriteLine(
                "Starting Unisave framework " + FrameworkMeta.Version
            );
            var app = BackendApplication.Start(backendTypes, envStore);
            
            // store to cache
            cachedApp = app;
            cachedGameAssemblyTypes = gameAssemblyTypes;
            cachedEnvironmentVariables = environmentVariables;
            
            return app;
        }
        
        #endregion

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
        
        #region "Access logging"

        private static long requestIndex = 0;

        /// <summary>
        /// Logs access to the backend server
        /// (this will be performed by the Unisave Server instead,
        /// once it supports the Startup class API)
        /// </summary>
        private static void LogAccess(IOwinContext ctx, long milliseconds)
        {
            try
            {
                requestIndex += 1;
                
                string id = "R" + requestIndex; // will be request ID sent via header
                string now = DateTime.UtcNow.ToString("yyyy-dd-MM H:mm:ss");
                string method = ctx.Request.Method;
                string path = ctx.Request.Path.Value;
                string status = ctx.Response.StatusCode.ToString();
                string bytesSent = ctx.Response.Headers["Content-Length"] ?? "-";

                // [2023-12-03 21:52:37] R1385 GET /MyFacet/Foo 200 138B 45ms
                Console.WriteLine(
                    $"[{now}] {id} {method} {path} {status} {bytesSent}B {milliseconds}ms"
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        
        #endregion
    }
}
