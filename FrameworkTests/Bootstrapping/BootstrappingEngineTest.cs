using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TinyIoC;
using Unisave.Bootstrapping;
using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Foundation;
using Unisave.Runtime;
using UnityEngine;

namespace FrameworkTests.Bootstrapping
{
    [TestFixture]
    public class BootstrappingEngineTest
    {
        private IContainer container = new TinyIoCAdapter(new TinyIoCContainer());
        
        private static List<Type> executionLog = new List<Type>();

        #region "Bootstrappers"

        private abstract class TestingBootstrapperBase : IBootstrapper
        {
            public virtual void Main()
            {
                executionLog.Add(this.GetType());
            }
        }
        
        private class SimpleBootstrapper : TestingBootstrapperBase { }
    
        #endregion

        [SetUp]
        public void SetUp()
        {
            executionLog.Clear();
        }

        void AssertExecutionLog(Type[] expectedLog)
        {
            string expected = string.Join(
                "\n", expectedLog.Select(t => t.FullName)
            );
            string actual = string.Join(
                "\n", executionLog.Select(t => t.FullName)
            );
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ItRunsSimpleBootstrapper()
        {
            var engine = new BootstrappingEngine(container, new[] {
                typeof(SimpleBootstrapper)
            });

            engine.Run();
            
            AssertExecutionLog(new [] {
                typeof(SimpleBootstrapper)
            });
        }
        
        [Test]
        public void ItFiltersOutNonBootstrapperTypes()
        {
            var engine = new BootstrappingEngine(container, new[] {
                typeof(BootstrappingEngineTest), // unrelated class
                typeof(Entity), // abstract class,
                typeof(Entrypoint), // static class
                typeof(List<>), // generic type
                typeof(IArango), // interface
                typeof(Vector2), // struct
                typeof(IBootstrapper), // bootstrapper interface
                typeof(TestingBootstrapperBase), // abstract bootstrapper
                
                // actual bootstrappers
                typeof(SimpleBootstrapper)
            });
            
            Assert.AreEqual(1, engine.BootstrapperTypes.Length);
            Assert.AreEqual(
                typeof(SimpleBootstrapper),
                engine.BootstrapperTypes[0]
            );
        }

        // TODO: ItProvidesBootstrapperDependencies
        // from the container via the constructor

        // TODO: ItOrdersBootstrappersByDependencies
        
        // TODO: ItOrdersBootstrappersAlphabetically

        // TODO: ItDetectsOrderingLoops
    }
}