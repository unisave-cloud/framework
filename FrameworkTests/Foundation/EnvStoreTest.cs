using NUnit.Framework;
using Unisave.Foundation;

namespace FrameworkTests.Foundation
{
    [TestFixture]
    public class EnvStoreTest
    {
        [Test]
        public void BasicOperationsWork()
        {
            var env = new EnvStore();

            env["FOO"] = "bar";
            env["BAZ"] = "42";
            
            Assert.AreEqual("bar", env.GetString("FOO"));
            Assert.AreEqual(42, env.GetInt("BAZ"));
            Assert.AreEqual("42", env.GetString("BAZ"));
            
            Assert.AreEqual("nope", env.GetString("NOPE", "nope"));
            Assert.AreEqual(null, env.GetString("NOPE"));
            
            Assert.AreEqual(0, env.GetInt("NOPE"));
            Assert.AreEqual(66, env.GetInt("NOPE", 66));
            
            Assert.IsTrue(env.Has("FOO"));
            Assert.IsTrue(env.Has("BAZ"));
            
            Assert.IsFalse(env.Has("NOPE"));
            Assert.IsFalse(env.Has("foo"));
            Assert.IsFalse(env.Has("baz"));
        }

        [Test]
        public void BooleanValuesCanBeRead()
        {
            var env = new EnvStore();

            env["TRUE_1"] = "true";
            env["TRUE_2"] = "True";
            env["TRUE_3"] = "TRUE";
            env["TRUE_4"] = "  TRue  ";
            env["TRUE_5"] = "1";
            env["TRUE_6"] = "  1 ";
            
            env["FALSE_1"] = "false";
            env["FALSE_2"] = "False";
            env["FALSE_3"] = "FALSE";
            env["FALSE_4"] = "  FALse  ";
            env["FALSE_5"] = "0";
            env["FALSE_6"] = "  0  ";
            
            env["MALFORMED_FALSE_1"] = "lorem ipsum";
            env["MALFORMED_FALSE_2"] = "123";
            env["MALFORMED_FALSE_3"] = "";
            env["MALFORMED_FALSE_4"] = "null";
            
            Assert.IsTrue(env.GetBool("TRUE_1"));
            Assert.IsTrue(env.GetBool("TRUE_2"));
            Assert.IsTrue(env.GetBool("TRUE_3"));
            Assert.IsTrue(env.GetBool("TRUE_4"));
            Assert.IsTrue(env.GetBool("TRUE_5"));
            Assert.IsTrue(env.GetBool("TRUE_6"));
            
            Assert.IsFalse(env.GetBool("FALSE_1"));
            Assert.IsFalse(env.GetBool("FALSE_2"));
            Assert.IsFalse(env.GetBool("FALSE_3"));
            Assert.IsFalse(env.GetBool("FALSE_4"));
            Assert.IsFalse(env.GetBool("FALSE_5"));
            Assert.IsFalse(env.GetBool("FALSE_6"));
            
            Assert.IsFalse(env.GetBool("MALFORMED_FALSE_1"));
            Assert.IsFalse(env.GetBool("MALFORMED_FALSE_2"));
            Assert.IsFalse(env.GetBool("MALFORMED_FALSE_3"));
            Assert.IsFalse(env.GetBool("MALFORMED_FALSE_4"));
        }

        [Test]
        public void BooleanValuesCanBeSet()
        {
            var env = new EnvStore();
            
            env.Set("FOO", true);
            env.Set("BAR", false);
            
            Assert.AreEqual("true", env["FOO"]);
            Assert.AreEqual("false", env["BAR"]);
        }
    }
}