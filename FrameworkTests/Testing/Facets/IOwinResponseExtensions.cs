using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using FrameworkTests.Testing.Foundation;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Testing.Facets
{
    public static class OwinResponseExtensions
    {
        /// <summary>
        /// Gets the value returned from a facet call
        /// </summary>
        /// <param name="response"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> GetReturnedValue<T>(
            this IOwinResponse response
        )
        {
            JsonObject body = await response.ReadJsonBody<JsonObject>();

            await response.RethrowIfException();
            
            Assert.AreEqual("ok", body["status"].AsString);
            
            return Serializer.FromJson<T>(body["returned"]);
        }

        /// <summary>
        /// Asserts that the facet call ended with an OK void response.
        /// </summary>
        /// <param name="response"></param>
        public static async Task AssertOkVoidResponse(this IOwinResponse response)
        {
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            
            await response.RethrowIfException();
            
            Assert.AreEqual("ok", body["status"].AsString);
            
            Assert.IsTrue(body["returned"].IsNull);
        }
        
        /// <summary>
        /// Gets the thrown exception from a facet call
        /// </summary>
        /// <param name="response"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> GetThrownException<T>(
            this IOwinResponse response
        ) where T : Exception
        {
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            
            Assert.AreEqual(
                "exception", body["status"].AsString,
                "The facet call did not throw any exception."
            );
            
            Assert.IsTrue(
                body.ContainsKey("isKnownException"),
                "The isKnownException field is missing from the response."
            );
            
            return Serializer.FromJson<T>(body["exception"]);
        }
        
        /// <summary>
        /// Rethrows the returned exception if the facet returned an exception.
        /// Does nothing otherwise.
        /// </summary>
        /// <param name="response"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task RethrowIfException(
            this IOwinResponse response
        )
        {
            JsonObject body = await response.ReadJsonBody<JsonObject>();

            if (body["status"].AsString == "exception")
            {
                Exception e = await response.GetThrownException<Exception>();
                ExceptionDispatchInfo.Capture(e).Throw();
            }
        }
    }
}