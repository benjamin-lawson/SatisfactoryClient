using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    [Serializable]
    public class PasswordLoginRequest
    {
        [JsonPropertyName("MinimumPrivilegeLevel")] public string PrivilageLevel { get; set; }
        [JsonPropertyName("Password")] public string Password { get; set; }
    }
}
