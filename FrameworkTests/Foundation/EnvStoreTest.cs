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
    }
}