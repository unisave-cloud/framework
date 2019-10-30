using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.TestingUtils
{
    /// <summary>
    /// Unisave assert class
    /// </summary>
    public static class UAssert
    {
        /// <summary>
        /// Asserts that two objects are the same when serialized
        /// </summary>
        public static void AreJsonEqual(object expected, object actual)
        {
            Assert.AreEqual(
                Serializer.ToJson(expected).ToString(),
                Serializer.ToJson(actual).ToString()
            );
        }
    }
}