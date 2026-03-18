using VivaEngageMcp.Server.Mcp.Contracts;
using VivaEngageMcp.Server.Mcp.Mapping;
using VivaEngageMcp.Server.VivaEngageRest.Models;

namespace VivaEngageMcp.Server.Tests;

public sealed class DtoMappingTests
{
    [Fact]
    public void MapCurrentUser_MapsAllFields()
    {
        var source = new YammerUserResponse
        {
            Id = 12345,
            Name = "jdoe",
            FullName = "Jane Doe",
            JobTitle = "Engineer",
            Email = "jdoe@example.com",
            NetworkId = 99,
            WebUrl = "https://www.yammer.com/example.com/users/12345"
        };

        var result = VivaEngageRestMapper.MapCurrentUser(source);

        Assert.Equal(12345, result.Id);
        Assert.Equal("jdoe", result.Name);
        Assert.Equal("Jane Doe", result.FullName);
        Assert.Equal("Engineer", result.JobTitle);
        Assert.Equal("jdoe@example.com", result.Email);
        Assert.Equal(99, result.NetworkId);
        Assert.Equal("https://www.yammer.com/example.com/users/12345", result.WebUrl);
    }

    [Fact]
    public void MapCurrentUser_HandlesNullOptionalFields()
    {
        var source = new YammerUserResponse { Id = 1, Name = "user", NetworkId = 1 };
        var result = VivaEngageRestMapper.MapCurrentUser(source);

        Assert.Null(result.FullName);
        Assert.Null(result.JobTitle);
        Assert.Null(result.Email);
        Assert.Null(result.WebUrl);
    }

    [Fact]
    public void MapGroup_MapsAllFields()
    {
        var source = new YammerGroupResponse
        {
            Id = 42,
            Name = "eng",
            Description = "Engineering group",
            Privacy = "public",
            WebUrl = "https://www.yammer.com/example.com/groups/eng",
            Stats = new YammerGroupStats { Members = 150 }
        };

        var result = VivaEngageRestMapper.MapGroup(source);

        Assert.Equal(42, result.Id);
        Assert.Equal("eng", result.Name);
        Assert.Equal("Engineering group", result.Description);
        Assert.Equal("public", result.Privacy);
        Assert.Equal("https://www.yammer.com/example.com/groups/eng", result.WebUrl);
        Assert.Equal(150, result.MemberCount);
    }

    [Fact]
    public void MapMessage_MapsBodyAndDates()
    {
        var source = new YammerMessageResponse
        {
            Id = 100,
            ThreadId = 100,
            GroupId = 42,
            SenderId = 12345,
            Body = new YammerMessageBody { Plain = "Hello world" },
            CreatedAt = "2024-01-15T10:30:00+00:00",
            Privacy = "public",
            WebUrl = "https://www.yammer.com/example.com/messages/100",
            LikedBy = new YammerLikedBy { Count = 5 }
        };

        var result = VivaEngageRestMapper.MapMessage(source);

        Assert.Equal(100, result.Id);
        Assert.Equal(100, result.ThreadId);
        Assert.Equal(42, result.GroupId);
        Assert.Equal(12345, result.SenderId);
        Assert.Equal("Hello world", result.Body);
        Assert.Equal("2024-01-15T10:30:00+00:00", result.CreatedAt?.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        Assert.Equal("public", result.Privacy);
        Assert.Equal("https://www.yammer.com/example.com/messages/100", result.WebUrl);
        Assert.Equal(5, result.LikedCount);
    }

    [Fact]
    public void MapMessage_WithSenderIndex_ResolvesSenderInfo()
    {
        var source = new YammerMessageResponse { Id = 1, SenderId = 999 };
        var senders = new Dictionary<long, YammerReference>
        {
            [999] = new YammerReference
            {
                Id = 999,
                Type = "user",
                Name = "jsmith",
                FullName = "John Smith",
                Email = "jsmith@example.com"
            }
        };

        var result = VivaEngageRestMapper.MapMessage(source, senders);

        Assert.Equal("John Smith", result.SenderName);
        Assert.Equal("jsmith@example.com", result.SenderEmail);
    }

    [Fact]
    public void MapMessage_InvalidDateString_ReturnsNullDate()
    {
        var source = new YammerMessageResponse { Id = 1, CreatedAt = "not-a-date" };
        var result = VivaEngageRestMapper.MapMessage(source);
        Assert.Null(result.CreatedAt);
    }

    [Fact]
    public void MapThreadSummary_IncludesThreadCount()
    {
        var source = new YammerMessageResponse { Id = 10, ThreadId = 10 };
        var threadedExtended = new Dictionary<string, YammerThreadedExtended>
        {
            ["10"] = new YammerThreadedExtended { Total = 7 }
        };

        var result = VivaEngageRestMapper.MapThreadSummary(source, threadedExtended, null, null);

        Assert.Equal(10, result.ThreadId);
        Assert.Equal(7, result.MessageCount);
    }

    [Fact]
    public void MapSearchUser_MapsAllFields()
    {
        var source = new YammerUserResponse
        {
            Id = 555,
            Name = "alice",
            FullName = "Alice Wonder",
            Email = "alice@example.com",
            WebUrl = "https://www.yammer.com/users/555"
        };

        var result = VivaEngageRestMapper.MapSearchUser(source);

        Assert.Equal(555, result.Id);
        Assert.Equal("alice", result.Name);
        Assert.Equal("Alice Wonder", result.FullName);
        Assert.Equal("alice@example.com", result.Email);
        Assert.Equal("https://www.yammer.com/users/555", result.WebUrl);
    }

    [Fact]
    public void MapTopic_MapsAllFields()
    {
        var source = new YammerTopicResponse { Id = 77, Name = "innovation", WebUrl = "https://www.yammer.com/topics/77" };
        var result = VivaEngageRestMapper.MapTopic(source);

        Assert.Equal(77, result.Id);
        Assert.Equal("innovation", result.Name);
        Assert.Equal("https://www.yammer.com/topics/77", result.WebUrl);
    }
}
