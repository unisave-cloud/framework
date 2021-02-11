using NUnit.Framework;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace FrameworkTests.Serialization.Primitives
{
    [TestFixture]
    public class BinarySerializerTest
    {
        [Test]
        public void ItSerializesBinaryData()
        {
            Assert.AreEqual(
                "\"base64:AQIDBAU=\"",
                Serializer.ToJsonString(new byte[] { 1, 2, 3, 4, 5 })
            );
        }

        [Test]
        public void ItDeserializesBinaryData()
        {
            Assert.AreEqual(
                new byte[] { 1, 2, 3, 4, 5 },
                Serializer.FromJsonString<byte[]>("\"base64:AQIDBAU=\"")
            );
        }
    
        [Test]
        public void ItDeserializesArrayFormat()
        {
            Assert.AreEqual(
                new byte[] { 1, 2, 3, 4, 5 },
                Serializer.FromJsonString<byte[]>("[1,2,3,4,5]")
            );
        }

        [Test]
        public void ArrayFormatSerializationCanBeForced()
        {
            var context = default(SerializationContext);
            context.serializeBinaryAsByteArray = true;
            
            Assert.AreEqual(
                "[1,2,3,4,5]",
                Serializer.ToJsonString(new byte[] { 1, 2, 3, 4, 5 }, context)
            );
        }
    }
}