using VivaEngageMcp.Server.Mcp.Contracts;
using VivaEngageMcp.Server.Mcp.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VivaEngageMcp.Server.Graph;
using VivaEngageMcp.Server.VivaEngageRest;

namespace VivaEngageMcp.Server.Tests;

public sealed class PaginationTests
{
    private readonly Mock<IVivaEngageGraphService> _graphService = new();
    private readonly Mock<IVivaEngageRestService> _restService = new();

    private VivaEngageTools CreateSut() => new(
        _graphService.Object,
        _restService.Object,
        NullLogger<VivaEngageTools>.Instance);

    [Fact]
    public async Task ListCommunities_PassesNextLinkToGraphService()
    {
        const string expectedNextLink = "https://graph.microsoft.com/v1.0/employeeExperience/communities?$skiptoken=abc";

        _graphService.Setup(s => s.ListCommunitiesAsync(
                It.Is<ListCommunitiesRequest>(r => r.NextLink == expectedNextLink),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<CommunityDto>
            {
                Items = [new CommunityDto { Id = "1", DisplayName = "Test" }],
                NextLink = null
            });

        var sut = CreateSut();
        var result = await sut.ListCommunitiesAsync(nextLink: expectedNextLink);

        Assert.Single(result.Items);
        Assert.Null(result.NextLink);
    }

    [Fact]
    public async Task ListCommunities_PropagatesNextLinkFromGraphService()
    {
        const string returnedNextLink = "https://graph.microsoft.com/v1.0/employeeExperience/communities?$skiptoken=xyz";

        _graphService.Setup(s => s.ListCommunitiesAsync(It.IsAny<ListCommunitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<CommunityDto>
            {
                Items = [new CommunityDto { Id = "1", DisplayName = "Test" }],
                NextLink = returnedNextLink
            });

        var sut = CreateSut();
        var result = await sut.ListCommunitiesAsync();

        Assert.Equal(returnedNextLink, result.NextLink);
    }

    [Fact]
    public async Task ListFeedMessages_PassesPaginationParameters()
    {
        _restService.Setup(s => s.ListFeedMessagesAsync(
                It.Is<ListFeedMessagesRequest>(r =>
                    r.OlderThan == 9999 &&
                    r.NewerThan == 1000 &&
                    r.Limit == 10),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<VivaEngageMessageThreadSummaryDto> { Items = [] });

        var sut = CreateSut();
        await sut.ListFeedMessagesAsync(olderThan: 9999, newerThan: 1000, limit: 10);

        _restService.Verify(s => s.ListFeedMessagesAsync(
            It.Is<ListFeedMessagesRequest>(r =>
                r.OlderThan == 9999 &&
                r.NewerThan == 1000 &&
                r.Limit == 10),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListGroupMessages_PassesPaginationParameters()
    {
        _restService.Setup(s => s.ListGroupMessagesAsync(
                It.Is<ListGroupMessagesRequest>(r =>
                    r.GroupId == 123 &&
                    r.OlderThan == 5000 &&
                    r.Limit == 15),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<VivaEngageMessageDto> { Items = [] });

        var sut = CreateSut();
        await sut.ListGroupMessagesAsync(groupId: 123, olderThan: 5000, limit: 15);

        _restService.Verify(s => s.ListGroupMessagesAsync(
            It.Is<ListGroupMessagesRequest>(r =>
                r.GroupId == 123 &&
                r.OlderThan == 5000 &&
                r.Limit == 15),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
