using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryClient.DTO
{
    public class ClientResponse<T>
    {
        public bool IsSuccessful { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public RequestResponse<T>? RequestResponse { get; set; } = null;
        public ErrorResponse? ErrorResponse { get; set; } = null;

        public bool HasError => ErrorResponse != null && !string.IsNullOrEmpty(ErrorResponse.ErrorCode);
    }
}
