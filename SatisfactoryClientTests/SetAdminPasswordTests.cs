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
    public class SetAdminPasswordTests
    {
        private const string _ip = "127.0.0.1";

        [Test]
        public async Task SetAdminPasswordSuccessful()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => TestUtilities.MatchRequestFunction("SetAdminPassword", pred))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"data\":{\"authenticationToken\":\"TestToken1234!\"}}")
                });

            var client = new SatisfactoryClient(_ip, 7777, client: mockHandler.CreateClient());

            var response = await client.SetAdminPasswordAsync("Test1234!");

            Assert.IsTrue(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(string.IsNullOrEmpty(response.ErrorResponse?.ErrorCode));
            Assert.That(response.RequestResponse?.Data.AuthenticationToken, Is.EqualTo("TestToken1234!"));
        }

        [Test]
        public async Task SetAdminPasswordEmptyString()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => TestUtilities.MatchRequestFunction("SetAdminPassword", pred))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(TestUtilities.GenerateErrorString("cannot_reset_admin_password", "Attempt to set Password to empty string. Admin Password cannot be reset"))
                });
            var client = new SatisfactoryClient(_ip, 7777, client: mockHandler.CreateClient());

            var response = await client.SetAdminPasswordAsync("");

            Assert.IsFalse(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(!string.IsNullOrEmpty(response.ErrorResponse?.ErrorCode));
            Assert.That(response.ErrorResponse?.ErrorCode, Is.EqualTo("cannot_reset_admin_password"));
            Assert.IsTrue(response.RequestResponse?.Data is null);
        }
    }
}
