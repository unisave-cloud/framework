using System;
using LightJson;
using NUnit.Framework;
using Unisave.Facades;
using Unisave.Facets;

namespace FrameworkTests.Runtime
{
    public class EntrypointEnvVarsTest : EntrypointFixture
    {
        #region "Backend definition"

        public class MyFacet : Facet
        {
            public static string lastFooValue = null;
            
            public void MyProcedure()
            {
                lastFooValue = Env.GetString("FOO");
            }
        }
        
        #endregion
        
        public override void SetUpEntrypointContext()
        {
            SetEntrypointContext(new Type[] {
                typeof(MyFacet)
            });
        }

        [Test]
        public void ItPassesEnvVarsAlong()
        {
            MyFacet.lastFooValue = null;
            EntrypointResponse response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray(),
                env: "FOO=Hello world!\nBAR=lorem ipsum\n"
            );
            response.AssertReturned(JsonValue.Null);
            Assert.AreEqual("Hello world!", MyFacet.lastFooValue);
        }
    }
}