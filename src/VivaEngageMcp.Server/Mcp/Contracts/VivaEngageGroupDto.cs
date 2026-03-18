namespace VivaEngageMcp.Server.Mcp.Contracts;

public sealed class VivaEngageGroupDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Privacy { get; init; }
    public string? WebUrl { get; init; }
    public int? MemberCount { get; init; }
}
