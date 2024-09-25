using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    public class QueryServerStateResponse
    {
        [JsonPropertyName("serverGameState")] public ServerState ServerState { get; set; }
    }

    [Serializable]
    public class ServerState
    {
        [JsonPropertyName("activeSessionName")] public string ActiveSessionName { get; set; }
        [JsonPropertyName("numConnectedPlayers")] public int ConnectedPlayers { get; set; }
        [JsonPropertyName("playerLimit")] public int PlayerLimit { get; set; }
        [JsonPropertyName("techTier")] public int TechTier { get; set; }
        [JsonPropertyName("activeSchematic")] public string ActiveSchematic { get; set; }
        [JsonPropertyName("gamePhase")] public string GamePhase { get; set; }
        [JsonPropertyName("isGameRunning")] public bool IsGameRunning { get; set; }
        [JsonPropertyName("totalGameDuration")] public int TotalGameDuration { get; set; }
        [JsonPropertyName("isGamePaused")] public bool IsPaused { get; set; }
        [JsonPropertyName("averageTickRate")] public float AverageTickRate { get; set; }
        [JsonPropertyName("autoLoadSessionName")] public string AutoLoadSessionName { get; set; }
    }
}
