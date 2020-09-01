using System.Collections.Generic;
using System.Net.Http;
using NUnit.Framework;
using Unisave.Http.Client;

namespace FrameworkTests.Http.Client
{
    [TestFixture]
    public class AuthorizationTest
    {
        private Factory factory;
        
        [SetUp]
        public void SetUp()
        {
            factory = new Factory(
                new HttpClient()
            );
        }
        
        [Test]
        public void ItDoesBasicAuthentication()
        {
            bool tested = false;
            
            factory.Fake(request => {
                Assert.AreEqual(
                    "Basic am9obkBkb2UuY29tOnNlY3JldA==",
                    request.Header("Authorization")
                );
                tested = true;
                return null;
            });

            factory.PendingRequest()
                .WithBasicAuth("john@doe.com", "secret")
                .Get("https://example.com/");
            
            Assert.IsTrue(tested);
        }

        [Test]
        public void BasicAuthDoesNotGetOverridenByHeaders()
        {
            bool tested = false;
            
            factory.Fake(request => {
                Assert.AreEqual(
                    "Basic am9obkBkb2UuY29tOnNlY3JldA==",
                    request.Header("Authorization")
                );
                tested = true;
                return null;
            });

            factory.PendingRequest()
                .WithBasicAuth("john@doe.com", "secret")
                .WithHeaders(new Dictionary<string, string> {
                    ["Authorization"] = "lorem ipsum"
                })
                .Get("https://example.com/");
            
            Assert.IsTrue(tested);
        }

        [Test]
        public void BasicAuthCanBeSentViaHeaders()
        {
            bool tested = false;
            
            factory.Fake(request => {
                Assert.AreEqual(
                    "Basic am9obkBkb2UuY29tOnNlY3JldA==",
                    request.Header("Authorization")
                );
                tested = true;
                return null;
            });

            factory.PendingRequest()
                .WithHeaders(new Dictionary<string, string> {
                    ["Authorization"] = "Basic am9obkBkb2UuY29tOnNlY3JldA=="
                })
                .Get("https://example.com/");
            
            Assert.IsTrue(tested);
        }
        
        [Test]
        public void ItDoesBearerTokenAuthentication()
        {
            bool tested = false;
            
            factory.Fake(request => {
                Assert.AreEqual(
                    "Bearer <cryptic-token-string>",
                    request.Header("Authorization")
                );
                tested = true;
                return null;
            });

            factory.PendingRequest()
                .WithToken("<cryptic-token-string>")
                .Get("https://example.com/");
            
            Assert.IsTrue(tested);
        }
    }
}