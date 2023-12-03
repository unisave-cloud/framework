using System.Collections.Generic;
using LightJson;
using NUnit.Framework;

namespace FrameworkTests.Testing
{
    /// <summary>
    /// Assertions about JSON data
    /// </summary>
    public static class JsonAssert
    {
        /// <summary>
        /// Tests if two JSON values are exactly equal
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreEqual(JsonValue expected, JsonValue actual)
        {
            Assert.AreEqual(expected.Type, actual.Type, "JSON types do not match");

            switch (expected.Type)
            {
                case JsonValueType.String:
                    Assert.AreEqual(expected.AsString, actual.AsString);
                    break;
                
                case JsonValueType.Boolean:
                    Assert.AreEqual(expected.AsBoolean, actual.AsBoolean);
                    break;
                
                case JsonValueType.Null:
                    // if both null, then both null
                    break;
                
                case JsonValueType.Number:
                    Assert.AreEqual(expected.AsNumber, actual.AsNumber);
                    break;
                
                case JsonValueType.Array:
                    ArraysAreEqual(expected, actual);
                    break;
                
                case JsonValueType.Object:
                    ObjectsAreEqual(expected, actual);
                    break;
            }
        }
        
        /// <summary>
        /// Tests that two JSON objects are equal
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        private static void ObjectsAreEqual(JsonObject expected, JsonObject actual)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            
            Assert.AreEqual(
                expected.Count, actual.Count,
                "JSON Objects are of different size"
            );
            
            ObjectIsSubset(expected, actual);
        }

        /// <summary>
        /// Tests that two JSON arrays are value-equal
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void ArraysAreEqual(JsonArray expected, JsonArray actual)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            
            Assert.AreEqual(
                expected.Count, actual.Count,
                "The two JSON arrays have different sizes"
            );

            for (int i = 0; i < expected.Count; i++)
                AreEqual(expected[i], actual[i]);
        }

        /// <summary>
        /// Tests that the expected object is a subset of the given object
        /// (in terms of field equality)
        /// </summary>
        public static void ObjectIsSubset(JsonObject expected, JsonObject actual)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            
            foreach (KeyValuePair<string, JsonValue> pair in expected)
            {
                if (!actual.ContainsKey(pair.Key))
                    Assert.Fail("Missing key " + pair.Key);
                
                AreEqual(expected[pair.Key], actual[pair.Key]);
            }
        }
    }
}