using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CusdisPushoverWebhook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CusdisPushoverWebhook;

public class CusdisNotification(
    IConfiguration configuration,
    JsonSerializerOptions jsonOptions,
    ILogger<CusdisNotification> logger)
{
    [Function("CusdisNotification")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "get")]
        HttpRequest request)
    {
        logger.LogInformation("Processing Cusdis webhook request");

        if (!request.Method.Equals("POST"))
        {
            logger.LogError("Invalid request method: Only POST allowed");
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
            logger.LogError("Invalid webhook data: Request body did not match expected schema");
            return new NotFoundObjectResult(
                "Invalid webhook data: Request body did not match expected schema");
        }

        var commentData = webhookData?.Data;
        if (commentData == null)
        {
            logger.LogError("Invalid webhook data: {webhookDataJson}", JsonSerializer.Serialize(webhookData));
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
                logger.LogInformation("Notification sent successfully to Pushover!");
                responseMessage.Append("Notification sent successfully Pushover!");
            }
            else
            {
                logger.LogError("Failed to send notification: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                responseMessage.Append($"Failed to send notification: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception exception)
        {
            logger.LogError("Failed to send notification: {Message}", exception.Message);
            responseMessage.Append($"Failed to send notification: {exception.Message}");
        }

        return new OkObjectResult(responseMessage.ToString());
    }
}