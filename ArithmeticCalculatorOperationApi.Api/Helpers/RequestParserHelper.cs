using System.Text.Json;
using ArithmeticCalculatorUserApi.Domain.Constants;

namespace ArithmeticCalculatorUserApi.Helpers
{
    public static class RequestParserHelper
    {
        public static bool TryParseRequest<T>(string requestBody, out T? parsedObject, out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                parsedObject = default;
                errorMessage = ApiResponseMessages.MissingBody;
                return false;
            }

            try
            {
                parsedObject = JsonSerializer.Deserialize<T>(requestBody);

                if (parsedObject == null)
                {
                    errorMessage = ApiResponseMessages.InvalidRequestBody;
                    return false;
                }

                errorMessage = null;
                return true;
            }
            catch (JsonException)
            {
                parsedObject = default;
                errorMessage = ApiResponseMessages.InvalidJsonFormat;
                return false;
            }
        }
    }
}
