using System.ComponentModel.DataAnnotations;

namespace VivaEngageMcp.Server.Configuration;

public sealed class VivaEngageOptions
{
    public const string SectionName = "VivaEngage";

    [Required]
    public string BaseUrl { get; set; } = "https://www.yammer.com/api/v1";

    public bool UseEntraTokens { get; set; } = true;

    public string DelegatedPermissionName { get; set; } = "access_as_user";

    [Required]
    public string RequestedScopeOrResource { get; set; } = "CONFIGURE_ME";
}
