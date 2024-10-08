using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    [Serializable]
    public class UploadSaveGameRequest
    {
        [JsonPropertyName("SaveName")] public string SaveName { get; set; }
        [JsonPropertyName("LoadSaveGame")] public bool DoLoadSaveGameOnUpload { get; set; }
        [JsonPropertyName("EnableAdvancedGameSettings")] public bool DoEnableAdvancedGameSettingsOnLoad { get; set; }
    }
}
