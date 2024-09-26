using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    [Serializable]
    public class EnumerateSessionsResponse
    {
        [JsonPropertyName("sessions")] public List<Session> Sessions { get;set; }
        [JsonPropertyName("currentSessionIndex")] public int CurrentSessionIndex { get; set; }

        public Session CurrentSession => Sessions[CurrentSessionIndex];
    }

    [Serializable]
    public class Session
    {
        [JsonPropertyName("sessionName")] public string Name { get; set; }
        [JsonPropertyName("saveHeaders")] public List<SaveHeader> SaveHeaders { get; set; }
    }

    [Serializable]
    public class SaveHeader
    {
        [JsonPropertyName("saveVersion")] public int SaveVersion { get; set; }
        [JsonPropertyName("buildVersion")] public int BuildVersion { get; set; }
        [JsonPropertyName("saveName")] public string Name { get; set; }
        [JsonPropertyName("saveLocationInfo")] public string SaveLocation { get; set; }
        [JsonPropertyName("mapName")] public string MapName { get; set; }
        [JsonPropertyName("mapOptions")] public string MapOptions { get; set; }
        [JsonPropertyName("sessionName")] public string SessionName { get; set; }
        [JsonPropertyName("playDurationSeconds")] public long PlayDurationSections { get; set; }
        [JsonPropertyName("saveDateTime")] public string SaveDateTimeString { get; set; }
        [JsonPropertyName("isModdedSave")] public bool IsModded { get; set; }
        [JsonPropertyName("isEditedSave")] public bool IsEdited { get; set; }
        [JsonPropertyName("isCreativeModeEnabled")] public bool IsCreativeMode { get; set; }

        public DateTime SaveDate => DateTime.ParseExact(SaveDateTimeString, "yyyy.MM.dd-HH.mm.ss", null);
    }
}
