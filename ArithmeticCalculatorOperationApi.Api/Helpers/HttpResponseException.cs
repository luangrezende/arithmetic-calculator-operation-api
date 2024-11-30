using System.Net;

namespace ArithmeticCalculatorUserApi.Helpers
{
    public class HttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public object? ResponseBody { get; }

        public HttpResponseException(HttpStatusCode statusCode, object? responseBody = null)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
