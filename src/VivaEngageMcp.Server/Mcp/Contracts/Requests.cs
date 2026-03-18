using System.ComponentModel.DataAnnotations;

namespace VivaEngageMcp.Server.Mcp.Contracts;

public sealed class ListCommunitiesRequest
{
    [Range(1, 100)]
    public int Top { get; init; } = 25;

    public string? Search { get; init; }

    public string? NextLink { get; init; }
}

public sealed class ListGroupsForUserRequest
{
    [Required]
    public long UserId { get; init; }

    [Range(1, 100)]
    public int Page { get; init; } = 1;
}

public enum FeedType
{
    MyFeed,
    Network,
    Following,
    Sent,
    Received,
    Private
}

public sealed class ListFeedMessagesRequest
{
    public FeedType FeedType { get; init; } = FeedType.MyFeed;
    public long? OlderThan { get; init; }
    public long? NewerThan { get; init; }
    public bool Threaded { get; init; } = true;

    [Range(1, 100)]
    public int Limit { get; init; } = 20;
}

public sealed class ListGroupMessagesRequest
{
    [Required]
    public long GroupId { get; init; }

    public long? OlderThan { get; init; }
    public long? NewerThan { get; init; }
    public bool Threaded { get; init; } = true;

    [Range(1, 100)]
    public int Limit { get; init; } = 20;
}

public sealed class VivaEngageSearchRequest
{
    [Required]
    public string Query { get; init; } = string.Empty;

    public string[] ModelTypes { get; init; } = ["MESSAGETHREAD", "USER", "GROUP", "TOPIC"];

    [Range(1, 100)]
    public int Page { get; init; } = 1;

    [Range(1, 50)]
    public int NumPerPage { get; init; } = 20;

    public long? SearchGroup { get; init; }
}
