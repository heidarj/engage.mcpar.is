using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using VivaEngageMcp.Server.Graph;
using VivaEngageMcp.Server.Mcp.Contracts;
using VivaEngageMcp.Server.VivaEngageRest;

namespace VivaEngageMcp.Server.Mcp.Tools;

[McpServerToolType]
public sealed class VivaEngageTools
{
    private readonly IVivaEngageGraphService _graphService;
    private readonly IVivaEngageRestService _restService;
    private readonly ILogger<VivaEngageTools> _logger;

    public VivaEngageTools(
        IVivaEngageGraphService graphService,
        IVivaEngageRestService restService,
        ILogger<VivaEngageTools> logger)
    {
        _graphService = graphService;
        _restService = restService;
        _logger = logger;
    }

    [McpServerTool(Name = "get_current_user", ReadOnly = true)]
    [Description("Returns the current signed-in Viva Engage user/network profile.")]
    public async Task<VivaEngageCurrentUserDto> GetCurrentUserAsync(
        CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Tool: get_current_user invoked.");
        try
        {
            var result = await _restService.GetCurrentUserAsync(cancellationToken);
            sw.Stop();
            _logger.LogInformation("Tool: get_current_user completed in {Ms}ms.", sw.ElapsedMilliseconds);
            return result;
        }
        catch (VivaEngageServiceException ex)
        {
            _logger.LogError(ex, "Tool: get_current_user failed.");
            throw;
        }
    }

    [McpServerTool(Name = "list_communities", ReadOnly = true)]
    [Description("Lists Viva Engage communities using the Microsoft Graph API. Supports pagination via nextLink.")]
    public async Task<PagedResult<CommunityDto>> ListCommunitiesAsync(
        [Description("Maximum number of communities to return (1-100). Default: 25.")] int top = 25,
        [Description("Optional search filter on community display name.")] string? search = null,
        [Description("Pagination token from a previous call's nextLink.")] string? nextLink = null,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Tool: list_communities invoked. Top={Top} Search={Search}", top, search);
        if (top < 1 || top > 100)
            throw new ArgumentOutOfRangeException(nameof(top), "top must be between 1 and 100.");
        try
        {
            var request = new ListCommunitiesRequest { Top = top, Search = search, NextLink = nextLink };
            var result = await _graphService.ListCommunitiesAsync(request, cancellationToken);
            sw.Stop();
            _logger.LogInformation("Tool: list_communities returned {Count} items in {Ms}ms.", result.Items.Count, sw.ElapsedMilliseconds);
            return result;
        }
        catch (VivaEngageServiceException ex)
        {
            _logger.LogError(ex, "Tool: list_communities failed.");
            throw;
        }
    }

    [McpServerTool(Name = "get_community", ReadOnly = true)]
    [Description("Returns metadata for a single Viva Engage community by ID.")]
    public async Task<CommunityDto> GetCommunityAsync(
        [Description("The community ID (Graph community ID).")] string communityId,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Tool: get_community invoked. CommunityId={CommunityId}", communityId);
        if (string.IsNullOrWhiteSpace(communityId))
            throw new ArgumentException("communityId is required.", nameof(communityId));
        try
        {
            var result = await _graphService.GetCommunityAsync(communityId, cancellationToken);
            sw.Stop();
            _logger.LogInformation("Tool: get_community completed in {Ms}ms.", sw.ElapsedMilliseconds);
            return result;
        }
        catch (VivaEngageServiceException ex)
        {
            _logger.LogError(ex, "Tool: get_community failed.");
            throw;
        }
    }

    [McpServerTool(Name = "list_feed_messages", ReadOnly = true)]
    [Description("Lists messages from the user's Viva Engage feed using the legacy REST API. Feed types: my_feed, network, following, sent, received, private.")]
    public async Task<PagedResult<VivaEngageMessageThreadSummaryDto>> ListFeedMessagesAsync(
        [Description("Feed type: my_feed, network, following, sent, received, private. Default: my_feed.")] string feedType = "my_feed",
        [Description("Return messages older than this message ID.")] long? olderThan = null,
        [Description("Return messages newer than this message ID.")] long? newerThan = null,
        [Description("Whether to return messages threaded. Default: true.")] bool threaded = true,
        [Description("Maximum number of messages to return (1-100). Default: 20.")] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Tool: list_feed_messages invoked. FeedType={FeedType}", feedType);
        if (limit < 1 || limit > 100)
            throw new ArgumentOutOfRangeException(nameof(limit), "limit must be between 1 and 100.");

        var feedTypeEnum = feedType.ToLowerInvariant() switch
        {
            "my_feed" => FeedType.MyFeed,
            "network" => FeedType.Network,
            "following" => FeedType.Following,
            "sent" => FeedType.Sent,
            "received" => FeedType.Received,
            "private" => FeedType.Private,
            _ => throw new ArgumentException($"Unknown feed type '{feedType}'. Valid values: my_feed, network, following, sent, received, private.", nameof(feedType))
        };

        try
        {
            var request = new ListFeedMessagesRequest
            {
                FeedType = feedTypeEnum,
                OlderThan = olderThan,
                NewerThan = newerThan,
                Threaded = threaded,
                Limit = limit
            };
            var result = await _restService.ListFeedMessagesAsync(request, cancellationToken);
            sw.Stop();
            _logger.LogInformation("Tool: list_feed_messages returned {Count} items in {Ms}ms.", result.Items.Count, sw.ElapsedMilliseconds);
            return result;
        }
        catch (VivaEngageServiceException ex)
        {
            _logger.LogError(ex, "Tool: list_feed_messages failed.");
            throw;
        }
    }

    [McpServerTool(Name = "list_group_messages", ReadOnly = true)]
    [Description("Lists messages in a specific Viva Engage group/community using the legacy REST API.")]
    public async Task<PagedResult<VivaEngageMessageDto>> ListGroupMessagesAsync(
        [Description("The Viva Engage group ID.")] long groupId,
        [Description("Return messages older than this message ID.")] long? olderThan = null,
        [Description("Return messages newer than this message ID.")] long? newerThan = null,
        [Description("Whether to return messages threaded. Default: true.")] bool threaded = true,
        [Description("Maximum number of messages to return (1-100). Default: 20.")] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Tool: list_group_messages invoked. GroupId={GroupId}", groupId);
        if (limit < 1 || limit > 100)
            throw new ArgumentOutOfRangeException(nameof(limit), "limit must be between 1 and 100.");

        try
        {
            var request = new ListGroupMessagesRequest
            {
                GroupId = groupId,
                OlderThan = olderThan,
                NewerThan = newerThan,
                Threaded = threaded,
                Limit = limit
            };
            var result = await _restService.ListGroupMessagesAsync(request, cancellationToken);
            sw.Stop();
            _logger.LogInformation("Tool: list_group_messages returned {Count} items in {Ms}ms.", result.Items.Count, sw.ElapsedMilliseconds);
            return result;
        }
        catch (VivaEngageServiceException ex)
        {
            _logger.LogError(ex, "Tool: list_group_messages failed.");
            throw;
        }
    }

    [McpServerTool(Name = "get_thread_messages", ReadOnly = true)]
    [Description("Returns the full message list for a specific Viva Engage thread using the legacy REST API.")]
    public async Task<IReadOnlyList<VivaEngageMessageDto>> GetThreadMessagesAsync(
        [Description("The Viva Engage thread ID.")] long threadId,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Tool: get_thread_messages invoked. ThreadId={ThreadId}", threadId);
        try
        {
            var result = await _restService.GetThreadMessagesAsync(threadId, cancellationToken);
            sw.Stop();
            _logger.LogInformation("Tool: get_thread_messages returned {Count} messages in {Ms}ms.", result.Count, sw.ElapsedMilliseconds);
            return result;
        }
        catch (VivaEngageServiceException ex)
        {
            _logger.LogError(ex, "Tool: get_thread_messages failed.");
            throw;
        }
    }

    [McpServerTool(Name = "search_viva_engage", ReadOnly = true)]
    [Description("Searches Viva Engage for messages, users, topics, and groups using the legacy REST API.")]
    public async Task<VivaEngageSearchResultDto> SearchVivaEngageAsync(
        [Description("The search query string.")] string query,
        [Description("Model types to search: MESSAGETHREAD, USER, GROUP, TOPIC. Default: all.")] string[]? modelTypes = null,
        [Description("Page number for pagination. Default: 1.")] int page = 1,
        [Description("Number of results per page (1-50). Default: 20.")] int numPerPage = 20,
        [Description("Optional group ID to scope the search to.")] long? searchGroup = null,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Tool: search_viva_engage invoked. Query={Query}", query);
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("query is required.", nameof(query));
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "page must be >= 1.");
        if (numPerPage < 1 || numPerPage > 50)
            throw new ArgumentOutOfRangeException(nameof(numPerPage), "numPerPage must be between 1 and 50.");

        try
        {
            var request = new VivaEngageSearchRequest
            {
                Query = query,
                ModelTypes = modelTypes ?? ["MESSAGETHREAD", "USER", "GROUP", "TOPIC"],
                Page = page,
                NumPerPage = numPerPage,
                SearchGroup = searchGroup
            };
            var result = await _restService.SearchAsync(request, cancellationToken);
            sw.Stop();
            _logger.LogInformation("Tool: search_viva_engage completed in {Ms}ms.", sw.ElapsedMilliseconds);
            return result;
        }
        catch (VivaEngageServiceException ex)
        {
            _logger.LogError(ex, "Tool: search_viva_engage failed.");
            throw;
        }
    }

    [McpServerTool(Name = "get_my_assigned_roles", ReadOnly = true)]
    [Description("Returns the current signed-in user's Viva Engage assigned roles using the Microsoft Graph API.")]
    public async Task<AssignedRolesDto> GetMyAssignedRolesAsync(
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Tool: get_my_assigned_roles invoked.");
        try
        {
            var result = await _graphService.GetMyAssignedRolesAsync(cancellationToken);
            sw.Stop();
            _logger.LogInformation("Tool: get_my_assigned_roles returned {Count} roles in {Ms}ms.", result.Items.Count, sw.ElapsedMilliseconds);
            return result;
        }
        catch (VivaEngageServiceException ex)
        {
            _logger.LogError(ex, "Tool: get_my_assigned_roles failed.");
            throw;
        }
    }

    [McpServerTool(Name = "list_groups_for_user", ReadOnly = true)]
    [Description("Lists the Viva Engage groups the specified user belongs to, from the legacy REST API.")]
    public async Task<PagedResult<VivaEngageGroupDto>> ListGroupsForUserAsync(
        [Description("The Viva Engage user ID.")] long userId,
        [Description("Page number for pagination. Default: 1.")] int page = 1,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Tool: list_groups_for_user invoked. UserId={UserId}", userId);
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "page must be >= 1.");

        try
        {
            var request = new ListGroupsForUserRequest { UserId = userId, Page = page };
            var result = await _restService.ListGroupsForUserAsync(request, cancellationToken);
            sw.Stop();
            _logger.LogInformation("Tool: list_groups_for_user returned {Count} groups in {Ms}ms.", result.Items.Count, sw.ElapsedMilliseconds);
            return result;
        }
        catch (VivaEngageServiceException ex)
        {
            _logger.LogError(ex, "Tool: list_groups_for_user failed.");
            throw;
        }
    }
}
