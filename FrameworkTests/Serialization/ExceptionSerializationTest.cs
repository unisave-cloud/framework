using System;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization
{
    [TestFixture]
    public class ExceptionSerializationTest
    {
        [Test]
        public void ItSerializesExceptions()
        {
            Exception exception = null;

            try
            {
                throw new InvalidOperationException();
            }
            catch (Exception e)
            {
                Assert.AreEqual(
                    e.ToString(),
                    Serializer.FromJson(
                        Serializer.ToJson(e),
                        typeof(Exception)
                    ).ToString()
                );

                exception = e;
            }

            Assert.AreEqual(
                exception.ToString(),
                Serializer.FromJson(
                    Serializer.ToJson(exception),
                    typeof(Exception)
                ).ToString()
            );
        }
    }
}