﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    public class RenameServerRequest
    {
        [JsonPropertyName("ServerName")] public string ServerName { get; set; }
    }
}
