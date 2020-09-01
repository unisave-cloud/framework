using NUnit.Framework;
using Unisave.Utils;

namespace FrameworkTests.Utils
{
    [TestFixture]
    public class HashTest
    {
        [Test]
        public void ItMakesHashes()
        {
            string value = "password";
            string hash = Hash.Make(value);
            
            Assert.AreNotEqual(value, hash);
        }

        [Test]
        public void ItChecksValues()
        {
            string hash = Hash.Make("password");
            
            Assert.IsTrue(Hash.Check("password", hash));
            Assert.IsFalse(Hash.Check("lorem ipsum", hash));
        }
    }
}