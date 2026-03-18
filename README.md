# engage.mcpar.is — Viva Engage MCP Server

A production-minded, **read-only MCP server for Viva Engage**, built on **.NET 10** and **ASP.NET Core**, using the official [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk).

MCP clients (GitHub Copilot, Claude, etc.) connect to this server to read Viva Engage data on behalf of the signed-in user.

---

## What it does

Exposes the following MCP tools over a single `/mcp` HTTP endpoint:

| Tool | Backend | Description |
|---|---|---|
| `get_current_user` | Legacy REST | Current user's Viva Engage profile |
| `list_communities` | Microsoft Graph | List communities with pagination |
| `get_community` | Microsoft Graph | Single community metadata |
| `list_feed_messages` | Legacy REST | User's feed messages |
| `list_group_messages` | Legacy REST | Messages in a group |
| `get_thread_messages` | Legacy REST | All messages in a thread |
| `search_viva_engage` | Legacy REST | Search messages, users, groups, topics |
| `get_my_assigned_roles` | Microsoft Graph | User's Viva Engage roles |
| `list_groups_for_user` | Legacy REST | Groups the user belongs to |

All tools are strictly **read-only**.

---

## API surface reality

Viva Engage spans two API surfaces:

**Microsoft Graph** is the modern surface and covers:
- Communities (`/employeeExperience/communities`)
- Assigned roles (`/me/employeeExperience/assignedRoles`)

**Legacy Viva Engage (Yammer) REST API** is still required for:
- User profile (`users/current.json`)
- Feed messages (`messages/my_feed.json`, etc.)
- Group messages (`messages/in_group/{id}.json`)
- Thread messages (`messages/in_thread/{id}.json`)
- Search (`search.json`)
- User groups (`groups/for_user/{id}.json`)

This server uses both surfaces correctly.

---

## Auth architecture

1. MCP clients authenticate to this server using an Entra-issued bearer token (audience = this API)
2. This API validates the incoming token
3. For Graph calls: acquires a downstream delegated token via OBO (on-behalf-of) flow
4. For legacy Viva Engage REST calls: acquires a delegated Yammer/Viva Engage token via OBO
5. Downstream tokens are never exposed to clients

---

## Read-only guarantee

- Only read Graph delegated permissions are requested
- No `POST`, `PATCH`, or `DELETE` tool surfaces exist
- All MCP tools are marked `ReadOnly = true`
- No write operations in the codebase

---

## Required Entra app registration

### Create a multitenant app registration

1. Go to [Azure Portal → Entra ID → App registrations](https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade) → New registration
2. Name: `Viva Engage MCP` (or your choice)
3. Supported account types: **Accounts in any organizational directory** (multitenant)
4. Redirect URI: none required for this API
5. Note the **Application (client) ID** and **Directory (tenant) ID**

### Create a client secret

App registration → Certificates & secrets → New client secret

### Add a custom scope for MCP clients

App registration → Expose an API:
- Set Application ID URI: `api://<client-id>`
- Add scope: `Mcp.Read` (or similar)

### Add API permissions

App registration → API permissions → Add a permission:

**Microsoft Graph (Delegated):**
- `Community.Read.All`
- `EngagementRole.Read`
- `User.Read`

**Yammer (Delegated):**
- `user_impersonation` (also shown as `access_as_user`)

---

## Local configuration

Copy `appsettings.example.json` and fill in your values:

```json
{
  "Entra": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "common",
    "ClientId": "<your-client-id>",
    "ClientSecret": "<your-client-secret>",
    "Audience": "api://<your-client-id>",
    "ApiScope": "Mcp.Read"
  },
  "VivaEngage": {
    "RequestedScopeOrResource": "https://api.yammer.com/user_impersonation"
  }
}
```

Use **user secrets** for local development (never commit `ClientSecret`):

```bash
cd src/VivaEngageMcp.Server
dotnet user-secrets set "Entra:ClientId" "<your-client-id>"
dotnet user-secrets set "Entra:ClientSecret" "<your-client-secret>"
```

---

## How to run locally

```bash
dotnet run --project src/VivaEngageMcp.Server
```

The server starts on `https://localhost:7xxx` (or the port shown in launch settings).

Health check: `GET /healthz`

---

## How to connect an MCP client

Configure your MCP client to connect to:

```
http://localhost:<port>/mcp
```

The client must provide a valid bearer token for this API as `Authorization: Bearer <token>`.

---

## How to run tests

```bash
dotnet test
```

Tests cover: config validation, DTO mapping, input validation, read-only guardrails, pagination handling, and exception types.

---

## Project structure

```
src/VivaEngageMcp.Server/
├── Program.cs
├── Configuration/      # Strongly typed options
├── Auth/               # IGraphTokenService, IVivaEngageTokenService, implementations
├── Graph/              # IVivaEngageGraphService, implementation, error types
├── VivaEngageRest/     # IVivaEngageRestService, implementation, raw Yammer models
├── Mcp/
│   ├── Contracts/      # DTOs and request models
│   ├── Mapping/        # Mapper functions
│   └── Tools/          # MCP tool handlers
├── Middleware/         # Origin validation, exception handling
└── Diagnostics/        # (health, readyz, whoami endpoints in Program.cs)

tests/VivaEngageMcp.Server.Tests/
```

---

## Known limitations

- `get_my_assigned_roles` uses a raw HTTP request because `assignedRoles` is not yet in the Graph SDK (5.79.0). It will work at runtime when the Graph API is available.
- Personal Microsoft accounts are not supported (several Graph Viva Engage APIs explicitly exclude them).
- Daemon/app-only mode is not supported for legacy Viva Engage REST (delegated only by design).
- The exact Yammer OBO resource/scope may require verification — configure `VivaEngage:RequestedScopeOrResource` appropriately for your tenant.

