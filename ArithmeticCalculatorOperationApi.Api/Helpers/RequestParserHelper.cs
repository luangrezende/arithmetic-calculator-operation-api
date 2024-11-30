using System.Text.Json;
using ArithmeticCalculatorOperationApi.Domain.Constants;

namespace ArithmeticCalculatorOperationApi.Helpers
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
