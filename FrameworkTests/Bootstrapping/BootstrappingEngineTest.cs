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
        private IContainer container = new TinyIoCAdapter();
        
        private static List<Type> executionLog = new List<Type>();

        #region "Bootstrappers"

        private abstract class TestingBootstrapperBase : Bootstrapper
        {
            public override void Main()
            {
                executionLog.Add(this.GetType());
            }
        }
        
        private class SimpleBootstrapper : TestingBootstrapperBase { }
        
        private class ZzzBootstrapper : TestingBootstrapperBase { }

        private class SimpleBeforeBootstrapper : TestingBootstrapperBase
        {
            public override Type[] RunBefore => new[] {
                typeof(SimpleBootstrapper)
            };
        }
        
        private class CycleBootstrapper : TestingBootstrapperBase
        {
            public override Type[] RunAfter => new[] {
                typeof(SimpleBootstrapper)
            };
            
            public override Type[] RunBefore => new[] {
                typeof(SimpleBeforeBootstrapper)
            };
        }

        private class FrameworkBootstrapper : TestingBootstrapperBase
        {
            public override int StageNumber => BootstrappingStage.Framework;
        }
        
        private class FrameworkAfterBootstrapper : TestingBootstrapperBase
        {
            public override int StageNumber => BootstrappingStage.Framework;

            public override Type[] RunAfter => new[] {
                typeof(FrameworkBootstrapper)
            };
        }
    
        #endregion
        
        private static BackendTypes Shuffle(BackendTypes types)
        {
            var rnd = new Random();
            return new BackendTypes(
                types
                    .Select(x => (x, rnd.Next()))
                    .OrderBy(tuple => tuple.Item2)
                    .Select(tuple => tuple.Item1)
            );
        }

        [SetUp]
        public void SetUp()
        {
            executionLog.Clear();
        }

        void AssertExecutionLog(Type[] expectedLog)
        {
            string expected = string.Join(
                "\n", expectedLog.Select(t => t.Name)
            );
            string actual = string.Join(
                "\n", executionLog.Select(t => t.Name)
            );
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ItRunsSimpleBootstrapper()
        {
            var engine = new BootstrappingEngine(container, new BackendTypes {
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
            var engine = new BootstrappingEngine(container, new BackendTypes {
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

        [Test]
        public void ItOrdersBootstrappersByStages()
        {
            for (int i = 0; i < 20; i++)
            {
                SetUp();
            
                var engine = new BootstrappingEngine(container, Shuffle(new BackendTypes {
                    typeof(SimpleBootstrapper),
                    typeof(FrameworkBootstrapper),
                }));

                engine.Run();
                
                AssertExecutionLog(new [] {
                    typeof(FrameworkBootstrapper),
                    typeof(SimpleBootstrapper)
                });
            }
        }
        
        [Test]
        public void ItOrdersBootstrappersByDependencies()
        {
            for (int i = 0; i < 20; i++)
            {
                SetUp();

                var engine = new BootstrappingEngine(container, new BackendTypes {
                    typeof(SimpleBootstrapper),
                    typeof(SimpleBeforeBootstrapper)
                });

                engine.Run();

                AssertExecutionLog(new[] {
                    typeof(SimpleBeforeBootstrapper),
                    typeof(SimpleBootstrapper)
                });
            }
            
            for (int i = 0; i < 20; i++)
            {
                SetUp();

                var engine = new BootstrappingEngine(container, new BackendTypes {
                    typeof(FrameworkBootstrapper),
                    typeof(FrameworkAfterBootstrapper)
                });

                engine.Run();

                AssertExecutionLog(new[] {
                    typeof(FrameworkBootstrapper),
                    typeof(FrameworkAfterBootstrapper)
                });
            }
        }

        [Test]
        public void ItOrdersBootstrappersAlphabetically()
        {
            for (int i = 0; i < 20; i++)
            {
                SetUp();

                var engine = new BootstrappingEngine(container, new BackendTypes {
                    typeof(SimpleBootstrapper),
                    typeof(ZzzBootstrapper)
                });

                engine.Run();

                AssertExecutionLog(new[] {
                    typeof(SimpleBootstrapper),
                    typeof(ZzzBootstrapper)
                });
            }
        }

        [Test]
        public void ItOrdersBootstrappersCompletely()
        {
            for (int i = 0; i < 20; i++)
            {
                SetUp();

                var engine = new BootstrappingEngine(container, new BackendTypes {
                    typeof(SimpleBootstrapper),
                    typeof(ZzzBootstrapper),
                    typeof(SimpleBeforeBootstrapper),
                    typeof(FrameworkBootstrapper),
                    typeof(FrameworkAfterBootstrapper)
                });

                engine.Run();

                AssertExecutionLog(new[] {
                    // framework stage
                    // plane 0
                    typeof(FrameworkBootstrapper),
                    // plane 1
                    typeof(FrameworkAfterBootstrapper),
                    
                    // default stage
                    // plane 0
                    typeof(SimpleBeforeBootstrapper),
                    typeof(ZzzBootstrapper),
                    // plane 1
                    typeof(SimpleBootstrapper)
                });
            }
        }

        [Test]
        public void ItDetectsOrderingLoops()
        {
            var engine = new BootstrappingEngine(container, new BackendTypes {
                typeof(SimpleBootstrapper),
                typeof(ZzzBootstrapper),
                typeof(SimpleBeforeBootstrapper),
                typeof(FrameworkBootstrapper),
                typeof(FrameworkAfterBootstrapper),
                
                typeof(CycleBootstrapper) // culprit
            });

            var e = Assert.Throws<BootstrappingException>(() => {
                engine.Run();
            });
            
            Assert.IsTrue(e.Message.Contains(nameof(SimpleBootstrapper)));
            Assert.IsTrue(e.Message.Contains(nameof(SimpleBeforeBootstrapper)));
            Assert.IsTrue(e.Message.Contains(nameof(CycleBootstrapper)));
        }

        [Test]
        public void ItFailsOnWrongDependencyType()
        {
            var engine = new BootstrappingEngine(container, new BackendTypes {
                typeof(SimpleBootstrapper),
                typeof(ZzzBootstrapper),
                typeof(SimpleBeforeBootstrapper),
                
                typeof(FrameworkAfterBootstrapper) // culprit
            });

            var e = Assert.Throws<BootstrappingException>(() => {
                engine.Run();
            });
            
            Assert.IsTrue(e.Message.Contains(nameof(FrameworkAfterBootstrapper)));
            Assert.IsTrue(e.Message.Contains(nameof(FrameworkBootstrapper)));
        }
    }
}