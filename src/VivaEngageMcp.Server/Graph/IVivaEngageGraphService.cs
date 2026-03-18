using VivaEngageMcp.Server.Mcp.Contracts;

namespace VivaEngageMcp.Server.Graph;

public interface IVivaEngageGraphService
{
    Task<PagedResult<CommunityDto>> ListCommunitiesAsync(ListCommunitiesRequest request, CancellationToken cancellationToken = default);
    Task<CommunityDto> GetCommunityAsync(string communityId, CancellationToken cancellationToken = default);
    Task<AssignedRolesDto> GetMyAssignedRolesAsync(CancellationToken cancellationToken = default);
}
