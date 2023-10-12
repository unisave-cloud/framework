using System;
using System.IO;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Facets;

namespace FrameworkTests.Facets
{
    [TestFixture]
    public class FacetCallTest : BaseFacetCallingTest
    {
        public class MyFacet : Facet
        {
            public static bool flag = false;
            
            public void MyProcedure()
            {
                flag = true;
            }
        }

        [SetUp]
        public void SetUp()
        {
            CreateApplication(new Type[] {
                typeof(MyFacet)
            });
        }

        [Test]
        public async Task ItRunsMyProcedure()
        {
            MyFacet.flag = false;
            
            IOwinResponse response = await CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray(),
                sessionId: null
            );
            
            // validate the response content
            Assert.AreEqual(200, response.StatusCode);
            
            Console.WriteLine("Response:");
            Console.WriteLine(
                await new StreamReader(response.Body).ReadToEndAsync()
            );
            
            // check that facet was actually invoked
            Assert.IsTrue(MyFacet.flag);
        }
    }
}