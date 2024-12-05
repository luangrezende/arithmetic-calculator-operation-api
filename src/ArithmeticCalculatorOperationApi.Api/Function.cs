using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ArithmeticCalculatorOperationApi.Domain.Constants;
using ArithmeticCalculatorOperationApi.Domain.Models.DTO;
using ArithmeticCalculatorOperationApi.Domain.Models.Request;
using ArithmeticCalculatorOperationApi.Domain.Models.Response;
using ArithmeticCalculatorOperationApi.Domain.Services;
using ArithmeticCalculatorOperationApi.Domain.Services.Interfaces;
using ArithmeticCalculatorOperationApi.Helpers;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories;
using ArithmeticCalculatorOperationApi.Infrastructure.Repositories.Interfaces;
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
        services.AddScoped<IOperationTypeService, OperationTypeService>();
        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<IRandomStringService, RandomStringService>();

        services.AddScoped<HttpClient>();
        services.AddScoped<LambdaInvoker>();

        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IOperationTypeRepository, OperationTypeRepository>();

        services.AddScoped(sp => new JwtTokenValidator(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!));

        services.AddAWSService<IAmazonLambda>();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            if (request.HttpMethod == "OPTIONS")
                return BuildPreflightResponse();

            return request.HttpMethod switch
            {
                "GET" when request.Path == "/v1/operations/types" => await GetOperationsType(request),
                "GET" when request.Path == "/v1/operations/records" => await GetPagedOperations(request),
                "POST" when request.Path == "/v1/operations/records" => await AddOperation(request),
                "DELETE" when request.Path == "/v1/operations/records" => await SoftDeleteOperationRecords(request),

                _ => BuildResponse(HttpStatusCode.NotFound, new { error = ApiResponseMessages.EndpointNotFound }),
            };
        }
        catch (Exception ex)
        {
            return HandleException(ex, context);
        }
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

    public async Task<APIGatewayProxyResponse> SoftDeleteOperationRecords(APIGatewayProxyRequest request)
    {
        var userId = ValidateTokenAndGetUserId(request);

        var operationService = _serviceProvider.GetRequiredService<IOperationService>();

        if (string.IsNullOrWhiteSpace(request.Body))
            return BuildResponse(HttpStatusCode.BadRequest, new { error = OperationsMessages.RequestBodyRequired });

        var requestBody = ParseRequestOrThrow<SoftDeleteOperationRecordRequest>(request.Body);
        if (requestBody?.Ids == null || !requestBody.Ids.Any())
            return BuildResponse(HttpStatusCode.BadRequest, new { error = OperationsMessages.ListOfIdsRequired });

        var success = await operationService.SoftDeleteOperationRecordsAsync(userId, requestBody.Ids);

        if (!success)
            return BuildResponse(HttpStatusCode.NotFound, new { error = OperationsMessages.RecordsNotFoundOrDeleted });

        return BuildResponse(HttpStatusCode.NoContent, null!);
    }

    private async Task<APIGatewayProxyResponse> GetPagedOperations(APIGatewayProxyRequest request)
    {
        var userId = ValidateTokenAndGetUserId(request);
        var operationService = _serviceProvider.GetRequiredService<IOperationService>();

        int page = int.TryParse(request.QueryStringParameters?["page"], out var p) ? p : 0;
        int pageSize = int.TryParse(request.QueryStringParameters?["pageSize"], out var ps) ? ps : 10;
        string query = request.QueryStringParameters?["query"] ?? string.Empty;

        var (totalRecords, records) = await operationService.GetPagedOperationsAsync(userId, page, pageSize, query);

        var mappedRecords = records.Select(record => new OperationRecordResponse
        {
            Id = record.Id,
            Cost = record.Cost,
            UserBalance = record.UserBalance,
            Expression = record.Expression,
            Result = record.Result,
            CreatedAt = record.CreatedAt
        }).ToList();

        return BuildResponse(HttpStatusCode.OK, new OperationRecordPagedResponse
        {
            Records = mappedRecords,
            Total = totalRecords,
            Page = page,
            PageSize = pageSize,
        });
    }

    private async Task<APIGatewayProxyResponse> GetOperationsType(APIGatewayProxyRequest request)
    {
        var (userId, token) = ValidateTokenAndReturnOrThrow(request);

        var operationTypeService = _serviceProvider.GetRequiredService<IOperationTypeService>();

        var operationTypes = await operationTypeService.GetAllAsync();

        if (operationTypes == null || !operationTypes.Any())
            return BuildResponse(HttpStatusCode.NotFound, new { Message = ApiResponseMessages.NoOperationsFound });

        var operationResponses = operationTypes.Select(op => new OperationTypeResponse
        {
            Id = op.Id,
            Cost = op.Cost,
            Description = op.Description,
        }).ToList();

        return BuildResponse(HttpStatusCode.OK, operationResponses);
    }

    private async Task<APIGatewayProxyResponse> AddOperation(APIGatewayProxyRequest request)
    {
        var (userId, token) = ValidateTokenAndReturnOrThrow(request);

        var addOperationRequest = ParseRequestOrThrow<AddOperationRequest>(request.Body);

        var operationService = _serviceProvider.GetRequiredService<IOperationService>();
        var userService = _serviceProvider.GetRequiredService<IUserService>();

        if (addOperationRequest?.AccountId == Guid.Empty)
            return BuildResponse(HttpStatusCode.BadRequest, new
            {
                error = ApiResponseMessages.AccountIdNotNull
            });

        if (string.IsNullOrEmpty(addOperationRequest?.Expression))
            return BuildResponse(HttpStatusCode.BadRequest, new
            {
                error = ApiResponseMessages.ExpressionNotNull
            });

        var operationPrice = await operationService.CalculateOperationPriceAsync(addOperationRequest.Expression!);
        var result = await operationService.CalculateOperation(addOperationRequest.Expression!);

        var updatedBalance = await userService.DebitUserBalanceDirectAsync(addOperationRequest.AccountId, operationPrice, token);

        var operationDto = new OperationRecordDTO
        {
            UserId = userId,
            Cost = operationPrice,
            UserBalance = updatedBalance,
            Expression = addOperationRequest.Expression,
            Result = result
        };

        var operationSaved = await operationService.SaveOperationRecordAsync(operationDto);

        return BuildResponse(HttpStatusCode.Created, new OperationResponse
        {
            Message = ApiResponseMessages.OperationAdded,
            OperationRecord = new OperationRecordResponse
            {
                Id = operationSaved.Id,
                Cost = operationDto.Cost,
                Expression = operationDto.Expression,
                Result = operationDto.Result,
                UserBalance = operationDto.UserBalance,
                CreatedAt = operationSaved.CreatedAt,
            }
        });
    }

    private (Guid, string) ValidateTokenAndReturnOrThrow(APIGatewayProxyRequest request)
    {
        var jwtTokenValidator = _serviceProvider.GetRequiredService<JwtTokenValidator>();
        if (!request.Headers.TryGetValue("Authorization", out var authorization) || string.IsNullOrWhiteSpace(authorization))
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiResponseMessages.InvalidToken);

        var token = authorization.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        if (!jwtTokenValidator.ValidateToken(token, out var userId))
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiResponseMessages.InvalidToken);

        return (userId, token);
    }

    private Guid ValidateTokenAndGetUserId(APIGatewayProxyRequest request)
    {
        var jwtTokenValidator = _serviceProvider.GetRequiredService<JwtTokenValidator>();

        if (!request.Headers.TryGetValue("Authorization", out var authorization) || string.IsNullOrWhiteSpace(authorization))
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiResponseMessages.InvalidToken);

        var token = authorization.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

        if (!jwtTokenValidator.ValidateToken(token, out var userId))
            throw new HttpResponseException(HttpStatusCode.Unauthorized, ApiResponseMessages.InvalidToken);

        return userId;
    }

    private APIGatewayProxyResponse HandleException(Exception ex, ILambdaContext context)
    {
        context.Logger.LogError($"Exception: {ex.Message}");

        return ex switch
        {
            HttpResponseException httpEx => BuildResponse(httpEx.StatusCode, new { error = httpEx.ResponseBody }),
            SecurityTokenExpiredException => BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiResponseMessages.TokenExpired }),
            SecurityTokenMalformedException => BuildResponse(HttpStatusCode.BadRequest, new { error = ApiResponseMessages.InvalidToken }),
            _ => BuildResponse(HttpStatusCode.InternalServerError, new { error = ex.Message }),
        };
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
