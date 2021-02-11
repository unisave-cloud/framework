using System;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Primitives
{
    [TestFixture]
    public class TimeSpanSerializationTest
    {
        [Test]
        public void ItSerializesTimeSpans()
        {
            var ts = new TimeSpan(123456789L);
            
            Assert.AreEqual(
                "12.3456789",
                Serializer.ToJsonString(ts)
            );

            Assert.AreEqual(
                ts,
                Serializer.FromJsonString<TimeSpan>("12.3456789")
            );
        }

        [Test]
        public void ItSerializesNegativeTimeSpan()
        {
            var ts = new TimeSpan(-123456789L);
            
            Assert.AreEqual(
                "-12.3456789",
                Serializer.ToJsonString(ts)
            );

            Assert.AreEqual(
                ts,
                Serializer.FromJsonString<TimeSpan>("-12.3456789")
            );
        }

        [Test]
        public void ItDeserializesLegacyTimeSpan()
        {
            Assert.AreEqual(
                new TimeSpan(123456789L),
                Serializer.FromJsonString<TimeSpan>(
                    "{\"_ticks\":\"123456789\"}"
                )
            );
        }
    }
}