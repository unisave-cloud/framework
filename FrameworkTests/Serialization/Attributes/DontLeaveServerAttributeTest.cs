using NUnit.Framework;
using Unisave;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace FrameworkTests.Serialization.Attributes
{
    [TestFixture]
    public class DontLeaveServerAttributeTest
    {
        private class MyCustomType
        {
            // plain field
            
            public string fooYes;
            
            [DontLeaveServer]
            public string fooNo;
            
            // backing field
            
            public string BarYes { get; set; }
            
            [DontLeaveServer]
            public string BarNo { get; set; }
        }
        
        [Test]
        public void MarkedFieldsDontGetSerializedWhenLeavingServer()
        {
            var context = new SerializationContext {
                securityDomainCrossing = SecurityDomainCrossing.LeavingServer
            };

            Assert.AreEqual(
                "{'fooYes':'fooYes','BarYes':'BarYes'}".Replace('\'', '"'),
                Serializer.ToJsonString(new MyCustomType {
                    fooYes = "fooYes",
                    fooNo = "fooNo",
                    BarYes = "BarYes",
                    BarNo = "BarNo"
                }, context)
            );
        }
        
        [Test]
        public void MarkedFieldsGetSerializedWithinServer()
        {
            var context = new SerializationContext {
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };

            Assert.AreEqual(
                ("{'fooYes':'fooYes','fooNo':'fooNo'" +
                ",'BarYes':'BarYes','BarNo':'BarNo'}").Replace('\'', '"'),
                Serializer.ToJsonString(new MyCustomType {
                    fooYes = "fooYes",
                    fooNo = "fooNo",
                    BarYes = "BarYes",
                    BarNo = "BarNo"
                }, context)
            );
        }
        
        [Test]
        public void MarkedFieldsGetSerializedWhenEnteringServer()
        {
            var context = new SerializationContext {
                securityDomainCrossing = SecurityDomainCrossing.EnteringServer
            };

            Assert.AreEqual(
                ("{'fooYes':'fooYes','fooNo':'fooNo'" +
                 ",'BarYes':'BarYes','BarNo':'BarNo'}").Replace('\'', '"'),
                Serializer.ToJsonString(new MyCustomType {
                    fooYes = "fooYes",
                    fooNo = "fooNo",
                    BarYes = "BarYes",
                    BarNo = "BarNo"
                }, context)
            );
        }
    }
}