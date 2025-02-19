using System;
using System.Reflection;
using System.Runtime.Serialization;
using LightJson;
using NUnit.Framework;
using Unisave.Arango;
using Unisave.Exceptions;
using Unisave.Serialization;
using Unisave.Serialization.Exceptions;

namespace FrameworkTests.Serialization.Exceptions
{
    [TestFixture]
    public class ExceptionSerializationTest
    {
        // TODO: refactor by splitting it up into multiple test files
        
        [Serializable]
        private class StubException : Exception
        {
            public StubException() : base() { }
            public StubException(string msg) : base(msg) { }
            public StubException(Exception inner) : base(null, inner) { }
            protected StubException(SerializationInfo info, StreamingContext c)
                : base(info, c) { }
        }
        
        // [Serializable]
        // private class NonExistingStubException : Exception
        // {
        //     public NonExistingStubException() : base() { }
        //     protected NonExistingStubException(SerializationInfo info, StreamingContext c)
        //         : base(info, c) { }
        // }

        private const string NonExistingStubExceptionSerialized
            = "{\"ClassName\":\"FrameworkTests.Serialization.Exceptions.E" +
              "xceptionSerializationTest+NonExistingStubException\",\"Mes" +
              "sage\":null,\"Data\":null,\"InnerException\":null,\"HelpUR" +
              "L\":null,\"StackTraceString\":\"  at FrameworkTests.Serial" +
              "ization.Exceptions.ExceptionSerializationTest.MakeItThrown" +
              " (System.Exception e) [0x00002] in /home/jirka/unisave/fra" +
              "mework/FrameworkTests/Serialization/Exceptions/ExceptionSe" +
              "rializationTest.cs:74 \",\"RemoteStackTraceString\":null," +
              "\"RemoteStackIndex\":0,\"ExceptionMethod\":null,\"HResult\"" +
              ":-2146233088,\"Source\":\"FrameworkMonoTests\"}";
        
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
                Assert.That(deserialized, Is.SameAs(e));
            }
         
            // stack is not the same - more info is appended
            Assert.That(deserialized.StackTrace, Is.Not.EqualTo(original.StackTrace));
            
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
                "Type FrameworkTests.Serialization.Exceptions.ExceptionSerial" +
                "izationTest+NonExistingStubException wasn't found.",
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
        public void ClassNameHasToBeAnExceptionType()
        {
            var json = new JsonObject()
                .Add(
                    "ClassName",
                    typeof(ExceptionSerializationTest).FullName
                )
                .Add("Message", "Lorem ipsum");
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(typeof(SerializedException), deserialized.GetType());
            Assert.AreEqual(
                json.ToString(),
                ((SerializedException)deserialized).SerializedValue.ToString()
            );
            StringAssert.Contains(
                $"Type {typeof(ExceptionSerializationTest).FullName} " +
                "is not an exception",
                deserialized.InnerException?.ToString()
            );
        }
        
        [Test]
        [Ignore("Test is used to debug the serializer")]
        public void MostOfKnownExceptionsCanBeSerializedAndDeserialized()
        {
            int successful = 0;
            int total = 0;
            int ok = 0;
            
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            foreach (Type type in a.GetTypes())
            {
                if (!typeof(Exception).IsAssignableFrom(type))
                    continue;

                var defaultConstructor = type.GetConstructor(new Type[] { });
                
                // log those that cannot be created
                if (defaultConstructor == null)
                {
                    Console.WriteLine("X> " + type);
                    continue;
                }
                
                object instance;
                try
                {
                    instance = (Exception) defaultConstructor.Invoke(new object[] { });
                }
                catch
                {
                    Console.WriteLine("XC> " + type);
                    continue;
                }
                
                // count only those that can be created
                total += 1;

                // test serialization
                var original = MakeItThrown((Exception)instance);
                var json = Serializer.ToJson(original);
                
                var deserialized = Serializer.FromJson<Exception>(json);

                if (deserialized is SerializedException)
                {
                    Console.WriteLine("XS> " + type);
                    continue;
                }
                
                Assert.AreEqual(original.GetType(), deserialized.GetType());

                ok += 1;

                if (original.Message != deserialized.Message)
                {
                    Console.WriteLine("message> " + type);
                    continue;
                }
                
                if (original.StackTrace != deserialized.StackTrace)
                {
                    Console.WriteLine("trace> " + type);
                    continue;
                }
                
                if (original.ToString() != deserialized.ToString())
                {
                    Console.WriteLine("ToString> " + type);
                    continue;
                }
                
                successful += 1;
            }
            
            Console.WriteLine("Successful: " + successful);
            Console.WriteLine("Ok: " + ok);
            Console.WriteLine("Total: " + total);
            
            Assert.IsTrue((ok / (float)total) > 0.6);
        }
        
        #endregion
        
        #region "Testing specific exception for passing through the system"

        private void PassThrough<T>(
            T exception,
            out T original,
            out T deserialized
        ) where T : Exception
        {
            original = (T) MakeItThrown(exception);
            
            deserialized = (T) Serializer.FromJson<Exception>(
                Serializer.ToJson(original)
            );
        }
        
        [Test]
        public void PlainExceptionCanPassThrough()
        {
            PassThrough(
                new Exception("Lorem ipsum"),
                out Exception original,
                out Exception deserialized
            );
            
            Assert.AreEqual(original.ToString(), deserialized.ToString());
        }
        
        [Test]
        public void NullReferenceExceptionCanPassThrough()
        {
            PassThrough(
                new NullReferenceException("Lorem ipsum"),
                out NullReferenceException original,
                out NullReferenceException deserialized
            );
            
            Assert.AreEqual(original.ToString(), deserialized.ToString());
        }
        
        [Test]
        public void InvalidOperationExceptionCanPassThrough()
        {
            PassThrough(
                new InvalidOperationException("Lorem ipsum"),
                out InvalidOperationException original,
                out InvalidOperationException deserialized
            );
            
            Assert.AreEqual(original.ToString(), deserialized.ToString());
        }
        
        [Test]
        public void AggregateExceptionCanPassThrough()
        {
            PassThrough(
                new AggregateException(
                    "Lorem ipsum",
                    MakeItThrown(new InvalidOperationException("foo bar")),
                    MakeItThrown(new NullReferenceException("baz"))
                ),
                out AggregateException original,
                out AggregateException deserialized
            );

            Assert.AreEqual(original.ToString(), deserialized.ToString());
        }
        
        [Test]
        public void TargetInvocationExceptionCanPassThrough()
        {
            PassThrough(
                new TargetInvocationException(
                    MakeItThrown(new NullReferenceException())
                ),
                out TargetInvocationException original,
                out TargetInvocationException deserialized
            );

            Assert.AreEqual(original.ToString(), deserialized.ToString());
        }
        
        [Test]
        public void ArangoExceptionCanPassThrough()
        {
            PassThrough(
                new ArangoException(422, 1200, "Lorem ipsum dolor"),
                out ArangoException original,
                out ArangoException deserialized
            );
            
            Assert.AreEqual(original.ToString(), deserialized.ToString());
            Assert.AreEqual(original.ErrorMessage, deserialized.ErrorMessage);
            Assert.AreEqual(original.ErrorNumber, deserialized.ErrorNumber);
            Assert.AreEqual(original.HttpStatus, deserialized.HttpStatus);
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

        [Test]
        public void UnisaveExceptionCanBeCreatedOutsideOfCsharp()
        {
            var json = new JsonObject()
                .Add("ClassName", "Unisave.Exceptions.UnisaveException")
                .Add("Message", "foo")
                .Add("StackTraceString", "bar");
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(typeof(UnisaveException), deserialized.GetType());
            var exception = (UnisaveException) deserialized;
            
            Assert.AreEqual("foo", exception.Message);
            Assert.AreEqual("bar", exception.StackTrace);
        }
        
        [Test]
        public void BackendExecutionTimeoutExceptionCanBeCreatedOutsideOfCsharp()
        {
            var json = new JsonObject()
                .Add("ClassName", "Unisave.Exceptions.BackendExecutionTimeoutException")
                .Add("Message", "foo")
                .Add("StackTraceString", "bar");
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(typeof(BackendExecutionTimeoutException), deserialized.GetType());
            var exception = (BackendExecutionTimeoutException) deserialized;
            
            Assert.AreEqual("foo", exception.Message);
            Assert.AreEqual("bar", exception.StackTrace);
        }
        
        [Test]
        public void StackOverflowExceptionCanBeCreatedOutsideOfCsharp()
        {
            var json = new JsonObject()
                .Add("ClassName", "System.StackOverflowException")
                .Add("Message", "foo")
                .Add("StackTraceString", "bar");
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(typeof(StackOverflowException), deserialized.GetType());
            var exception = (StackOverflowException) deserialized;
            
            Assert.AreEqual("foo", exception.Message);
            Assert.AreEqual("bar", exception.StackTrace);
        }
        
        [Test]
        public void OutOfMemoryExceptionCanBeCreatedOutsideOfCsharp()
        {
            var json = new JsonObject()
                .Add("ClassName", "System.OutOfMemoryException")
                .Add("Message", "foo")
                .Add("StackTraceString", "bar");
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(typeof(OutOfMemoryException), deserialized.GetType());
            var exception = (OutOfMemoryException) deserialized;
            
            Assert.AreEqual("foo", exception.Message);
            Assert.AreEqual("bar", exception.StackTrace);
        }
        
        #endregion
        
        #region "Reading legacy exceptions"

        [Test]
        public void LegacyExceptionDeserializationHasBeenRemoved()
        {
            // The legacy deserialization was removed, because it used the
            // BinaryFormatter class which is insecure and was to be removed
            // in .NET 9, plus throwing exceptions in previous .NET versions.
            
            // Legacy exceptions were serialized to a base64-encoded string,
            // so the way to detect a legacy exception is to see that we are
            // trying to deserialize from a JSON string instead of an object.

            JsonValue json = "dummy-base64-string-here=";
            
            var deserialized = Serializer.FromJson<Exception>(json);
            
            Assert.AreEqual(typeof(SerializedException), deserialized.GetType());
            Assert.AreEqual(
                json.ToString(),
                ((SerializedException)deserialized).SerializedValue.ToString()
            );
            StringAssert.Contains(
                "Legacy exception deserialization has been removed.",
                deserialized.InnerException?.ToString()
            );
        }
        
        #endregion
    }
}