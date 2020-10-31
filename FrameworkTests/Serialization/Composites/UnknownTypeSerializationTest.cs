using System;
using LightJson;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Composites
{
    [TestFixture]
    public class UnknownTypeSerializationTest
    {
        /*
         * Goal:
         * Restore the object during deserialization as accurate as possible.
         * 
         * Therefore proper behaviour:
         * Serialize ALL fields (even private). Don't serialize properties
         * (not directly, like for entities), but serialize their auto-generated
         * backing fields. Also auto-generated backing fields should have
         * the same name as the corresponding property.
         *
         * Edge-cases:
         * - additional fields are ignored (e.g. removed fields)
         * - missing fields will be left at the default value (e.g. added fields)
         *
         * Added complexity:
         * - introduce inheritance
         */
        
        private class CustomClassType
        {
            // should be serialized as-is
            public string publicField;
            
            // should be serialized as-is
            private string privateField;
            
            // has auto-generated backing field that should be serialized
            public string PublicGetSetProperty { get; set; }
            
            // has auto-generated backing field that should be serialized
            public string PublicGetPrivateSetProperty { get; private set; }
            
            // should not be present in the serialized JSON (no backing field)
            public string SomeProperty
            {
                get => PublicGetSetProperty;
                set => PublicGetSetProperty = value;
            }
            
            // also shouldn't be present in the JSON
            public string PrivateField => privateField;

            public CustomClassType(string a, string b)
            {
                privateField = a;
                PublicGetPrivateSetProperty = b;
            }
        }

        private class DerivedClassType : CustomClassType
        {
            // should be serialized as-is
            public string addedField;
            
            // has auto-generated backing field that should be serialized
            public string AddedGetSetProperty { get; set; }

            public DerivedClassType(string a, string b) : base (a, b) { }
        }
        
        ///////////
        // Tests //
        ///////////
        
        [Test]
        public void ItSerializesUnknownType()
        {
            var value = new DerivedClassType("a", "b") {
                publicField = "foo",
                PublicGetSetProperty = "bar",
                addedField = "baz",
                AddedGetSetProperty = "asd"
            };
            Assert.AreEqual(value.PublicGetSetProperty, value.SomeProperty);
            
            var expectedJson = new JsonObject {
                ["addedField"] = "baz",
                ["AddedGetSetProperty"] = "asd",
                ["publicField"] = "foo",
                ["privateField"] = "a",
                ["PublicGetSetProperty"] = "bar",
                ["PublicGetPrivateSetProperty"] = "b"
            };
            
            Assert.AreEqual(
                expectedJson.ToString(),
                Serializer.ToJsonString(value)
            );
        }

        [Test]
        public void ItDeserializesUnknownType()
        {
            var json = new JsonObject {
                ["privateField"] = "a",
                ["PublicGetPrivateSetProperty"] = "b",
                ["publicField"] = "foo",
                ["PublicGetSetProperty"] = "bar",
                ["addedField"] = "baz",
                ["AddedGetSetProperty"] = "asd"
            };

            var value = Serializer.FromJson<DerivedClassType>(json);
            
            Assert.IsInstanceOf<DerivedClassType>(value);
            Assert.AreEqual("a", value.PrivateField);
            Assert.AreEqual("b", value.PublicGetPrivateSetProperty);
            Assert.AreEqual("foo", value.publicField);
            Assert.AreEqual("bar", value.PublicGetSetProperty);
            Assert.AreEqual("bar", value.SomeProperty);
            Assert.AreEqual("baz", value.addedField);
            Assert.AreEqual("asd", value.AddedGetSetProperty);
        }
        
        [Test]
        public void AdditionalFieldsWillBeIgnoredDuringDeserialization()
        {
            var json = new JsonObject {
                ["privateField"] = "a",
                ["PublicGetPrivateSetProperty"] = "b",
                ["publicField"] = "foo",
                ["PublicGetSetProperty"] = "bar",
                ["addedField"] = "baz",
                ["AddedGetSetProperty"] = "asd",
                
                ["AdditionalField"] = "lorem ipsum"
            };

            var value = Serializer.FromJson<DerivedClassType>(json);
            
            Assert.IsInstanceOf<DerivedClassType>(value);
            Assert.AreEqual("a", value.PrivateField);
            Assert.AreEqual("b", value.PublicGetPrivateSetProperty);
            Assert.AreEqual("foo", value.publicField);
            Assert.AreEqual("bar", value.PublicGetSetProperty);
            Assert.AreEqual("bar", value.SomeProperty);
            Assert.AreEqual("baz", value.addedField);
            Assert.AreEqual("asd", value.AddedGetSetProperty);
        }

        [Test]
        public void MissingFieldsWillBeLeftAtDefaultDuringDeserialization()
        {
            var json = new JsonObject {
                ["privateField"] = "a",
                ["PublicGetPrivateSetProperty"] = "b",
                //["publicField"] = "foo", // missing
                ["PublicGetSetProperty"] = "bar",
                //["addedField"] = "baz", // missing
                ["AddedGetSetProperty"] = "asd"
            };

            var value = Serializer.FromJson<DerivedClassType>(json);
            
            Assert.IsInstanceOf<DerivedClassType>(value);
            
            Assert.IsNull(value.publicField); // missing
            Assert.IsNull(value.addedField); // missing
            
            Assert.AreEqual("a", value.PrivateField);
            Assert.AreEqual("b", value.PublicGetPrivateSetProperty);
            Assert.AreEqual("bar", value.PublicGetSetProperty);
            Assert.AreEqual("bar", value.SomeProperty);
            Assert.AreEqual("asd", value.AddedGetSetProperty);
        }
    }
}