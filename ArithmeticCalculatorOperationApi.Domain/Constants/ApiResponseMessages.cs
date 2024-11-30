namespace ArithmeticCalculatorOperationApi.Domain.Constants
{
    public static class ApiResponseMessages
    {
        public const string MissingBody = "The request body cannot be null or empty.";
        public const string InvalidCredentials = "The provided username or password is invalid.";
        public const string InternalServerError = "An internal server error occurred.";
        public const string EndpointNotFound = "The requested endpoint was not found.";
        public const string AccountNotFound = "The specified account was not found.";
        public const string InvalidRequestBody = "The request body is invalid.";
        public const string InvalidJsonFormat = "The JSON format in the request body is invalid.";
        public const string MissingUsernameOrPassword = "Both username and password are required.";
        public const string UsernamePasswordNameRequired = "Username, password, and name are required.";
        public const string UsernameAlreadyExists = "This email is already registered.";
        public const string ErrorCreatingUser = "An error occurred while creating the user.";
        public const string UserCreatedSuccessfully = "The user was created successfully.";
        public const string UserInactive = "The user account is inactive.";
        public const string AccountIdAmountRequired = "Account ID and a valid amount are required.";
        public const string AddBalanceFailed = "An error occurred while adding balance to the account.";
        public const string AddBalanceSuccess = "The balance was added successfully.";
        public const string InvalidToken = "Invalid token.";
        public const string TokenExpired = "Token expired.";
        public const string UserNotFound = "User not found.";
        public const string UserPasswordMatchError = "Password and ConfirmPassword must match.";
        public const string InsufficientBalance = "Insufficient balance to complete the transaction.";
        public const string DebitBalanceSuccess = "Balance debited successfully.";
        public const string AccountNotBelongToUser = "The specified account does not belong to the authenticated user.";
        public const string InvalidRefreshToken = "Invalid refresh token.";
        public const string MissingRefreshToken = "The refresh token is missing.";
        public const string InvalidAmount = "The amount must be greater than zero.";
        public const string ExceededMaximumAmount = "The maximum amount you can add is $500.";
        public const string LogoutSuccessful = "Logout successful.";
    }
}
