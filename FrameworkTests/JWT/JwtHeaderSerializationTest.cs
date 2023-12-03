using FrameworkTests.Testing;
using JWT;
using JWT.Builder;
using LightJson;
using LightJson.Serialization;
using NUnit.Framework;
using Unisave.JWT;
using Unisave.Serialization;

namespace FrameworkTests.JWT
{
    [TestFixture]
    public class JwtHeaderSerializationTest
    {
        private readonly JwtHeader headerInstance = new JwtHeader {
            Type = "type",
            ContentType = "content-type",
            Algorithm = "algorithm",
            KeyId = "key-id",
            X5u = "x5u",
            X5c = new string[] { "x5c1", "x5c2", "x5c3" },
            X5t = "x5t"
        };

        private readonly JsonObject headerJson = new JsonObject {
            ["typ"] = "type",
            ["cty"] = "content-type",
            ["alg"] = "algorithm",
            ["kid"] = "key-id",
            ["x5u"] = "x5u",
            ["x5c"] = new JsonArray { "x5c1", "x5c2", "x5c3" },
            ["x5t"] = "x5t"
        };
        
        [Test]
        public void ItCanSerializeHeader()
        {
            var serializer = new LightJsonSerializer();

            string jsonString = serializer.Serialize(headerInstance);
            JsonObject json = JsonReader.Parse(jsonString).AsJsonObject;
            
            JsonAssert.AreEqual(headerJson, json);
        }
        
        [Test]
        public void ItCanDeserializeHeader()
        {
            var serializer = new LightJsonSerializer();

            var instance = serializer.Deserialize<JwtHeader>(
                headerJson.ToString()
            );
            
            // compare instances by doing simple serialization and equality check
            JsonValue expected = Serializer.ToJson(headerInstance);
            JsonValue actual = Serializer.ToJson(instance);
            JsonAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ItSerializesNullsAsMissingKeys()
        {
            var serializer = new LightJsonSerializer();
            
            string jsonString = serializer.Serialize(new JwtHeader {
                Algorithm = "algorithm",
                KeyId = "key-id"
            });
            JsonObject json = JsonReader.Parse(jsonString).AsJsonObject;
            
            JsonAssert.AreEqual(
                new JsonObject {
                    ["alg"] = "algorithm",
                    ["kid"] = "key-id"
                },
                json
            );
        }
    }
}