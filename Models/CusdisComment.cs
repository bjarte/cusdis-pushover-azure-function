namespace CusdisPushoverWebhook.Models;

public record CusdisComment(
    string ByNickname,
    string Content,
    string PageId,
    string PageTitle,
    string ApproveLink
);