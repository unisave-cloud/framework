using System;
using System.Linq.Expressions;
using LightJson;
using NUnit.Framework;
using Unisave;
using Unisave.Arango.Expressions;
using Unisave.Entities;
using Unisave.Serialization;
using UnityEngine;

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
        public void MemberAccessOnParametersCannotBeDone()
        {
            var e = Assert.Throws<AqlParsingException>(() => {
                parser.Parse((x) => x.AsString).ToAql();
            });
            Assert.That(e.Message, Contains.Substring(
                "AQL cannot represent the following member access operation"
            ));
            
            e = Assert.Throws<AqlParsingException>(() => {
                parser.Parse((x) => x["foo"].IsDateTime).ToAql();
            });
            Assert.That(e.Message, Contains.Substring(
                "AQL cannot represent the following member access operation"
            ));
        }

        // used in the following test
        private static int MyFunction(Vector2 v)
            => (int) v.sqrMagnitude;

        [Test]
        public void NonJsonValuesInSimplifiablePartsCanAppear()
        {
            Func<Vector2, float> myLambda = (v) => v.sqrMagnitude;
            
            Assert.AreEqual(
                "(x + 1)",
                parser.Parse((x) => x + myLambda(Vector2.down)).ToAql()
            );
            
            Assert.AreEqual(
                "(x + 1)",
                parser.Parse((x) => x + MyFunction(Vector2.down)).ToAql()
            );
        }

        [Test]
        public void MethodAccessOnParametersCannotBeDone()
        {
            var e = Assert.Throws<AqlParsingException>(() => {
                parser.Parse((x) => x.ToString()).ToAql();
            });
            Assert.That(e.Message, Contains.Substring(
                "Expression uses parameters in method"
            ));
            
            e = Assert.Throws<AqlParsingException>(() => {
                parser.Parse((x) => x["foo"].ToString()).ToAql();
            });
            Assert.That(e.Message, Contains.Substring(
                "Expression uses parameters in method"
            ));
        }

        // used in the test below
        public class PlayerEntity : Entity
        {
            public string Name { get; set; }
        }

        [Test]
        public void MemberAccessCanBeDoneOnEntities()
        {
            Assert.AreEqual(
                "player.Name",
                parser.ParseEntity<PlayerEntity>(
                    player => player.Name
                ).ToAql()
            );
            
            Assert.AreEqual(
                "player.Name",
                parser.ParseEntity<PlayerEntity>(
                    player => player["Name"]
                ).ToAql()
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
                "but this method cannot be translated to AQL"
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
        public void ItParsesDateTimeConstants()
        {
            // This test checks whether entities can be filtered by attributes.

            DateTime time = DateTime.UtcNow;
            string timeString = Serializer.ToJson(time);
            Expression<Func<Entity, bool>> expression
                = (entity) => entity.UpdatedAt <= time;
            
            Assert.AreEqual(
                $"(entity.UpdatedAt <= \"{timeString}\")",
                parser.ParseExpression(expression.Body).ToAql()
            );
        }

        // this class is used in the test below
        private class EntityWithEnumAttribute : Entity
        {
            public ConsoleColor EnumAttribute { get; set; }
        }
        
        [Test]
        public void ItParsesEnumConstants()
        {
            // This test checks whether entities can be filtered by attributes.

            Expression<Func<EntityWithEnumAttribute, bool>> expression
                = (entity) => entity.EnumAttribute == ConsoleColor.Cyan;

            int code = (int)ConsoleColor.Cyan;
            Assert.AreEqual(
                $"(entity.EnumAttribute == {code})",
                parser.ParseExpression(expression.Body).ToAql()
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
        
        [Test]
        public void ItParsesJsonArrayCreation()
        {
            Assert.AreEqual(
                "[1,2,3,'foo','bar']".Replace("'", "\""),
                parser.Parse(() => new JsonArray(1, 2, 3)
                    .Add("foo")
                    .Add("bar")
                ).ToAql()
            );
            
            Assert.AreEqual(
                "[null,null,null,'foo','bar']".Replace("'", "\""),
                parser.Parse(() => new JsonArray(new JsonValue[3])
                    .Add("foo")
                    .Add("bar")
                ).ToAql()
            );
        }
        
        [Test]
        public void ItParsesParametricJsonArrayCreation()
        {
            Assert.AreEqual(
                "[x, 'bar']".Replace("'", "\""),
                parser.Parse((x) => new JsonArray()
                    .Add(x)
                    .Add("bar")
                ).ToAql()
            );
            
            Assert.AreEqual(
                "[1, 2, x, 'foo', 'bar']".Replace("'", "\""),
                parser.Parse((x) => new JsonArray(1, 2, x)
                    .Add("foo")
                    .Add("bar")
                ).ToAql()
            );
            
            Assert.AreEqual(
                "[null, null, null, x, 'bar']".Replace("'", "\""),
                parser.Parse((x) => new JsonArray(new JsonValue[3])
                    .Add(x)
                    .Add("bar")
                ).ToAql()
            );
        }
        
        // TODO: query expressions ... LENGTH(FOR ... RETURN ...)
        
        // TODO: ternary operator
    }
}