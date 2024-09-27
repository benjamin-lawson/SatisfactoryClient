using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    [Serializable]
    public class RunCommandResponse
    {
        [JsonPropertyName("commandResult")] public string CommandResult { get; set; }
        [JsonPropertyName("returnValue")] public bool WasSuccessful { get; set; }
    }
}
