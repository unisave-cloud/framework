using System;
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
    }
}