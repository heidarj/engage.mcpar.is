using Microsoft.Extensions.Options;
using VivaEngageMcp.Server.Configuration;

namespace VivaEngageMcp.Server.Middleware;

/// <summary>
/// Validates the Origin header for MCP requests, rejecting requests from unknown origins.
/// Health probe paths are excluded from this check.
/// </summary>
public sealed class OriginValidationMiddleware
{
    private static readonly string[] HealthPaths = ["/healthz", "/readyz"];

    private readonly RequestDelegate _next;
    private readonly ILogger<OriginValidationMiddleware> _logger;
    private readonly string[] _allowedOrigins;

    public OriginValidationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<OriginValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? ["http://localhost", "https://localhost"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip for health probes
        var path = context.Request.Path.Value ?? string.Empty;
        if (HealthPaths.Any(h => path.StartsWith(h, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Skip if no Origin header (non-browser clients)
        if (!context.Request.Headers.TryGetValue("Origin", out var originValues))
        {
            await _next(context);
            return;
        }

        var origin = originValues.ToString();
        if (!IsAllowedOrigin(origin))
        {
            _logger.LogWarning("Request from disallowed origin '{Origin}' rejected.", origin);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden: Origin not allowed.");
            return;
        }

        await _next(context);
    }

    private bool IsAllowedOrigin(string origin) =>
        _allowedOrigins.Any(allowed =>
            string.Equals(allowed, origin, StringComparison.OrdinalIgnoreCase));
}
