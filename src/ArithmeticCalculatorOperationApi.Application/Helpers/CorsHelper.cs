namespace ArithmeticCalculatorOperationApi.Application.Helpers;

public static class CorsHelper
{
    public static Dictionary<string, string> GetCorsHeaders() => new()
    {
        { "Access-Control-Allow-Origin", "*" },
        { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
        { "Access-Control-Allow-Headers", "Content-Type, Authorization, X-Amz-Date, X-Api-Key, X-Amz-Security-Token" }
    };
}
