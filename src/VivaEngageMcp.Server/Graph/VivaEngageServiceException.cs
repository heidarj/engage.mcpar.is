namespace VivaEngageMcp.Server.Graph;

public enum VivaEngageServiceErrorKind
{
    Unknown,
    NotFound,
    Unauthorized,
    Forbidden,
    Throttled,
    InvalidInput,
    UpstreamFailure
}

/// <summary>
/// Represents a typed error from downstream Graph or REST service calls.
/// </summary>
public sealed class VivaEngageServiceException : Exception
{
    public VivaEngageServiceErrorKind Kind { get; }

    public VivaEngageServiceException(string message, VivaEngageServiceErrorKind kind = VivaEngageServiceErrorKind.Unknown)
        : base(message)
    {
        Kind = kind;
    }

    public VivaEngageServiceException(string message, Exception inner, VivaEngageServiceErrorKind kind = VivaEngageServiceErrorKind.Unknown)
        : base(message, inner)
    {
        Kind = kind;
    }

    /// <summary>Maps a caught Graph SDK or HTTP exception to a typed service exception.</summary>
    public static VivaEngageServiceException FromGraphException(Exception ex)
    {
        if (ex is Microsoft.Graph.Models.ODataErrors.ODataError odataEx)
        {
            var status = odataEx.ResponseStatusCode;
            var kind = status switch
            {
                401 => VivaEngageServiceErrorKind.Unauthorized,
                403 => VivaEngageServiceErrorKind.Forbidden,
                404 => VivaEngageServiceErrorKind.NotFound,
                429 => VivaEngageServiceErrorKind.Throttled,
                >= 500 => VivaEngageServiceErrorKind.UpstreamFailure,
                _ => VivaEngageServiceErrorKind.Unknown
            };
            return new VivaEngageServiceException(
                $"Graph API error ({status}): {odataEx.Error?.Message ?? odataEx.Message}",
                ex,
                kind);
        }
        if (ex is HttpRequestException httpEx)
        {
            var kind = httpEx.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => VivaEngageServiceErrorKind.Unauthorized,
                System.Net.HttpStatusCode.Forbidden => VivaEngageServiceErrorKind.Forbidden,
                System.Net.HttpStatusCode.NotFound => VivaEngageServiceErrorKind.NotFound,
                System.Net.HttpStatusCode.TooManyRequests => VivaEngageServiceErrorKind.Throttled,
                _ => VivaEngageServiceErrorKind.UpstreamFailure
            };
            return new VivaEngageServiceException(
                $"HTTP error calling Graph API: {httpEx.Message}",
                ex,
                kind);
        }
        return new VivaEngageServiceException($"Unexpected error calling Graph API: {ex.Message}", ex);
    }

    /// <summary>Maps a caught HTTP exception from the legacy REST service to a typed exception.</summary>
    public static VivaEngageServiceException FromRestException(Exception ex, int? statusCode = null)
    {
        if (ex is HttpRequestException httpEx)
        {
            var code = statusCode ?? (int?)httpEx.StatusCode;
            var kind = code switch
            {
                401 => VivaEngageServiceErrorKind.Unauthorized,
                403 => VivaEngageServiceErrorKind.Forbidden,
                404 => VivaEngageServiceErrorKind.NotFound,
                429 => VivaEngageServiceErrorKind.Throttled,
                >= 500 => VivaEngageServiceErrorKind.UpstreamFailure,
                _ => VivaEngageServiceErrorKind.Unknown
            };
            return new VivaEngageServiceException(
                $"HTTP error calling Viva Engage REST API: {httpEx.Message}",
                ex,
                kind);
        }
        return new VivaEngageServiceException($"Unexpected error calling Viva Engage REST API: {ex.Message}", ex);
    }
}
