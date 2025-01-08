namespace ArithmeticCalculatorOperationApi.Application.Helpers
{
    public static class OperationExpressionHelper
    {
        public static bool IsNumeric(this string value)
        {
            return double.TryParse(value, out _);
        }
    }
}
