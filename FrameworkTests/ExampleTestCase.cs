using NUnit.Framework;
using System;
using Unisave;
using LightJson;
using UnityEngine;

namespace FrameworkTests
{
    [TestFixture()]
    public class ExampleTestCase
    {
        [Test()]
        public void TestCase()
        {
            UnisavePlayer p = new UnisavePlayer("foo");

            Assert.AreEqual("foo", p.Id);

            var json = new JsonObject()
                .Add("bar", 42)
                .ToString();

            Assert.AreEqual(@"{""bar"":42}", json);

            //Vector3 a = new Vector3(1, 0, 0);
            //Vector3 b = new Vector3(0, 1, 0);
            //var v = Vector3.RotateTowards(a, b, 3.14f / 4f, 0.01f);

            Assert.AreEqual("(1.0, 2.0, 3.0)", (new Vector3(1, 2, 3)).ToString());

            //Debug.Log("Hello world!");
        }
    }
}
