using VivaEngageMcp.Server.Mcp.Contracts;
using VivaEngageMcp.Server.Mcp.Mapping;
using VivaEngageMcp.Server.VivaEngageRest.Models;
using Microsoft.Graph.Models;

namespace VivaEngageMcp.Server.Tests;

public sealed class CommunityMapperTests
{
    [Fact]
    public void MapFromGraph_MapsAllFields()
    {
        var community = new Community
        {
            Id = "abc-123",
            DisplayName = "Engineering",
            Description = "All engineers",
            Privacy = CommunityPrivacy.Public,
            GroupId = "11111111-1111-1111-1111-111111111111"
        };

        var result = CommunityMapper.MapFromGraph(community);

        Assert.Equal("abc-123", result.Id);
        Assert.Equal("Engineering", result.DisplayName);
        Assert.Equal("All engineers", result.Description);
        Assert.Equal("public", result.Privacy);
        Assert.Equal("11111111-1111-1111-1111-111111111111", result.GroupId);
    }

    [Fact]
    public void MapFromGraph_NullOptionalFields_ReturnedAsNull()
    {
        var community = new Community
        {
            Id = "xyz",
            DisplayName = "Marketing",
            Privacy = CommunityPrivacy.Private
        };

        var result = CommunityMapper.MapFromGraph(community);

        Assert.Null(result.Description);
        Assert.Null(result.GroupId);
    }

    [Fact]
    public void MapFromGraph_NullIds_FallbackToEmptyString()
    {
        var community = new Community
        {
            Id = null,
            DisplayName = null,
            Privacy = CommunityPrivacy.UnknownFutureValue
        };

        var result = CommunityMapper.MapFromGraph(community);

        Assert.Equal(string.Empty, result.Id);
        Assert.Equal(string.Empty, result.DisplayName);
        Assert.Equal("unknownfuturevalue", result.Privacy);
    }
}
