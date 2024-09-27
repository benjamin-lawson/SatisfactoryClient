using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    public class SaveRequest
    {
        [JsonPropertyName("SaveName")] public string SaveName { get; set; }
    }
}
