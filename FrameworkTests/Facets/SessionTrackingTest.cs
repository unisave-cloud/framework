using System;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Facets;

namespace FrameworkTests.Facets
{
    [TestFixture]
    public class SessionTrackingTest : BaseFacetCallingTest
    {
        #region "Backend definition"
        
        public class MyFacet : Facet
        {
            public void MyProcedure()
            {
                //
            }
        }
        
        #endregion
        
        [SetUp]
        public void SetUp()
        {
            CreateApplication(new Type[] {
                typeof(MyFacet)
            });
        }
        
        /*
         * TODO: test exceptions, session, logging, legacy entrypoint
         * - add request-scoped services & facades
         * - test concurrency (asynchronous & multithreading)
         */

        [Test]
        public async Task ItCreatesNewSession()
        {
            // IOwinResponse response = await CallFacet(
            //     facetName: typeof(MyFacet).FullName,
            //     methodName: nameof(MyFacet.MyProcedure),
            //     arguments: new JsonArray(),
            //     request => {
            //         request.Headers["Cookie"] = "unisave_session_id=123456789";
            //     }
            // );
        }
    }
}