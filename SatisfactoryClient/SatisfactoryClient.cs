using SatisfactorySdk.DTO;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http.Json;
using System.Net;

namespace SatisfactorySdk
{
    public class SatisfactoryClient
    {
        private string? _authToken;
        private HttpClient _httpClient;
        private ILogger _logger;
        private string _fullConnectionString = "";

        public string ConnectionString => _fullConnectionString;

        public SatisfactoryClient(string ip, int port, string authToken = "",  bool trustSelfSignedCerts = false, bool usePort = true, HttpClient? client = null, ILogger? logger = null) 
            : this(usePort ? $"https://{ip}:{port}/api/v1" : $"https://{ip}/api/v1", authToken: authToken, trustSelfSignedCerts: trustSelfSignedCerts, client: client, logger: logger)
        {
            if (usePort && (port < 0 || port > 65535))
            {
                throw new InvalidDataException($"Port {port} is an invalid port number");
            }
        }
        public SatisfactoryClient(string connectionString, string authToken = "", bool trustSelfSignedCerts = false, HttpClient? client = null, ILogger? logger = null)
        {
            

            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip |
                                                 DecompressionMethods.Deflate;
            }

            if (trustSelfSignedCerts)
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            }

            if (client != null)
            {
                _httpClient = client;
            }
            else
            {
                _httpClient = new HttpClient(handler);
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
                _logger = factory.CreateLogger<SatisfactoryClient>();
            }
            else
            {
                _logger = logger;
            }

            _fullConnectionString = connectionString;
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
            if (string.IsNullOrEmpty(functionName))
            {
                throw new InvalidDataException("Function name cannot be null or empty");
            }

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
            var response = await _httpClient.PostAsync(_fullConnectionString, SerializeRequestData<T>(functionName, data));
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
                clientResponse.IsSuccessful = response.IsSuccessStatusCode && string.IsNullOrEmpty(clientResponse.ErrorResponse?.ErrorCode);
                return clientResponse;
            }

            try
            {
                clientResponse.RequestResponse = JsonSerializer.Deserialize<RequestResponse<Q>>(responseBody);
                clientResponse.IsSuccessful = response.IsSuccessStatusCode && !clientResponse.HasError && clientResponse.RequestResponse.Data is not null;
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"Execption in reading response body: {ex.Message}");
                clientResponse.ErrorResponse = new ErrorResponse() { ErrorCode = "unknown_exception", ErrorMessage = ex.Message };
                clientResponse.IsSuccessful = false;
            }

            return clientResponse;
        }

        /// <summary>
        /// Retrieves the health of the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#HealthCheck">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<HealthCheckResponse>> HealthCheckAsync(string clientCustomData) =>
            await PostToServerAsync<HealthCheckRequest, HealthCheckResponse>("HealthCheck", new HealthCheckRequest() { ClientCustomData = clientCustomData });

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
        public async Task<ClientResponse<AuthTokenResponse>> SetAdminPasswordAsync(string password) =>
            await PostToServerAsync<PasswordChangeRequest, AuthTokenResponse>("SetAdminPassword", new PasswordChangeRequest() { Password = password, AuthenticationToken = _authToken });

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
        /// Runs a command just like the in-game console on the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#RunCommand">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<RunCommandResponse>> RunCommandAsync(string command) =>
            await PostToServerAsync<RunCommandRequest, RunCommandResponse>("RunCommand", new RunCommandRequest() { Command = command });

        /// <summary>
        /// Creates a new session. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#CreateNewGame">Wiki Docs</see> for more information. <br/>
        /// <b>NOTE: The API will be unavailable until the game is created.</b>
        /// </summary>
        public async Task<ClientResponse<bool>> CreateNewGameAsync(
                string sessionName,
                string? mapName,
                string? startingLocation,
                bool skipOnboarding,
                Dictionary<string, string> advancedGameSettings,
                Dictionary<string, string> customOptions
            ) => await PostToServerAsync<CreateNewGameRequest, bool>("CreateNewGame", new CreateNewGameRequest()
                {
                    NewGameData = new NewGameData()
                    {
                        SessionName = sessionName,
                        MapName = mapName,
                        StartingLocation = startingLocation,
                        SkipOnboarding = skipOnboarding,
                        AdvancedGameSettings = advancedGameSettings,
                        CustomOptionsOnlyForModding = customOptions
                    }
                });

        /// <summary>
        /// Loads the game with the save file provided. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#LoadGame">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<bool>> SaveGameAsync(string saveName, bool enabledAdvancedGameSettings) =>
             await PostToServerAsync<LoadGameRequest, bool>("SaveGame", new LoadGameRequest() { SaveName = saveName, AdvancedGameSettingsEnabled = enabledAdvancedGameSettings });

        /// <summary>
        /// Saves the game with the file name passed. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#SaveGame">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<bool>> SaveGameAsync(string saveName) =>
             await PostToServerAsync<SaveRequest, bool>("SaveGame", new SaveRequest() { SaveName = saveName });

        /// <summary>
        /// Deletes the save file with the file name passed. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#DeleteSaveFile">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<bool>> DeleteSaveFileAsync(string saveName) =>
             await PostToServerAsync<SaveRequest, bool>("DeleteSaveFile", new SaveRequest() { SaveName = saveName });

        /// <summary>
        /// Deletes all the save files associated with a session. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#DeleteSaveSession">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<bool>> DeleteSaveSessionAsync(string sessionName) =>
             await PostToServerAsync<SessionChangeRequest, bool>("DeleteSaveSession", new SessionChangeRequest() { SessionName = sessionName });

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

        /// <summary>
        /// Attempts to shutdown the server. Checkout the <see href="https://satisfactory.wiki.gg/wiki/Dedicated_servers/HTTPS_API#Shutdown">Wiki Docs</see> for more information.
        /// </summary>
        public async Task<ClientResponse<bool>> ShutdownAsync() =>
            await PostToServerAsync<Dictionary<string, string>, bool>("Shutdown");


        public async Task<ClientResponse<bool>> DownloadSaveGameAsync(string saveName, string savePath)
        {
            var response = await _httpClient.PostAsync(_fullConnectionString, SerializeRequestData("DownloadSaveGame", new DownloadSaveGameRequest { SaveName = saveName}));

            _logger.LogDebug($"DownloadSaveGame Response Status: {response.StatusCode}");
            ClientResponse<bool> clientResponse = new ClientResponse<bool>()
            {
                StatusCode = response.StatusCode
            };

            try
            {
                clientResponse.ErrorResponse = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"Execption in reading error response: {ex.Message}");
            }

            try
            {
                using (var s = await response.Content.ReadAsStreamAsync())
                {
                    using (var fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        await s.CopyToAsync(fs);
                    }
                }
                clientResponse.IsSuccessful = response.IsSuccessStatusCode && !clientResponse.HasError && File.Exists(savePath);
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"Execption in reading response body: {ex.Message}");
                clientResponse.ErrorResponse = new ErrorResponse() { ErrorCode = "unknown_exception", ErrorMessage = ex.Message };
                clientResponse.IsSuccessful = false;
            }

            return clientResponse;
        }

        public async Task<ClientResponse<bool>> UploadSaveGameAsync(string saveName, string savePath, bool loadSaveGameOnUpload, bool enableAdvancedGameSettings)
        {
            if (!File.Exists(savePath)) 
            {
                throw new FileNotFoundException($"No file at {savePath}");
            }

            RequestData<UploadSaveGameRequest> requestData = new RequestData<UploadSaveGameRequest>
            {
                Function = "UploadSaveGame",
                Data = new UploadSaveGameRequest
                {
                    SaveName = saveName,
                    DoLoadSaveGameOnUpload = loadSaveGameOnUpload,
                    DoEnableAdvancedGameSettingsOnLoad = enableAdvancedGameSettings
                }
            };

            var fileContent = new ByteArrayContent(File.ReadAllBytes(savePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

            var content = new MultipartFormDataContent
            {
                { JsonContent.Create(requestData, mediaType: MediaTypeHeaderValue.Parse("application/json")), "data" },
                { fileContent, "saveGameFile" }
            };

            content.Headers.ContentLength = (await content.ReadAsByteArrayAsync()).LongLength;

            var response = await _httpClient.PostAsync(_fullConnectionString, content);

            ClientResponse<bool> clientResponse = new ClientResponse<bool>()
            {
                StatusCode = response.StatusCode
            };

            try
            {
                clientResponse.ErrorResponse = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"Execption in reading error response: {ex.Message}");
            }

            clientResponse.IsSuccessful = clientResponse.IsSuccessful = response.IsSuccessStatusCode && !clientResponse.HasError;
            return clientResponse;
        }
        
    }
}
