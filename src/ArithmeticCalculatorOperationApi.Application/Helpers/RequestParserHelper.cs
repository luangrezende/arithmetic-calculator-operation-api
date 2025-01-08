using ArithmeticCalculatorOperationApi.Application.Constants;
using System.Text.Json;

namespace ArithmeticCalculatorOperationApi.Application.Helpers
{
    public static class RequestParserHelper
    {
        public static bool TryParseRequest<T>(string requestBody, out T? parsedObject, out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                parsedObject = default;
                errorMessage = ApiErrorMessages.MissingBody;
                return false;
            }

            try
            {
                parsedObject = JsonSerializer.Deserialize<T>(requestBody);

                if (parsedObject == null)
                {
                    errorMessage = ApiErrorMessages.InvalidRequestBody;
                    return false;
                }

                errorMessage = null;
                return true;
            }
            catch (JsonException)
            {
                parsedObject = default;
                errorMessage = ApiErrorMessages.InvalidJsonFormat;
                return false;
            }
        }
    }
}
