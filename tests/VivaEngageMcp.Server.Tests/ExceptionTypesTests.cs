using VivaEngageMcp.Server.Graph;
using VivaEngageMcp.Server.Auth;

namespace VivaEngageMcp.Server.Tests;

public sealed class ExceptionTypesTests
{
    [Fact]
    public void VivaEngageAuthException_StoresMessage()
    {
        var ex = new VivaEngageAuthException("Auth failed");
        Assert.Equal("Auth failed", ex.Message);
    }

    [Fact]
    public void VivaEngageAuthException_StoresInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new VivaEngageAuthException("Auth failed", inner);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void VivaEngageServiceException_DefaultKindIsUnknown()
    {
        var ex = new VivaEngageServiceException("message");
        Assert.Equal(VivaEngageServiceErrorKind.Unknown, ex.Kind);
    }

    [Theory]
    [InlineData(VivaEngageServiceErrorKind.NotFound)]
    [InlineData(VivaEngageServiceErrorKind.Throttled)]
    [InlineData(VivaEngageServiceErrorKind.Unauthorized)]
    public void VivaEngageServiceException_StoresKind(VivaEngageServiceErrorKind kind)
    {
        var ex = new VivaEngageServiceException("msg", kind);
        Assert.Equal(kind, ex.Kind);
    }

    [Theory]
    [InlineData(401, VivaEngageServiceErrorKind.Unauthorized)]
    [InlineData(403, VivaEngageServiceErrorKind.Forbidden)]
    [InlineData(404, VivaEngageServiceErrorKind.NotFound)]
    [InlineData(429, VivaEngageServiceErrorKind.Throttled)]
    [InlineData(500, VivaEngageServiceErrorKind.UpstreamFailure)]
    public void FromRestException_MapsHttpStatusToKind(int statusCode, VivaEngageServiceErrorKind expectedKind)
    {
        var httpEx = new HttpRequestException("error", null,
            (System.Net.HttpStatusCode)statusCode);
        var ex = VivaEngageServiceException.FromRestException(httpEx);
        Assert.Equal(expectedKind, ex.Kind);
    }
}
