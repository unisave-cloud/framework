using System.Diagnostics;
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
            Assert.IsTrue(container.CanResolve<Stopwatch>());
            Assert.IsFalse(container.IsRegistered<Stopwatch>());
            
            var firstInstance = container.Resolve<Stopwatch>();
            var secondInstance = container.Resolve<Stopwatch>();
            Assert.IsNotNull(firstInstance);
            Assert.IsNotNull(secondInstance);
            Assert.AreNotSame(firstInstance, secondInstance);
        }

        [Test]
        public void ItRegistersConcreteClassesAsSingletons()
        {
            Assert.IsTrue(container.CanResolve<Stopwatch>());
            Assert.IsFalse(container.IsRegistered<Stopwatch>());
            
            container.RegisterSingleton<Stopwatch>();
            
            Assert.IsTrue(container.CanResolve<Stopwatch>());
            Assert.IsTrue(container.IsRegistered<Stopwatch>());
            
            var firstInstance = container.Resolve<Stopwatch>();
            var secondInstance = container.Resolve<Stopwatch>();
            Assert.IsNotNull(firstInstance);
            Assert.IsNotNull(secondInstance);
            Assert.AreSame(firstInstance, secondInstance);
        }

        [Test]
        public void ItRegistersConcreteClassesAsInstances()
        {
            var instance = new Stopwatch();
            
            Assert.IsTrue(container.CanResolve<Stopwatch>());
            Assert.IsFalse(container.IsRegistered<Stopwatch>());
            
            container.RegisterInstance<Stopwatch>(instance);
            
            Assert.IsTrue(container.CanResolve<Stopwatch>());
            Assert.IsTrue(container.IsRegistered<Stopwatch>());

            var resolvedInstance = container.Resolve<Stopwatch>();
            Assert.IsNotNull(resolvedInstance);
            Assert.AreSame(instance, resolvedInstance);
        }
    }
}