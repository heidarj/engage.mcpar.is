using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using VivaEngageMcp.Server.Mcp.Contracts;
using VivaEngageMcp.Server.Mcp.Mapping;

namespace VivaEngageMcp.Server.Graph;

/// <summary>
/// Implements Viva Engage Graph operations using the Microsoft Graph SDK.
/// Uses a per-request GraphServiceClient injected via DI (configured with OBO).
/// </summary>
public sealed class VivaEngageGraphService : IVivaEngageGraphService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<VivaEngageGraphService> _logger;

    public VivaEngageGraphService(
        GraphServiceClient graphClient,
        ILogger<VivaEngageGraphService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    public async Task<PagedResult<CommunityDto>> ListCommunitiesAsync(
        ListCommunitiesRequest request,
        CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Graph: listing communities. Top={Top} Search={Search} HasNextLink={HasNextLink}",
            request.Top, request.Search, request.NextLink is not null);
        try
        {
            Microsoft.Graph.Models.CommunityCollectionResponse? response;

            if (!string.IsNullOrEmpty(request.NextLink))
            {
                // Use the OData next link directly via WithUrl
                response = await _graphClient.EmployeeExperience.Communities
                    .WithUrl(request.NextLink)
                    .GetAsync(cancellationToken: cancellationToken);
            }
            else
            {
                response = await _graphClient.EmployeeExperience.Communities.GetAsync(cfg =>
                {
                    cfg.QueryParameters.Top = request.Top;
                    cfg.QueryParameters.Select = ["id", "displayName", "description", "privacy", "groupId"];
                    if (!string.IsNullOrEmpty(request.Search))
                    {
                        cfg.QueryParameters.Filter = $"contains(displayName, '{EscapeOdataString(request.Search)}')";
                    }
                }, cancellationToken);
            }

            var items = response?.Value?.Select(CommunityMapper.MapFromGraph).ToList()
                ?? [];
            sw.Stop();
            _logger.LogInformation("Graph: listed {Count} communities in {Ms}ms.", items.Count, sw.ElapsedMilliseconds);
            return new PagedResult<CommunityDto> { Items = items, NextLink = response?.OdataNextLink };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Graph: failed to list communities.");
            throw VivaEngageServiceException.FromGraphException(ex);
        }
    }

    public async Task<CommunityDto> GetCommunityAsync(
        string communityId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(communityId);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Graph: getting community {CommunityId}.", communityId);
        try
        {
            var community = await _graphClient.EmployeeExperience.Communities[communityId].GetAsync(cfg =>
            {
                cfg.QueryParameters.Select = ["id", "displayName", "description", "privacy", "groupId"];
            }, cancellationToken);

            if (community is null)
            {
                throw new VivaEngageServiceException($"Community '{communityId}' not found.", VivaEngageServiceErrorKind.NotFound);
            }
            sw.Stop();
            _logger.LogInformation("Graph: got community {CommunityId} in {Ms}ms.", communityId, sw.ElapsedMilliseconds);
            return CommunityMapper.MapFromGraph(community);
        }
        catch (VivaEngageServiceException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Graph: failed to get community {CommunityId}.", communityId);
            throw VivaEngageServiceException.FromGraphException(ex);
        }
    }

    public async Task<AssignedRolesDto> GetMyAssignedRolesAsync(CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Graph: getting assigned roles for current user.");
        try
        {
            // /me/employeeExperience/assignedRoles is not yet in the Graph SDK (as of 5.79.0).
            // We call it via a raw request through the request adapter.
            const string assignedRolesUrl = "https://graph.microsoft.com/v1.0/me/employeeExperience/assignedRoles";
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri(assignedRolesUrl)
            };
            requestInfo.Headers.Add("Accept", "application/json");

            var response = await _graphClient.RequestAdapter.SendAsync(
                requestInfo,
                RawAssignedRolesResponse.CreateFromDiscriminatorValue,
                cancellationToken: cancellationToken);

            var items = response?.Value?
                .Select(r => new AssignedRoleItemDto
                {
                    Id = r.Id ?? string.Empty,
                    DisplayName = r.DisplayName ?? string.Empty
                })
                .ToList() ?? [];
            sw.Stop();
            _logger.LogInformation("Graph: got {Count} assigned roles in {Ms}ms.", items.Count, sw.ElapsedMilliseconds);
            return new AssignedRolesDto { Items = items };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Graph: failed to get assigned roles.");
            throw VivaEngageServiceException.FromGraphException(ex);
        }
    }

    private static string EscapeOdataString(string value) =>
        value.Replace("'", "''", StringComparison.Ordinal);
}

/// <summary>
/// Minimal Kiota-compatible response model for /me/employeeExperience/assignedRoles.
/// Required because the Microsoft Graph SDK 5.79.0 does not yet have built-in support
/// for this endpoint.
/// </summary>
internal sealed class RawAssignedRolesResponse : IParsable
{
    public List<RawEngagementRoleItem>? Value { get; private set; }

    public static RawAssignedRolesResponse CreateFromDiscriminatorValue(IParseNode parseNode) =>
        new();

    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => new Dictionary<string, Action<IParseNode>>
    {
        ["value"] = n => Value = n.GetCollectionOfObjectValues<RawEngagementRoleItem>(
            RawEngagementRoleItem.CreateFromDiscriminatorValue)?.ToList()
    };

    public void Serialize(ISerializationWriter writer)
    {
        writer.WriteCollectionOfObjectValues("value", Value);
    }
}

internal sealed class RawEngagementRoleItem : IParsable
{
    public string? Id { get; private set; }
    public string? DisplayName { get; private set; }

    public static RawEngagementRoleItem CreateFromDiscriminatorValue(IParseNode parseNode) =>
        new();

    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => new Dictionary<string, Action<IParseNode>>
    {
        ["id"] = n => Id = n.GetStringValue(),
        ["displayName"] = n => DisplayName = n.GetStringValue()
    };

    public void Serialize(ISerializationWriter writer)
    {
        writer.WriteStringValue("id", Id);
        writer.WriteStringValue("displayName", DisplayName);
    }
}
