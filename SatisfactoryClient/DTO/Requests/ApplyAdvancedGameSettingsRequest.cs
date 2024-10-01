using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    [Serializable]
    public class ApplyAdvancedGameSettingsRequest
    {
        [JsonPropertyName("AppliedAdvancedGameSettings")] public Dictionary<string, string> AppliedAdvancedGameSettings { get; set; }
    }
}
