using System;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Composites
{
    [TestFixture]
    public class SerializableTypeSerializationTest
    {
        // TODO: define and implement proper serialization

        [Serializable]
        public class MyContainer
        {
            public string foo;
        }

        [Test]
        public void SerializationIsNotImplemented()
        {
            Assert.Throws<NotImplementedException>(() => {
                Serializer.ToJson(new MyContainer {
                    foo = "foo"
                });
            });
        }
    }
}