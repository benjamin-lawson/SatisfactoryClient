using SatisfactoryClient.DTO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SatisfactoryClient;
using Microsoft.Extensions.Logging;

namespace ConsoleApplication
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace).AddSimpleConsole();
            });
            var logger = factory.CreateLogger<HttpsClient>();

            var client = new HttpsClient(
                "192.168.1.105", 
                authToken: "ewoJInBsIjogIkFQSVRva2VuIgp9.0B06ACAC3AF1553B885C3D30B77150A6BDA281EEEC20C90B0EBF1984F728F29F4418A8AD668534CA631F4ED81D4D434D8E64AF60DECBF8541117B7DDA78F8642", 
                trustSelfSignedCerts: true, 
                logger: logger
            );
            var healthCheck = client.HealthCheckAsync().Result;
            PrintClientResponse(healthCheck);

            var authTokenVerify = client.VerifyAuthenticationTokenAsync().Result;
            PrintClientResponse(authTokenVerify);

            var serverState = client.QueryServerStateAsync().Result;
            PrintClientResponse(serverState);

            var serverOptions = client.GetServerOptionsAsync().Result;
            PrintClientResponse(serverOptions);

            var advancedGameSettings = client.GetAdvancedGameSettingsAsync().Result;
            PrintClientResponse(advancedGameSettings);

            var passwordlessLogin = client.PasswordlessLoginAsync(PrivilegeLevelEnum.Client).Result;
            PrintClientResponse(passwordlessLogin);

            var passwordLogin = client.PasswordLoginAsync("***", PrivilegeLevelEnum.Administrator).Result;
            PrintClientResponse(passwordlessLogin);

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();



            // Perform the initial login to get a admin token
            var loginResponse = client.PasswordlessLoginAsync(PrivilegeLevelEnum.InitialAdmin).Result;
            // Set the client's auth token to what was received before
            client.TrySetAuthToken(loginResponse.RequestResponse.Data.AuthenticationToken);
            // Claim the server and set the admin password for the server
            var claimServerResponse = client.ClaimServerAsync("Test Server", "Admin1234!");
            // Login to the server using the newly set admin password
            loginResponse = client.PasswordLoginAsync("Admin1234!", PrivilegeLevelEnum.Administrator).Result;
        }

        public static void PrintClientResponse<T>(ClientResponse<T> response)
        {
            Console.WriteLine(JsonSerializer.Serialize(response));
            Console.WriteLine();
        }
    }
}
