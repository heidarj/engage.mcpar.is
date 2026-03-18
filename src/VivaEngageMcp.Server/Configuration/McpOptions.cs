using System.ComponentModel.DataAnnotations;

namespace VivaEngageMcp.Server.Configuration;

public sealed class McpOptions
{
    public const string SectionName = "Mcp";

    [Required]
    public string EndpointPath { get; set; } = "/mcp";

    [Required]
    public string ServerName { get; set; } = "viva-engage-mcp";

    [Required]
    public string ServerVersion { get; set; } = "0.1.0";
}
