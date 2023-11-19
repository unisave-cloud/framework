using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Foundation;

namespace FrameworkTests.Foundation
{
    /// <summary>
    /// Tests the <see cref="RequestContext"/> class
    /// </summary>
    [TestFixture]
    public class RequestContextTest
    {
        private IContainer dummyAppServices;
        private OwinContext dummyRequest;

        [SetUp]
        public void SetUp()
        {
            var dummyApp = BackendApplication.Start(Type.EmptyTypes, new EnvStore());
            dummyAppServices = dummyApp.Services;
            dummyRequest = new OwinContext(new Dictionary<string, object>());
        }
        
        [Test]
        public async Task ItProvidesCurrentContext()
        {
            Assert.IsNull(RequestContext.Current);
            
            using (var ctx = new RequestContext(dummyAppServices, dummyRequest))
            {
                Assert.AreSame(ctx, RequestContext.Current);

                int previousThread = Thread.CurrentThread.ManagedThreadId;
                await Task.Yield();
                Assert.AreNotEqual(previousThread, Thread.CurrentThread.ManagedThreadId);
                
                Assert.AreSame(ctx, RequestContext.Current);
            }
            
            Assert.IsNull(RequestContext.Current);
        }

        [Test]
        public async Task ItProvidesCurrentContextForParallelRequests()
        {
            /*
             * NOTE: This test may deadlock, if there are less than 3 threads
             * in the thread pool. This may happen and it's nothing wrong with
             * the test. Wasted 1 hour trying to get around this problem and
             * so far no success.
             * Deadlock reason: The barrier waits for 3 threads to continue.
             * There need to be these 3 threads, otherwise it waits indefinitely.
             */
            
            // makes sure both requests actually run at the same time
            var barrier = new Barrier(2 + 1);
            
            async Task ExecuteRequest()
            {
                Assert.IsNull(RequestContext.Current);
                using (var ctx = new RequestContext(dummyAppServices, dummyRequest))
                {
                    Assert.AreSame(ctx, RequestContext.Current);
                    barrier.SignalAndWait();
                    Assert.AreSame(ctx, RequestContext.Current);
                    await Task.Yield();
                    Assert.AreSame(ctx, RequestContext.Current);
                    barrier.SignalAndWait();
                    Assert.AreSame(ctx, RequestContext.Current);
                }
                Assert.IsNull(RequestContext.Current);
            }

            // start the two requests in parallel
            Assert.IsNull(RequestContext.Current);
            Task request1 = Task.Run(ExecuteRequest);
            Task request2 = Task.Run(ExecuteRequest);
            
            // now we all synchronize on the barrier
            barrier.SignalAndWait();
            
            Assert.IsNull(RequestContext.Current);
            
            // now we all synchronize on the barrier again
            barrier.SignalAndWait();
            
            Assert.IsNull(RequestContext.Current);

            // wait for the requests to finish
            await Task.WhenAll(request1, request2);
            
            Assert.IsNull(RequestContext.Current);
        }

        [Test]
        public async Task TaskRunLoosesTheContextOnlyWithSuppression()
        {
            async Task SubTaskExpectingNull()
            {
                Assert.IsNull(RequestContext.Current);
                await Task.Yield();
                Assert.IsNull(RequestContext.Current);
            }
            
            async Task SubTaskExpecting(RequestContext ctx)
            {
                Assert.AreSame(ctx, RequestContext.Current);
                await Task.Yield();
                Assert.AreSame(ctx, RequestContext.Current);
            }
            
            using (var ctx = new RequestContext(dummyAppServices, dummyRequest))
            {
                Assert.AreSame(ctx, RequestContext.Current);

                // the context is cleared with suppression
                Task task;
                using (ExecutionContext.SuppressFlow())
                {
                    task = Task.Run(SubTaskExpectingNull);
                }
                await task;
                
                Assert.AreSame(ctx, RequestContext.Current);
                
                // but the default behaviour caries it even inside Task.Run()
                await Task.Run(() => SubTaskExpecting(ctx));
                
                Assert.AreSame(ctx, RequestContext.Current);
            }
        }

        [Test]
        public async Task DeferredAwaitDoesNotLooseTheContext()
        {
            async Task SubTask(RequestContext expectedContext)
            {
                Assert.AreSame(expectedContext, RequestContext.Current);
                await Task.Yield();
                Assert.AreSame(expectedContext, RequestContext.Current);
            }
            
            using (var ctx = new RequestContext(dummyAppServices, dummyRequest))
            {
                Assert.AreSame(ctx, RequestContext.Current);

                Task task1 = SubTask(ctx);
                Task task2 = SubTask(ctx);
                Task task3 = SubTask(ctx);
                Task task4 = SubTask(ctx);
                
                await Task.Yield();
                
                // deferred await
                await Task.WhenAll(task1, task2, task3, task4);
                
                Assert.AreSame(ctx, RequestContext.Current);
            }
        }

        [Test]
        public void ContextIsNotLostEvenWhenStartingNewThread()
        {
            void ThreadBody()
            {
                Assert.IsNotNull(RequestContext.Current);
            }
            
            using (var ctx = new RequestContext(dummyAppServices, dummyRequest))
            {
                Assert.AreSame(ctx, RequestContext.Current);

                Thread thread = new Thread(ThreadBody);
                thread.Start();
                thread.Join();
                
                Assert.AreSame(ctx, RequestContext.Current);
            }
        }
    }
}