namespace VivaEngageMcp.Server.Mcp.Contracts;

public sealed class AssignedRoleItemDto
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

public sealed class AssignedRolesDto
{
    public IReadOnlyList<AssignedRoleItemDto> Items { get; init; } = [];
}
