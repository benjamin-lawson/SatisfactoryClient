using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    [Serializable]
    public class ErrorResponse
    {
        [JsonPropertyName("errorCode")] public string ErrorCode { get; set; }
        [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; set; }
        [JsonPropertyName("errorData")] public dynamic? ErrorData { get; set; }

    }
}
