using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LightJson;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Unisave.Http.Client;

namespace FrameworkTests.Http.Client
{
    [TestFixture]
    public class PendingRequestTest
    {
        private HttpClient client;
        private Mock<HttpMessageHandler> handler;
        private PendingRequest pr;
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

            client = new HttpClient(handler.Object);
            
            pr = new PendingRequest(client);
        }
        
        [Test]
        public void ItMakesGetRequest()
        {
            const string url = "https://github.com/foo/bar";
            
            pr.Get(url);

            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual(url, request.RequestUri.ToString());
        }
        
        [Test]
        public void ItMakesGetRequestWithQuery()
        {
            const string url = "https://github.com/foo/bar?this-gets-removed";
            
            pr.Get(url, new Dictionary<string, string> {
                ["foo"] = "bar baz",
                ["asd"] = "5"
            });

            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual(
                "https://github.com/foo/bar?foo=bar+baz&asd=5",
                request.RequestUri.ToString()
            );
        }
        
        [Test]
        public void ItMakesGetRequestWithQueryInUrlArgument()
        {
            const string url = "https://github.com/foo/bar?lorem=ipsum";
            
            pr.Get(url);

            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual(url, request.RequestUri.ToString());
        }
        
        [Test]
        public void ItSendsCustomHeaders()
        {
            pr.WithHeaders(new Dictionary<string, string> {
                ["X-My-Header"] = "My header value"
            }).Get("http://github.com/");
            
            Assert.AreEqual(
                "My header value",
                request.Headers.GetValues("X-My-Header").First()
            );
        }
        
        [Test]
        public void SettingContentHeaderForTheRequestThrowsException()
        {
            Assert.Throws<ArgumentException>(() => {
                
                pr.WithHeaders(new Dictionary<string, string> {
                    ["Content-Type"] = "awesome/stuff"
                }).Get("http://github.com/");
                
            });
        }
        
        [Test]
        public void ItMakesEmptyPostRequest()
        {
            const string url = "https://github.com/foo/bar";
            
            pr.Post(url);

            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual(url, request.RequestUri.ToString());
            Assert.IsNull(request.Content);
        }
        
        [Test]
        public async Task ItMakesFormPostRequest()
        {
            const string url = "https://github.com/foo/bar";
            
            pr.Post(url, new Dictionary<string, string> {
                ["lorem"] = "ipsum",
                ["dolor"] = "sit"
            });

            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual(url, request.RequestUri.ToString());
            Assert.IsInstanceOf<FormUrlEncodedContent>(request.Content);
            Assert.AreEqual(
                "lorem=ipsum&dolor=sit",
                await request.Content.ReadAsStringAsync()
            );
        }
        
        [Test]
        public async Task ItMakesJsonPostRequest()
        {
            const string url = "https://github.com/foo/bar";
            
            pr.Post(url, new JsonObject {
                ["lorem"] = "ipsum",
                ["dolor"] = "sit"
            });

            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual(url, request.RequestUri.ToString());
            Assert.IsInstanceOf<StringContent>(request.Content);
            Assert.AreEqual(
                @"{'lorem':'ipsum','dolor':'sit'}".Replace("'", "\""),
                await request.Content.ReadAsStringAsync()
            );
            Assert.AreEqual(
                "application/json",
                request.Content.Headers.ContentType.MediaType
            );
        }
        
        [Test]
        public async Task ItMakesMultipartPostRequest()
        {
            const string url = "https://github.com/foo/bar";
            
            pr.Attach(
                "part-name",
                new StringContent("pixels!"),
                "file-name.jpg",
                new Dictionary<string, string> {
                    ["X-My-Header"] = "header-value"
                }
            ).Attach(
                "other-part",
                new StringContent("another file content!"),
                "other-file.jpg"
            ).Attach(
                "third-part",
                new JsonObject().Add("foo", "bar")
            ).Post(url);

            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual(url, request.RequestUri.ToString());
            Assert.IsInstanceOf<MultipartFormDataContent>(request.Content);
            string content = await request.Content.ReadAsStringAsync();
            StringAssert.Contains("part-name", content);
            StringAssert.Contains("file-name.jpg", content);
            StringAssert.Contains("pixels!", content);
            StringAssert.Contains("X-My-Header", content);
            StringAssert.Contains("header-value", content);
            StringAssert.Contains("another file content!", content);
            StringAssert.Contains("application/json", content);
        }
        
        [Test]
        public void ItMakesAnyRequestViaSend()
        {
            const string url = "https://github.com/foo/bar";
            
            pr.Send(HttpMethod.Head, url);

            Assert.AreEqual(HttpMethod.Head, request.Method);
            Assert.AreEqual(url, request.RequestUri.ToString());
            Assert.IsNull(request.Content);
        }
        
        [Test]
        public void ItMakesEmptyPutRequest()
        {
            const string url = "https://github.com/foo/bar";
            
            pr.Put(url);

            Assert.AreEqual(HttpMethod.Put, request.Method);
            Assert.AreEqual(url, request.RequestUri.ToString());
            Assert.IsNull(request.Content);
        }
        
        [Test]
        public void ItMakesEmptyPatchRequest()
        {
            const string url = "https://github.com/foo/bar";
            
            pr.Patch(url);

            Assert.AreEqual(new HttpMethod("PATCH"), request.Method);
            Assert.AreEqual(url, request.RequestUri.ToString());
            Assert.IsNull(request.Content);
        }
        
        [Test]
        public void ItMakesEmptyDeleteRequest()
        {
            const string url = "https://github.com/foo/bar";
            
            pr.Delete(url);

            Assert.AreEqual(HttpMethod.Delete, request.Method);
            Assert.AreEqual(url, request.RequestUri.ToString());
            Assert.IsNull(request.Content);
        }
    }
}