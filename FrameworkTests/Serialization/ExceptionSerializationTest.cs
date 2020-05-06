using System;
using System.Runtime.Serialization;
using LightJson;
using NUnit.Framework;
using Unisave.Serialization;
using Unisave.Serialization.Exceptions;

namespace FrameworkTests.Serialization
{
    [TestFixture]
    public class ExceptionSerializationTest
    {
        // TODO: check reconstruction of: UnisaveException, BackendExecutionTimeoutException
        // TODO: remove unused exceptions from Unisave.Exceptions namespace
        
        [Serializable]
        private class StubException : Exception
        {
            public StubException() : base() { }
            public StubException(string msg) : base(msg) { }
            public StubException(Exception inner) : base(null, inner) { }
            protected StubException(SerializationInfo info, StreamingContext c)
                : base(info, c) { }
        }
        
        /*[Serializable]
        private class NonExistingStubException : Exception
        {
            public NonExistingStubException() : base() { }
            protected NonExistingStubException(SerializationInfo info, StreamingContext c)
                : base(info, c) { }
        }*/
        
        private const string NonExistingStubExceptionLegacySerialized
            = "\"AAEAAAD\\/\\/\\/\\/\\/AQAAAAAAAAAMAgAAAEtGcmFtZXdvcmtUZXN0" +
              "cywgVmVyc2lvbj0xLjAuNzQzMS4xMzU0LCBDdWx0dXJlPW5ldXRyYWwsIF" +
              "B1YmxpY0tleVRva2VuPW51bGwFAQAAAFBGcmFtZXdvcmtUZXN0cy5TZXJp" +
              "YWxpemF0aW9uLkV4Y2VwdGlvblNlcmlhbGl6YXRpb25UZXN0K05vbkV4aX" +
              "N0aW5nU3R1YkV4Y2VwdGlvbgsAAAAJQ2xhc3NOYW1lB01lc3NhZ2UERGF0" +
              "YQ5Jbm5lckV4Y2VwdGlvbgdIZWxwVVJMEFN0YWNrVHJhY2VTdHJpbmcWUm" +
              "Vtb3RlU3RhY2tUcmFjZVN0cmluZxBSZW1vdGVTdGFja0luZGV4D0V4Y2Vw" +
              "dGlvbk1ldGhvZAdIUmVzdWx0BlNvdXJjZQEBAwMBAQEAAgABHlN5c3RlbS" +
              "5Db2xsZWN0aW9ucy5JRGljdGlvbmFyeRBTeXN0ZW0uRXhjZXB0aW9uCAgC" +
              "AAAABgMAAABQRnJhbWV3b3JrVGVzdHMuU2VyaWFsaXphdGlvbi5FeGNlcH" +
              "Rpb25TZXJpYWxpemF0aW9uVGVzdCtOb25FeGlzdGluZ1N0dWJFeGNlcHRp" +
              "b24KCgoKBgQAAADXASAgYXQgRnJhbWV3b3JrVGVzdHMuU2VyaWFsaXphdG" +
              "lvbi5FeGNlcHRpb25TZXJpYWxpemF0aW9uVGVzdC5NYWtlSXRUaHJvd24g" +
              "KFN5c3RlbS5FeGNlcHRpb24gZSkgWzB4MDAwMDJdIGluIC9ob21lL2ppcm" +
              "thL0ltcG9ydGFudENvZGUvVW5pc2F2ZS9GcmFtZXdvcmsvRnJhbWV3b3Jr" +
              "VGVzdHMvU2VyaWFsaXphdGlvbi9FeGNlcHRpb25TZXJpYWxpemF0aW9uVG" +
              "VzdC5jczoxMjEgCgAAAAAKABUTgAYFAAAADkZyYW1ld29ya1Rlc3RzCw==\"";
        
        private const string NonExistingStubExceptionSerialized
            = "{\"ClassName\":\"FrameworkTests.Serialization.ExceptionSer" +
              "ializationTest+NonExistingStubException\",\"Message\":null" +
              ",\"typeof:Data\":\"System.Collections.IDictionary\",\"Data" +
              "\":null,\"typeof:InnerException\":\"System.Exception\",\"I" +
              "nnerException\":null,\"HelpURL\":null,\"StackTraceString\"" +
              ":\"  at FrameworkTests.Serialization.ExceptionSerializatio" +
              "nTest.MakeItThrown (System.Exception e) [0x00002] in \\/ho" +
              "me\\/jirka\\/ImportantCode\\/Unisave\\/Framework\\/Framewo" +
              "rkTests\\/Serialization\\/ExceptionSerializationTest.cs:72" +
              " \",\"RemoteStackTraceString\":null,\"RemoteStackIndex\":0" +
              ",\"typeof:ExceptionMethod\":\"System.Object\",\"ExceptionM" +
              "ethod\":null,\"HResult\":-2146233088,\"Source\":\"Framewor" +
              "kTests\"}";
        
        /// <summary>
        /// Helper that makes an exception into a thrown exception
        /// </summary>
        private Exception MakeItThrown(Exception e)
        {
            try
            {
                throw e;
            }
            catch (Exception ce)
            {
                return ce;
            }
        }
        
        #region "Serializing backend-to-client"

        [Test]
        public void ExceptionCanBeSerialized()
        {
            var original = MakeItThrown(new StubException("lorem ipsum"));
            var json = Serializer.ToJson(original);
            
            Assert.AreEqual(typeof(StubException).FullName, json["ClassName"].AsString);
            Assert.AreEqual("lorem ipsum", json["Message"].AsString);
            StringAssert.Contains("MakeItThrown", json["StackTraceString"].AsString);
            Assert.IsTrue(json["InnerException"].IsNull);
        }

        [Test]
        public void ExceptionCanBeDeserializedFromPartialData()
        {
            var json = new JsonObject()
                .Add("ClassName", "System.InvalidOperationException")
                .Add("HResult", 123)
                .Add("Message", "Lorem ipsum dolor")
                .Add("StackTraceString", "foo bar");
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(
                typeof(InvalidOperationException),
                deserialized.GetType()
            );
            Assert.AreEqual("Lorem ipsum dolor", deserialized.Message);
            Assert.AreEqual("foo bar", deserialized.StackTrace);
            Assert.AreEqual(123, deserialized.HResult);
        }

        [Test]
        public void InnerExceptionIsSerialized()
        {
            var inner = MakeItThrown(new InvalidOperationException());
            var original = MakeItThrown(new StubException(inner));
            var json = Serializer.ToJson(original);
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(original.ToString(), deserialized.ToString());
            Assert.AreEqual(original.GetType(), deserialized.GetType());
            Assert.AreEqual(inner.ToString(), deserialized.InnerException?.ToString());
            Assert.AreEqual(inner.GetType(), deserialized.InnerException?.GetType());
        }

        [Test]
        public void ThrowingDeserializedExceptionPreservesStackTrace()
        {
            var original = MakeItThrown(new StubException());
            var json = Serializer.ToJson(original);
            
            var deserialized = Serializer.FromJson<Exception>(json);

            // stack deserialization just works
            Assert.AreEqual(original.StackTrace, deserialized.StackTrace);
            
            // just like MakeItThrown, but different so that the stack traces differ
            try
            {
                throw deserialized;
            }
            catch (Exception e)
            {
                Assert.AreSame(e, deserialized);
            }
         
            // stack is not the same - more info is appended
            Assert.AreNotEqual(original.StackTrace, deserialized.StackTrace);
            
            // but it still contains the original stack trace
            StringAssert.Contains(original.StackTrace, deserialized.StackTrace);
        }
        
        [Test]
        public void NonInstantiableExceptionWillBeWrapped()
        {
            /*
                // How to produce the serialized value
                var original = MakeItThrown(new NonExistingStubException());
                var json = Serializer.ToJson(original);
                NonExistingStubExceptionSerialized = json.ToString();
            */
            
            JsonValue json = JsonValue.Parse(
                NonExistingStubExceptionSerialized
            );
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(typeof(SerializedException), deserialized.GetType());
            Assert.AreEqual(
                json.ToString(),
                ((SerializedException)deserialized).SerializedValue.ToString()
            );
            StringAssert.Contains(
                "Type FrameworkTests.Serialization.ExceptionSerialization" +
                "Test+NonExistingStubException wasn't found.",
                deserialized.InnerException?.ToString()
            );
        }
        
        [Test]
        public void CompletelyMalformedExceptionWillBeWrapped()
        {
            var json = new JsonObject()
                .Add("foo", "bar")
                .Add("baz", 123);
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(typeof(SerializedException), deserialized.GetType());
            Assert.AreEqual(
                json.ToString(),
                ((SerializedException)deserialized).SerializedValue.ToString()
            );
            StringAssert.Contains(
                "Exception of type null makes no sense.",
                deserialized.InnerException?.ToString()
            );
        }
        
        [Test]
        public void ExceptionCanBeSerializedAndDeserialized()
        {
            var original = MakeItThrown(new StubException());
            var json = Serializer.ToJson(original);
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(original.ToString(), deserialized.ToString());
            Assert.AreEqual(original.GetType(), deserialized.GetType());
        }

        [Test]
        public void ExceptionWithCustomSerializedValuesCanBeSerializedAndDeserialized()
        {
            var original = MakeItThrown(new ArgumentException("foobar"));
            var json = Serializer.ToJson(original);
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(original.ToString(), deserialized.ToString());
            Assert.AreEqual(original.GetType(), deserialized.GetType());
            Assert.AreEqual(
                ((ArgumentException)original).ParamName,
                ((ArgumentException)deserialized).ParamName
            );
        }

        [Test]
        public void SerializedExceptionCanBeDotNetSerialized()
        {
            var payload = new JsonObject()
                .Add("foo", "bar")
                .Add("baz", 42);

            var original = new SerializedException(payload);
            var json = ExceptionSerializer.LegacyToJson(original);
            var deserialized = ExceptionSerializer.LegacyFromJson(json);

            Assert.AreEqual(typeof(SerializedException), deserialized.GetType());
            Assert.AreEqual(
                original.SerializedValue.ToString(),
                ((SerializedException)deserialized).SerializedValue.ToString()
            );
        }

        [Test]
        public void ClassNameHasToBeAnExceptionType()
        {
            var json = new JsonObject()
                .Add(
                    "ClassName",
                    "FrameworkTests.Serialization.ExceptionSerializationTest"
                )
                .Add("Message", "Lorem ipsum");
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(typeof(SerializedException), deserialized.GetType());
            Assert.AreEqual(
                json.ToString(),
                ((SerializedException)deserialized).SerializedValue.ToString()
            );
            StringAssert.Contains(
                "Type FrameworkTests.Serialization.ExceptionSerializationTest " +
                "is not an exception",
                deserialized.InnerException?.ToString()
            );
        }
        
        #endregion
        
        #region "Reading outside-of-c# generated exceptions"

        [Test]
        public void ExceptionCanBeCreatedOutsideOfCSharp()
        {
            var json = new JsonObject()
                .Add("ClassName", "System.TimeoutException")
                .Add("Message", "Lorem ipsum")
                .Add("StackTraceString", "   during Facet@Method execution");
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(
                typeof(TimeoutException),
                deserialized.GetType()
            );
            Assert.AreEqual("Lorem ipsum", deserialized.Message);
            Assert.AreEqual(
                "   during Facet@Method execution",
                deserialized.StackTrace
            );
            StringAssert.Contains(
                "   during Facet@Method execution",
                deserialized.ToString()
            );
            StringAssert.Contains(
                "Lorem ipsum",
                deserialized.ToString()
            );
            StringAssert.Contains(
                "System.TimeoutException",
                deserialized.ToString()
            );
        }
        
        #endregion
        
        #region "Reading legacy exceptions"
        
        [Test]
        public void LegacyExceptionsCanBeDeserialized()
        {
            var original = MakeItThrown(new StubException());
            var json = ExceptionSerializer.LegacyToJson(original);

            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(original.ToString(), deserialized.ToString());
            Assert.AreEqual(original.GetType(), deserialized.GetType());
        }

        [Test]
        public void LegacyNonExistingExceptionGetsWrapped()
        {
            /*
                // How to produce the serialized value
                var original = MakeItThrown(new NonExistingStubException());
                var json = ExceptionSerializer.LegacyToJson(original);
                NonExistingStubExceptionLegacySerialized = json.ToString();
            */
            
            JsonValue json = JsonValue.Parse(
                NonExistingStubExceptionLegacySerialized
            );
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(typeof(SerializedException), deserialized.GetType());
            Assert.AreEqual(
                json.ToString(),
                ((SerializedException)deserialized).SerializedValue.ToString()
            );
            StringAssert.Contains(
                "Unable to load type FrameworkTests.Serialization.Exception" +
                "SerializationTest+NonExistingStubException required for deserialization.",
                deserialized.InnerException?.ToString()
            );
        }
        
        #endregion
    }
}