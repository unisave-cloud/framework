using LightJson;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Primitives
{
    [TestFixture]
    public class LightJsonSerializationTest
    {
        [Test]
        public void ItSerializesJsonObjects()
        {
            var subject = new JsonObject {
                ["foo"] = 42,
                ["bar"] = "Hello!"
            };

            var json = Serializer.ToJsonString(subject);
            
            Assert.AreEqual(subject.ToString(), json);

            var deserialized = Serializer.FromJsonString<JsonObject>(json);
            
            Assert.AreEqual(subject.ToString(), deserialized.ToString());
        }
        
        [Test]
        public void ItSerializesJsonArrays()
        {
            var subject = new JsonArray { 42, "Hello!" };

            var json = Serializer.ToJsonString(subject);
            
            Assert.AreEqual(subject.ToString(), json);

            var deserialized = Serializer.FromJsonString<JsonArray>(json);
            
            Assert.AreEqual(subject.ToString(), deserialized.ToString());
        }
        
        [Test]
        public void ItSerializesJsonValues()
        {
            var subject = new JsonValue("Lorem ipsum!");

            var json = Serializer.ToJsonString(subject);
            
            Assert.AreEqual(subject.ToString(), json);

            var deserialized = Serializer.FromJsonString<JsonValue>(json);
            
            Assert.AreEqual(subject.ToString(), deserialized.ToString());
        }
    }
}