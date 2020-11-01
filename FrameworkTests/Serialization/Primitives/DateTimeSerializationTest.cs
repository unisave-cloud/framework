using System;
using LightJson;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Primitives
{
    [TestFixture]
    public class DateTimeSerializationTest
    {
        [Test]
        public void ItSerializesDateTimeNow()
        {
            // what are we trying to serialize
            var subject = DateTime.Now;
            
            // what the result should look like
            var serialized = subject.ToString(SerializationParams.DateTimeFormat);

            // make sure the result looks exactly as wanted
            Assert.AreEqual(
                serialized,
                Serializer.ToJson(subject).AsString
            );

            // and that the result can be loaded back and be exact
            var loaded = Serializer.FromJson<DateTime>((JsonValue)serialized);
            Assert.AreEqual(subject.Year, loaded.Year);
            Assert.AreEqual(subject.Month, loaded.Month);
            Assert.AreEqual(subject.Day, loaded.Day);
            Assert.AreEqual(subject.Hour, loaded.Hour);
            Assert.AreEqual(subject.Minute, loaded.Minute);
            Assert.AreEqual(subject.Second, loaded.Second);
            Assert.AreEqual(subject.Millisecond, loaded.Millisecond); // even ms
        }
        
        [Test]
        public void ItSerializesSpecificDateTime()
        {
            // what are we trying to serialize
            var subject = new DateTime(2000, 1, 2, 3, 4, 5, 6);
            
            // what the result should look like
            var serialized = "2000-01-02T03:04:05.006Z";

            // make sure the result looks exactly as wanted
            Assert.AreEqual(
                serialized,
                Serializer.ToJson(subject).AsString
            );

            // and that the result can be loaded back and be exact
            var loaded = Serializer.FromJson<DateTime>((JsonValue)serialized);
            Assert.AreEqual(subject.Year, loaded.Year);
            Assert.AreEqual(subject.Month, loaded.Month);
            Assert.AreEqual(subject.Day, loaded.Day);
            Assert.AreEqual(subject.Hour, loaded.Hour);
            Assert.AreEqual(subject.Minute, loaded.Minute);
            Assert.AreEqual(subject.Second, loaded.Second);
            Assert.AreEqual(subject.Millisecond, loaded.Millisecond); // even ms
            
            Assert.AreEqual(2000, loaded.Year);
            Assert.AreEqual(1, loaded.Month);
            Assert.AreEqual(2, loaded.Day);
            Assert.AreEqual(3, loaded.Hour);
            Assert.AreEqual(4, loaded.Minute);
            Assert.AreEqual(5, loaded.Second);
            Assert.AreEqual(6, loaded.Millisecond); // even ms
        }

        [Test]
        public void ItDeserializesNullAsDefaultValue()
        {
            DateTime loaded = Serializer.FromJson<DateTime>(JsonValue.Null);
            Assert.AreEqual(
                default(DateTime),
                loaded
            );
        }

        [Test]
        public void ItDeserializesNullableToNull()
        {
            DateTime? loaded = Serializer.FromJson<DateTime?>(JsonValue.Null);
            Assert.IsNull(loaded);
        }
        
        [Test]
        [Ignore("Nullable types are not yet implemented in serializer")]
        public void ItDeserializesNullableDateTime()
        {
            var subject = DateTime.Now;
            var subjectJson = Serializer.ToJson(subject);
            
            DateTime? loaded = Serializer.FromJson<DateTime?>(subjectJson);
            
            Assert.AreEqual(subject, loaded);
        }
        
        #region "Legacy datetime"
        
        public const string LegacyFormat = "yyyy-MM-dd H:mm:ss";
        
        [Test]
        public void ItDeserializesLegacyFormat()
        {
            // what are we trying to serialize
            var subject = DateTime.Now;
            
            // what the result should look like
            var serialized = subject.ToString(LegacyFormat);

            // load it and compare value by value
            var loaded = Serializer.FromJson<DateTime>((JsonValue)serialized);
            Assert.AreEqual(subject.Year, loaded.Year);
            Assert.AreEqual(subject.Month, loaded.Month);
            Assert.AreEqual(subject.Day, loaded.Day);
            Assert.AreEqual(subject.Hour, loaded.Hour);
            Assert.AreEqual(subject.Minute, loaded.Minute);
            Assert.AreEqual(subject.Second, loaded.Second);
            Assert.AreEqual(0, loaded.Millisecond); // have not been serialized
        }
        
        #endregion
    }
}