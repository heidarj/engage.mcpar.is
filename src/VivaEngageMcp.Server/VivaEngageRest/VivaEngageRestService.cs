using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VivaEngageMcp.Server.Auth;
using VivaEngageMcp.Server.Configuration;
using VivaEngageMcp.Server.Graph;
using VivaEngageMcp.Server.Mcp.Contracts;
using VivaEngageMcp.Server.Mcp.Mapping;
using VivaEngageMcp.Server.VivaEngageRest.Models;

namespace VivaEngageMcp.Server.VivaEngageRest;

public sealed class VivaEngageRestService : IVivaEngageRestService
{
    private readonly HttpClient _httpClient;
    private readonly IVivaEngageTokenService _tokenService;
    private readonly VivaEngageOptions _options;
    private readonly ILogger<VivaEngageRestService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public VivaEngageRestService(
        HttpClient httpClient,
        IVivaEngageTokenService tokenService,
        IOptions<VivaEngageOptions> options,
        ILogger<VivaEngageRestService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _options = options.Value;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<VivaEngageCurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("VivaEngageRest: getting current user.");
        var response = await GetJsonAsync<YammerUserResponse>(
            "users/current.json",
            cancellationToken);
        sw.Stop();
        _logger.LogInformation("VivaEngageRest: got current user in {Ms}ms.", sw.ElapsedMilliseconds);
        return VivaEngageRestMapper.MapCurrentUser(response);
    }

    public async Task<PagedResult<VivaEngageGroupDto>> ListGroupsForUserAsync(
        ListGroupsForUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("VivaEngageRest: listing groups for user {UserId}.", request.UserId);
        var url = $"groups/for_user/{request.UserId}.json?page={request.Page}";
        var groups = await GetJsonAsync<List<YammerGroupResponse>>(url, cancellationToken);
        sw.Stop();
        _logger.LogInformation("VivaEngageRest: listed {Count} groups in {Ms}ms.", groups?.Count ?? 0, sw.ElapsedMilliseconds);
        var items = groups?.Select(VivaEngageRestMapper.MapGroup).ToList() ?? [];
        return new PagedResult<VivaEngageGroupDto> { Items = items };
    }

    public async Task<PagedResult<VivaEngageMessageThreadSummaryDto>> ListFeedMessagesAsync(
        ListFeedMessagesRequest request,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("VivaEngageRest: listing feed messages. FeedType={FeedType}", request.FeedType);

        var feedPath = request.FeedType switch
        {
            FeedType.MyFeed => "messages/my_feed.json",
            FeedType.Network => "messages.json",
            FeedType.Following => "messages/following.json",
            FeedType.Sent => "messages/sent.json",
            FeedType.Received => "messages/received.json",
            FeedType.Private => "messages/private.json",
            _ => "messages/my_feed.json"
        };

        var qs = BuildMessageQueryString(request.OlderThan, request.NewerThan, request.Threaded, request.Limit);
        var envelope = await GetJsonAsync<YammerMessagesEnvelope>($"{feedPath}{qs}", cancellationToken);
        sw.Stop();

        var messages = envelope?.Messages ?? [];
        var refs = BuildReferenceIndex(envelope?.References);
        _logger.LogInformation("VivaEngageRest: listed {Count} feed messages in {Ms}ms.", messages.Count, sw.ElapsedMilliseconds);

        var items = messages
            .Select(m => VivaEngageRestMapper.MapThreadSummary(m, envelope?.ThreadedExtended, refs.senders, refs.groups))
            .ToList();

        return new PagedResult<VivaEngageMessageThreadSummaryDto>
        {
            Items = items,
            NextLink = BuildOlderThanLink(envelope)
        };
    }

    public async Task<PagedResult<VivaEngageMessageDto>> ListGroupMessagesAsync(
        ListGroupMessagesRequest request,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("VivaEngageRest: listing group messages for group {GroupId}.", request.GroupId);

        var qs = BuildMessageQueryString(request.OlderThan, request.NewerThan, request.Threaded, request.Limit);
        var envelope = await GetJsonAsync<YammerMessagesEnvelope>(
            $"messages/in_group/{request.GroupId}.json{qs}",
            cancellationToken);
        sw.Stop();

        var messages = envelope?.Messages ?? [];
        var refs = BuildReferenceIndex(envelope?.References);
        _logger.LogInformation("VivaEngageRest: listed {Count} group messages in {Ms}ms.", messages.Count, sw.ElapsedMilliseconds);

        var items = messages
            .Select(m => VivaEngageRestMapper.MapMessage(m, refs.senders, refs.groups))
            .ToList();

        return new PagedResult<VivaEngageMessageDto>
        {
            Items = items,
            NextLink = BuildOlderThanLink(envelope)
        };
    }

    public async Task<IReadOnlyList<VivaEngageMessageDto>> GetThreadMessagesAsync(
        long threadId,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("VivaEngageRest: getting thread messages for thread {ThreadId}.", threadId);

        var envelope = await GetJsonAsync<YammerMessagesEnvelope>(
            $"messages/in_thread/{threadId}.json",
            cancellationToken);
        sw.Stop();

        var messages = envelope?.Messages ?? [];
        var refs = BuildReferenceIndex(envelope?.References);
        _logger.LogInformation("VivaEngageRest: got {Count} thread messages in {Ms}ms.", messages.Count, sw.ElapsedMilliseconds);

        return messages
            .Select(m => VivaEngageRestMapper.MapMessage(m, refs.senders, refs.groups))
            .ToList();
    }

    public async Task<VivaEngageSearchResultDto> SearchAsync(
        VivaEngageSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("VivaEngageRest: searching. Query={Query}", request.Query);

        var qs = new System.Text.StringBuilder("search.json?");
        qs.Append($"search={Uri.EscapeDataString(request.Query)}");
        qs.Append($"&page={request.Page}");
        qs.Append($"&num_per_page={request.NumPerPage}");
        if (request.ModelTypes.Length > 0)
        {
            qs.Append($"&model_types={Uri.EscapeDataString(string.Join(",", request.ModelTypes))}");
        }
        if (request.SearchGroup.HasValue)
        {
            qs.Append($"&search_network_id={request.SearchGroup.Value}");
        }

        var result = await GetJsonAsync<YammerSearchResponse>(qs.ToString(), cancellationToken);
        sw.Stop();
        _logger.LogInformation("VivaEngageRest: search completed in {Ms}ms.", sw.ElapsedMilliseconds);

        return new VivaEngageSearchResultDto
        {
            Messages = result?.Messages?.Messages?
                .Select(m => VivaEngageRestMapper.MapThreadSummary(m, null, null, null))
                .ToList() ?? [],
            Users = result?.Users?.Users?
                .Select(VivaEngageRestMapper.MapSearchUser)
                .ToList() ?? [],
            Groups = result?.Groups?.Groups?
                .Select(VivaEngageRestMapper.MapGroup)
                .ToList() ?? [],
            Topics = result?.Topics?.Topics?
                .Select(VivaEngageRestMapper.MapTopic)
                .ToList() ?? [],
            TotalMessages = result?.Messages?.Count,
            TotalUsers = result?.Users?.Count,
            TotalGroups = result?.Groups?.Count,
            TotalTopics = result?.Topics?.Count
        };
    }

    private async Task<T> GetJsonAsync<T>(string relativeUrl, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("No HTTP context user available.");

        var token = await _tokenService.GetVivaEngageAccessTokenForUserAsync(user, cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "VivaEngageRest: HTTP request failed for {Url}.", relativeUrl);
            throw VivaEngageServiceException.FromRestException(ex);
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("VivaEngageRest: throttled (429) on {Url}.", relativeUrl);
            throw new VivaEngageServiceException(
                "Viva Engage REST API throttled the request (429). Please retry later.",
                VivaEngageServiceErrorKind.Throttled);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("VivaEngageRest: non-success {Status} for {Url}.", response.StatusCode, relativeUrl);
            var kind = (int)response.StatusCode switch
            {
                401 => VivaEngageServiceErrorKind.Unauthorized,
                403 => VivaEngageServiceErrorKind.Forbidden,
                404 => VivaEngageServiceErrorKind.NotFound,
                >= 500 => VivaEngageServiceErrorKind.UpstreamFailure,
                _ => VivaEngageServiceErrorKind.Unknown
            };
            throw new VivaEngageServiceException(
                $"Viva Engage REST API returned {(int)response.StatusCode} for {relativeUrl}.",
                kind);
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
        if (result is null)
        {
            throw new VivaEngageServiceException(
                $"Viva Engage REST API returned empty or null response for {relativeUrl}.",
                VivaEngageServiceErrorKind.UpstreamFailure);
        }
        return result;
    }

    private static string BuildMessageQueryString(long? olderThan, long? newerThan, bool threaded, int limit)
    {
        var parts = new List<string>();
        if (olderThan.HasValue) parts.Add($"older_than={olderThan.Value}");
        if (newerThan.HasValue) parts.Add($"newer_than={newerThan.Value}");
        if (threaded) parts.Add("threaded=true");
        parts.Add($"limit={limit}");
        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    private static (IReadOnlyDictionary<long, YammerReference> senders, IReadOnlyDictionary<long, string> groups)
        BuildReferenceIndex(List<YammerReference>? refs)
    {
        if (refs is null or { Count: 0 })
            return (new Dictionary<long, YammerReference>(), new Dictionary<long, string>());

        var senders = refs
            .Where(r => string.Equals(r.Type, "user", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(r => r.Id);

        var groups = refs
            .Where(r => string.Equals(r.Type, "group", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(r => r.Id, r => r.Name ?? string.Empty);

        return (senders, groups);
    }

    private static string? BuildOlderThanLink(YammerMessagesEnvelope? envelope)
    {
        if (envelope?.Meta?.OlderAvailable is true && envelope.Meta.OldestMessageId.HasValue)
        {
            return $"older_than:{envelope.Meta.OldestMessageId.Value}";
        }
        return null;
    }
}
