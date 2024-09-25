using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    [Serializable]
    public class RequestData<T>
    {
        [JsonPropertyName("function")] public string Function { get; set; } = "";
        [JsonPropertyName("data")]  public T Data { get; set; }
    }
}
