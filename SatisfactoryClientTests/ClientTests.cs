using SatisfactorySdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryClientTests
{
    public class ClientTests
    {
        [Test]
        public void ClientInstantiation()
        {
            SatisfactoryClient client = new SatisfactoryClient("127.0.0.1");
            Assert.That(client.FullConnectionString, Is.EqualTo("https://127.0.0.1:7777/api/v1"));
        }

        [Test]
        public void UniquePort()
        {
            SatisfactoryClient client = new SatisfactoryClient("127.0.0.1", port: 7878);
            Assert.That(client.FullConnectionString, Is.EqualTo("https://127.0.0.1:7878/api/v1"));
        }

        [Test]
        public void UsePort()
        {
            SatisfactoryClient client = new SatisfactoryClient("127.0.0.1", usePort: false);
            Assert.That(client.FullConnectionString, Is.EqualTo("https://127.0.0.1/api/v1"));
        }

        [Test]
        public void AuthToken()
        {
            HttpClient httpClient = new HttpClient();
            SatisfactoryClient client = new SatisfactoryClient("127.0.0.1", client: httpClient, authToken: "TEST1234");
            Assert.That(client.FullConnectionString, Is.EqualTo("https://127.0.0.1:7777/api/v1"));
            Assert.That(httpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo("TEST1234"));
        }

        [Test]
        public void InvalidPort()
        {
            Assert.Throws<InvalidDataException>(() => new SatisfactoryClient("127.0.0.1", port: 99999));
        }

        [Test]
        public void SetAuthToken()
        {
            HttpClient httpClient = new HttpClient();
            SatisfactoryClient client = new SatisfactoryClient("127.0.0.1", client: httpClient);
            Assert.IsTrue(client.TrySetAuthToken("TEST1234"));
            Assert.That(httpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo("TEST1234"));
        }

        [Test]
        public void SetAuthTokenEmptyString()
        {
            SatisfactoryClient client = new SatisfactoryClient("127.0.0.1");
            Assert.IsFalse(client.TrySetAuthToken(""));
        }
    }
}
