using System;
using System.IO;
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
                /*
                 * DO NOT USE async void METHODS!
                 * The request context may get disposed anytime during the
                 * method execution and so it should not be used!
                 * These tests are here just for completeness!
                 */
                
                await Task.Delay(50);
                
                flag = true;
            }

            public async void AsyncVoidThrowingProcedure()
            {
                /*
                 * DO NOT USE async void METHODS!
                 * The request context may get disposed anytime during the
                 * method execution and so it should not be used!
                 * These tests are here just for completeness!
                 */

                // just to detect container disposal
                var myStream = new MemoryStream(32);
                RequestContext.Current.Services.RegisterInstance<MemoryStream>(
                    myStream
                );
                
                await Task.Delay(50);

                try
                {
                    // the dummy stream should be closed by now
                    // and all request-scoped services disposed
                    myStream.WriteByte(42); // throws ObjectDisposedException
                    
                    // I wanted to use the response stream, but it gets
                    // replaced by the BackendApplicationFixture so that it
                    // can be read. So it throws a different exception than
                    // it would in production.
                }
                catch (Exception e)
                {
                    asyncVoidException = e;
                    
                    // the exception can continue, but it will not be logged,
                    // because nobody awaits this async method
                    throw;
                }
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
        public async Task AsyncVoidProcedureExitsBeforeFinishes()
        {
            MyFacet.flag = false;
            
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.AsyncVoidProcedure),
                arguments: new JsonArray()
            );
            await response.AssertOkVoidResponse();
            
            // no yet true
            Assert.IsFalse(MyFacet.flag);

            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(10);

                // set to true, success!
                if (MyFacet.flag)
                    return;
            }
            
            Assert.Fail("The flag was not set to true in the time limit.");
        }

        [Test]
        public async Task AsyncVoidCanThrowAndNobodyCares()
        {
            MyFacet.asyncVoidException = null;
            
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.AsyncVoidThrowingProcedure),
                arguments: new JsonArray()
            );
            await response.AssertOkVoidResponse();
            
            // not thrown yet
            Assert.IsNull(MyFacet.asyncVoidException);

            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(10);

                // it has thrown!
                if (MyFacet.asyncVoidException != null)
                {
                    Assert.IsInstanceOf<ObjectDisposedException>(
                        MyFacet.asyncVoidException
                    );
                    return;
                }
            }
            
            Assert.Fail("The method has not thrown in the time limit.");
        }
    }
}