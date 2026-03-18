using System.ComponentModel.DataAnnotations;

namespace VivaEngageMcp.Server.Configuration;

public sealed class EntraOptions
{
    public const string SectionName = "Entra";

    [Required]
    public string Instance { get; set; } = "https://login.microsoftonline.com/";

    [Required]
    public string TenantId { get; set; } = "common";

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    public string ApiScope { get; set; } = "Mcp.Read";
}
