namespace VivaEngageMcp.Server.Mcp.Contracts;

public sealed class VivaEngageSearchResultDto
{
    public IReadOnlyList<VivaEngageMessageThreadSummaryDto> Messages { get; init; } = [];
    public IReadOnlyList<VivaEngageSearchUserDto> Users { get; init; } = [];
    public IReadOnlyList<VivaEngageGroupDto> Groups { get; init; } = [];
    public IReadOnlyList<VivaEngageTopicDto> Topics { get; init; } = [];
    public int? TotalMessages { get; init; }
    public int? TotalUsers { get; init; }
    public int? TotalGroups { get; init; }
    public int? TotalTopics { get; init; }
}

public sealed class VivaEngageSearchUserDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? WebUrl { get; init; }
}

public sealed class VivaEngageTopicDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? WebUrl { get; init; }
}
