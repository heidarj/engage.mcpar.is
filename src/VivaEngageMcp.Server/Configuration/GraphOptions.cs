using System.ComponentModel.DataAnnotations;

namespace VivaEngageMcp.Server.Configuration;

public sealed class GraphOptions
{
    public const string SectionName = "Graph";

    [Required]
    public string BaseUrl { get; set; } = "https://graph.microsoft.com/v1.0";

    [Required]
    public string[] Scopes { get; set; } =
    [
        "https://graph.microsoft.com/Community.Read.All",
        "https://graph.microsoft.com/EngagementRole.Read",
        "https://graph.microsoft.com/User.Read"
    ];
}
