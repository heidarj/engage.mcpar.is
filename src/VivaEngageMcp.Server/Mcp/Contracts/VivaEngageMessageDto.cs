namespace VivaEngageMcp.Server.Mcp.Contracts;

public sealed class VivaEngageMessageDto
{
    public long Id { get; init; }
    public long? ThreadId { get; init; }
    public long? GroupId { get; init; }
    public string? GroupName { get; init; }
    public string? Body { get; init; }
    public string? ContentType { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public long? SenderId { get; init; }
    public string? SenderName { get; init; }
    public string? SenderEmail { get; init; }
    public string? Privacy { get; init; }
    public string? WebUrl { get; init; }
    public int? LikedCount { get; init; }
    public int? RepliesCount { get; init; }
}

public sealed class VivaEngageMessageThreadSummaryDto
{
    public long ThreadId { get; init; }
    public VivaEngageMessageDto? LatestMessage { get; init; }
    public int? MessageCount { get; init; }
}
