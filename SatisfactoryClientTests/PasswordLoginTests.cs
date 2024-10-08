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
    public class PasswordLoginTests
    {
        private const string _ip = "127.0.0.1";

        [Test]
        public async Task PasswordLoginCorrectPassword()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => TestUtilities.MatchRequestFunction("PasswordLogin", pred))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"data\":{\"authenticationToken\":\"Token1234\"}}")
                });
            var client = new SatisfactoryClient(_ip, 7777, client: mockHandler.CreateClient());

            var response = await client.PasswordLoginAsync("Test1234!", PrivilegeLevelEnum.InitialAdmin);

            Assert.IsTrue(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(string.IsNullOrEmpty(response.ErrorResponse?.ErrorCode));
            Assert.IsTrue(!string.IsNullOrEmpty(response.RequestResponse?.Data.AuthenticationToken));
        }

        [Test]
        public async Task PasswordLoginNotAllowed()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => TestUtilities.MatchRequestFunction("PasswordLogin", pred))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(TestUtilities.GenerateErrorString("wrong_password", "Provided Password did not match any of the passwords set for this Dedicated Server"))
                });
            var client = new SatisfactoryClient(_ip, 7777, client: mockHandler.CreateClient());

            var response = await client.PasswordLoginAsync("Wrong123", PrivilegeLevelEnum.InitialAdmin);

            Assert.IsFalse(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(!string.IsNullOrEmpty(response.ErrorResponse?.ErrorCode));
            Assert.That(response.ErrorResponse?.ErrorCode, Is.EqualTo("wrong_password"));
            Assert.IsTrue(response.RequestResponse?.Data is null);
        }
    }
}
