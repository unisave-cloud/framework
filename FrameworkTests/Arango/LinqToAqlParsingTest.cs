using System;
using LightJson;
using NUnit.Framework;
using Unisave.Arango.Expressions;

namespace FrameworkTests.Arango
{
    [TestFixture]
    public class LinqToAqlParsingTest
    {
        private class DummyClass
        {
            public int foo;

            public int Foo => foo;
        }
        
        private LinqToAqlExpressionParser parser;
        
        [SetUp]
        public void SetUp()
        {
            parser = new LinqToAqlExpressionParser();
        }
        
        [Test]
        public void ItParsesScalarConstants()
        {
            Assert.AreEqual(
                "5",
                parser.Parse(() => 5).ToAql()
            );
            
            Assert.AreEqual(
                "3.14",
                parser.Parse(() => 3.14).ToAql()
            );
            
            Assert.AreEqual(
                "true",
                parser.Parse(() => true).ToAql()
            );
            
            Assert.AreEqual(
                "\"foo\"",
                parser.Parse(() => "foo").ToAql()
            );

            Assert.AreEqual(
                "null",
                parser.Parse(() => (JsonObject) null).ToAql()
            );
        }
        
        [Test]
        public void ItParsesParameters()
        {
            Assert.AreEqual(
                "x",
                parser.Parse((x) => x).ToAql()
            );
            
            Assert.AreEqual(
                "foo",
                parser.Parse((x, foo) => foo).ToAql()
            );
        }

        [Test]
        public void ItParsesTypeConversions()
        {
            // NOTE: writing down "(int)3.14" is not enough,
            // because the compiler automatically computes the constant "3"

            double pi = 3.14;
            
            Assert.AreEqual(
                "3",
                parser.Parse(() => (int)pi).ToAql()
            );
        }
        
        [Test]
        public void ItParsesMemberAccess()
        {
            Assert.AreEqual(
                "null",
                parser.Parse(() => JsonValue.Null).ToAql()
            );
            
            var dummy = new DummyClass { foo = 42 };
            Assert.AreEqual(
                "42",
                parser.Parse(() => dummy.foo).ToAql()
            );
            Assert.AreEqual(
                "42",
                parser.Parse(() => dummy.Foo).ToAql()
            );
        }

        [Test]
        public void ItParsesIndexerMemberAccess()
        {
            Assert.AreEqual(
                "foo.bar",
                parser.Parse((foo) => foo["bar"]).ToAql()
            );
            
            Assert.AreEqual(
                "foo.bar.baz",
                parser.Parse((foo) => foo["bar"]["baz"]).ToAql()
            );
            
            Assert.AreEqual(
                "foo[\"2x\"]",
                parser.Parse((foo) => foo["2x"]).ToAql()
            );
            
            Assert.AreEqual(
                "foo[2]",
                parser.Parse((foo) => foo[2]).ToAql()
            );
            
            Assert.AreEqual(
                "foo[null]",
                parser.Parse((foo) => foo[null]).ToAql()
            );
            
            Assert.AreEqual(
                "foo[3]",
                parser.Parse((foo) => foo[1 + 2]).ToAql()
            );
            
            Assert.AreEqual(
                "foo[(-x)]",
                parser.Parse((foo, x) => foo[-x]).ToAql()
            );
        }

        [Test]
        public void ItParsesExternalConstants()
        {
            // let's have some external constant
            int x = 42;
            
            // direct access (closure member access)
            Assert.AreEqual(
                "42",
                parser.Parse(() => x).ToAql()
            );
            
            // indirect via method call (method gets executed once)
            Assert.AreEqual(
                "42",
                parser.Parse(() => Math.Abs(-x)).ToAql()
            );
        }
        
        [Test]
        public void NonParametrizedFunctionsGetExecuted()
        {
            Assert.AreEqual(
                "16",
                parser.Parse(() => Math.Pow(2, 4)).ToAql()
            );
        }
        
        [Test]
        public void ParametrizedFunctionsThrowException()
        {
            var e = Assert.Throws<AqlParsingException>(() => {
                parser.Parse((x) => Math.Pow(x, 4)).ToAql();
            });
            
            Assert.That(e.Message, Contains.Substring(
                "but this function cannot be translated to AQL"
            ));
        }

        [Test]
        public void ItParsesArangoFunctions()
        {
            Assert.AreEqual(
                "DOCUMENT(\"users\", \"123\")",
                parser.Parse(() => AF.Document("users", "123")).ToAql()
            );
            
            Assert.AreEqual(
                "DOCUMENT(\"users\", x.foo)",
                parser.Parse((x) => AF.Document("users", x["foo"])).ToAql()
            );
        }

        [Test]
        public void ItParsesBasicUnaryOperators()
        {
            Assert.AreEqual(
                "(-x)",
                parser.Parse((x) => -x).ToAql()
            );
            
            Assert.AreEqual(
                "x", // gets simplified by something (compiler?)
                parser.Parse((x) => +x).ToAql()
            );
            
            Assert.AreEqual(
                "(NOT x)",
                parser.Parse((x) => !x).ToAql()
            );
        }
        
        [Test]
        public void ItParsesBasicBinaryOperators()
        {
            Assert.AreEqual(
                "(x + 2)",
                parser.Parse((x) => x + 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x - 2)",
                parser.Parse((x) => x - 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x * 2)",
                parser.Parse((x) => x * 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x / 2)",
                parser.Parse((x) => x / 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x % 2)",
                parser.Parse((x) => x % 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x == 2)",
                parser.Parse((x) => x == 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x != 2)",
                parser.Parse((x) => x != 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x >= 2)",
                parser.Parse((x) => x >= 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x > 2)",
                parser.Parse((x) => x > 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x <= 2)",
                parser.Parse((x) => x <= 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x < 2)",
                parser.Parse((x) => x < 2).ToAql()
            );
            
            Assert.AreEqual(
                "(x AND false)",
                parser.Parse((x) => x && false).ToAql()
            );
            
            Assert.AreEqual(
                "(x OR true)",
                parser.Parse((x) => x || true).ToAql()
            );
            
            Assert.AreEqual(
                "(x AND false)",
                parser.Parse((x) => x & false).ToAql()
            );
            
            Assert.AreEqual(
                "(x OR true)",
                parser.Parse((x) => x | true).ToAql()
            );
        }

        [Test]
        public void ItParsesStringConcatenation()
        {
            Assert.AreEqual(
                "CONCAT(x, \"bar\")",
                parser.Parse((x) => x + "bar").ToAql()
            );
            
            Assert.AreEqual(
                "CONCAT(CONCAT(\"foo\", x), \"baz\")",
                parser.Parse((x) => "foo" + x + "baz").ToAql()
            );
            
            Assert.AreEqual(
                "CONCAT(\"foo\", CONCAT(5, x))",
                parser.Parse((x) => "foo" + (5 + (string)x)).ToAql()
            );
            
            Assert.AreEqual(
                "CONCAT(\"foo\", 5, x)",
                parser.Parse((x) => AF.Concat("foo", 5, x)).ToAql()
            );
        }

        [Test]
        public void ItParsesJsonObjectCreation()
        {
            Assert.AreEqual(
                "{'foo':'bar','bar':'baz'}".Replace("'", "\""),
                parser.Parse(() => new JsonObject()
                    .Add("foo", "bar")
                    .Add("bar", "baz")
                ).ToAql()
            );
            
            Assert.AreEqual(
                "{'foo':4}".Replace("'", "\""),
                parser.Parse(() => new JsonObject()
                    .Add("foo", 2 + 2)
                ).ToAql()
            );
        }
        
        [Test]
        public void ItParsesParametricJsonObjectCreation()
        {
            Assert.AreEqual(
                "{'x': x}".Replace("'", "\""),
                parser.Parse((x) => new JsonObject()
                    .Add("x", x)
                ).ToAql()
            );
            
            Assert.AreEqual(
                "{'x': 42, 'y': y, 'z': null}".Replace("'", "\""),
                parser.Parse((y) => new JsonObject()
                    .Add("x", 42)
                    .Add("y", y)
                    .Add("z")
                ).ToAql()
            );
            
            Assert.AreEqual(
                "{'x': x, 'x2': (x * x)}".Replace("'", "\""),
                parser.Parse((x) => new JsonObject()
                    .Add("x", x)
                    .Add("x2", x * x)
                ).ToAql()
            );
        }
        
        // TODO: parametric and non-parametric JSON array creation
        
        // TODO: ternary operator
    }
}