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
    public class SetClientPasswordTests
    {
        private const string _ip = "127.0.0.1";

        [Test]
        public async Task SetClientPasswordSuccessful()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => TestUtilities.MatchRequestFunction("SetClientPassword", pred))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var client = new SatisfactoryClient(_ip, 7777, client: mockHandler.CreateClient());

            var response = await client.SetClientPasswordAsync("Test1234!");

            Assert.IsTrue(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(string.IsNullOrEmpty(response.ErrorResponse?.ErrorCode));
        }

        [Test]
        public async Task RenameServerNotAllowed()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => TestUtilities.MatchRequestFunction("SetClientPassword", pred))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(TestUtilities.GenerateErrorString("password_in_use", "Same password is already used as Admin Password"))
                });
            var client = new SatisfactoryClient(_ip, 7777, client: mockHandler.CreateClient());

            var response = await client.SetClientPasswordAsync("BadPass");

            Assert.IsFalse(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(!string.IsNullOrEmpty(response.ErrorResponse?.ErrorCode));
            Assert.That(response.ErrorResponse?.ErrorCode, Is.EqualTo("password_in_use"));
            Assert.IsTrue(response.RequestResponse?.Data is null);
        }
    }
}
