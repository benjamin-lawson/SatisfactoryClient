using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    [Serializable]
    public class CreateNewGameRequest
    {
        [JsonPropertyName("NewGameData")] public NewGameData NewGameData { get; set; }
    }

    [Serializable]
    public class NewGameData
    {
        [JsonPropertyName("SessionName")] public string SessionName { get; set; }
        [JsonPropertyName("MapName")] public string? MapName { get; set; }
        [JsonPropertyName("StartingLocation")] public string? StartingLocation { get; set; }
        [JsonPropertyName("SkipOnboarding")] public bool SkipOnboarding { get; set; }
        [JsonPropertyName("AdvancedGameSettings")] public Dictionary<string, string> AdvancedGameSettings { get; set; } = new();
        [JsonPropertyName("CustomOptionsOnlyForModding")] public Dictionary<string, string> CustomOptionsOnlyForModding { get; set; } = new();
    }
}
