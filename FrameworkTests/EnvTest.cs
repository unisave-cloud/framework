using NUnit.Framework;
using Unisave.Foundation;

namespace FrameworkTests
{
    [TestFixture]
    public class EnvTest
    {
        [Test]
        public void ItRemembersValues()
        {
            var env = new EnvStore();

            Assert.IsNull(env["FOO"]);
            
            env["FOO"] = "bar";
            
            Assert.AreEqual("bar", env["FOO"]);
        }

        [Test]
        public void ItParsesText()
        {
            string source = @"
                FOO=bar
                BAZ=42
                APP = lorem ipsum
                # comment = nope
                HASH = # asd
            ";

            var env = EnvStore.Parse(source);
            
            Assert.AreEqual("bar", env["FOO"]);
            Assert.AreEqual("42", env["BAZ"]);
            Assert.AreEqual("lorem ipsum", env["APP"]);
            Assert.AreEqual(null, env["# comment"]);
            Assert.AreEqual("# asd", env["HASH"]);
        }

        [Test]
        public void ItParsesNull()
        {
            Assert.DoesNotThrow(() => {
                EnvStore.Parse(null);
            });
        }

        [Test]
        public void ItLoadsIntegers()
        {
            string source = @"
                BAZ=42
                BAR=059
                FOO=asd
            ";

            var env = EnvStore.Parse(source);
            
            Assert.AreEqual(42, env.GetInt("BAZ"));
            Assert.AreEqual(59, env.GetInt("BAR"));
            Assert.AreEqual(-1, env.GetInt("FOO", -1));
        }
    }
}