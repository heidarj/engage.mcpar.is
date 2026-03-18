using Microsoft.Graph.Models;
using VivaEngageMcp.Server.Mcp.Contracts;

namespace VivaEngageMcp.Server.Mcp.Mapping;

public static class CommunityMapper
{
    public static CommunityDto MapFromGraph(Community c) => new()
    {
        Id = c.Id ?? string.Empty,
        DisplayName = c.DisplayName ?? string.Empty,
        Description = c.Description,
        Privacy = c.Privacy?.ToString()?.ToLowerInvariant() ?? "unknownfuturevalue",
        GroupId = c.GroupId
    };
}
