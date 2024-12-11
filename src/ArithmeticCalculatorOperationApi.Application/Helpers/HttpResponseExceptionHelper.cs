using System.Net;

namespace ArithmeticCalculatorOperationApi.Application.Helpers
{
    public class HttpResponseExceptionHelper : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public string Message { get; }

        public HttpResponseExceptionHelper(HttpStatusCode statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
