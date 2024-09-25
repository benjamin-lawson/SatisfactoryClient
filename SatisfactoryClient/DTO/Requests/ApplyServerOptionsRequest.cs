using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    [Serializable]
    public class ApplyServerOptionsRequest
    {
        [JsonPropertyName("UpdatedServerOptions")] public Dictionary<string, string> UpdatedServerOptions { get; set; }
    }
}
