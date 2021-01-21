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

        [Test]
        public void ItSerializesObjectsAsJsonValue()
        {
            /*
             * NOTE:
             * it tests whether it converts types correctly
             * (because JsonObject to JsonValue is not a cast but a conversion)
             */
            
            Assert.AreEqual(
                "{}",
                Serializer.ToJsonString(
                    new JsonValue(new JsonObject())
                )
            );

            JsonValue json = Serializer.FromJsonString<JsonValue>("{}");
            Assert.AreEqual("{}", json.ToString());
        }

        [Test]
        public void ItSerializesNullAsJsonValue()
        {
            Assert.AreEqual(
                "null",
                Serializer.ToJsonString((object)null)
            );
            
            Assert.AreEqual(
                "null",
                Serializer.ToJsonString<JsonValue>((object)null)
            );
            
            Assert.AreEqual(
                "null",
                Serializer.ToJsonString(JsonValue.Null)
            );
            
            Assert.AreEqual(
                "null",
                Serializer.ToJsonString<object>(JsonValue.Null)
            );
            
            JsonValue json = Serializer.FromJsonString<JsonValue>("null");
            Assert.IsTrue(json.IsNull);
            
            JsonObject jsonObj = Serializer.FromJsonString<JsonObject>("null");
            Assert.IsNull(jsonObj);
            
            JsonArray jsonArr = Serializer.FromJsonString<JsonArray>("null");
            Assert.IsNull(jsonArr);
            
            object something = Serializer.FromJsonString<object>("null");
            Assert.IsNull(something);
        }
    }
}