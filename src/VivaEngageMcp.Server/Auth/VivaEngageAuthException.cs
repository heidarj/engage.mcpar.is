namespace VivaEngageMcp.Server.Auth;

/// <summary>
/// Represents authentication or authorization failures in the Viva Engage MCP server.
/// </summary>
public sealed class VivaEngageAuthException : Exception
{
    public VivaEngageAuthException(string message) : base(message) { }
    public VivaEngageAuthException(string message, Exception inner) : base(message, inner) { }
}
