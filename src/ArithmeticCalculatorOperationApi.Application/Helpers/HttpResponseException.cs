using System.Net;

namespace ArithmeticCalculatorOperationApi.Application.Helpers
{
    public class HttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public string Message { get; }

        public HttpResponseException(HttpStatusCode statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
