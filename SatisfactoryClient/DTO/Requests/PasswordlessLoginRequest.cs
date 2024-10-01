using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    [Serializable]
    public class PasswordlessLoginRequest
    {
        [JsonPropertyName("MinimumPrivilegeLevel")] public string PrivilageLevel { get; set; }
    }
}
