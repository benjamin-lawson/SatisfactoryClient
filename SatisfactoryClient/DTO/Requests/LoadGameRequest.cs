using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    [Serializable]
    public class LoadGameRequest
    {
        [JsonPropertyName("SaveName")] public string SaveName { get; set; }
        [JsonPropertyName("EnableAdvancedGameSettings")] public bool AdvancedGameSettingsEnabled { get; set; }
    }
}
