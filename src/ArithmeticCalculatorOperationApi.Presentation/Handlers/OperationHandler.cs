using Amazon.Lambda.APIGatewayEvents;
using ArithmeticCalculatorOperationApi.Application.Constants;
using ArithmeticCalculatorOperationApi.Application.DTOs;
using ArithmeticCalculatorOperationApi.Application.Helpers;
using ArithmeticCalculatorOperationApi.Application.Interfaces.Services;
using ArithmeticCalculatorOperationApi.Application.Models.Request;
using ArithmeticCalculatorOperationApi.Application.Models.Response;
using ArithmeticCalculatorOperationApi.Domain.Configurations;
using ArithmeticCalculatorOperationApi.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace ArithmeticCalculatorOperationApi.Presentation.Handlers
{
    public class OperationHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public OperationHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request)
        {
            return request.HttpMethod switch
            {
                "OPTIONS" => HandleOptionsRequest(),
                "GET" when request.Path == "/operation/health" => HandleHealthCheck(),
                "GET" when request.Path == "/operation/types" => await GetOperationsType(request),
                "GET" when request.Path == "/operation/records" => await GetPagedOperations(request),
                "GET" when request.Path == "/operation/dashboard" => await GetDashboardData(request),
                "POST" when request.Path == "/operation/records" => await AddOperation(request),
                "DELETE" when request.Path == "/operation/records" => await SoftDeleteOperationRecords(request),
                _ => ResponseHelper.BuildResponse(HttpStatusCode.NotFound, new { error = ApiErrorMessages.EndpointNotFound })
            };
        }

        public async Task<APIGatewayProxyResponse> SoftDeleteOperationRecords(APIGatewayProxyRequest request)
        {
            var userId = ValidateTokenAndReturnUserId(request);

            var operationService = _serviceProvider.GetRequiredService<IOperationService>();

            if (string.IsNullOrWhiteSpace(request.Body))
                return ResponseHelper.BuildResponse(HttpStatusCode.BadRequest, new { error = OperationsErrorMessages.RequestBodyRequired });

            var requestBody = ResponseHelper.ParseRequestOrThrow<SoftDeleteOperationRecordRequest>(request.Body);
            if (requestBody?.Ids == null || !requestBody.Ids.Any())
                return ResponseHelper.BuildResponse(HttpStatusCode.BadRequest, new { error = OperationsErrorMessages.ListOfIdsRequired });

            var success = await operationService.SoftDeleteOperationRecordsAsync(userId, requestBody.Ids);

            if (!success)
                return ResponseHelper.BuildResponse(HttpStatusCode.NotFound, new { error = OperationsErrorMessages.RecordsNotFoundOrDeleted });

            return ResponseHelper.BuildResponse(HttpStatusCode.NoContent, null!);
        }

        public async Task<APIGatewayProxyResponse> GetDashboardData(APIGatewayProxyRequest request)
        {
            var userId = ValidateTokenAndReturnUserId(request);

            var operationService = _serviceProvider.GetRequiredService<IOperationService>();
            var data = await operationService.GetDashboardDataAsync(userId);

            return ResponseHelper.BuildResponse(HttpStatusCode.OK, data);
        }

        private async Task<APIGatewayProxyResponse> GetPagedOperations(APIGatewayProxyRequest request)
        {
            var userId = ValidateTokenAndReturnUserId(request);

            var (page, pageSize, query) = ParseQueryParameters(request);

            var operationService = _serviceProvider.GetRequiredService<IOperationService>();
            var (totalRecords, records) = await operationService.GetPagedOperationsAsync(userId, page, pageSize, query);

            var mappedRecords = MapOperationRecords(records);

            return ResponseHelper.BuildResponse(HttpStatusCode.OK, new OperationRecordPagedResponse
            {
                Records = mappedRecords,
                Total = totalRecords,
                Page = page,
                PageSize = pageSize,
            });
        }
        
        private (int Page, int PageSize, string Query) ParseQueryParameters(APIGatewayProxyRequest request)
        {
            int page = int.TryParse(request.QueryStringParameters?["page"], out var p) ? p : 0;
            int pageSize = int.TryParse(request.QueryStringParameters?["pageSize"], out var ps) ? ps : 10;
            string query = request.QueryStringParameters?["query"] ?? string.Empty;

            return (page, pageSize, query);
        }

        private List<OperationRecordResponse> MapOperationRecords(IEnumerable<OperationRecordDTO> records)
        {
            return records.Select(record =>
            {
                return new OperationRecordResponse
                {
                    Id = record.Id,
                    Cost = record.Cost,
                    UserBalance = record.UserBalance,
                    Type = OperationConfiguration.ArithmeticStringType,
                    Expression = record.Expression,
                    Result = record.Result,
                    CreatedAt = record.CreatedAt
                };
            }).ToList();
        }


        private async Task<APIGatewayProxyResponse> GetOperationsType(APIGatewayProxyRequest request)
        {
            var (userId, token) = ValidateTokenAndReturnWithUserIdOrThrow(request);

            var operationTypeService = _serviceProvider.GetRequiredService<IOperationTypeService>();

            var operationTypes = await operationTypeService.GetAllAsync();

            if (operationTypes == null || !operationTypes.Any())
                return ResponseHelper.BuildResponse(HttpStatusCode.NotFound, new { Message = ApiErrorMessages.NoOperationsFound });

            var operationResponses = operationTypes.Select(op => new OperationTypeResponse
            {
                Id = op.Id,
                Cost = op.Cost,
                Description = op.Description,
                OperatorCode = op.OperatorCode,
            }).ToList();

            return ResponseHelper.BuildResponse(HttpStatusCode.OK, operationResponses);
        }

        private async Task<APIGatewayProxyResponse> AddOperation(APIGatewayProxyRequest request)
        {
            var (userId, token) = ValidateTokenAndReturnWithUserIdOrThrow(request);

            var addOperationRequest = ResponseHelper.ParseRequestOrThrow<AddOperationRequest>(request.Body);

            var operationService = _serviceProvider.GetRequiredService<IOperationService>();
            var userService = _serviceProvider.GetRequiredService<IUserService>();

            if (addOperationRequest?.AccountId == Guid.Empty)
                return ResponseHelper.BuildResponse(HttpStatusCode.BadRequest, new
                {
                    error = ApiErrorMessages.AccountIdNotNull
                });

            if (string.IsNullOrEmpty(addOperationRequest?.Expression))
                return ResponseHelper.BuildResponse(HttpStatusCode.BadRequest, new
                {
                    error = ApiErrorMessages.ExpressionNotNull
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

            return ResponseHelper.BuildResponse(HttpStatusCode.Created, new OperationResponse
            {
                Message = ApiErrorMessages.OperationAdded,
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

        private (Guid, string) ValidateTokenAndReturnWithUserIdOrThrow(APIGatewayProxyRequest request)
        {
            var (userId, token) = ExtractTokenAndValidate(request);
            return (userId, token);
        }

        private Guid ValidateTokenAndReturnUserId(APIGatewayProxyRequest request)
        {
            var (userId, _) = ExtractTokenAndValidate(request);
            return userId;
        }

        private (Guid, string) ExtractTokenAndValidate(APIGatewayProxyRequest request)
        {
            var jwtTokenValidator = _serviceProvider.GetRequiredService<JwtTokenValidator>();

            if (!request.Headers.TryGetValue("Authorization", out var authorization) || string.IsNullOrWhiteSpace(authorization))
                throw new HttpResponseExceptionHelper(HttpStatusCode.Unauthorized, ApiErrorMessages.InvalidToken);

            var token = authorization.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

            if (!jwtTokenValidator.ValidateToken(token, out var userId))
                throw new HttpResponseExceptionHelper(HttpStatusCode.Unauthorized, ApiErrorMessages.InvalidToken);

            return (userId, token);
        }

        private static APIGatewayProxyResponse HandleOptionsRequest()
        {
            return ResponseHelper.BuildResponse(HttpStatusCode.OK, new { message = "CORS preflight" });
        }

        private static APIGatewayProxyResponse HandleHealthCheck()
        {
            return ResponseHelper.BuildResponse(HttpStatusCode.OK, new { message = "Operation API is healthy", timestamp = DateTime.UtcNow });
        }
    }
}
