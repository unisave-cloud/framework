using NUnit.Framework;
using Unisave.Serialization.Context;

namespace FrameworkTests.Serialization
{
    [TestFixture]
    public class SerializationContextTest
    {
        [Test]
        public void DefaultSerializationContextHasProperValues()
        {
            var context = default(SerializationContext);
            
            Assert.AreEqual(
                SerializationReason.Transfer,
                context.reason
            );
            
            Assert.AreEqual(
                TypeSerialization.DuringPolymorphism,
                context.typeSerialization
            );
            
            Assert.AreEqual(
                SecurityDomainCrossing.NoCrossing,
                context.securityDomainCrossing
            );
        }

        [Test]
        public void DefaultDeserializationContextHasProperValues()
        {
            var context = default(DeserializationContext);
            
            Assert.AreEqual(
                SerializationReason.Transfer,
                context.reason
            );
            
            Assert.AreEqual(
                SecurityDomainCrossing.NoCrossing,
                context.securityDomainCrossing
            );
        }
    }
}