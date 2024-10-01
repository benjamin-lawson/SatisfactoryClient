﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    [Serializable]
    public class RunCommandRequest
    {
        [JsonPropertyName("Command")] public string Command { get; set; }
    }
}
