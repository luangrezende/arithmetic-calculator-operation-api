using Amazon.Lambda;
using Amazon.Lambda.Model;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class LambdaInvoker
{
    private readonly IAmazonLambda _lambdaClient;
    private readonly ILogger<LambdaInvoker> _logger;

    public LambdaInvoker(IAmazonLambda lambdaClient, ILogger<LambdaInvoker> logger)
    {
        _lambdaClient = lambdaClient;
        _logger = logger;
    }

    public async Task<T> InvokeLambdaAsync<T>(string TargetLambdaArn, object payload)
    {
        var serializedPayload = JsonSerializer.Serialize(payload);

        _logger.LogInformation("Invoking Lambda function '{FunctionName}' with payload: {Payload}",
            TargetLambdaArn, serializedPayload);

        var request = new InvokeRequest
        {
            FunctionName = TargetLambdaArn,
            Payload = serializedPayload,
            InvocationType = InvocationType.RequestResponse
        };

        var response = await _lambdaClient.InvokeAsync(request);

        _logger.LogInformation("Lambda '{FunctionName}' responded with StatusCode: {StatusCode}",
            TargetLambdaArn, response.StatusCode);

        if (!string.IsNullOrEmpty(response.FunctionError))
        {
            using var errReader = new StreamReader(response.Payload);
            var errContent = await errReader.ReadToEndAsync();

            _logger.LogError("FunctionError from '{FunctionName}': {FunctionError}",
                TargetLambdaArn, response.FunctionError);
            _logger.LogError("Error payload: {ErrorPayload}", errContent);

            throw new InvalidOperationException(
                $"Lambda {TargetLambdaArn} execution failed: {response.FunctionError}");
        }

        // --- Ler resposta ---
        using var reader = new StreamReader(response.Payload);
        var responseContent = await reader.ReadToEndAsync();

        _logger.LogInformation("Lambda '{FunctionName}' raw response: {ResponseContent}",
            TargetLambdaArn, responseContent);

        try
        {
            var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new InvalidOperationException($"Invalid response from Lambda {TargetLambdaArn}.");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response from Lambda '{FunctionName}'", TargetLambdaArn);
            throw;
        }
    }
}