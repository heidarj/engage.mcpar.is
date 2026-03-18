namespace VivaEngageMcp.Server.Mcp.Contracts;

public sealed class VivaEngageCurrentUserDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public string? JobTitle { get; init; }
    public string? Email { get; init; }
    public long NetworkId { get; init; }
    public string? WebUrl { get; init; }
}
