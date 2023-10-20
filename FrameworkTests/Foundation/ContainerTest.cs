using System;
using System.Collections.Generic;
using Microsoft.Owin;
using Moq;
using NUnit.Framework;
using TinyIoC;
using Unisave.Foundation;

namespace FrameworkTests.Foundation
{
    [TestFixture]
    public class ContainerTest
    {
        private TinyIoCContainer tinyIoCContainer;
        private IContainer container;

        // dummy interface
        public interface IFoo : IDisposable
        {
            void DoFoo();
        }
        
        // another dummy interface
        public interface IBar : IDisposable
        {
            void DoBar();
        }
        
        // dummy service
        public class Foo : IFoo
        {
            public void DoFoo() { }
            
            public void Dispose() { }
        }

        // dummy request-scoped service
        public class RequestBaz
        {
            public readonly IFoo foo;

            public RequestBaz(IFoo foo)
            {
                this.foo = foo;
            }
        }

        // dummy service that needs a container to be constructed
        public class ExampleContainerUsingService
        {
            public ExampleContainerUsingService(IContainer container) { }
        }
        
        [SetUp]
        public void SetUp()
        {
            tinyIoCContainer = new TinyIoCContainer();
            container = new TinyIoCAdapter(tinyIoCContainer);
        }

        [TearDown]
        public void TearDown()
        {
            container.Dispose();
        }
        
        [Test]
        public void ItResolvesItself()
        {
            Assert.AreSame(
                tinyIoCContainer,
                container.Resolve<TinyIoCContainer>()
            );
            
            Assert.AreSame(
                container,
                container.Resolve<TinyIoCAdapter>()
            );
            
            Assert.AreSame(
                container,
                container.Resolve<IContainer>()
            );
        }

        [Test]
        public void ItResolvesItselfWhenConstructingService()
        {
            container.Resolve<ExampleContainerUsingService>();
        }

        [Test]
        public void ItResolvesConcreteClassesTransiently()
        {
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsFalse(container.IsRegistered<Foo>());
            
            var firstInstance = container.Resolve<Foo>();
            var secondInstance = container.Resolve<Foo>();
            Assert.IsNotNull(firstInstance);
            Assert.IsNotNull(secondInstance);
            Assert.AreNotSame(firstInstance, secondInstance);
        }

        [Test]
        public void ItRegistersConcreteClassesAsSingletons()
        {
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsFalse(container.IsRegistered<Foo>());
            
            container.RegisterSingleton<Foo>();
            
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsTrue(container.IsRegistered<Foo>());
            
            var firstInstance = container.Resolve<Foo>();
            var secondInstance = container.Resolve<Foo>();
            Assert.IsNotNull(firstInstance);
            Assert.IsNotNull(secondInstance);
            Assert.AreSame(firstInstance, secondInstance);
        }

        [Test]
        public void ItRegistersConcreteClassesAsInstances()
        {
            var instance = new Foo();
            
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsFalse(container.IsRegistered<Foo>());
            
            container.RegisterInstance<Foo>(instance);
            
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsTrue(container.IsRegistered<Foo>());

            var resolvedInstance = container.Resolve<Foo>();
            Assert.IsNotNull(resolvedInstance);
            Assert.AreSame(instance, resolvedInstance);
        }

        [Test]
        public void ItCreatesChildContainers()
        {
            string log = "";
            
            // global service (e.g. database connection)
            container.RegisterSingleton<IFoo, Foo>();
            IFoo foo = container.Resolve<IFoo>();
            
            // request comes, we create a request-scoped child container
            using (var child = container.CreateChildContainer())
            {
                // register request-scoped service (e.g. http context)
                child.RegisterSingleton<IBar>(_ => {
                    var mock = new Mock<IBar>();
                    log += "+Bar";
                    mock.Setup(b => b.Dispose()).Callback(() => {
                        log += "~Bar";
                    });
                    return mock.Object;
                });
                
                // nothing should be created yet
                Assert.AreEqual("", log);
                
                // now user requests the http context so it's created
                var childBar = child.Resolve<IBar>();
                Assert.AreEqual("+Bar", log);
                
                // we can also resolve from parent via the child
                var childFoo = child.Resolve<IFoo>();
                Assert.AreSame(foo, childFoo);
            }
            
            // now after the child dispose, the http context should be disposed
            Assert.AreEqual("+Bar~Bar", log);
        }

        [Test]
        public void ItDisposesOwnedInstances()
        {
            bool fooDisposed = false;
            
            var mock = new Mock<IFoo>();
            mock.Setup(b => b.Dispose()).Callback(() => {
                fooDisposed = true;
            });
            IFoo foo = mock.Object;
            
            Assert.IsFalse(fooDisposed);
            container.RegisterInstance<IFoo>(foo, transferOwnership: true);
            Assert.IsFalse(fooDisposed);
            
            Assert.IsFalse(fooDisposed);
            container.Resolve<IFoo>();
            Assert.IsFalse(fooDisposed);
            
            container.Dispose();
            Assert.IsTrue(fooDisposed); // is disposed
        }
        
        [Test]
        public void ItDoesntDisposeExternalInstances()
        {
            bool fooDisposed = false;
            
            var mock = new Mock<IFoo>();
            mock.Setup(b => b.Dispose()).Callback(() => {
                fooDisposed = true;
            });
            IFoo foo = mock.Object;
            
            Assert.IsFalse(fooDisposed);
            container.RegisterInstance<IFoo>(foo, transferOwnership: false);
            Assert.IsFalse(fooDisposed);
            
            Assert.IsFalse(fooDisposed);
            container.Resolve<IFoo>();
            Assert.IsFalse(fooDisposed);
            
            container.Dispose();
            Assert.IsFalse(fooDisposed); // stays alive
        }

        private RequestContext ConstructRequestContext()
        {
            var dummyRequest = new OwinContext(new Dictionary<string, object>());
            return new RequestContext(container, dummyRequest);
        }
        
        [Test]
        public void ItRegistersConcreteClassesAsPerRequestSingletons()
        {
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsFalse(container.IsRegistered<Foo>());
            
            container.RegisterPerRequestSingleton<Foo>();
            
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsTrue(container.IsRegistered<Foo>());
        
            using (var ctx = ConstructRequestContext())
            {
                Assert.IsTrue(ctx.Services.CanResolve<Foo>());
                Assert.IsTrue(ctx.Services.IsRegistered<Foo>());
                // Assert.IsFalse(ctx.Services.IsRegistered<Foo>(bubbleUp: false));
                
                var firstInstance = ctx.Services.Resolve<Foo>();
                // Assert.IsTrue(ctx.Services.IsRegistered<Foo>(bubbleUp: false));
                
                var secondInstance = ctx.Services.Resolve<Foo>();
                
                Assert.IsNotNull(firstInstance);
                Assert.IsNotNull(secondInstance);
                Assert.AreSame(firstInstance, secondInstance);
            }
            
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsTrue(container.IsRegistered<Foo>());
        }
        
        [Test]
        public void ItRegistersFactoryAsPerRequestSingletons()
        {
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsFalse(container.IsRegistered<Foo>());
            
            container.RegisterPerRequestSingleton<Foo>(
                _ => new Foo()
            );
            
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsTrue(container.IsRegistered<Foo>());
        
            using (var ctx = ConstructRequestContext())
            {
                Assert.IsTrue(ctx.Services.CanResolve<Foo>());
                Assert.IsTrue(ctx.Services.IsRegistered<Foo>());
                // Assert.IsFalse(ctx.Services.IsRegistered<Foo>(bubbleUp: false));
                
                var firstInstance = ctx.Services.Resolve<Foo>();
                // Assert.IsTrue(ctx.Services.IsRegistered<Foo>(bubbleUp: false));
                
                var secondInstance = ctx.Services.Resolve<Foo>();
                
                Assert.IsNotNull(firstInstance);
                Assert.IsNotNull(secondInstance);
                Assert.AreSame(firstInstance, secondInstance);
            }
            
            Assert.IsTrue(container.CanResolve<Foo>());
            Assert.IsTrue(container.IsRegistered<Foo>());
        }

        [Test]
        public void IfOnlyChildCanResolveItGetsUsed()
        {
            using (var ctx = ConstructRequestContext())
            {
                ctx.Services.RegisterTransient<IFoo, Foo>();
                
                Assert.DoesNotThrow(() => {
                    ctx.Services.Resolve<IFoo>();
                });
            }
        }
        
        [Test]
        public void IfOnlyChildCanConstructItGetsUsed()
        {
            using (var ctx = ConstructRequestContext())
            {
                ctx.Services.RegisterTransient<IFoo, Foo>();
                
                Assert.DoesNotThrow(() => {
                    ctx.Services.Resolve<RequestBaz>();
                });
            }
        }
        
        [Test]
        public void IfOnlyParentCanResolveItGetsUsed()
        {
            container.RegisterTransient<IFoo, Foo>();
            
            using (var ctx = ConstructRequestContext())
            {
                Assert.DoesNotThrow(() => {
                    ctx.Services.Resolve<IFoo>();
                });
            }
        }
        
        [Test]
        public void IfOnlyParentCanConstructItGetsUsed()
        {
            container.RegisterTransient<IFoo, Foo>();
            
            using (var ctx = ConstructRequestContext())
            {
                Assert.DoesNotThrow(() => {
                    ctx.Services.Resolve<RequestBaz>();
                });
            }
        }
        
        [Test]
        public void IfOnlyChildRegistersItGetsUsed()
        {
            var foo = new Foo();
            
            using (var ctx = ConstructRequestContext())
            {
                ctx.Services.RegisterInstance<Foo>(foo);
                
                var resolved = ctx.Services.Resolve<Foo>();
                Assert.AreSame(foo, resolved); // (does not get constructed by parent)
            }
        }
        
        [Test]
        public void IfOnlyParentRegistersItGetsUsed()
        {
            var foo = new Foo();
            
            container.RegisterInstance<Foo>(foo);
            
            using (var ctx = ConstructRequestContext())
            {
                var resolved = ctx.Services.Resolve<Foo>();
                Assert.AreSame(foo, resolved); // (does not get constructed by child)
            }
        }
        
        [Test]
        public void IfNoneRegisterChildGetsUsed()
        {
            var parentFoo = new Foo();
            var childFoo = new Foo();
            
            container.RegisterInstance<IFoo>(parentFoo);
            
            using (var ctx = ConstructRequestContext())
            {
                container.RegisterInstance<IFoo>(childFoo);
                
                var baz = ctx.Services.Resolve<RequestBaz>();
                Assert.AreSame(childFoo, baz.foo); // uses the foo of the constructor
            }
        }
        
        [Test]
        public void IfBothRegisterChildGetsUsed()
        {
            var parentFoo = new Foo();
            var childFoo = new Foo();
            
            container.RegisterInstance<Foo>(parentFoo);
            
            using (var ctx = ConstructRequestContext())
            {
                container.RegisterInstance<Foo>(childFoo);
                
                var foo = ctx.Services.Resolve<Foo>();
                Assert.AreSame(childFoo, foo);
            }
        }
        
        [Test]
        public void TwoRequestsHaveSeparatePerRequestSingletons()
        {
            container.RegisterPerRequestSingleton<IFoo, Foo>();
            
            IFoo first;
            IFoo second;
            
            using (var ctx = ConstructRequestContext())
            {
                first = ctx.Services.Resolve<IFoo>();
            }
            
            using (var ctx = ConstructRequestContext())
            {
                second = ctx.Services.Resolve<IFoo>();
            }
            
            Assert.AreNotSame(first, second);
        }
        
        [Test]
        public void PerRequestSingletonIsDisposedWithTheRequest()
        {
            string log = "";
            
            var mock = new Mock<IFoo>();
            mock.Setup(b => b.Dispose()).Callback(() => {
                log += "~Foo";
            });
            IFoo foo = mock.Object;
            
            container.RegisterPerRequestSingleton<IFoo>(_ => {
                log += "+Foo";
                return foo;
            });
            
            Assert.AreEqual("", log);
            
            using (var ctx = ConstructRequestContext())
            {
                Assert.AreEqual("", log);
                
                // first time creates the instance
                ctx.Services.Resolve<IFoo>();
                Assert.AreEqual("+Foo", log);
                
                // second time just returns it
                ctx.Services.Resolve<IFoo>();
                Assert.AreEqual("+Foo", log);
                
                // third as well
                ctx.Services.Resolve<IFoo>();
                Assert.AreEqual("+Foo", log);
            }
            
            Assert.AreEqual("+Foo~Foo", log);
        }
        
        [Test]
        public void ResolvingPerRequestSingletonFromParentFails()
        {
            container.RegisterPerRequestSingleton<IFoo, Foo>();
        
            var e = Assert.Throws<TinyIoCResolutionException>(() => {
                container.Resolve<IFoo>();
            });
            
            Assert.IsTrue(e.InnerException.Message.Contains(
                "Per request singleton cannot be resolved from global context."
            ));
        }
        
        [Test]
        public void PerRequestSingletonsCanDependOnRequestSpecificServices()
        {
            container.RegisterPerRequestSingleton<RequestBaz>();
            
            using (var ctx = ConstructRequestContext())
            {
                var foo = new Foo();
                ctx.Services.RegisterInstance<IFoo>(foo);
                
                var baz = container.Resolve<RequestBaz>();
                
                Assert.AreSame(foo, baz.foo);
            }
        }
    }
}
