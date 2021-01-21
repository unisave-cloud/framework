using LightJson;
using NUnit.Framework;
using Unisave.Serialization;
using Unisave.Serialization.Context;
using UnityEngine;

namespace FrameworkTests.Serialization.Composites
{
    [TestFixture]
    public class TypeSerializationTest
    {
        public class MyClass
        {
            public string foo = "foo";
        }
        
        #region "Serialization"

        [Test]
        public void TestNeverSerializing()
        {
            var context = default(SerializationContext);
            context.typeSerialization = TypeSerialization.Never;
            
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = "foo"
                }.ToString(),
                Serializer.ToJsonString<MyClass>(new MyClass(), context)
            );
            
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = "foo"
                }.ToString(),
                Serializer.ToJsonString<object>(new MyClass(), context)
            );
        }
        
        [Test]
        public void TestDuringPolymorphismSerializing()
        {
            var context = default(SerializationContext);
            context.typeSerialization = TypeSerialization.DuringPolymorphism;
            
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = "foo"
                }.ToString(),
                Serializer.ToJsonString<MyClass>(new MyClass(), context)
            );
            
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = "foo",
                    ["$type"] = typeof(MyClass).FullName
                }.ToString(),
                Serializer.ToJsonString<object>(new MyClass(), context)
            );
        }
        
        [Test]
        public void TestAlwaysSerializing()
        {
            var context = default(SerializationContext);
            context.typeSerialization = TypeSerialization.Always;
            
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = "foo",
                    ["$type"] = typeof(MyClass).FullName
                }.ToString(),
                Serializer.ToJsonString<MyClass>(new MyClass(), context)
            );
            
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = "foo",
                    ["$type"] = typeof(MyClass).FullName
                }.ToString(),
                Serializer.ToJsonString<object>(new MyClass(), context)
            );
        }
        
        #endregion
        
        #region "Deserialization"

        [Test]
        public void DeserializationWithoutTypeUsesTypeContext()
        {
            MyClass value = Serializer.FromJson<MyClass>(
                new JsonObject {
                    ["foo"] = "foo"
                }
            );
            Assert.IsInstanceOf<MyClass>(value);
            Assert.AreEqual("foo", value.foo);
        }
        
        [Test]
        public void DeserializationWithMatchingTypeWorks()
        {
            MyClass value = Serializer.FromJson<MyClass>(
                new JsonObject {
                    ["foo"] = "foo",
                    ["$type"] = typeof(MyClass).FullName
                }
            );
            Assert.IsInstanceOf<MyClass>(value);
            Assert.AreEqual("foo", value.foo);
        }
        
        [Test]
        public void DeserializationWithTypeAndWithoutContextWorks()
        {
            object value = Serializer.FromJson<object>(
                new JsonObject {
                    ["foo"] = "foo",
                    ["$type"] = typeof(MyClass).FullName
                }
            );
            Assert.IsInstanceOf<MyClass>(value);
            Assert.AreEqual("foo", ((MyClass) value).foo);
        }
        
        [Test]
        public void DeserializationWithContextMismatchFails()
        {
            Assert.Throws<UnisaveSerializationException>(() => {
                Serializer.FromJson<Vector3>(
                    new JsonObject {
                        ["foo"] = "foo",
                        ["$type"] = typeof(MyClass).FullName
                    }
                );
            });
        }
        
        [Test]
        public void DeserializationWithUnknownTypeFails()
        {
            Assert.Throws<UnisaveSerializationException>(() => {
                Serializer.FromJson<MyClass>(
                    new JsonObject {
                        ["foo"] = "foo",
                        ["$type"] = "Non-Existing-Type-Name"
                    }
                );
            });
        }
        
        #endregion
    }
}