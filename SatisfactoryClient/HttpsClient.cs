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

namespace SatisfactoryClient
{
    public class HttpsClient
    {
        private string _ip;
        private int _port = 7777;
        private string _authToken;
        private Regex _ipRegex = new(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
        private HttpClient _httpClient;
        private ILogger _logger;

        public string IP => _ip;
        public int Port => _port;
        public string FullConnectionString => $"https://{IP}:{Port}/api/v1";

        public HttpsClient(string ip, string authToken, int port = 7777, bool trustSelfSignedCerts = false, HttpClient client = null, ILogger logger = null) 
        {
            if (!_ipRegex.Match(ip).Success)
            {
                throw new InvalidDataException($"IP {ip} not in format xxx.xxx.xxx.xxx");
            }

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

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");

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
        }

        private HttpContent SerializeRequestData<T>(string functionName, T? data = default)
        {
            RequestData<T> requestData = new RequestData<T>() { Function = functionName };
            requestData.Data = data is null ? (T)Activator.CreateInstance(typeof(T)) : data;
            HttpContent content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }

        internal async Task<Q> PostToServerAsync<T, Q>(string functionName, T? data = default)
        {
            var response = await _httpClient.PostAsync(FullConnectionString, SerializeRequestData<T>(functionName));

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"{functionName} got {response.StatusCode} from API");
            }

            if (typeof(Q) == typeof(bool)) return (Q)Convert.ChangeType(response.IsSuccessStatusCode, typeof(Q));

            _logger.LogDebug($"{functionName} Response: {await response.Content.ReadAsStringAsync()}");

            return (await response.Content.ReadFromJsonAsync<RequestResponse<Q>>()).Data;
        }

        public async Task<HealthCheckResponse> HealthCheckAsync() => await PostToServerAsync<HealthCheckRequest, HealthCheckResponse>("HealthCheck");
        public async Task<bool> VerifyAuthenticationTokenAsync() => await PostToServerAsync<Dictionary<string, string>, bool>("VerifyAuthenticationToken");
        public async Task<QueryServerStateResponse> QueryServerStateAsync() => await PostToServerAsync<Dictionary<string, string>, QueryServerStateResponse>("QueryServerState");
        public async Task<GetServerOptionsResponse> GetServerOptionsAsync() => await PostToServerAsync<Dictionary<string, string>, GetServerOptionsResponse>("GetServerOptions");
        public async Task<bool> ApplyServerOptionsAsync(Dictionary<string, string> settings) => await PostToServerAsync<ApplyServerOptionsRequest, bool>("ApplyServerOptions", new ApplyServerOptionsRequest() { UpdatedServerOptions = settings });
        public async Task<GetAdvancedGameSettingsResponse> GetAdvancedGameSettingsAsync() => await PostToServerAsync<Dictionary<string, string>, GetAdvancedGameSettingsResponse>("GetAdvancedGameSettings");
        public async Task<bool> ApplyAdvancedGameSettingsAsync(Dictionary<string, string> settings) => await PostToServerAsync<ApplyAdvancedGameSettingsRequest, bool>("ApplyAdvancedGameSettings", new ApplyAdvancedGameSettingsRequest() { AppliedAdvancedGameSettings = settings });
        public async Task<EnumerateSessionsResponse> EnumerateSessionsAsync() => await PostToServerAsync<Dictionary<string, string>, EnumerateSessionsResponse>("EnumerateSessions");
    }
}
