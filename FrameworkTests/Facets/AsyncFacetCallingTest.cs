using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FrameworkTests.Testing;
using FrameworkTests.Testing.Facets;
using FrameworkTests.Testing.Foundation;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Facets;
using Unisave.Foundation;

namespace FrameworkTests.Facets
{
    [TestFixture]
    public class AsyncFacetCallingTest : BackendApplicationFixture
    {
        #region "Backend definition"
        
        public class MyFacet : Facet
        {
            public static bool flag = false;
            public static Exception asyncVoidException = null;
            
            public async Task MyProcedure()
            {
                await Task.Yield();
                
                flag = true;
            }
            
            public async Task MyParametrizedProcedure(bool value)
            {
                await Task.Yield();

                flag = value;
            }

            public async Task<int> SquaringFunction(int value)
            {
                await Task.Yield();
                
                return value * value;
            }

            public async Task ThrowingProcedure()
            {
                await Task.Yield();
                
                throw new InvalidOperationException("Hello world!");
            }

            public async void AsyncVoidProcedure()
            {
                await Task.Yield();
                
                // "async void" facet methods are disallowed, because:
                // - uncaught exceptions can crash the whole process
                // - the request context can be disposed before the code finishes
                
                Assert.Fail("This code should not be executed.");
            }
        }
        
        #endregion
        
        public override void SetUpBackendApplication()
        {
            CreateApplication(new Type[] {
                typeof(MyFacet)
            });
        }

        [Test]
        public async Task ItRunsMyProcedure()
        {
            MyFacet.flag = false;
            
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyProcedure),
                arguments: new JsonArray()
            );
            await response.AssertOkVoidResponse();
            
            // check that facet was actually invoked
            Assert.IsTrue(MyFacet.flag);
        }
        
        [Test]
        public async Task ItRunsMyParametrizedProcedure()
        {
            MyFacet.flag = false;
            
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyParametrizedProcedure),
                arguments: new JsonArray() { true }
            );
            await response.AssertOkVoidResponse();
            
            Assert.IsTrue(MyFacet.flag);
            
            response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.MyParametrizedProcedure),
                arguments: new JsonArray() { false }
            );
            await response.AssertOkVoidResponse();
            
            Assert.IsFalse(MyFacet.flag);
        }
        
        [Test]
        public async Task ItRunsFunctions()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.SquaringFunction),
                arguments: new JsonArray() { 5 }
            );
            int returned = await response.GetReturnedValue<int>();
            Assert.AreEqual(25, returned);
        }
        
        [Test]
        public async Task ItHandlesUnknownExceptions()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.ThrowingProcedure),
                arguments: new JsonArray()
            );
            Exception e = await response.GetThrownException<Exception>();
            Assert.IsInstanceOf<InvalidOperationException>(e);
            Assert.AreEqual("Hello world!", e.Message);
            
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            Assert.IsFalse(body["isKnownException"].AsBoolean);
        }

        [Test]
        public async Task AsyncVoidProcedureCannotBeCalled()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.AsyncVoidProcedure),
                arguments: new JsonArray()
            );
            Exception e = await response.GetThrownException<Exception>();
            Assert.IsInstanceOf<MethodSearchException>(e);
            string fullMethodName = typeof(MyFacet).FullName
                + "." + nameof(MyFacet.AsyncVoidProcedure);
            Assert.AreEqual(
                $"Method '{fullMethodName}' cannot be declared as " +
                $"'async void'. Use 'async Task' instead.",
                e.Message
            );
            
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            Assert.IsFalse(body["isKnownException"].AsBoolean);
        }
    }
}