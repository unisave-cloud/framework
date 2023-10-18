using System;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave;
using Unisave.Facets;
using Unisave.Foundation;

namespace FrameworkTests.Facets
{
    [TestFixture]
    public class MiddlewareTest : BaseFacetCallingTest
    {
        #region "Backend definition"
        
        [Middleware(typeof(MyMiddleware), "C1")]
        [Middleware(2, typeof(MyMiddleware), "C2")]
        public class FacetWithMiddleware : Facet
        {
            /// <summary>
            /// This field is edited by the middleware as it runs
            /// </summary>
            public static string middlewareLog;
            
            [Middleware(-5, typeof(MyMiddleware), "M1")]
            [Middleware(5, typeof(MyMiddleware), "M2")]
            public void MethodWithMiddleware()
            {
                MyMiddleware.AppendToLog("B1");
            }
    
            public void MethodWithoutMiddleware()
            {
                MyMiddleware.AppendToLog("B2");
            }
        }
        
        public class MyMiddleware : FacetMiddleware
        {
            public override FacetResponse Handle(
                FacetRequest request,
                Func<FacetRequest, FacetResponse> next,
                string[] parameters
            )
            {
                AppendToLog(parameters[0]);
                
                var response = next.Invoke(request);
    
                AppendToLog(parameters[0] + "'");
    
                return response;
            }
            
            public static void AppendToLog(string item)
            {
                string log = FacetWithMiddleware.middlewareLog;
    
                if (log == null)
                    log = "";
    
                if (log.Length != 0)
                    log += ",";
    
                log += item;
                
                FacetWithMiddleware.middlewareLog = log;
            }
        }
        
        #endregion
        
        [SetUp]
        public void SetUp()
        {
            CreateApplication(new Type[] {
                typeof(FacetWithMiddleware),
                typeof(MyMiddleware)
            });
        }
        
        [Test]
        public async Task ItRunsMiddlewareAsProperlyDefined()
        {
            FacetWithMiddleware.middlewareLog = null;
            IOwinResponse response = await CallFacet(
                facetName: typeof(FacetWithMiddleware).FullName,
                methodName: nameof(FacetWithMiddleware.MethodWithoutMiddleware),
                arguments: new JsonArray()
            );
            await AssertOkVoidResponse(response);
            Assert.AreEqual(
                "C1,C2,B2,C2',C1'",
                FacetWithMiddleware.middlewareLog
            );
            
            FacetWithMiddleware.middlewareLog = null;
            response = await CallFacet(
                facetName: typeof(FacetWithMiddleware).FullName,
                methodName: nameof(FacetWithMiddleware.MethodWithMiddleware),
                arguments: new JsonArray()
            );
            await AssertOkVoidResponse(response);
            Assert.AreEqual(
                "C1,C2,M1,M2,B1,M2',M1',C2',C1'",
                FacetWithMiddleware.middlewareLog
            );
        }
    }
}