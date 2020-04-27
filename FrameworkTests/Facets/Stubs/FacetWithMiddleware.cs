using System;
using Unisave;
using Unisave.Facets;
using Unisave.Foundation;

namespace FrameworkTests.Facets.Stubs
{
    [Middleware(typeof(MyMiddleware), "C1")]
    [Middleware(2, typeof(MyMiddleware), "C2")]
    public class FacetWithMiddleware : Facet
    {
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
        public MyMiddleware(Application app) : base(app) { }
        
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
            string log = MiddlewareTest.middlewareLog;

            if (log == null)
                log = "";

            if (log.Length != 0)
                log += ",";

            log += item;
            
            MiddlewareTest.middlewareLog = log;
        }
    }
}