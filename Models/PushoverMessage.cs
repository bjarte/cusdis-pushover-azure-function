namespace CusdisPushoverWebhook.Models;

public record PushoverMessage(
    string Token,
    string User,
    string Title,
    string Message,
    string UrlTitle,
    string Url
);