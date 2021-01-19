using System.Collections.Generic;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Collections
{
    /*
     * Which collections?
     * 
     * - List : ICollection<T> -> tested in a different file
     * - LinkedList : ICollection<T>
     * - Stack
     * - Queue
     * - HashSet : ICollection<T>
     * - SortedSet : ICollection<T>
     *
     * --------------------
     *
     * - Dictionary : IDictionary<K, V> -> tested in a different file
     * - SortedDictionary : IDictionary<K, V>
     * - SortedList : IDictionary<K, V>
     */
    
    [TestFixture]
    public class OtherCollectionsSerializationTest
    {
        [Test]
        public void ItSerializesLinkedLists()
        {
            var a = new LinkedList<int>();
            a.AddLast(1); a.AddLast(2); a.AddLast(3);
            
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJsonString(a)
            );
            
            var b = new LinkedList<string>();
            b.AddLast("a"); b.AddLast("b"); b.AddLast("c"); b.AddLast((string)null);
            
            Assert.AreEqual(
                "['a','b','c',null]".Replace('\'', '"'),
                Serializer.ToJsonString(b)
            );
        }

        [Test]
        public void ItDeserializesLinkedLists()
        {
            var a = new LinkedList<int>();
            a.AddLast(1); a.AddLast(2); a.AddLast(3);
            
            Assert.AreEqual(
                a,
                Serializer.FromJsonString<LinkedList<int>>("[1,2,3]")
            );
            
            var b = new LinkedList<string>();
            b.AddLast("a"); b.AddLast("b"); b.AddLast("c"); b.AddLast((string)null);
            
            Assert.AreEqual(
                b,
                Serializer.FromJsonString<LinkedList<string>>(
                    "['a','b','c',null]".Replace('\'', '"')
                )
            );
        }
        
        [Test]
        public void ItSerializesStacks()
        {
            var a = new Stack<int>();
            a.Push(1); a.Push(2); a.Push(3);
            
            Assert.AreEqual(
                "[3,2,1]",
                Serializer.ToJsonString(a)
            );
            
            var b = new Stack<string>();
            b.Push("a"); b.Push("b"); b.Push("c"); b.Push(null);
            
            Assert.AreEqual(
                "[null,'c','b','a']".Replace('\'', '"'),
                Serializer.ToJsonString(b)
            );
        }
        
        [Test]
        public void ItDeserializesStacks()
        {
            var a = new Stack<int>();
            a.Push(1); a.Push(2); a.Push(3);
            
            Assert.AreEqual(
                a,
                Serializer.FromJsonString<Stack<int>>("[3,2,1]")
            );
            
            var b = new Stack<string>();
            b.Push("a"); b.Push("b"); b.Push("c"); b.Push(null);
            
            Assert.AreEqual(
                b,
                Serializer.FromJsonString<Stack<string>>(
                    "[null,'c','b','a']".Replace('\'', '"')
                )
            );
        }
        
        [Test]
        public void ItSerializesQueues()
        {
            var a = new Queue<int>();
            a.Enqueue(1); a.Enqueue(2); a.Enqueue(3);
            
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJsonString(a)
            );
            
            var b = new Queue<string>();
            b.Enqueue("a"); b.Enqueue("b"); b.Enqueue("c"); b.Enqueue(null);
            
            Assert.AreEqual(
                "['a','b','c',null]".Replace('\'', '"'),
                Serializer.ToJsonString(b)
            );
        }

        [Test]
        public void ItDeserializesQueues()
        {
            var a = new Queue<int>();
            a.Enqueue(1); a.Enqueue(2); a.Enqueue(3);
            
            Assert.AreEqual(
                a,
                Serializer.FromJsonString<Queue<int>>("[1,2,3]")
            );
            
            var b = new Queue<string>();
            b.Enqueue("a"); b.Enqueue("b"); b.Enqueue("c"); b.Enqueue(null);
            
            Assert.AreEqual(
                b,
                Serializer.FromJsonString<Queue<string>>(
                    "['a','b','c',null]".Replace('\'', '"')
                )
            );
        }
        
        [Test]
        public void ItSerializesHashSets()
        {
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJsonString(new HashSet<int> {1, 2, 3, 3, 3})
            );
            
            Assert.AreEqual(
                "['a','b','c',null]".Replace('\'', '"'),
                Serializer.ToJsonString(new HashSet<string> {"a", "b", "c", null})
            );
        }

        [Test]
        public void ItDeserializesHashSets()
        {
            Assert.AreEqual(
                new HashSet<int> {1, 2, 3},
                Serializer.FromJsonString<HashSet<int>>("[1,2,3,3,3]")
            );
            
            Assert.AreEqual(
                new HashSet<string> {"a", "b", "c", null},
                Serializer.FromJsonString<HashSet<string>>(
                    "['a','b','b','c',null]".Replace('\'', '"')
                )
            );
        }
        
        [Test]
        public void ItSerializesSortedSets()
        {
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJsonString(new SortedSet<int> {3, 2, 3, 3, 1})
            );
            
            Assert.AreEqual(
                "[null,'a','b','c']".Replace('\'', '"'),
                Serializer.ToJsonString(new SortedSet<string> {"a", "b", "c", null})
            );
        }

        [Test]
        public void ItDeserializesSortedSets()
        {
            Assert.AreEqual(
                new SortedSet<int> {1, 2, 3},
                Serializer.FromJsonString<SortedSet<int>>("[3,3,3,2,1]")
            );
            
            Assert.AreEqual(
                new SortedSet<string> {"a", "b", "c", null},
                Serializer.FromJsonString<SortedSet<string>>(
                    "['a','b','b','c',null]".Replace('\'', '"')
                )
            );
        }
        
        [Test]
        public void ItSerializesSortedDictionaries()
        {
            var subject = new SortedDictionary<string, int> {
                ["lorem"] = 42,
                ["ipsum"] = 2
            };

            Assert.AreEqual(
                "{\"ipsum\":2,\"lorem\":42}",
                Serializer.ToJson(subject).ToString()
            );

            Assert.AreEqual(
                subject,
                Serializer.FromJsonString<SortedDictionary<string, int>>(
                    "{\"ipsum\":2,\"lorem\":42}"
                )
            );
        }
        
        [Test]
        public void ItSerializesSortedLists()
        {
            var subject = new SortedList<string, int> {
                ["lorem"] = 42,
                ["ipsum"] = 2
            };

            Assert.AreEqual(
                "{\"ipsum\":2,\"lorem\":42}",
                Serializer.ToJson(subject).ToString()
            );

            Assert.AreEqual(
                subject,
                Serializer.FromJsonString<SortedList<string, int>>(
                    "{\"ipsum\":2,\"lorem\":42}"
                )
            );
        }
    }
}