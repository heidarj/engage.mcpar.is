using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using VivaEngageMcp.Server.Auth;
using VivaEngageMcp.Server.Configuration;
using VivaEngageMcp.Server.Graph;
using VivaEngageMcp.Server.Middleware;
using VivaEngageMcp.Server.VivaEngageRest;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────────────────────
builder.Services.AddOptions<EntraOptions>()
    .Bind(builder.Configuration.GetSection(EntraOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<GraphOptions>()
    .Bind(builder.Configuration.GetSection(GraphOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<VivaEngageOptions>()
    .Bind(builder.Configuration.GetSection(VivaEngageOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<McpOptions>()
    .Bind(builder.Configuration.GetSection(McpOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ── Auth ───────────────────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection(EntraOptions.SectionName))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph()
    .AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.FallbackPolicy = options.DefaultPolicy;
});

// ── HTTP context accessor ──────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Token services ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IGraphTokenService, GraphTokenService>();
builder.Services.AddScoped<IVivaEngageTokenService, VivaEngageTokenService>();

// ── Graph service ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IVivaEngageGraphService, VivaEngageGraphService>();

// ── Legacy Viva Engage REST service ───────────────────────────────────────────
builder.Services.AddHttpClient<IVivaEngageRestService, VivaEngageRestService>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<VivaEngageOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ── MCP ────────────────────────────────────────────────────────────────────────
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// ── Health checks ──────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ── Logging ────────────────────────────────────────────────────────────────────
builder.Logging.AddConsole();

var app = builder.Build();

// Log configuration on startup
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
var mcpOpts = app.Services.GetRequiredService<IOptions<McpOptions>>().Value;
startupLogger.LogInformation(
    "Starting {ServerName} v{ServerVersion} on endpoint {Endpoint}",
    mcpOpts.ServerName, mcpOpts.ServerVersion, mcpOpts.EndpointPath);

// ── Middleware pipeline ────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<OriginValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// ── Health endpoints (no auth required) ───────────────────────────────────────
app.MapHealthChecks("/healthz").AllowAnonymous();
app.MapGet("/readyz", () => Results.Ok(new { status = "ready" })).AllowAnonymous();

// ── Optional authenticated whoami endpoint ─────────────────────────────────────
app.MapGet("/whoami", (HttpContext ctx) =>
{
    var user = ctx.User;
    return Results.Ok(new
    {
        name = user.Identity?.Name,
        claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList()
    });
}).RequireAuthorization();

// ── MCP endpoint ──────────────────────────────────────────────────────────────
app.MapMcp(mcpOpts.EndpointPath)
    .RequireAuthorization();

app.Run();

// Make Program accessible for integration testing
public partial class Program { }
