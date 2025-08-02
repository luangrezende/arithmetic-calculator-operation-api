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

    public async Task<T> InvokeLambdaAsync<T>(string functionName, object payload)
    {
        var serializedPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        _logger.LogInformation("Invoking Lambda function '{FunctionName}' with payload:", functionName);
        _logger.LogInformation("Payload: {Payload}", serializedPayload);

        var request = new InvokeRequest
        {
            FunctionName = functionName,
            Payload = serializedPayload,
            InvocationType = InvocationType.RequestResponse
        };

        _logger.LogDebug("Lambda invoke request - FunctionName: {FunctionName}, InvocationType: {InvocationType}", 
            functionName, request.InvocationType);

        var response = await _lambdaClient.InvokeAsync(request);

        _logger.LogInformation("Lambda function '{FunctionName}' responded with status code: {StatusCode}", 
            functionName, response.StatusCode);

        if (response.StatusCode != 200)
        {
            _logger.LogError("Failed to invoke Lambda function '{FunctionName}'. Status code: {StatusCode}, ExecutedVersion: {ExecutedVersion}", 
                functionName, response.StatusCode, response.ExecutedVersion);
            throw new InvalidOperationException($"Failed to invoke Lambda function {functionName}. Status code: {response.StatusCode}");
        }

        using var reader = new StreamReader(response.Payload);
        var responseContent = await reader.ReadToEndAsync();

        _logger.LogInformation("Lambda function '{FunctionName}' response content:", functionName);
        _logger.LogInformation("Response: {ResponseContent}", responseContent);

        return JsonSerializer.Deserialize<T>(responseContent)
               ?? throw new InvalidOperationException($"Invalid response from Lambda {functionName}.");
    }
}