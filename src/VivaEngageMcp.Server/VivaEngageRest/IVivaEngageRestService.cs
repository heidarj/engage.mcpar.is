using VivaEngageMcp.Server.Mcp.Contracts;

namespace VivaEngageMcp.Server.VivaEngageRest;

public interface IVivaEngageRestService
{
    Task<VivaEngageCurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<VivaEngageGroupDto>> ListGroupsForUserAsync(ListGroupsForUserRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<VivaEngageMessageThreadSummaryDto>> ListFeedMessagesAsync(ListFeedMessagesRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<VivaEngageMessageDto>> ListGroupMessagesAsync(ListGroupMessagesRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VivaEngageMessageDto>> GetThreadMessagesAsync(long threadId, CancellationToken cancellationToken = default);
    Task<VivaEngageSearchResultDto> SearchAsync(VivaEngageSearchRequest request, CancellationToken cancellationToken = default);
}
