using VivaEngageMcp.Server.Mcp.Contracts;

namespace VivaEngageMcp.Server.Tests;

public sealed class RequestModelTests
{
    [Fact]
    public void ListCommunitiesRequest_DefaultsAreCorrect()
    {
        var request = new ListCommunitiesRequest();
        Assert.Equal(25, request.Top);
        Assert.Null(request.Search);
        Assert.Null(request.NextLink);
    }

    [Fact]
    public void ListFeedMessagesRequest_DefaultsAreCorrect()
    {
        var request = new ListFeedMessagesRequest();
        Assert.Equal(FeedType.MyFeed, request.FeedType);
        Assert.Null(request.OlderThan);
        Assert.Null(request.NewerThan);
        Assert.True(request.Threaded);
        Assert.Equal(20, request.Limit);
    }

    [Fact]
    public void ListGroupMessagesRequest_DefaultsAreCorrect()
    {
        var request = new ListGroupMessagesRequest();
        Assert.Equal(0, request.GroupId);
        Assert.Null(request.OlderThan);
        Assert.Null(request.NewerThan);
        Assert.True(request.Threaded);
        Assert.Equal(20, request.Limit);
    }

    [Fact]
    public void VivaEngageSearchRequest_DefaultModelTypesIncludeAllTypes()
    {
        var request = new VivaEngageSearchRequest();
        Assert.Contains("MESSAGETHREAD", request.ModelTypes);
        Assert.Contains("USER", request.ModelTypes);
        Assert.Contains("GROUP", request.ModelTypes);
        Assert.Contains("TOPIC", request.ModelTypes);
    }

    [Fact]
    public void PagedResult_DefaultsToEmptyItems()
    {
        var result = new PagedResult<CommunityDto>();
        Assert.Empty(result.Items);
        Assert.Null(result.NextLink);
    }

    [Fact]
    public void AssignedRolesDto_DefaultsToEmptyItems()
    {
        var dto = new AssignedRolesDto();
        Assert.Empty(dto.Items);
    }

    [Fact]
    public void VivaEngageSearchResultDto_DefaultsToEmptyCollections()
    {
        var dto = new VivaEngageSearchResultDto();
        Assert.Empty(dto.Messages);
        Assert.Empty(dto.Users);
        Assert.Empty(dto.Groups);
        Assert.Empty(dto.Topics);
    }
}
