using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CusdisPushoverWebhook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace CusdisPushoverWebhook;

public class CusdisNotification(
    IConfiguration configuration,
    JsonSerializerOptions jsonOptions)
{
    [Function("CusdisNotification")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "get")]
        HttpRequest request)
    {
        if (!request.Method.Equals("POST"))
        {
            return new BadRequestObjectResult(
                "Invalid request method: Only POST allowed");
        }

        CusdisWebhook? webhookData;
        try
        {
            webhookData = await request.ReadFromJsonAsync<CusdisWebhook>();
        }
        catch (JsonException)
        {
            return new NotFoundObjectResult(
                "Invalid webhook data: Request body did not match expected schema");
        }

        var commentData = webhookData?.Data;
        if (commentData == null)
        {
            return new NotFoundObjectResult(
                $"Invalid webhook data: {JsonSerializer.Serialize(webhookData)}");
        }

        var name = commentData.ByNickname;
        var comment = commentData.Content;

        var responseMessage = new StringBuilder();

        responseMessage.Append("Received webhook from Cusdis\n\n" +
            $"Name:    {name}\n" +
            $"Comment: {comment}\n\n");

        try
        {
            var pushoverMessage = new PushoverMessage(
                Token: configuration["PushoverToken"]!,
                User: configuration["PushoverUser"]!,
                Title: $"New comment from {name}",
                Message: comment,
                UrlTitle: "Go to Cusdis to handle comment",
                Url: configuration["CusdisUrl"]!
            );

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync(
                configuration["PushoverApiUrl"],
                pushoverMessage,
                jsonOptions
            );

            if (response.IsSuccessStatusCode)
            {
                responseMessage.Append("Notification sent successfully!");
            }
            else
            {
                responseMessage.Append($"Failed to send notification: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception exception)
        {
            responseMessage.Append($"Failed to send notification: {exception.Message}");
        }

        return new OkObjectResult(responseMessage.ToString());
    }
}