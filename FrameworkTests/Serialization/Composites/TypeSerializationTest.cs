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
            var context = SerializationContext.DefaultContext();
            context.typeSerialization = TypeSerialization.Never;
            
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = "foo"
                }.ToString(),
                Serializer.ToJson<MyClass>(new MyClass()).ToString()
            );
            
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = "foo"
                }.ToString(),
                Serializer.ToJson<object>(new MyClass()).ToString()
            );
        }
        
        [Test]
        public void TestDuringPolymorphismSerializing()
        {
            var context = SerializationContext.DefaultContext();
            context.typeSerialization = TypeSerialization.DuringPolymorphism;
            
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = "foo"
                }.ToString(),
                Serializer.ToJson<MyClass>(new MyClass()).ToString()
            );
            
            Assert.AreEqual(
                new JsonObject {
                    ["$type"] = typeof(MyClass).FullName,
                    ["foo"] = "foo"
                }.ToString(),
                Serializer.ToJson<object>(new MyClass()).ToString()
            );
        }
        
        [Test]
        public void TestAlwaysSerializing()
        {
            var context = SerializationContext.DefaultContext();
            context.typeSerialization = TypeSerialization.Always;
            
            Assert.AreEqual(
                new JsonObject {
                    ["$type"] = typeof(MyClass).FullName,
                    ["foo"] = "foo"
                }.ToString(),
                Serializer.ToJson<MyClass>(new MyClass()).ToString()
            );
            
            Assert.AreEqual(
                new JsonObject {
                    ["$type"] = typeof(MyClass).FullName,
                    ["foo"] = "foo"
                }.ToString(),
                Serializer.ToJson<object>(new MyClass()).ToString()
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
                    ["$type"] = typeof(MyClass).FullName,
                    ["foo"] = "foo"
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
                    ["$type"] = typeof(MyClass).FullName,
                    ["foo"] = "foo"
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
                        ["$type"] = typeof(MyClass).FullName,
                        ["foo"] = "foo"
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
                        ["$type"] = "Non-Existing-Type-Name",
                        ["foo"] = "foo"
                    }
                );
            });
        }
        
        #endregion
    }
}