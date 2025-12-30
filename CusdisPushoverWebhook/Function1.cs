using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CusdisPushoverWebhook;

public class Function1(ILogger<Function1> logger)
{
    [Function("Function1")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest request)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var url = request.GetDisplayUrl();

        var queryString = string.Join("\n", request.Query.Select(kvp => $"{kvp.Key}={kvp.Value}"));

        if (string.IsNullOrEmpty(queryString))
        {
            queryString = "(empty)";
        }

        var formString = string.Empty;

        if (request.Method.Equals("POST"))
        {
            var data = await request.ReadFromJsonAsync<dynamic>();
            formString = System.Text.Json.JsonSerializer.Serialize(data);
        }

        if (string.IsNullOrEmpty(formString))
        {
            formString = "(empty)";
        }

        return new OkObjectResult($"Here's the request you sent:\n\n" +
            $"Url: {url}\n\n" +
            $"Query parameters:\n" +
            $"{queryString}\n" +
            $"Post parameters:\n\n" +
            $"{formString}");
    }
}