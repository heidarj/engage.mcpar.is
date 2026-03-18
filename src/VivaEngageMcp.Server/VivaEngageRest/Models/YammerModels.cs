using System.Text.Json.Serialization;

namespace VivaEngageMcp.Server.VivaEngageRest.Models;

/// <summary>Raw Yammer REST API response models (internal, not exposed externally).</summary>

internal sealed class YammerUserResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("job_title")]
    public string? JobTitle { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("network_id")]
    public long NetworkId { get; set; }

    [JsonPropertyName("web_url")]
    public string? WebUrl { get; set; }
}

internal sealed class YammerGroupResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("privacy")]
    public string? Privacy { get; set; }

    [JsonPropertyName("web_url")]
    public string? WebUrl { get; set; }

    [JsonPropertyName("stats")]
    public YammerGroupStats? Stats { get; set; }
}

internal sealed class YammerGroupStats
{
    [JsonPropertyName("members")]
    public int? Members { get; set; }
}

internal sealed class YammerMessagesEnvelope
{
    [JsonPropertyName("messages")]
    public List<YammerMessageResponse>? Messages { get; set; }

    [JsonPropertyName("threaded_extended")]
    public Dictionary<string, YammerThreadedExtended>? ThreadedExtended { get; set; }

    [JsonPropertyName("meta")]
    public YammerMessagesMeta? Meta { get; set; }

    [JsonPropertyName("references")]
    public List<YammerReference>? References { get; set; }
}

internal sealed class YammerMessagesMeta
{
    [JsonPropertyName("older_available")]
    public bool OlderAvailable { get; set; }

    [JsonPropertyName("newest_message_id")]
    public long? NewestMessageId { get; set; }

    [JsonPropertyName("oldest_message_id")]
    public long? OldestMessageId { get; set; }
}

internal sealed class YammerThreadedExtended
{
    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("latest_reply_ids")]
    public List<long>? LatestReplyIds { get; set; }
}

internal sealed class YammerMessageResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("thread_id")]
    public long? ThreadId { get; set; }

    [JsonPropertyName("group_id")]
    public long? GroupId { get; set; }

    [JsonPropertyName("sender_id")]
    public long? SenderId { get; set; }

    [JsonPropertyName("body")]
    public YammerMessageBody? Body { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("privacy")]
    public string? Privacy { get; set; }

    [JsonPropertyName("web_url")]
    public string? WebUrl { get; set; }

    [JsonPropertyName("liked_by")]
    public YammerLikedBy? LikedBy { get; set; }

    [JsonPropertyName("replied_to_id")]
    public long? RepliedToId { get; set; }
}

internal sealed class YammerMessageBody
{
    [JsonPropertyName("plain")]
    public string? Plain { get; set; }

    [JsonPropertyName("rich")]
    public string? Rich { get; set; }

    [JsonPropertyName("parsed")]
    public string? Parsed { get; set; }
}

internal sealed class YammerLikedBy
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

internal sealed class YammerReference
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("web_url")]
    public string? WebUrl { get; set; }
}

internal sealed class YammerSearchResponse
{
    [JsonPropertyName("messages")]
    public YammerSearchMessagesSection? Messages { get; set; }

    [JsonPropertyName("users")]
    public YammerSearchUsersSection? Users { get; set; }

    [JsonPropertyName("groups")]
    public YammerSearchGroupsSection? Groups { get; set; }

    [JsonPropertyName("topics")]
    public YammerSearchTopicsSection? Topics { get; set; }
}

internal sealed class YammerSearchMessagesSection
{
    [JsonPropertyName("messages")]
    public List<YammerMessageResponse>? Messages { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

internal sealed class YammerSearchUsersSection
{
    [JsonPropertyName("users")]
    public List<YammerUserResponse>? Users { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

internal sealed class YammerSearchGroupsSection
{
    [JsonPropertyName("groups")]
    public List<YammerGroupResponse>? Groups { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

internal sealed class YammerSearchTopicsSection
{
    [JsonPropertyName("topics")]
    public List<YammerTopicResponse>? Topics { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

internal sealed class YammerTopicResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("web_url")]
    public string? WebUrl { get; set; }
}
