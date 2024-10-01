using SatisfactoryClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SatisfactoryClient.DTO;
using System.Net.Http.Headers;
using Moq.Protected;

namespace SatisfactoryClientTests
{
    public class RequestSerializationTests
    {
        private HttpsClient _client;

        [SetUp] 
        public void SetUp() 
        {
            _client = new HttpsClient("127.0.0.1");
        }

        [Test]
        public void SerializeEmptyData()
        {
            HttpContent content = _client.SerializeRequestData<HealthCheckRequest>("HealthCheck");

            Assert.That(content.Headers.ContentType.ToString(), Is.EqualTo("application/json"));
            Assert.IsTrue(content.ReadAsStringAsync().Result.Contains("HealthCheck"));
            Assert.IsTrue(content.ReadAsStringAsync().Result.Contains("clientCustomData"));
        }

        [Test]
        public void SerializeData()
        {
            HttpContent content = _client.SerializeRequestData<HealthCheckRequest>("HealthCheck", new HealthCheckRequest
            {
                ClientCustomData = "TEST1234"
            });

            Assert.That(content.Headers.ContentType.ToString(), Is.EqualTo("application/json"));
            Assert.IsTrue(content.ReadAsStringAsync().Result.Contains("HealthCheck"));
            Assert.IsTrue(content.ReadAsStringAsync().Result.Contains("clientCustomData"));
            Assert.IsTrue(content.ReadAsStringAsync().Result.Contains("TEST1234"));
        }

        [Test]
        public void NoFunctionGiven()
        {
            Assert.Throws<InvalidDataException>(() => _client.SerializeRequestData<HealthCheckRequest>(""));
        }
    }
}
