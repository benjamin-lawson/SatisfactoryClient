using SatisfactoryClient.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using SatisfactoryClient.DT;

namespace SatisfactoryClient
{
    public class HttpsClient
    {
        private string _ip;
        private int _port = 7777;
        private string _authToken;
        private HttpClient _httpClient;
        private ILogger _logger;
        private string _fullConnectionString = "";

        public string IP => _ip;
        public int Port => _port;
        public string FullConnectionString => _fullConnectionString;

        public HttpsClient(string ip, string authToken = "", int port = 7777, bool trustSelfSignedCerts = false, bool usePort = true, HttpClient? client = null, ILogger? logger = null) 
        {
            _ip = ip;

            if (port < 0 || port > 65535)
            {
                throw new InvalidDataException($"Port {port} is an invalid port number");
            }

            _port = port;

            if (string.IsNullOrEmpty(authToken))
            {
                throw new InvalidDataException($"Auth token cannot be empty or null string");
            }

            _authToken = authToken;

            if (client != null)
            {
                _httpClient = client;
                return;
            }

            if (trustSelfSignedCerts)
            {
                var handler = new HttpClientHandler();
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
                _httpClient = new HttpClient(handler);
            }
            else
            {
                _httpClient = new HttpClient();
            }

            if (!string.IsNullOrEmpty(authToken))
            {
                _authToken = authToken;
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
            }
            

            if (logger is null)
            {
                using ILoggerFactory factory = LoggerFactory.Create(builder => 
                {
                    builder.SetMinimumLevel(LogLevel.Debug).AddSimpleConsole();
                });
                _logger= factory.CreateLogger<HttpsClient>();
            }
            else
            {
                _logger = logger;
            }

            _fullConnectionString = usePort ? $"https://{IP}:{Port}/api/v1" : $"https://{IP}/api/v1";
        }

        /// <summary>
        /// Sets the bearer authentication token to be used by the client.
        /// </summary>
        /// <param name="authToken">The authentication token retrieved by the in-game console or auth function</param>
        /// <returns></returns>
        public bool TrySetAuthToken(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                return false;
            }

            _authToken = authToken;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
            _logger.LogDebug("Set new auth token");
            return true;
        }

        /// <summary>
        /// Serializes the data and builds the POST body content.
        /// </summary>
        /// <typeparam name="T">The request data type</typeparam>
        /// <param name="functionName">The name of the function to run on the server</param>
        /// <param name="data">[Optional] The data to be passed to the server</param>
        /// <returns></returns>
        internal HttpContent SerializeRequestData<T>(string functionName, T? data = default)
        {
            RequestData<T> requestData = new RequestData<T>() { Function = functionName };
            requestData.Data = data is null ? (T)Activator.CreateInstance(typeof(T)) : data;

            string contentString = JsonSerializer.Serialize(requestData);
            _logger.LogTrace($"Request Content: {contentString}");

            HttpContent content = new StringContent(contentString, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }

        /// <summary>
        /// Runs a function on the server and parse the response.
        /// </summary>
        /// <typeparam name="T">The request data type</typeparam>
        /// <typeparam name="Q">The response data type</typeparam>
        /// <param name="functionName">The name of the function to run on the server</param>
        /// <param name="data">[Optional] The data to be passed to the server</param>
        /// <returns></returns>
        internal async Task<ClientResponse<Q>> PostToServerAsync<T, Q>(string functionName, T? data = default)
        {
            var response = await _httpClient.PostAsync(FullConnectionString, SerializeRequestData<T>(functionName, data));
            string responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"{functionName} Response: {await response.Content.ReadAsStringAsync()}");
            ClientResponse<Q> clientResponse = new ClientResponse<Q>() { 
                StatusCode = response.StatusCode 
            };

            try
            {
                clientResponse.ErrorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"Execption in reading error response: {ex.Message}");
            }

            if (typeof(Q) == typeof(bool)) 
            {
                return clientResponse;
            }

            try
            {
                clientResponse.RequestResponse = JsonSerializer.Deserialize<RequestResponse<Q>>(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"Execption in reading response body: {ex.Message}");
            }

            clientResponse.IsSuccessful = response.IsSuccessStatusCode && !clientResponse.HasError;

            return clientResponse;
        }

        /// <summary>
        /// Retrieves the health of the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#HealthCheck">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<HealthCheckResponse>> HealthCheckAsync() => 
            await PostToServerAsync<HealthCheckRequest, HealthCheckResponse>("HealthCheck");

        /// <summary>
        /// Verifies the bearer token is valid. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#VerifyAuthentication_Token">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<bool>> VerifyAuthenticationTokenAsync() => 
            await PostToServerAsync<Dictionary<string, string>, bool>("VerifyAuthenticationToken");

        /// <summary>
        /// Attempts to perform a passwordless login at the provided privilege level. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#PasswordlessLogin">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<AuthTokenResponse>> PasswordlessLoginAsync(PrivilegeLevelEnum privilegeLevel) =>
            await PostToServerAsync<PasswordlessLoginRequest, AuthTokenResponse>("PasswordlessLogin", new PasswordlessLoginRequest() { PrivilageLevel = Enum.GetName(privilegeLevel) });

        /// <summary>
        /// Attempts to perform a password login at the provided privilege level. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#PasswordLogin">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<AuthTokenResponse>> PasswordLoginAsync(string password, PrivilegeLevelEnum privilegeLevel) =>
            await PostToServerAsync<PasswordLoginRequest, AuthTokenResponse>("PasswordLogin", new PasswordLoginRequest() { Password = password, PrivilageLevel = Enum.GetName(privilegeLevel) });

        /// <summary>
        /// Attempts to claim the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#ClaimServer">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<AuthTokenResponse>> ClaimServerAsync(string serverName, string adminPassword) =>
            await PostToServerAsync<ClaimServerRequest, AuthTokenResponse>("ClaimServer", new ClaimServerRequest() { AdminPassword = adminPassword, ServerName = serverName });

        /// <summary>
        /// Attempts to rename the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#RenameServer">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<bool>> RenameServerAsync(string serverName) =>
            await PostToServerAsync<RenameServerRequest, bool>("RenameServer", new RenameServerRequest() { ServerName = serverName });

        /// <summary>
        /// Attempts to set the client password for the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#SetClientPassword">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<bool>> SetClientPasswordAsync(string password) =>
            await PostToServerAsync<PasswordChangeRequest, bool>("SetClientPassword", new PasswordChangeRequest() { Password = password });

        /// <summary>
        /// Attempts to set the admin password for the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#SetAdminPassword">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<bool>> SetAdminPasswordAsync(string password) =>
            await PostToServerAsync<PasswordChangeRequest, bool>("SetAdminPassword", new PasswordChangeRequest() { Password = password });

        /// <summary>
        /// Retrieves the current state of the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#QueryServerState">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<QueryServerStateResponse>> QueryServerStateAsync() => 
            await PostToServerAsync<Dictionary<string, string>, QueryServerStateResponse>("QueryServerState");

        /// <summary>
        /// Retrieves the current settings of the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#GetServerOptions">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<GetServerOptionsResponse>> GetServerOptionsAsync() => 
            await PostToServerAsync<Dictionary<string, string>, GetServerOptionsResponse>("GetServerOptions");

        /// <summary>
        /// Sets the passed settings as pending settings. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#ApplyServerOptions">Wiki Docs</see> for more information. <br/>
        /// <b>NOTE: Pending settings will require a server restart to apply.</b>
        /// </summary>
        public async Task<ClientResponse<bool>> ApplyServerOptionsAsync(Dictionary<string, string> settings) => 
            await PostToServerAsync<ApplyServerOptionsRequest, bool>("ApplyServerOptions", new ApplyServerOptionsRequest() { UpdatedServerOptions = settings });

        /// <summary>
        /// Retrieves the current advanced game settings of the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#GetAdvancedGameSettings">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<GetAdvancedGameSettingsResponse>> GetAdvancedGameSettingsAsync() => 
            await PostToServerAsync<Dictionary<string, string>, GetAdvancedGameSettingsResponse>("GetAdvancedGameSettings");

        /// <summary>
        /// Sets the passed game settings as pending settings. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#ApplyAdvancedGameSettings">Wiki Docs</see> for more information. <br/>
        /// <b>NOTE: Pending game settings will require a server restart to apply.</b>
        /// </summary>
        public async Task<ClientResponse<bool>> ApplyAdvancedGameSettingsAsync(Dictionary<string, string> settings) => 
            await PostToServerAsync<ApplyAdvancedGameSettingsRequest, bool>("ApplyAdvancedGameSettings", new ApplyAdvancedGameSettingsRequest() { AppliedAdvancedGameSettings = settings });

        /// <summary>
        /// Attempts to set the auto loaded session. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#SetAutoLoadSessionName">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<bool>> SetAutoLoadSessionName(string sessionName) =>
             await PostToServerAsync<SessionChangeRequest, bool>("SetAutoLoadSessionName", new SessionChangeRequest() { SessionName = sessionName });

        /// <summary>
        /// Retrieves all of the sessions for the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#EnumerateSessions">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<EnumerateSessionsResponse>> EnumerateSessionsAsync() => 
            await PostToServerAsync<Dictionary<string, string>, EnumerateSessionsResponse>("EnumerateSessions");

        
    }
}
