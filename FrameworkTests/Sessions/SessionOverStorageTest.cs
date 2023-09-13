using System;
using LightJson;
using Moq;
using NUnit.Framework;
using Unisave;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Foundation;
using Unisave.Serialization;
using Unisave.Sessions;

namespace FrameworkTests.Sessions
{
    [TestFixture]
    public class SessionOverStorageTest
    {
        private BackendApplication app;
        private SessionOverStorage session;
        private Mock<ISessionStorage> storageMock;
        
        [SetUp]
        public void SetUp()
        {
            app = new BackendApplication(new Type[0]);
            
            storageMock = new Mock<ISessionStorage>();
            session = new SessionOverStorage(storageMock.Object, 42);
            app.Services.RegisterInstance<ISession>(session);
            
            Facade.SetApplication(app);
        }

        [TearDown]
        public void TearDown()
        {
            Facade.SetApplication(null);
        }
        
        [Test]
        public void TestSetAndGet()
        {
            Assert.IsNull(Session.Get<string>("foo"));
            
            Session.Set("foo", "bar");
            
            Assert.AreEqual("bar", Session.Get<string>("foo"));
        }

        [Test]
        public void TestDefaultGetValue()
        {
            Assert.AreEqual("123", Session.Get("foo", "123"));
            Assert.AreEqual(null, Session.Get<string>("foo"));
            Assert.AreEqual(0, Session.Get<int>("foo"));
        }

        [Test]
        public void TestHasAndExists()
        {
            Assert.IsFalse(Session.Has("foo"));
            Assert.IsFalse(Session.Exists("foo"));
            
            Session.Set("foo", null);
            
            Assert.IsFalse(Session.Has("foo"));
            Assert.IsTrue(Session.Exists("foo"));
            
            Session.Set("foo", "");
            
            Assert.IsTrue(Session.Has("foo"));
            Assert.IsTrue(Session.Exists("foo"));
        }

        [Test]
        public void TestAll()
        {
            Assert.AreEqual(
                "{}".Replace("'", "\""),
                Session.All().ToString()
            );
            
            Session.Set("foo", "bar");
            
            Assert.AreEqual(
                "{'foo':'bar'}".Replace("'", "\""),
                Session.All().ToString()
            );
            
            Session.Set("baz", 20);
            
            Assert.AreEqual(
                "{'foo':'bar','baz':20}".Replace("'", "\""),
                Session.All().ToString()
            );
            
            Session.Set("asd", null);
            
            Assert.AreEqual(
                "{'foo':'bar','baz':20,'asd':null}".Replace("'", "\""),
                Session.All().ToString()
            );
        }

        [Test]
        public void TestForget()
        {
            Session.Set("foo", "bar");
            Session.Set("baz", 20);
            Session.Set("asd", null);
            
            Assert.AreEqual(
                "{'foo':'bar','baz':20,'asd':null}".Replace("'", "\""),
                Session.All().ToString()
            );
            
            Session.Forget("foo");
            
            Assert.AreEqual(
                "{'baz':20,'asd':null}".Replace("'", "\""),
                Session.All().ToString()
            );
        }

        [Test]
        public void TestClear()
        {
            Session.Set("foo", "bar");
            Session.Set("baz", 20);
            Session.Set("asd", null);
            
            Assert.AreEqual(
                "{'foo':'bar','baz':20,'asd':null}".Replace("'", "\""),
                Session.All().ToString()
            );
            
            Session.Clear();
            
            Assert.AreEqual("{}", Session.All().ToString());
        }

        [Test]
        public void TestStore()
        {
            Session.Set("foo", "bar");
            Session.Set("baz", 20);

            storageMock.Setup(s => s.Store(
                "123456789",
                It.Is<JsonObject>(j =>
                    j.ToString() == "{'foo':'bar','baz':20}".Replace("'", "\"")
                ),
                42
            ));
            
            session.StoreSession("123456789");
            
            storageMock.VerifyAll();
        }
        
        [Test]
        public void TestLoad()
        {
            Session.Set("old", "data");
            
            storageMock
                .Setup(s => s.Load("123456789"))
                .Returns(new JsonObject()
                    .Add("foo", "bar")
                    .Add("baz", 20)
                );
            
            session.LoadSession("123456789");

            Assert.AreEqual("bar", Session.Get<string>("foo"));
            Assert.AreEqual(20, Session.Get<int>("baz"));
            
            Assert.IsFalse(Session.Exists("old"));
            
            storageMock.VerifyAll();
        }
    }
}