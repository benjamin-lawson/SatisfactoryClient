using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    public class SessionChangeRequest
    {
        [JsonPropertyName("SessionName")] public string SessionName { get; set; }
    }
}
