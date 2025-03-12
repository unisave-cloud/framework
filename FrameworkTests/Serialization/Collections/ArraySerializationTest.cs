using System;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Collections
{
    [TestFixture]
    public class ArraySerializationTest
    {
        private class MyType
        {
            public int foo;
            public string bar;
        }
        
        [Test]
        public void ItSerializesAnArray()
        {
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJsonString(new int[] {1, 2, 3})
            );
            
            Assert.AreEqual(
                "['a','b','c',null]".Replace('\'', '"'),
                Serializer.ToJsonString(new string[] {"a", "b", "c", null})
            );
        }

        [Test]
        public void ItDeserializesAnArray()
        {
            Assert.AreEqual(
                new int[] {1, 2, 3},
                Serializer.FromJsonString<int[]>("[1,2,3]")
            );
            
            Assert.AreEqual(
                new string[] {"a", "b", "c", null},
                Serializer.FromJsonString<string[]>(
                    "['a','b','c',null]".Replace('\'', '"')
                )
            );
        }

        [Test]
        public void ItSerializesAnArrayOfCustomTypes()
        {
            var data = new MyType[] {
                new MyType(),
                null,
                new MyType {
                    foo = 42,
                    bar = "lorem"
                }
            };
            
            var json = "[{'foo':0,'bar':null},null,{'foo':42,'bar':'lorem'}]"
                .Replace('\'', '"');
            
            Assert.AreEqual(json, Serializer.ToJsonString(data));
        }

        [Test]
        public void ItDeserializesAnArrayOfCustomTypes()
        {
            var json = "[{'foo':0,'bar':null},null,{'foo':42,'bar':'lorem'}]"
                .Replace('\'', '"');
            
            var data = Serializer.FromJsonString<MyType[]>(json);
            
            Assert.AreEqual(3, data.Length);
            
            Assert.AreEqual(0, data[0].foo);
            Assert.AreEqual(null, data[0].bar);
            
            Assert.IsNull(data[1]);
            
            Assert.AreEqual(42, data[2].foo);
            Assert.AreEqual("lorem", data[2].bar);
        }
        
        [Test]
        public void ItSerializes2DArray()
        {
            var myArray = new int[2, 3] {
                {1, 2, 3},
                {4, 5, 6}
            };
            
            Assert.AreEqual(
                "[[1,2,3],[4,5,6]]",
                Serializer.ToJsonString(myArray)
            );
        }
        
        [Test]
        public void ItDeserializes2DArray()
        {
            var myArray = new int[2, 3] {
                {1, 2, 3},
                {4, 5, 6}
            };
            
            int[,] deserialized = Serializer.FromJsonString<int[,]>(
                "[[1,2,3],[4,5,6]]"
            );
            
            Assert.AreEqual(2, deserialized.GetLength(0));
            Assert.AreEqual(3, deserialized.GetLength(1));
            Assert.AreEqual(6, deserialized.Length);
            
            Assert.AreEqual(1, deserialized[0, 0]);
            Assert.AreEqual(3, deserialized[0, 2]);
            Assert.AreEqual(4, deserialized[1, 0]);
            Assert.AreEqual(6, deserialized[1, 2]);
            
            Assert.AreEqual(myArray, deserialized);
        }
        
        [Test]
        public void ItSerializesEmpty2DArray()
        {
            var myFirstArray = new int[0, 3] {};
            var mySecondArray = new int[2, 0] {{}, {}};
            var myThirdArray = new int[0, 0] {};
            
            Assert.AreEqual(
                "[]", Serializer.ToJsonString(myFirstArray)
            );
            
            Assert.AreEqual(
                "[[],[]]", Serializer.ToJsonString(mySecondArray)
            );
            
            Assert.AreEqual(
                "[]", Serializer.ToJsonString(myThirdArray)
            );
        }

        [Test]
        public void ItDeserializesEmpty2DArray()
        {
            int[,] first = Serializer.FromJsonString<int[,]>("[]");
            Assert.AreEqual(0, first.GetLength(0));
            Assert.AreEqual(0, first.GetLength(1));
            Assert.AreEqual(0, first.Length);
            
            int[,] second = Serializer.FromJsonString<int[,]>("[[],[]]");
            Assert.AreEqual(2, second.GetLength(0));
            Assert.AreEqual(0, second.GetLength(1));
            Assert.AreEqual(0, second.Length);
        }
        
        [Test]
        public void ItSerializesStaggeredArray()
        {
            int[][] myArray = new int[][] {
                new int[] {1, 2, 3, 4},
                new int[] {5, 6}
            };
            
            Assert.AreEqual(
                "[[1,2,3,4],[5,6]]",
                Serializer.ToJsonString(myArray)
            );
        }
        
        [Test]
        public void ItDeserializesStaggeredArray()
        {
            int[][] myArray = new int[][] {
                new int[] {1, 2, 3, 4},
                new int[] {5, 6}
            };
            
            Assert.AreEqual(
                "[[1,2,3,4],[5,6]]",
                Serializer.ToJsonString(myArray)
            );
            
            int[][] deserialized = Serializer.FromJsonString<int[][]>(
                "[[1,2,3,4],[5,6]]"
            );
            
            Assert.AreEqual(2, deserialized.Length);
            Assert.AreEqual(4, deserialized[0].Length);
            Assert.AreEqual(2, deserialized[1].Length);
            
            Assert.AreEqual(1, deserialized[0][0]);
            Assert.AreEqual(4, deserialized[0][3]);
            Assert.AreEqual(5, deserialized[1][0]);
            Assert.AreEqual(6, deserialized[1][1]);
            
            Assert.AreEqual(myArray, deserialized);
        }
    }
}