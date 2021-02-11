using System;
using NUnit.Framework;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace FrameworkTests.Serialization.Security
{
    [TestFixture]
    public class InsecureDeserializationTest
    {
        [Test]
        public void DeserializingAsObjectWillThrow()
        {
            Assert.Throws<InsecureDeserializationException>(() => {
                Serializer.FromJsonString<object>("{}");
            });
            
            Assert.Throws<InsecureDeserializationException>(() => {
                Serializer.FromJsonString<dynamic>("{}");
            });
        }

        [Test]
        public void SafetyMeasureCanBeDisabled()
        {
            var context = default(DeserializationContext);
            context.suppressInsecureDeserializationException = true;
            
            Assert.DoesNotThrow(() => {
                Serializer.FromJsonString<object>("{}", context);
            });
            
            Assert.DoesNotThrow(() => {
                Serializer.FromJsonString<dynamic>("{}", context);
            });
        }
    }
}