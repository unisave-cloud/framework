using NUnit.Framework;
using Unisave.Serialization;
using UnityEngine;

namespace FrameworkTests.Serialization.Primitives
{
    [TestFixture]
    public class FakeUnitySerializationTest
    {
        [Test]
        public void ItSerializesVectors()
        {
            // Vector2
            Assert.AreEqual(
                @"{""x"":1,""y"":2}",
                Serializer.ToJson(new Vector2(1, 2)).ToString()
            );
            Assert.AreEqual(
                new Vector2(1, 2),
                Serializer.FromJsonString<Vector2>(@"{""x"":1.0,""y"":2.0}")
            );
            
            // Vector2Int
            Assert.AreEqual(
                @"{""x"":1,""y"":2}",
                Serializer.ToJson(new Vector2Int(1, 2)).ToString()
            );
            Assert.AreEqual(
                new Vector2Int(1, 2),
                Serializer.FromJsonString<Vector2Int>(@"{""x"":1,""y"":2}")
            );
            
            // Vector3
            Assert.AreEqual(
                @"{""x"":1,""y"":2,""z"":3}",
                Serializer.ToJson(new Vector3(1, 2, 3)).ToString()
            );
            Assert.AreEqual(
                new Vector3(1, 2, 3),
                Serializer.FromJsonString<Vector3>(
                    @"{""x"":1.0,""y"":2.0,""z"":3.0}"
                )
            );
            
            // Vector3Int
            Assert.AreEqual(
                @"{""x"":1,""y"":2,""z"":3}",
                Serializer.ToJson(new Vector3Int(1, 2, 3)).ToString()
            );
            Assert.AreEqual(
                new Vector3Int(1, 2, 3),
                Serializer.FromJsonString<Vector3Int>(
                    @"{""x"":1,""y"":2,""z"":3}"
                )
            );
            
            // Vector4
            Assert.AreEqual(
                @"{""x"":1,""y"":2,""z"":3,""w"":4}",
                Serializer.ToJson(new Vector4(1, 2, 3, 4)).ToString()
            );
            Assert.AreEqual(
                new Vector4(1, 2, 3, 4),
                Serializer.FromJsonString<Vector4>(
                    @"{""x"":1.0,""y"":2.0,""z"":3.0,""w"":4.0}"
                )
            );
        }
        
        [Test]
        public void ItSerializesColors()
        {
            // Color
            Assert.AreEqual(
                @"{""r"":0.5,""g"":0.25,""b"":0.75,""a"":1}",
                Serializer.ToJson(new Color(0.5f, 0.25f, 0.75f, 1f)).ToString()
            );
            Assert.AreEqual(
                new Color(0.5f, 0.25f, 0.75f, 1f),
                Serializer.FromJsonString<Color>(
                    @"{""r"":0.5,""g"":0.25,""b"":0.75,""a"":1}"
                )
            );
            
            // Color32
            Assert.AreEqual(
                @"{""r"":1,""g"":2,""b"":3,""a"":4}",
                Serializer.ToJson(new Color32(1, 2, 3, 4)).ToString()
            );
            Assert.AreEqual(
                new Color32(1, 2, 3, 4),
                Serializer.FromJsonString<Color32>(
                    @"{""r"":1,""g"":2,""b"":3,""a"":4}"
                )
            );
        }
    }
}