using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using Unisave.Serialization;

namespace FrameworkTests.Testing.Foundation
{
    /// <summary>
    /// Extension helpers that operate on the OWIN HTTP level
    /// </summary>
    public static class OwinResponseExtensions
    {
        /// <summary>
        /// Reads the OWIN response body and parses it from JSON
        /// </summary>
        /// <param name="response"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> ReadJsonBody<T>(
            this IOwinResponse response
        )
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            
            string json = await new StreamReader(response.Body).ReadToEndAsync();
            
            return Serializer.FromJsonString<T>(json);
        }
    }
}