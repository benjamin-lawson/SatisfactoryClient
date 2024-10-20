﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    public class PasswordChangeRequest
    {
        [JsonPropertyName("Password")] public string Password { get; set; }
        [JsonPropertyName("AuthenticationToken")] public string AuthenticationToken { get; set; }
    }
}
