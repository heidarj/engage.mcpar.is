using VivaEngageMcp.Server.Mcp.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VivaEngageMcp.Server.Graph;
using VivaEngageMcp.Server.VivaEngageRest;
using VivaEngageMcp.Server.Mcp.Contracts;

namespace VivaEngageMcp.Server.Tests;

/// <summary>
/// Tests that confirm all MCP tools are read-only and never call mutation operations.
/// </summary>
public sealed class ReadOnlyGuardrailTests
{
    private readonly Mock<IVivaEngageGraphService> _graphService = new(MockBehavior.Strict);
    private readonly Mock<IVivaEngageRestService> _restService = new(MockBehavior.Strict);

    private VivaEngageTools CreateSut() => new(
        _graphService.Object,
        _restService.Object,
        NullLogger<VivaEngageTools>.Instance);

    [Fact]
    public async Task GetCurrentUser_OnlyCallsGetCurrentUser()
    {
        _restService.Setup(s => s.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VivaEngageCurrentUserDto { Id = 1, Name = "user" });

        var sut = CreateSut();
        await sut.GetCurrentUserAsync(CancellationToken.None);

        // Verify only the read operation was called
        _restService.Verify(s => s.GetCurrentUserAsync(It.IsAny<CancellationToken>()), Times.Once);
        _restService.VerifyNoOtherCalls();
        _graphService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ListCommunities_OnlyCallsListCommunities()
    {
        _graphService.Setup(s => s.ListCommunitiesAsync(It.IsAny<ListCommunitiesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<CommunityDto> { Items = [] });

        var sut = CreateSut();
        await sut.ListCommunitiesAsync();

        _graphService.Verify(s => s.ListCommunitiesAsync(It.IsAny<ListCommunitiesRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _graphService.VerifyNoOtherCalls();
        _restService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCommunity_OnlyCallsGetCommunity()
    {
        _graphService.Setup(s => s.GetCommunityAsync("test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommunityDto { Id = "test-id", DisplayName = "Test" });

        var sut = CreateSut();
        await sut.GetCommunityAsync("test-id");

        _graphService.Verify(s => s.GetCommunityAsync("test-id", It.IsAny<CancellationToken>()), Times.Once);
        _graphService.VerifyNoOtherCalls();
        _restService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMyAssignedRoles_OnlyCallsGetMyAssignedRoles()
    {
        _graphService.Setup(s => s.GetMyAssignedRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssignedRolesDto { Items = [] });

        var sut = CreateSut();
        await sut.GetMyAssignedRolesAsync();

        _graphService.Verify(s => s.GetMyAssignedRolesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _graphService.VerifyNoOtherCalls();
        _restService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetThreadMessages_OnlyCallsGetThreadMessages()
    {
        _restService.Setup(s => s.GetThreadMessagesAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var sut = CreateSut();
        await sut.GetThreadMessagesAsync(42);

        _restService.Verify(s => s.GetThreadMessagesAsync(42, It.IsAny<CancellationToken>()), Times.Once);
        _restService.VerifyNoOtherCalls();
        _graphService.VerifyNoOtherCalls();
    }
}
