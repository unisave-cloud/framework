using LightJson;
using NUnit.Framework;
using Unisave.Arango.Database;
using Unisave.Arango.Expressions;

namespace FrameworkTests.Arango
{
    [TestFixture]
    public class ExpressionEvaluationTest
    {
        private LinqToAqlExpressionParser parser;
        private ExecutionFrame frame;
        private AqlFunctionRepository functionRepository;
        
        [SetUp]
        public void SetUp()
        {
            functionRepository = new AqlFunctionRepository();
            functionRepository.RegisterFunctions();
            
            parser = new LinqToAqlExpressionParser();
            frame = new ExecutionFrame(functionRepository);
        }
        
        [Test]
        public void ItEvaluatesConstants()
        {
            Assert.AreEqual(
                "5",
                parser
                    .Parse(() => 5)
                    .EvaluateInFrame(frame)
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
                    .EvaluateInFrame(frame.AddVariable("x", 42))
                    .ToString()
            );

            Assert.Throws<QueryExecutionException>(() => {
                parser
                    .Parse((notX) => notX) // parameter cannot be resolved
                    .EvaluateInFrame(frame.AddVariable("x", 42));
            });
        }

        [Test]
        public void ItEvaluatesArangoFunctions()
        {
            functionRepository.Register("DOCUMENT", (args) => new JsonObject()
                .Add("collection", args[0])
                .Add("key", args[1])
            );
            
            Assert.AreEqual(
                "{'collection':'foo','key':'bar'}".Replace("'", "\""),
                parser
                    .Parse(() => AF.Document("foo", "bar"))
                    .EvaluateInFrame(frame)
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
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "5.5".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 + 3.5)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "'2asd'".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 + "asd")
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "1".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 - 1)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "8".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 * 4)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "0.5".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 / 4)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "2".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 % 3)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "true".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 == 2)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "false".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 != 2)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "false".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 > 2)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "true".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 >= 2)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "false".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 < 2)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "true".Replace("'", "\""),
                parser
                    .Parse((x2) => x2 <= 2)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "false".Replace("'", "\""),
                parser
                    .Parse((t) => t && false)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "true".Replace("'", "\""),
                parser
                    .Parse((t) => t || true)
                    .EvaluateInFrame(frame)
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
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "-2".Replace("'", "\""),
                parser
                    .Parse((x2) => -x2)
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "false".Replace("'", "\""),
                parser
                    .Parse((t) => !t)
                    .EvaluateInFrame(frame)
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
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "'a'".Replace("'", "\""),
                parser
                    .Parse((a) => a[0])
                    .EvaluateInFrame(frame)
                    .ToString()
            );
            
            Assert.AreEqual(
                "'c'".Replace("'", "\""),
                parser
                    .Parse((a) => a[-1])
                    .EvaluateInFrame(frame)
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
                    .EvaluateInFrame(frame)
                    .ToString()
            );
        }
    }
}