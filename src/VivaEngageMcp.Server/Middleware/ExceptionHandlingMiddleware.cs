using VivaEngageMcp.Server.Graph;
using VivaEngageMcp.Server.Auth;

namespace VivaEngageMcp.Server.Middleware;

/// <summary>
/// Catches typed service exceptions and maps them to appropriate HTTP responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (VivaEngageAuthException ex)
        {
            _logger.LogWarning("Auth exception: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Bearer realm=\"viva-engage-mcp\"";
            await context.Response.WriteAsJsonAsync(new { error = "authentication_required", message = ex.Message });
        }
        catch (VivaEngageServiceException ex)
        {
            _logger.LogWarning("Service exception: Kind={Kind} Message={Message}", ex.Kind, ex.Message);
            var statusCode = ex.Kind switch
            {
                VivaEngageServiceErrorKind.NotFound => StatusCodes.Status404NotFound,
                VivaEngageServiceErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
                VivaEngageServiceErrorKind.Forbidden => StatusCodes.Status403Forbidden,
                VivaEngageServiceErrorKind.Throttled => StatusCodes.Status429TooManyRequests,
                VivaEngageServiceErrorKind.InvalidInput => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status502BadGateway
            };
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new { error = ex.Kind.ToString().ToLowerInvariant(), message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Input validation error: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "invalid_input", message = ex.Message });
        }
        catch (OperationCanceledException)
        {
            // Don't log or respond to cancelled requests
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "internal_error", message = "An unexpected error occurred." });
        }
    }
}
