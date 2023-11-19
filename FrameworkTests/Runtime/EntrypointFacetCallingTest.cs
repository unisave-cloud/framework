using System;
using System.Linq;
using LightJson;
using NUnit.Framework;
using Unisave.Facets;
using Unisave.Serialization;

namespace FrameworkTests.Runtime
{
    /// <summary>
    /// Tests the legacy Entrypoint class,
    /// that it's properly connected to the new OWIN runtime
    /// </summary>
    [TestFixture]
    public class EntrypointFacetCallingTest : EntrypointFixture
    {
        #region "Backend definition"
        
        public class MyFacet : Facet
        {
            public static bool flag = false;
            
            public void MyProcedure()
            {
                flag = true;
            }
            
            public void MyParametrizedProcedure(bool value)
            {
                flag = value;
            }

            public int SquaringFunction(int value)
            {
                return value * value;
            }

            public void ThrowingProcedure()
            {
                throw new InvalidOperationException("Hello world!");
            }

            public byte[] LargeResponse(int count)
            {
                byte[] data = new byte[count];
                for (int i = 0; i < count; i++)
                    data[i] = (byte)i;
                return data;
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
        public void ItRunsMyProcedure()
        {
            MyFacet.flag = false;
            
            EntrypointResponse response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray()
            );
            response.AssertReturned(JsonValue.Null);
            
            // check that facet was actually invoked
            Assert.IsTrue(MyFacet.flag);
        }
        
        [Test]
        public void ItRunsMyParametrizedProcedure()
        {
            MyFacet.flag = false;
            
            EntrypointResponse response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyParametrizedProcedure),
                arguments: new JsonArray() { true }
            );
            response.AssertReturned(JsonValue.Null);
            
            Assert.IsTrue(MyFacet.flag);
            
            response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyParametrizedProcedure),
                arguments: new JsonArray() { false }
            );
            response.AssertReturned(JsonValue.Null);
            
            Assert.IsFalse(MyFacet.flag);
        }

        [Test]
        public void ItRunsFunctions()
        {
            EntrypointResponse response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.SquaringFunction),
                arguments: new JsonArray() { 5 }
            );
            response.AssertReturned(25);
        }
        
        [Test]
        public void ItHandlesUnknownExceptions()
        {
            EntrypointResponse response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.ThrowingProcedure),
                arguments: new JsonArray()
            );
            Exception e = response.GetException();
            Assert.IsInstanceOf<InvalidOperationException>(e);
            Assert.AreEqual("Hello world!", e.Message);
        }
        
        [Test]
        public void ItPassesAlongLargeResponses()
        {
            EntrypointResponse response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.LargeResponse),
                arguments: new JsonArray(1024 * 100) // ~ 100 KB
            );
            response.AssertSuccess();

            byte[] data = Serializer.FromJson<byte[]>(
                response.Json["returned"]
            );
            
            for (int i = 0; i < data.Length; i++)
                Assert.AreEqual((byte)i, data[i]);
        }
    }
}