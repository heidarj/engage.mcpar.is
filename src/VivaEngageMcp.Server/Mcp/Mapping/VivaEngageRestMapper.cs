using VivaEngageMcp.Server.Mcp.Contracts;
using VivaEngageMcp.Server.VivaEngageRest.Models;

namespace VivaEngageMcp.Server.Mcp.Mapping;

internal static class VivaEngageRestMapper
{
    public static VivaEngageCurrentUserDto MapCurrentUser(YammerUserResponse r) => new()
    {
        Id = r.Id,
        Name = r.Name ?? string.Empty,
        FullName = r.FullName,
        JobTitle = r.JobTitle,
        Email = r.Email,
        NetworkId = r.NetworkId,
        WebUrl = r.WebUrl
    };

    public static VivaEngageGroupDto MapGroup(YammerGroupResponse r) => new()
    {
        Id = r.Id,
        Name = r.Name ?? string.Empty,
        Description = r.Description,
        Privacy = r.Privacy,
        WebUrl = r.WebUrl,
        MemberCount = r.Stats?.Members
    };

    public static VivaEngageMessageDto MapMessage(
        YammerMessageResponse m,
        IReadOnlyDictionary<long, YammerReference>? senderIndex = null,
        IReadOnlyDictionary<long, string>? groupNameIndex = null)
    {
        string? senderName = null;
        string? senderEmail = null;
        if (senderIndex is not null && m.SenderId.HasValue && senderIndex.TryGetValue(m.SenderId.Value, out var sender))
        {
            senderName = sender.FullName ?? sender.Name;
            senderEmail = sender.Email;
        }

        string? groupName = null;
        if (groupNameIndex is not null && m.GroupId.HasValue)
        {
            groupNameIndex.TryGetValue(m.GroupId.Value, out groupName);
        }

        return new VivaEngageMessageDto
        {
            Id = m.Id,
            ThreadId = m.ThreadId,
            GroupId = m.GroupId,
            GroupName = groupName,
            Body = m.Body?.Plain,
            ContentType = "plain",
            CreatedAt = ParseYammerDate(m.CreatedAt),
            SenderId = m.SenderId,
            SenderName = senderName,
            SenderEmail = senderEmail,
            Privacy = m.Privacy,
            WebUrl = m.WebUrl,
            LikedCount = m.LikedBy?.Count
        };
    }

    public static VivaEngageMessageThreadSummaryDto MapThreadSummary(
        YammerMessageResponse m,
        Dictionary<string, YammerThreadedExtended>? threadedExtended,
        IReadOnlyDictionary<long, YammerReference>? senderIndex,
        IReadOnlyDictionary<long, string>? groupNameIndex)
    {
        int? count = null;
        if (threadedExtended is not null && m.ThreadId.HasValue &&
            threadedExtended.TryGetValue(m.ThreadId.Value.ToString(), out var ext))
        {
            count = ext.Total;
        }
        return new VivaEngageMessageThreadSummaryDto
        {
            ThreadId = m.ThreadId ?? m.Id,
            LatestMessage = MapMessage(m, senderIndex, groupNameIndex),
            MessageCount = count
        };
    }

    public static VivaEngageSearchUserDto MapSearchUser(YammerUserResponse u) => new()
    {
        Id = u.Id,
        Name = u.Name ?? string.Empty,
        FullName = u.FullName,
        Email = u.Email,
        WebUrl = u.WebUrl
    };

    public static VivaEngageTopicDto MapTopic(YammerTopicResponse t) => new()
    {
        Id = t.Id,
        Name = t.Name ?? string.Empty,
        WebUrl = t.WebUrl
    };

    private static DateTimeOffset? ParseYammerDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTimeOffset.TryParse(value, out var result)) return result;
        return null;
    }
}
