using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    [Serializable]
    public class HealthCheckRequest
    {
        public string ClientCustomData { get; set; } = "";
    }
}
