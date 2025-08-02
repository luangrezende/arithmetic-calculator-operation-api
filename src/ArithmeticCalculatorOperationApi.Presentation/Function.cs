using System.Net;
using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ArithmeticCalculatorOperationApi.Application.Constants;
using ArithmeticCalculatorOperationApi.Application.Helpers;
using ArithmeticCalculatorOperationApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorOperationApi.Application.Interfaces.Services;
using ArithmeticCalculatorOperationApi.Application.Services;
using ArithmeticCalculatorOperationApi.Infrastructure.Persistence.Repositories;
using ArithmeticCalculatorOperationApi.Infrastructure.Persistence.Services;
using ArithmeticCalculatorOperationApi.Infrastructure.Security;
using ArithmeticCalculatorOperationApi.Presentation.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ArithmeticCalculatorOperationApi.Presentation;

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
        services.AddLogging(builder =>
        {
            builder.AddLambdaLogger();
        });

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOperationTypeService, OperationTypeService>();
        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<IRandomStringService, RandomStringService>();

        // HTTP e AWS Lambda client
        services.AddScoped<HttpClient>();
        services.AddScoped<LambdaInvoker>();
        services.AddAWSService<IAmazonLambda>();

        // Repositories
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IOperationTypeRepository, OperationTypeRepository>();

        // MySQL connection service
        services.AddScoped<IDbConnectionService>(provider =>
        {
            var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
                ?? throw new InvalidOperationException(ApiErrorMessages.ConnectionStringNotSet);
            return new MySqlConnectionService(connectionString);
        });

        // JWT validator
        services.AddScoped(sp =>
            new JwtTokenValidator(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!));
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var handler = new OperationHandler(_serviceProvider);

        try
        {
            return await handler.HandleRequest(request);
        }
        catch (Exception ex)
        {
            LogError(context, "Unhandled Exception", ex);
            return HandleException(ex, context);
        }
    }

    private static void LogError(ILambdaContext context, string errorType, Exception ex)
    {
        var correlationId = Guid.NewGuid();
        context.Logger.LogError($"[{correlationId}] {errorType}: {ex.Message} \nStackTrace: {ex.StackTrace}");
    }

    private APIGatewayProxyResponse HandleException(Exception ex, ILambdaContext context)
    {
        LogError(context, ex.GetType().Name, ex);

        return ex switch
        {
            HttpResponseExceptionHelper httpEx => ResponseHelper.BuildResponse(httpEx.StatusCode, new { error = httpEx.Message }),
            InvalidOperationException invalidEx => ResponseHelper.BuildResponse(HttpStatusCode.BadRequest, new { error = invalidEx.Message }),
            SecurityTokenExpiredException => ResponseHelper.BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiErrorMessages.TokenExpired }),
            SecurityTokenMalformedException => ResponseHelper.BuildResponse(HttpStatusCode.Unauthorized, new { error = ApiErrorMessages.InvalidToken }),
            ArgumentException argEx => ResponseHelper.BuildResponse(HttpStatusCode.BadRequest, new { error = argEx.Message }),
            _ => ResponseHelper.BuildResponse(HttpStatusCode.InternalServerError, new { error = ApiErrorMessages.InternalServerError }),
        };
    }
}
