using System;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Primitives
{
    [TestFixture]
    public class TupleSerializationTest
    {
        [Test]
        public void ItSerializesTuples()
        {
            Assert.AreEqual(
                "[1]",
                Serializer.ToJsonString(Tuple.Create(1))
            );
            
            Assert.AreEqual(
                "[1,2]",
                Serializer.ToJsonString(Tuple.Create(1, 2))
            );
            
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJsonString(Tuple.Create(1, 2, 3))
            );
            
            Assert.AreEqual(
                "[1,2,3,4]",
                Serializer.ToJsonString(Tuple.Create(1, 2, 3, 4))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5]",
                Serializer.ToJsonString(Tuple.Create(1, 2, 3, 4, 5))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5,6]",
                Serializer.ToJsonString(Tuple.Create(1, 2, 3, 4, 5, 6))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5,6,7]",
                Serializer.ToJsonString(Tuple.Create(1, 2, 3, 4, 5, 6, 7))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5,6,7,8]",
                Serializer.ToJsonString(Tuple.Create(1, 2, 3, 4, 5, 6, 7, 8))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5,6,7,8,9]",
                Serializer.ToJsonString(
                    new Tuple<int, int, int, int, int, int, int, Tuple<int, int>>(
                        1, 2, 3, 4, 5, 6, 7, new Tuple<int, int>(8, 9)
                    )
                )
            );
        }
        
        [Test]
        public void ItDeserializesTuples()
        {
            var t1 = Serializer.FromJsonString<Tuple<int>>("[1]");
            Assert.AreEqual(1, t1.Item1);
            
            var t2 = Serializer.FromJsonString<
                Tuple<int, int>
            >("[1,2]");
            Assert.AreEqual(1, t2.Item1);
            Assert.AreEqual(2, t2.Item2);
            
            var t3 = Serializer.FromJsonString<
                Tuple<int, int, int>
            >("[1,2,3]");
            Assert.AreEqual(1, t3.Item1);
            Assert.AreEqual(2, t3.Item2);
            Assert.AreEqual(3, t3.Item3);
            
            var t4 = Serializer.FromJsonString<
                Tuple<int, int, int, int>
            >("[1,2,3,4]");
            Assert.AreEqual(1, t4.Item1);
            Assert.AreEqual(2, t4.Item2);
            Assert.AreEqual(3, t4.Item3);
            Assert.AreEqual(4, t4.Item4);
            
            var t5 = Serializer.FromJsonString<
                Tuple<int, int, int, int, int>
            >("[1,2,3,4,5]");
            Assert.AreEqual(1, t5.Item1);
            Assert.AreEqual(2, t5.Item2);
            Assert.AreEqual(3, t5.Item3);
            Assert.AreEqual(4, t5.Item4);
            Assert.AreEqual(5, t5.Item5);
            
            var t6 = Serializer.FromJsonString<
                Tuple<int, int, int, int, int, int>
            >("[1,2,3,4,5,6]");
            Assert.AreEqual(1, t6.Item1);
            Assert.AreEqual(2, t6.Item2);
            Assert.AreEqual(3, t6.Item3);
            Assert.AreEqual(4, t6.Item4);
            Assert.AreEqual(5, t6.Item5);
            Assert.AreEqual(6, t6.Item6);
            
            var t7 = Serializer.FromJsonString<
                Tuple<int, int, int, int, int, int, int>
            >("[1,2,3,4,5,6,7]");
            Assert.AreEqual(1, t7.Item1);
            Assert.AreEqual(2, t7.Item2);
            Assert.AreEqual(3, t7.Item3);
            Assert.AreEqual(4, t7.Item4);
            Assert.AreEqual(5, t7.Item5);
            Assert.AreEqual(6, t7.Item6);
            Assert.AreEqual(7, t7.Item7);
            
            var t8 = Serializer.FromJsonString<
                Tuple<int, int, int, int, int, int, int, Tuple<int>>
            >("[1,2,3,4,5,6,7,8]");
            Assert.AreEqual(1, t8.Item1);
            Assert.AreEqual(2, t8.Item2);
            Assert.AreEqual(3, t8.Item3);
            Assert.AreEqual(4, t8.Item4);
            Assert.AreEqual(5, t8.Item5);
            Assert.AreEqual(6, t8.Item6);
            Assert.AreEqual(7, t8.Item7);
            Assert.AreEqual(8, t8.Rest.Item1);
            
            var t9 = Serializer.FromJsonString<
                Tuple<int, int, int, int, int, int, int, Tuple<int, int>>
            >("[1,2,3,4,5,6,7,8,9]");
            Assert.AreEqual(1, t9.Item1);
            Assert.AreEqual(2, t9.Item2);
            Assert.AreEqual(3, t9.Item3);
            Assert.AreEqual(4, t9.Item4);
            Assert.AreEqual(5, t9.Item5);
            Assert.AreEqual(6, t9.Item6);
            Assert.AreEqual(7, t9.Item7);
            Assert.AreEqual(8, t9.Rest.Item1);
            Assert.AreEqual(9, t9.Rest.Item2);
        }

        [Test]
        public void ItDeserializesLegacyTuples()
        {
            var t1 = Serializer.FromJsonString<
                Tuple<int>
            >("{\"m_Item1\":1}");
            Assert.AreEqual(1, t1.Item1);
            
            var t2 = Serializer.FromJsonString<
                Tuple<int, int>
            >("{\"m_Item1\":1,\"m_Item2\":2}");
            Assert.AreEqual(1, t2.Item1);
            Assert.AreEqual(2, t2.Item2);
            
            var t3 = Serializer.FromJsonString<
                Tuple<int, int, int>
            >("{\"m_Item1\":1,\"m_Item2\":2,\"m_Item3\":3}");
            Assert.AreEqual(1, t3.Item1);
            Assert.AreEqual(2, t3.Item2);
            Assert.AreEqual(3, t3.Item3);
            
            var t4 = Serializer.FromJsonString<
                Tuple<int, int, int, int>
            >("{\"m_Item1\":1,\"m_Item2\":2,\"m_Item3\":3,\"m_Item4\":4}");
            Assert.AreEqual(1, t4.Item1);
            Assert.AreEqual(2, t4.Item2);
            Assert.AreEqual(3, t4.Item3);
            Assert.AreEqual(4, t4.Item4);
        }

        [Test]
        public void ItSerializesValueTuples()
        {
            Assert.AreEqual(
                "[1,2]",
                Serializer.ToJsonString((1, 2))
            );
            
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJsonString((1, 2, 3))
            );
            
            Assert.AreEqual(
                "[1,2,3,4]",
                Serializer.ToJsonString((1, 2, 3, 4))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5]",
                Serializer.ToJsonString((1, 2, 3, 4, 5))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5,6]",
                Serializer.ToJsonString((1, 2, 3, 4, 5, 6))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5,6,7]",
                Serializer.ToJsonString((1, 2, 3, 4, 5, 6, 7))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5,6,7,8]",
                Serializer.ToJsonString((1, 2, 3, 4, 5, 6, 7, 8))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5,6,7,8,9]",
                Serializer.ToJsonString((1, 2, 3, 4, 5, 6, 7, 8, 9))
            );
            
            Assert.AreEqual(
                "[1,2,3,4,5,6,7,8,9,10]",
                Serializer.ToJsonString((1, 2, 3, 4, 5, 6, 7, 8, 9, 10))
            );
        }

        [Test]
        public void ItDeserializesValueTuples()
        {
            var t1 = Serializer.FromJsonString<
                ValueTuple<int>
            >("[1]");
            Assert.AreEqual(1, t1.Item1);
            
            var t2 = Serializer.FromJsonString<
                (int, int)
            >("[1,2]");
            Assert.AreEqual(1, t2.Item1);
            Assert.AreEqual(2, t2.Item2);
            
            var t3 = Serializer.FromJsonString<
                (int, int, int)
            >("[1,2,3]");
            Assert.AreEqual(1, t3.Item1);
            Assert.AreEqual(2, t3.Item2);
            Assert.AreEqual(3, t3.Item3);
            
            var t4 = Serializer.FromJsonString<
                (int, int, int, int)
            >("[1,2,3,4]");
            Assert.AreEqual(1, t4.Item1);
            Assert.AreEqual(2, t4.Item2);
            Assert.AreEqual(3, t4.Item3);
            Assert.AreEqual(4, t4.Item4);
            
            var t5 = Serializer.FromJsonString<
                (int, int, int, int, int)
            >("[1,2,3,4,5]");
            Assert.AreEqual(1, t5.Item1);
            Assert.AreEqual(2, t5.Item2);
            Assert.AreEqual(3, t5.Item3);
            Assert.AreEqual(4, t5.Item4);
            Assert.AreEqual(5, t5.Item5);
            
            var t6 = Serializer.FromJsonString<
                (int, int, int, int, int, int)
            >("[1,2,3,4,5,6]");
            Assert.AreEqual(1, t6.Item1);
            Assert.AreEqual(2, t6.Item2);
            Assert.AreEqual(3, t6.Item3);
            Assert.AreEqual(4, t6.Item4);
            Assert.AreEqual(5, t6.Item5);
            Assert.AreEqual(6, t6.Item6);
            
            var t7 = Serializer.FromJsonString<
                (int, int, int, int, int, int, int)
            >("[1,2,3,4,5,6,7]");
            Assert.AreEqual(1, t7.Item1);
            Assert.AreEqual(2, t7.Item2);
            Assert.AreEqual(3, t7.Item3);
            Assert.AreEqual(4, t7.Item4);
            Assert.AreEqual(5, t7.Item5);
            Assert.AreEqual(6, t7.Item6);
            Assert.AreEqual(7, t7.Item7);
            
            var t8 = Serializer.FromJsonString<
                (int, int, int, int, int, int, int, int)
            >("[1,2,3,4,5,6,7,8]");
            Assert.AreEqual(1, t8.Item1);
            Assert.AreEqual(2, t8.Item2);
            Assert.AreEqual(3, t8.Item3);
            Assert.AreEqual(4, t8.Item4);
            Assert.AreEqual(5, t8.Item5);
            Assert.AreEqual(6, t8.Item6);
            Assert.AreEqual(7, t8.Item7);
            Assert.AreEqual(8, t8.Rest.Item1);
            Assert.AreEqual(8, t8.Item8);
            
            var t9 = Serializer.FromJsonString<
                (int, int, int, int, int, int, int, int, int)
            >("[1,2,3,4,5,6,7,8,9]");
            Assert.AreEqual(1, t9.Item1);
            Assert.AreEqual(2, t9.Item2);
            Assert.AreEqual(3, t9.Item3);
            Assert.AreEqual(4, t9.Item4);
            Assert.AreEqual(5, t9.Item5);
            Assert.AreEqual(6, t9.Item6);
            Assert.AreEqual(7, t9.Item7);
            Assert.AreEqual(8, t9.Rest.Item1);
            Assert.AreEqual(8, t9.Item8);
            Assert.AreEqual(9, t9.Rest.Item2);
            Assert.AreEqual(9, t9.Item9);
        }
        
        [Test]
        public void ItDeserializesLegacyValueTuples()
        {
            var t1 = Serializer.FromJsonString<
                ValueTuple<int>
            >("{\"Item1\":1}");
            Assert.AreEqual(1, t1.Item1);
            
            var t2 = Serializer.FromJsonString<
                (int, int)
            >("{\"Item1\":1,\"Item2\":2}");
            Assert.AreEqual(1, t2.Item1);
            Assert.AreEqual(2, t2.Item2);
            
            var t3 = Serializer.FromJsonString<
                (int, int, int)
            >("{\"Item1\":1,\"Item2\":2,\"Item3\":3}");
            Assert.AreEqual(1, t3.Item1);
            Assert.AreEqual(2, t3.Item2);
            Assert.AreEqual(3, t3.Item3);
            
            var t4 = Serializer.FromJsonString<
                (int, int, int, int)
            >("{\"Item1\":1,\"Item2\":2,\"Item3\":3,\"Item4\":4}");
            Assert.AreEqual(1, t4.Item1);
            Assert.AreEqual(2, t4.Item2);
            Assert.AreEqual(3, t4.Item3);
            Assert.AreEqual(4, t4.Item4);
        }
    }
}