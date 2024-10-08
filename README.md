# SatisfactoryClient
## Description
A client utilizing the HTTPS API provided by Satisfactory servers. for documentation on the API, see the [wiki docs](https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API).


## Getting a Authentication Token
There are a few ways of getting an authentication token. If the server has already been create and setup, you can run the `server.GenerateAPIToken` command using the in-game server console to get a token. If the server has not been setup, you can use the `ClaimServer` function to claim the server and generate an admin token.

### Using the in-game console 
1. Click on the Server Manager in the main menu.
2. Open the server and authenticate if required.
3. Open the server console and run the `server.GenerateAPIToken` command.
4. Copy the given token out of the console.

![generating token from in-game console](https://i.imgur.com/tvrydbX.png "Generating token from in-game consolve")

## Basic Example
```csharp
using SatisfactorySdk;

namespace ExampleApplication
{
	public class Program
	{
		public static async void Main(string[] args)
		{
			// Instantiate a new client. 
			// NOTE: This does not perform any commands nor test the connection to the server.
			var client = new SatisfactoryClient(
				"127.0.0.1",
				7777,
				authToken: "API_KEY"
			);
			
			// Perform a health check function on the server.
			// NOTE: This can throw an exception if a non-standard error occurs (e.g. Timeout, SSL, etc.)
			var healthCheckResponse = await client.HealthCheckAsync();
			
			// Print out the status code returned from the server.
			Console.WriteLine(healthCheckResponse.StatusCode);
			
			// You can check if a standard error was returned by the server (status code will still be 200).
			if (healthCheckResponse.HasError) 
			{
				// ClientResponse.ErrorResponse will contain all the information about the error.
				Console.WriteLine(healthCheckResponse.ErrorResponse.ErrorCode);
			}
			
			// Print out the health returned from the server.
			Console.WriteLine(healthCheckResponse.RequestResponse.Health);
		}
	}
}
```

## Advanced Examples
### Setting up a brand new server
```csharp
var client = new SatisfactoryClient("127.0.0.1", 7777);

// Perform the initial login to get a admin token
var loginResponse = await client.PasswordlessLoginAsync(PrivilegeLevelEnum.InitialAdmin);

// Set the client's auth token to what was received before
client.TrySetAuthToken(loginResponse.RequestResponse.Data.AuthenticationToken);

// Claim the server and set the admin password for the server
var claimServerResponse = await client.ClaimServerAsync("Test Server", "Admin1234!");

// Login to the server using the newly set admin password
loginResponse = await client.PasswordLoginAsync("Admin1234!", PrivilegeLevelEnum.Administrator);

// From this point, you can begin the rest of set up (i.e. settings, saves, sessions, etc.)
```

### Utilizing non-standard configurations
```csharp
var client = new SatisfactoryClient(
	"127.0.0.1",
	7788,
	authToken: "API_KEY", // Can pass the API bearer token in from instatiation rather than performing a login
	trustSelfSignedCerts: true, // You can either trust or not trust the self-signed certificates
	usePort: false, // You an decide to not use the port in the connection string (i.e. https://127.0.0.1/api/v1)
	client: new HttpClient(), // You can pass in the HTTP Client to be used by the Satisfactory Client (allows mocked responses for unit testing)
	logger: new Logger() // You can pass in your own logger for the Satisfactory Client to use
);

// You can also provide the full connection string of the server API
var client = new SatisfactoryClient(
	"https://127.0.0.1:7777/api/v1",
	authToken: "API_KEY", // Can pass the API bearer token in from instatiation rather than performing a login
	trustSelfSignedCerts: true, // You can either trust or not trust the self-signed certificates
	client: new HttpClient(), // You can pass in the HTTP Client to be used by the Satisfactory Client (allows mocked responses for unit testing)
	logger: new Logger() // You can pass in your own logger for the Satisfactory Client to use
);
```

## Requesting new features or reporting bugs
Please submit an issue in Github and mark either as a bug or enhancement.

