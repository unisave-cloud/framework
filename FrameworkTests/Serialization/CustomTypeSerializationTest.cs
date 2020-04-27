using System;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization
{
    [TestFixture]
    public class CustomTypeSerializationTest
    {
        private class CustomClassType
        {
            public string publicField;
            public string PublicGetSetProperty { get; set; }
            private string privateField;
            
            public string PublicGetPrivateSetProperty { get; private set; }

            public CustomClassType(string a, string b)
            {
                privateField = a;
                PublicGetPrivateSetProperty = b;
            }
        }

        private class DerivedClassType : CustomClassType
        {
            public string addedField;

            public DerivedClassType(string a, string b) : base (a, b) { }
        }
        
        [Test]
        public void ItSerializesCustomTypes()
        {
            string serialized = Serializer.ToJson(new DerivedClassType ("a", "b") {
                publicField = "foo",
                PublicGetSetProperty = "bar",
                addedField = "baz"
            }).ToString();
            
            Console.WriteLine(serialized);

            string reserialized = Serializer.ToJson(
                Serializer.FromJsonString<DerivedClassType>(serialized)
            ).ToString();

            Assert.AreEqual(serialized, reserialized);
        }
    }
}