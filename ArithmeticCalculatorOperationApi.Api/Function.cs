using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ArithmeticCalculatorOperationApi.Domain.Constants;
using ArithmeticCalculatorOperationApi.Domain.Enums;
using ArithmeticCalculatorOperationApi.Domain.Models.Request;
using ArithmeticCalculatorOperationApi.Domain.Models.Response;
using ArithmeticCalculatorOperationApi.Domain.Services;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using ArithmeticCalculatorOperationApi.Helpers;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories;
using ArithmeticCalculatorOperationApi.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ArithmeticCalculatorOperationApi;

public class Function
{
    private readonly IServiceProvider _serviceProvider;

    public Function()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<ITokenGeneratorService, TokenGeneratorService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped(sp => new JwtTokenValidator(Environment.GetEnvironmentVariable("jwtSecretKey")!));
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            if (request.HttpMethod == "OPTIONS")
                return BuildPreflightResponse();

            return request.HttpMethod switch
            {
                "GET" when request.Path == "/v1/operation/test" => await Test(request),
                "POST" when request.Path == "/v1/user/auth/login" => await Login(request),
                "POST" when request.Path == "/v1/user/auth/logout" => await Logout(request),
                "POST" when request.Path == "/v1/user/auth/refresh" => await RefreshToken(request),
                "POST" when request.Path == "/v1/user/auth/register" => await Register(request),
                "POST" when request.Path == "/v1/user/account/balance" => await AddBalance(request),
                "DELETE" when request.Path == "/v1/user/account/balance" => await DebitBalance(request),
                _ => BuildResponse(HttpStatusCode.NotFound, new { error = "Endpoint not found." }),
            };
        }
        catch (HttpResponseException ex)
        {
            context.Logger.LogError($"HttpResponseException: {ex.Message}");
            return BuildResponse(ex.StatusCode, new { error = ex.ResponseBody ?? "An error occurred." });
        }
        catch (SecurityTokenExpiredException ex)
        {
            context.Logger.LogError($"SecurityTokenExpiredException: {ex.Message}");
            return BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiResponseMessages.TokenExpired });
        } 
        catch (SecurityTokenMalformedException ex)
        {
            context.Logger.LogError($"SecurityTokenMalformedException: {ex.Message}");
            return BuildResponse(HttpStatusCode.BadRequest, new { error = ApiResponseMessages.InvalidToken });
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled exception: {ex.Message}");
            return BuildResponse(HttpStatusCode.InternalServerError, new { error = "Internal server error." });
        }
    }

    private async Task<APIGatewayProxyResponse> Test(APIGatewayProxyRequest request)
    {
        return BuildResponse(HttpStatusCode.OK, new { message = "test ok"});
    }

    private T ParseRequestOrThrow<T>(string requestBody)
    {
        if (!RequestParserHelper.TryParseRequest<T>(requestBody, out var parsedRequest, out var errorMessage))
            throw new HttpResponseException(HttpStatusCode.BadRequest, errorMessage!);

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(parsedRequest!);

        if (!Validator.TryValidateObject(parsedRequest!, validationContext, validationResults, true))
        {
            var errorMessages = validationResults
                .Select(result => result.ErrorMessage)
                .Where(msg => !string.IsNullOrWhiteSpace(msg))
                .ToList();

            throw new HttpResponseException(HttpStatusCode.BadRequest, errorMessages!.FirstOrDefault()); ;
        }

        return parsedRequest!;
    }

    private Guid ValidateTokenOrThrow(APIGatewayProxyRequest request)
    {
        var jwtTokenValidator = _serviceProvider.GetRequiredService<JwtTokenValidator>();
        if (!request.Headers.TryGetValue("Authorization", out var authorization) || string.IsNullOrWhiteSpace(authorization))
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiResponseMessages.InvalidToken);

        var token = authorization.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        if (!jwtTokenValidator.ValidateToken(token, out var userId))
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiResponseMessages.InvalidToken);

        return userId;
    }

    private async Task UpdateBalanceAsync(Guid userId, Guid accountId, decimal amount, BalanceOperation operation)
    {
        var bankAccountService = _serviceProvider.GetRequiredService<IBankAccountService>();

        if (!await bankAccountService.AccountBelongsToUserAsync(accountId, userId))
            throw new HttpResponseException(HttpStatusCode.Forbidden, ApiResponseMessages.AccountNotBelongToUser);

        bool success = operation switch
        {
            BalanceOperation.Add => await bankAccountService.AddBalanceAsync(accountId, amount),
            BalanceOperation.Debit => await bankAccountService.DebitBalanceAsync(accountId, amount),
            _ => throw new HttpResponseException(HttpStatusCode.BadRequest, "Invalid operation")
        };

        if (!success)
            throw new HttpResponseException(HttpStatusCode.BadRequest, operation == BalanceOperation.Add
                ? ApiResponseMessages.AddBalanceFailed
                : ApiResponseMessages.InsufficientBalance);
    }

    private async Task<APIGatewayProxyResponse> AddBalance(APIGatewayProxyRequest request)
    {
        var userId = ValidateTokenOrThrow(request);
        var addBalanceRequest = ParseRequestOrThrow<UpdateBalanceRequest>(request.Body);

        if (addBalanceRequest.Amount <= (int)BalanceConfiguration.BalanceMinimumValue 
            || addBalanceRequest.Amount > (int)BalanceConfiguration.BalanceMaximumValue)
        {
            return BuildResponse(HttpStatusCode.BadRequest, new
            {
                error = addBalanceRequest.Amount > (int)BalanceConfiguration.BalanceMaximumValue
                    ? ApiResponseMessages.ExceededMaximumAmount
                    : ApiResponseMessages.InvalidAmount
            });
        }

        await UpdateBalanceAsync(userId, addBalanceRequest.AccountId, addBalanceRequest.Amount, BalanceOperation.Add);

        return BuildResponse(HttpStatusCode.OK, new { message = ApiResponseMessages.AddBalanceSuccess });
    }

    private async Task<APIGatewayProxyResponse> DebitBalance(APIGatewayProxyRequest request)
    {
        var userId = ValidateTokenOrThrow(request);
        var debitBalanceRequest = ParseRequestOrThrow<UpdateBalanceRequest>(request.Body);

        await UpdateBalanceAsync(userId, debitBalanceRequest.AccountId, debitBalanceRequest.Amount, BalanceOperation.Debit);

        return BuildResponse(HttpStatusCode.OK, new { message = ApiResponseMessages.DebitBalanceSuccess });
    }

    private async Task<APIGatewayProxyResponse> RefreshToken(APIGatewayProxyRequest request)
    {
        var userService = _serviceProvider.GetRequiredService<IUserService>();
        var refreshTokenService = _serviceProvider.GetRequiredService<IRefreshTokenService>();
        var jwtTokenGenerator = _serviceProvider.GetRequiredService<ITokenGeneratorService>();

        var refreshTokenRequest = ParseRequestOrThrow<RefreshTokenRequest>(request.Body);

        var storedRefreshToken = await refreshTokenService.GetByTokenAsync(refreshTokenRequest.RefreshToken);
        if (storedRefreshToken == null || storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiResponseMessages.InvalidRefreshToken);

        await refreshTokenService.InvalidateTokenAsync(refreshTokenRequest.RefreshToken);

        var user = await userService.GetUserByIdAsync(storedRefreshToken.UserId) 
            ?? throw new HttpResponseException(HttpStatusCode.NotFound, ApiResponseMessages.UserNotFound);

        if (!user.IsActive())
            throw new HttpResponseException(HttpStatusCode.Conflict, ApiResponseMessages.UserInactive);

        var newAccessToken = jwtTokenGenerator.GenerateToken(user);
        var token = await refreshTokenService.AddAsync(user.Id);

        return BuildResponse(HttpStatusCode.OK, new TokenResponse
        {
            Token = newAccessToken,
            RefreshToken = token,
            Expiration = (int)TokenConfiguration.AccessTokenExpirationTimeInSeconds
        });
    }

    private async Task<APIGatewayProxyResponse> Login(APIGatewayProxyRequest request)
    {
        var userService = _serviceProvider.GetRequiredService<IUserService>();
        var refreshTokenService = _serviceProvider.GetRequiredService<IRefreshTokenService>();
        var jwtTokenGenerator = _serviceProvider.GetRequiredService<ITokenGeneratorService>();

        var loginRequest = ParseRequestOrThrow<TokenRequest>(request.Body);

        var user = await userService.GetUserByUsernameAsync(loginRequest.Username);

        if (!user!.IsActive())
            throw new HttpResponseException(HttpStatusCode.Conflict, ApiResponseMessages.UserInactive);

        var result = await userService.AuthenticateAsync(user.Username, loginRequest.Password) 
            ?? throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiResponseMessages.InvalidCredentials);

        var accessToken = jwtTokenGenerator.GenerateToken(result);
        var token = await refreshTokenService.AddAsync(result.Id);

        return BuildResponse(HttpStatusCode.OK, new TokenResponse
        {
            Token = accessToken,
            RefreshToken = token,
            Expiration = (int)TokenConfiguration.AccessTokenExpirationTimeInSeconds
        });
    }

    private async Task<APIGatewayProxyResponse> Logout(APIGatewayProxyRequest request)
    {
        var refreshTokenService = _serviceProvider.GetRequiredService<IRefreshTokenService>();

        var logoutRequest = ParseRequestOrThrow<RefreshTokenRequest>(request.Body);

        var isRevoked = await refreshTokenService.InvalidateTokenAsync(logoutRequest.RefreshToken);

        if (!isRevoked)
            throw new HttpResponseException(HttpStatusCode.BadRequest, ApiResponseMessages.InvalidToken);

        return BuildResponse(HttpStatusCode.OK, new
        {
            message = ApiResponseMessages.LogoutSuccessful
        });
    }

    private async Task<APIGatewayProxyResponse> Register(APIGatewayProxyRequest request)
    {
        var userService = _serviceProvider.GetRequiredService<IUserService>();

        var userRegisterRequest = ParseRequestOrThrow<UserRegisterRequest>(request.Body);

        if (!userRegisterRequest.IsValid())
            throw new HttpResponseException(HttpStatusCode.BadRequest, ApiResponseMessages.UserPasswordMatchError);

        var userFound = await userService.GetUserByUsernameAsync(userRegisterRequest.Username);

        if (userFound != null)
            throw new HttpResponseException(HttpStatusCode.Conflict, ApiResponseMessages.UsernameAlreadyExists);

        if (!await userService.CreateUserAsync(userRegisterRequest.Username, userRegisterRequest.Password, userRegisterRequest.Name))
            throw new HttpResponseException(HttpStatusCode.InternalServerError, ApiResponseMessages.ErrorCreatingUser);

        return BuildResponse(HttpStatusCode.Created, new { message = ApiResponseMessages.UserCreatedSuccessfully });
    }

    private async Task<APIGatewayProxyResponse> GetProfile(APIGatewayProxyRequest request)
    {
        var userId = ValidateTokenOrThrow(request);
        var userService = _serviceProvider.GetRequiredService<IUserService>();
        var bankAccountService = _serviceProvider.GetRequiredService<IBankAccountService>();

        var user = await userService.GetUserByIdAsync(userId) ?? throw new HttpResponseException(HttpStatusCode.NotFound, ApiResponseMessages.UserNotFound);
        var accounts = await bankAccountService.GetBankAccountsByUserIdAsync(userId);

        return BuildResponse(HttpStatusCode.OK, new UserProfileResponse
        {
            Username = user.Username,
            Name = user.Name,
            Status = user.Status,
            Accounts = accounts!.Select(account => new BankAccountResponse
            {
                Id = account.Id,
                Balance = account.Balance,
                Currency = account.Currency,
            }).ToList()
        });
    }

    private APIGatewayProxyResponse BuildPreflightResponse() =>
        new()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Headers = GetCorsHeaders()
        };

    private APIGatewayProxyResponse BuildResponse(HttpStatusCode statusCode, object body) =>
        new()
        {
            StatusCode = (int)statusCode,
            Headers = GetCorsHeaders(),
            Body = JsonSerializer.Serialize(
                new ApiResponse
                {
                    Data = body,
                    StatusCode = (int)statusCode,
                }),
        };

    private Dictionary<string, string> GetCorsHeaders() =>
        new()
        {
            { "Access-Control-Allow-Origin", "*" },
            { "Access-Control-Allow-Methods", "GET, POST, OPTIONS" },
            { "Access-Control-Allow-Headers", "Content-Type, Authorization" }
        };
}
