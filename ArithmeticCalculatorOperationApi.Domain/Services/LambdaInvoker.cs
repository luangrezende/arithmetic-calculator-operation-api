using Amazon.Lambda;
using Amazon.Lambda.Model;
using System.Text.Json;

public class LambdaInvoker
{
    private readonly IAmazonLambda _lambdaClient;

    public LambdaInvoker(IAmazonLambda lambdaClient)
    {
        _lambdaClient = lambdaClient;
    }

    public async Task<T> InvokeLambdaAsync<T>(string functionName, object payload)
    {
        var serializedPayload = JsonSerializer.Serialize(payload);

        var request = new InvokeRequest
        {
            FunctionName = functionName,
            Payload = serializedPayload,
            InvocationType = InvocationType.RequestResponse
        };

        var response = await _lambdaClient.InvokeAsync(request);

        if (response.StatusCode != 200)
        {
            throw new InvalidOperationException($"Failed to invoke Lambda function {functionName}. Status code: {response.StatusCode}");
        }

        using var reader = new StreamReader(response.Payload);
        var responseContent = await reader.ReadToEndAsync();

        return JsonSerializer.Deserialize<T>(responseContent)
               ?? throw new InvalidOperationException($"Invalid response from Lambda {functionName}.");
    }
}