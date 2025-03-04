using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LightJson;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Unisave.Exceptions;
using Unisave.HttpClient;

namespace FrameworkTests.HttpClient
{
    [TestFixture]
    public class FakingTest
    {
        private Mock<HttpMessageHandler> handler;
        private Factory factory;
        private HttpRequestMessage request;
        
        [SetUp]
        public void SetUp()
        {
            handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback((HttpRequestMessage msg, CancellationToken token) => {
                    request = msg;
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
                .Verifiable();

            factory = new Factory(
                new System.Net.Http.HttpClient(handler.Object)
            );

            request = null; // reset request capture
        }

        [Test]
        public void ItFakesResponses()
        {
            factory.Fake();

            var response = factory.PendingRequest().Post("http://some-url.com/");
            
            Assert.AreEqual(200, response.Status);
            Assert.IsTrue(response.IsOk);
            Assert.IsTrue(response.IsSuccessful);
            Assert.IsNull(response.Body());

            Assert.DoesNotThrow(() => {
                response.Throw();
            });
            
            Assert.IsNull(response.Body());
            Assert.IsNull(response.Json());
            
            // The request wasn't actually sent anywhere
            Assert.IsNull(request);
        }

        [Test]
        public void ItFakesSpecificUrls()
        {
            factory.Fake("github.com/*");
            
            factory.PendingRequest().Post("https://github.com/");
            
            // The request wasn't actually sent anywhere
            Assert.IsNull(request);
        }

        [Test]
        public void ItLetsRequestsThroughToNonFakedUrls()
        {
            factory.Fake("github.com/*");
            
            factory.PendingRequest().Post("https://bitbucket.com/");
            
            // The request was sent
            Assert.IsNotNull(request);
        }

        [Test]
        public void ItStubsResponses()
        {
            factory.Fake("github.com/*", Response.Create(new JsonObject {
                ["foo"] = "bar"
            }, 401));
            
            var response = factory.PendingRequest().Post("http://github.com/");
            
            Assert.AreEqual(401, response.Status);
            Assert.IsTrue(response.Failed);
            Assert.IsTrue(response.IsClientError);
            Assert.IsFalse(response.IsServerError);
            Assert.IsFalse(response.IsSuccessful);
            Assert.IsFalse(response.IsOk);
            Assert.IsFalse(response.IsRedirect);
            Assert.AreEqual(
                "{'foo':'bar'}".Replace("'", "\""),
                response.Body()
            );
            Assert.AreEqual(
                "{'foo':'bar'}".Replace("'", "\""),
                response.Json().ToString()
            );
            Assert.AreEqual("bar", response["foo"].AsString);
            
            // The request wasn't actually sent anywhere
            Assert.IsNull(request);
        }

        [Test]
        public void ItStubsResponsesViaCallback()
        {
            factory.Fake(
                "github.com/*",
                (Request r) => Response.Create(
                    new JsonObject {
                        ["response-foo"] = r["request-foo"]
                    },
                    500
                )
            );
            
            var response = factory.PendingRequest()
                .Post("http://github.com/", new JsonObject {
                    ["request-foo"] = "payload!"
                });
            
            Assert.AreEqual(500, response.Status);
            Assert.IsTrue(response.Failed);
            Assert.IsTrue(response.IsServerError);
            Assert.IsFalse(response.IsClientError);
            Assert.IsFalse(response.IsSuccessful);
            Assert.IsFalse(response.IsOk);
            Assert.IsFalse(response.IsRedirect);
            
            Assert.AreEqual("payload!", response["response-foo"].AsString);
            
            // The request wasn't actually sent anywhere
            Assert.IsNull(request);
        }

        [Test]
        public void CallbackReturningNullDoesNotStub()
        {
            factory.Fake((Request r) => null);
            
            factory.PendingRequest().Post("http://github.com/");
            
            // The request was sent
            Assert.IsNotNull(request);
        }

        [Test]
        public void ItCanFakeSequenceAndFailWhenEmpty()
        {
            factory.Fake(
                new ResponseSequence()
                    .Push("First!")
                    .PushStatus(404)
            );

            var response = factory.PendingRequest().Get("https://github.com/");
            Assert.AreEqual(200, response.Status);
            Assert.AreEqual("First!", response.Body());
            
            response = factory.PendingRequest().Get("https://github.com/");
            Assert.AreEqual(404, response.Status);
            Assert.IsNull(response.Body());
            
            Assert.Throws<InvalidOperationException>(() => {
                factory.PendingRequest().Get("https://github.com/");
            });
            
            // no request was actually sent
            Assert.IsNull(request);
        }

        [Test]
        public void ItCanReturnResponseWhenSequenceEmpty()
        {
            factory.Fake(
                new ResponseSequence()
                    .DontFailWhenEmpty()
            );
            
            var response = factory.PendingRequest().Get("https://github.com/");
            Assert.AreEqual(200, response.Status);
            
            response = factory.PendingRequest().Get("https://github.com/");
            Assert.AreEqual(200, response.Status);
            
            // no request was actually sent
            Assert.IsNull(request);
        }

        [Test]
        public void ItRecordsRequests()
        {
            factory.Fake();
            
            factory.PendingRequest()
                .WithHeaders(new Dictionary<string, string> {
                    ["X-My-Header"] = "foobar!"
                }).Get("https://github.com/");
            
            factory.PendingRequest()
                .Post("https://bitbucket.com/", new JsonObject {
                    ["foo"] = "bar"
                });
            
            factory.PendingRequest()
                .Post("http://form.com/", new Dictionary<string, string> {
                    ["foo"] = "bar"
                });
            
            // recording

            List<RequestResponsePair> records = factory.Recorded();
            
            Assert.AreEqual("https://github.com/", records[0].Request.Url);
            Assert.AreEqual("foobar!", records[0].Request.Header("X-My-Header"));
            
            Assert.AreEqual("https://bitbucket.com/", records[1].Request.Url);
            Assert.AreEqual("bar", records[1].Request["foo"].AsString);
            
            // assertions

            factory.AssertSent((request, response) =>
                request.HasHeader("X-My-Header", "foobar!") &&
                request.Url == "https://github.com/"
            );
            
            factory.AssertSent((request, response) =>
                request.Url == "https://bitbucket.com/" &&
                request["foo"] == "bar"
            );
            
            factory.AssertNotSent(request =>
                request.Url == "https://bitbucket.com/" &&
                request["foo"] == "baz"
            );
            
            factory.AssertSent((request, response) =>
                request.Url == "http://form.com/" &&
                request["foo"] == "bar"
            );

            Assert.Throws<UnisaveAssertionException>(() => {
                factory.AssertNothingSent();
            });
            
            // no request was actually sent
            Assert.IsNull(request);
        }

        [Test]
        public void IsAssertsNothingWasSent()
        {
            factory.Fake();
            
            Assert.DoesNotThrow(() => {
                factory.AssertNothingSent();
            });
            
            // no request was actually sent
            Assert.IsNull(request);
        }
    }
}