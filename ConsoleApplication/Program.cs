namespace ConsoleApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new SatisfactoryClient.HttpsClient("*****", "****", trustSelfSignedCerts: true);
            var healthCheck = client.HealthCheckAsync().Result;
            var authTokenVerify = client.VerifyAuthenticationTokenAsync().Result;
            var serverState = client.QueryServerStateAsync().Result;
            var serverOptions = client.GetServerOptionsAsync().Result;
            var advancedGameSettings = client.GetAdvancedGameSettingsAsync().Result;
        }
    }
}
