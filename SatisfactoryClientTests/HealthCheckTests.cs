using Moq;
using Moq.Contrib.HttpClient;
using System.Net;
using SatisfactorySdk;

namespace SatisfactoryClientTests
{
    public class HealthCheckTests
    {
        private const string _ip = "127.0.0.1";

        [Test]
        public async Task SuccessfulRequest()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => TestUtilities.MatchRequestFunction("HealthCheck", pred))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"data\":{\"health\":\"healthy\",\"serverCustomData\":\"\"}}")
                });

            var client = new SatisfactoryClient(_ip, 7777, client: mockHandler.CreateClient());
            var response = await client.HealthCheckAsync("");

            Assert.IsTrue(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(response.RequestResponse);
            Assert.That(response.RequestResponse.Data.Health, Is.EqualTo("healthy"));
        }

        [Test]
        public async Task SlowResponse()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => TestUtilities.MatchRequestFunction("HealthCheck", pred))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"data\":{\"health\":\"slow\",\"serverCustomData\":\"\"}}")
                });

            var client = new SatisfactoryClient(_ip, 7777, client: mockHandler.CreateClient());
            var response = await client.HealthCheckAsync("");

            Assert.IsTrue(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(response.RequestResponse);
            Assert.That(response.RequestResponse.Data.Health, Is.EqualTo("slow"));
        }
    }
}
