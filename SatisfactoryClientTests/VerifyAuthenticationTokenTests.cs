using Moq;
using Moq.Contrib.HttpClient;
using SatisfactorySdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryClientTests
{
    public class VerifyAuthenticationTokenTests
    {
        private const string _ip = "127.0.0.1";
        private SatisfactoryClient _client;

        public bool MatchRequestFunction(HttpRequestMessage request)
        {
            string bodyContent = request.Content.ReadAsStringAsync().Result;
            return bodyContent.Contains("VerifyAuthenticationToken");
        }

        [SetUp]
        public void Setup()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => MatchRequestFunction(pred) && pred.Headers.Authorization.Parameter == "Allowed1!")
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => MatchRequestFunction(pred) && pred.Headers.Authorization.Parameter != "Allowed1!")
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            _client = new SatisfactoryClient(_ip, 7777, client: mockHandler.CreateClient());
        }

        [Test]
        public async Task GoodAuthTokenPassed()
        {
            Assert.IsTrue(_client.TrySetAuthToken("Allowed1!"));
            var response = await _client.VerifyAuthenticationTokenAsync();

            Assert.IsTrue(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task BadAuthTokenPassed()
        {
            Assert.IsTrue(_client.TrySetAuthToken("BadActor"));
            var response = await _client.VerifyAuthenticationTokenAsync();

            Assert.IsFalse(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
