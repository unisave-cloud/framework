using LightJson;
using NUnit.Framework;
using Unisave.Arango.Emulation;
using Unisave.Arango.Execution;
using Unisave.Arango.Expressions;

namespace FrameworkTests.Arango
{
    [TestFixture]
    public class ExpressionEvaluationTest
    {
        private LinqToAqlExpressionParser parser;
        private ExecutionFrame frame;
        private QueryExecutor executor;

        private ArangoInMemory arango;
        
        [SetUp]
        public void SetUp()
        {
            parser = new LinqToAqlExpressionParser();
            
            arango = new ArangoInMemory();
            executor = arango.Executor;
            
            frame = new ExecutionFrame();
        }
        
        [Test]
        public void ItEvaluatesConstants()
        {
            Assert.AreEqual(
                "5",
                parser
                    .Parse(() => 5)
                    .Evaluate(executor, frame)
                    .ToString()
            );
        }
        
        [Test]
        public void ItEvaluatesParameters()
        {
            Assert.AreEqual(
                "42",
                parser
                    .Parse((x) => x)
                    .Evaluate(executor, frame.AddVariable("x", 42))
                    .ToString()
            );

            Assert.Throws<QueryExecutionException>(() => {
                parser
                    .Parse((notX) => notX) // parameter cannot be resolved
                    .Evaluate(executor, frame.AddVariable("x", 42));
            });
        }

        [Test]
        public void ItEvaluatesArangoFunctions()
        {
            // overwrite DOCUMENT function implementation
            arango.FunctionRepository
                .Register("DOCUMENT", (args) => new JsonObject()
                    .Add("collection", args[0])
                    .Add("key", args[1])
                );
            
            Assert.AreEqual(
                "{'collection':'foo','key':'bar'}".Replace("'", "\""),
                parser
                    .Parse(() => AF.Document("foo", "bar"))
                    .Evaluate(executor, frame)
                    .ToString()
            );
        }

        [Test]
        public void ItEvaluatesBinaryOperators()
        {
            frame = frame
                .AddVariable("x2", 2)
                .AddVariable("t", true);
            
            Assert.AreEqual(
                "5".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 + 3)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "5.5".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 + 3.5)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "'2asd'".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 + "asd")
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "1".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 - 1)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "8".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 * 4)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "0.5".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 / 4)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "2".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 % 3)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "true".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 == 2)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "false".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 != 2)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "false".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 > 2)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "true".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 >= 2)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "false".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 < 2)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "true".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 <= 2)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "false".Replace("'", "\""),
                parser
                    .Parse((t) => t && false)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "true".Replace("'", "\""),
                parser
                    .Parse((t) => t || true)
                    .Evaluate(executor, frame)
                    .ToString()
            );
        }

        [Test]
        public void ItEvaluatesUnaryOperators()
        {
            frame = frame
                .AddVariable("x2", 2)
                .AddVariable("t", true);
            
            Assert.AreEqual(
                "2".Replace("'", "\""),
                parser
                    .Parse((x2) => +x2)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "-2".Replace("'", "\""),
                parser
                    .Parse((x2) => -x2)
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "false".Replace("'", "\""),
                parser
                    .Parse((t) => !t)
                    .Evaluate(executor, frame)
                    .ToString()
            );
        }

        [Test]
        public void ItEvaluatesMemberAccess()
        {
            frame = frame
                .AddVariable("o", new JsonObject()
                    .Add("foo", 42)
                )
                .AddVariable("a", new JsonArray(
                    "a", "b", "c"
                ));
            
            Assert.AreEqual(
                "42".Replace("'", "\""),
                parser
                    .Parse((o) => o["foo"])
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "'a'".Replace("'", "\""),
                parser
                    .Parse((a) => a[0])
                    .Evaluate(executor, frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "'c'".Replace("'", "\""),
                parser
                    .Parse((a) => a[-1])
                    .Evaluate(executor, frame)
                    .ToString()
            );
        }

        [Test]
        public void ItEvaluatesJsonObjectExpression()
        {
            frame = frame
                .AddVariable("x", 42);
            
            Assert.AreEqual(
                "{'foo':42,'bar':84}".Replace("'", "\""),
                parser
                    .Parse((x) => new JsonObject()
                        .Add("foo", x)
                        .Add("bar", x * 2)
                    )
                    .Evaluate(executor, frame)
                    .ToString()
            );
        }
    }
}