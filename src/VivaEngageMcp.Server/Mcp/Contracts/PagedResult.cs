namespace VivaEngageMcp.Server.Mcp.Contracts;

/// <summary>Represents a paginated result set.</summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public string? NextLink { get; init; }
}
