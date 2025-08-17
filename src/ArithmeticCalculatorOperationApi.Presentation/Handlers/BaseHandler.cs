using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using ArithmeticCalculatorOperationApi.Application.Helpers;
using System.Text.RegularExpressions;

namespace ArithmeticCalculatorOperationApi.Presentation.Handlers;

public abstract class BaseHandler
{
    private static readonly Regex StageRegex = new(@"^/([^/]+)/api(/.*)?$", RegexOptions.Compiled);
    private static readonly Regex ApiPrefixRegex = new(@"^/api(/.*)?$", RegexOptions.Compiled);

    protected static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "/";

        var stageMatch = StageRegex.Match(path);
        if (stageMatch.Success)
        {
            return stageMatch.Groups[2].Value ?? "/";
        }

        var apiMatch = ApiPrefixRegex.Match(path);
        if (apiMatch.Success)
        {
            return apiMatch.Groups[1].Value ?? "/";
        }

        return path;
    }

    protected static APIGatewayProxyResponse HandleOptionsRequest()
    {
        return ResponseHelper.BuildResponse(HttpStatusCode.OK, new { message = "CORS preflight" });
    }

    protected static APIGatewayProxyResponse HandleNotFound()
    {
        return ResponseHelper.BuildResponse(HttpStatusCode.NotFound, new { error = "Endpoint not found" });
    }
}
