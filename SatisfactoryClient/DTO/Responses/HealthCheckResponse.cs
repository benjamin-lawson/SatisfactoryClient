using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    public class HealthCheckResponse
    {
        [JsonPropertyName("health")] public string Health { get; set; } = "";
        [JsonPropertyName("serverCustomData")] public string ServerCustomData { get; set; } = "";
    }
}
