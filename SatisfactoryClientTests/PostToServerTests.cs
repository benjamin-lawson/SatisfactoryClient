using Moq;
using Moq.Contrib.HttpClient;
using SatisfactorySdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClientTests
{
    [Serializable]
    public class TestFunctionRequest
    {
        public string TestProperty1 { get; set; }
        public int TestProperty2 { get; set; }
    }

    [Serializable]
    public class TestFunctionResponse
    {
        [JsonPropertyName("testProperty1")] public string TestProperty1 { get; set; }
        [JsonPropertyName("testProperty_2")] public int TestProperty2 { get; set; }
    }

    public class PostToServerTests
    {
        private const string _ip = "127.0.0.1";

        public bool MatchRequestFunction(HttpRequestMessage request)
        {
            string bodyContent = request.Content.ReadAsStringAsync().Result;
            return bodyContent.Contains("TestFunction");
        }

        [Test]
        public async Task GenericResponseType()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", MatchRequestFunction)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"data\":{\"testProperty1\":\"TEST1\",\"testProperty_2\":123}}")
                });

            var client = new SatisfactoryClient(_ip, client: mockHandler.CreateClient());
            var response = await client.PostToServerAsync<TestFunctionRequest, TestFunctionResponse>("TestFunction");

            Assert.IsTrue(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(response.RequestResponse);
            Assert.That(response.RequestResponse.Data.TestProperty1, Is.EqualTo("TEST1"));
            Assert.That(response.RequestResponse.Data.TestProperty2, Is.EqualTo(123));
        }

        [Test]
        public async Task BoolResponseType()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", MatchRequestFunction)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"data\":{\"testProperty1\":\"TEST1\",\"testProperty_2\":123}}")
                });

            var client = new SatisfactoryClient(_ip, client: mockHandler.CreateClient());
            var response = await client.PostToServerAsync<TestFunctionRequest, bool>("TestFunction");

            Assert.IsTrue(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNull(response.RequestResponse);
        }

        [Test]
        public async Task ApiError()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", MatchRequestFunction)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"errorCode\":\"bad_request\",\"errorMessage\":\"That was a really bad request\"}")
                });

            var client = new SatisfactoryClient(_ip, client: mockHandler.CreateClient());
            var response = await client.PostToServerAsync<TestFunctionRequest, TestFunctionResponse>("TestFunction");

            Assert.IsFalse(response.IsSuccessful);
            Assert.IsTrue(response.HasError);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(response.ErrorResponse);
            Assert.That(response.ErrorResponse.ErrorCode, Is.EqualTo("bad_request"));
            Assert.That(response.ErrorResponse.ErrorMessage, Is.EqualTo("That was a really bad request"));
        }

        [Test]
        public async Task ExceptionInDataDeserialization()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", MatchRequestFunction)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"data\":1}")
                });

            var client = new SatisfactoryClient(_ip, client: mockHandler.CreateClient());
            var response = await client.PostToServerAsync<TestFunctionRequest, TestFunctionResponse>("TestFunction");

            Assert.IsFalse(response.IsSuccessful);
            Assert.IsTrue(response.HasError);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(response.ErrorResponse);
            Assert.That(response.ErrorResponse.ErrorCode, Is.EqualTo("unknown_exception"));
        }
    }
}
