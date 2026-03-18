namespace VivaEngageMcp.Server.Mcp.Contracts;

public sealed class CommunityDto
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Privacy { get; init; } = "unknownFutureValue";
    public string? GroupId { get; init; }
}
