using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    public class GetAdvancedGameSettingsResponse
    {
        [JsonPropertyName("creativeModeEnabled")] public bool IsCreativeEnabled { get; set; }
        [JsonPropertyName("advancedGameSettings")] public Dictionary<string, string> AdvancedGameSettings { get; set; }

    }
}