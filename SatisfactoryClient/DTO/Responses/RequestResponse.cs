using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    [Serializable]
    public class RequestResponse<T>
    {
        [JsonPropertyName("data")] public T Data { get; set; }
    }
}
