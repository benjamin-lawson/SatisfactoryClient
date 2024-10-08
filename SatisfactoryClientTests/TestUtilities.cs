using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SatisfactorySdk.DTO;

namespace SatisfactoryClientTests
{
    public class TestUtilities
    {
        public static bool MatchRequestFunction(string functionName, HttpRequestMessage request)
        {
            string bodyContent = request.Content.ReadAsStringAsync().Result;
            return bodyContent.Contains(functionName);
        }

        public static string GenerateErrorString(string errorCode, string? errorMessage = "", object? errorData = null)
        {
            return JsonSerializer.Serialize(new ErrorResponse
            {
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                ErrorData = errorData
            });
        }
    }
}
