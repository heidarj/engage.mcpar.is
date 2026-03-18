using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VivaEngageMcp.Server.Auth;
using VivaEngageMcp.Server.Graph;
using VivaEngageMcp.Server.Mcp.Contracts;
using VivaEngageMcp.Server.VivaEngageRest;

namespace VivaEngageMcp.Server.Tests;

public sealed class ToolInputValidationTests
{
    private readonly Mock<IVivaEngageGraphService> _graphService = new();
    private readonly Mock<IVivaEngageRestService> _restService = new();

    private Mcp.Tools.VivaEngageTools CreateSut() => new(
        _graphService.Object,
        _restService.Object,
        NullLogger<Mcp.Tools.VivaEngageTools>.Instance);

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task ListCommunities_InvalidTop_ThrowsArgumentOutOfRangeException(int top)
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.ListCommunitiesAsync(top: top));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(100)]
    public async Task ListCommunities_ValidTop_CallsGraphService(int top)
    {
        _graphService.Setup(s => s.ListCommunitiesAsync(
                It.Is<ListCommunitiesRequest>(r => r.Top == top),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<CommunityDto> { Items = [] });

        var sut = CreateSut();
        var result = await sut.ListCommunitiesAsync(top: top);

        Assert.NotNull(result);
        _graphService.Verify(s => s.ListCommunitiesAsync(
            It.Is<ListCommunitiesRequest>(r => r.Top == top),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCommunity_NullOrEmptyId_ThrowsArgumentException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.GetCommunityAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.GetCommunityAsync("   "));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task ListFeedMessages_InvalidLimit_ThrowsArgumentOutOfRangeException(int limit)
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.ListFeedMessagesAsync(limit: limit));
    }

    [Fact]
    public async Task ListFeedMessages_InvalidFeedType_ThrowsArgumentException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.ListFeedMessagesAsync(feedType: "invalid_type"));
    }

    [Theory]
    [InlineData("my_feed")]
    [InlineData("network")]
    [InlineData("following")]
    [InlineData("sent")]
    [InlineData("received")]
    [InlineData("private")]
    public async Task ListFeedMessages_ValidFeedTypes_AreAccepted(string feedType)
    {
        _restService.Setup(s => s.ListFeedMessagesAsync(
                It.IsAny<ListFeedMessagesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<VivaEngageMessageThreadSummaryDto> { Items = [] });

        var sut = CreateSut();
        var result = await sut.ListFeedMessagesAsync(feedType: feedType);

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task ListGroupMessages_InvalidLimit_ThrowsArgumentOutOfRangeException(int limit)
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.ListGroupMessagesAsync(groupId: 1, limit: limit));
    }

    [Fact]
    public async Task SearchVivaEngage_EmptyQuery_ThrowsArgumentException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.SearchVivaEngageAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.SearchVivaEngageAsync("   "));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task SearchVivaEngage_InvalidNumPerPage_ThrowsArgumentOutOfRangeException(int numPerPage)
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.SearchVivaEngageAsync("test", numPerPage: numPerPage));
    }

    [Fact]
    public async Task SearchVivaEngage_InvalidPage_ThrowsArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.SearchVivaEngageAsync("test", page: 0));
    }
}
