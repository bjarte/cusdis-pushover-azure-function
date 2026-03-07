namespace CusdisPushoverWebhook.Models;

public record CusdisWebhook(
    string Type,
    CusdisComment Data
);