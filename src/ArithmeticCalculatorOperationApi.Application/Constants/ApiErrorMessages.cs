namespace ArithmeticCalculatorOperationApi.Application.Constants
{
    public static class ApiErrorMessages
    {
        public const string MissingBody = "The request body cannot be null or empty.";
        public const string InternalServerError = "An internal server error occurred.";
        public const string GenericError = "An error occurred.";
        public const string EndpointNotFound = "The requested endpoint was not found.";
        public const string InvalidRequestBody = "The request body is invalid.";
        public const string InvalidJsonFormat = "The JSON format in the request body is invalid.";
        public const string InvalidToken = "Invalid token.";
        public const string TokenMissing = "Token is missing.";
        public const string TokenExpired = "Token expired.";
        public const string OperationNotFound = "The requested operation does not exist.";
        public const string AccountIdNotNull = "accountId cannot be null.";
        public const string ExpressionNotNull = "expression cannot be null.";
        public const string OperationAdded = "Operation was successfully added.";
        public const string AddBalanceSuccess = "Balance added successfully.";
        public const string InvalidAmount = "The amount is invalid.";
        public const string InvalidExpression = "The provided expression is not a valid arithmetic operation.";
        public const string ExceededMaximumAmount = "The amount exceeds the maximum allowed.";
        public const string NoOperationsFound = "No operations were found.";
        public const string ConnectionStringNotSet = "Connection string is not set.";
    }
}
