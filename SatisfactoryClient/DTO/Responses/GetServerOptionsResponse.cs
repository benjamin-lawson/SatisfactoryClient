using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    public class GetServerOptionsResponse
    {
        [JsonPropertyName("serverOptions")] public Dictionary<string, string> ServerOptions { get; set; }
        [JsonPropertyName("pendingServerOptions")] public Dictionary<string, string> PendingServerOptions { get; set; }

    }
}
