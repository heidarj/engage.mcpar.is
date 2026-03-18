# Viva Engage MCP Server — Copilot Instructions

This repository is a .NET 10 ASP.NET Core MCP server for read-only Viva Engage access.

## Architecture principles

- This server is a protected API/resource server
- Clients authenticate to this API with Microsoft Entra bearer tokens
- The API validates tokens for itself (audience = `api://<client-id>`)
- **Graph-backed Viva Engage operations** use delegated downstream user tokens via OBO
- **Legacy Viva Engage REST operations** use Entra-backed delegated user auth for Yammer
- Never expose downstream tokens to clients

## API surface reality

- **Microsoft Graph** is the modern API surface for Viva Engage **communities** and **roles**
- **Legacy Viva Engage (Yammer) REST APIs** are still required for feed, thread, group messages, current user profile, and search
- Do not build this as Graph-only — both surfaces are intentionally required

## Token service abstractions

Always go through the token service abstractions:
- `IGraphTokenService` — for Graph downstream tokens
- `IVivaEngageTokenService` — for Yammer/Viva Engage downstream tokens

Do not bake token acquisition assumptions directly into tool handlers or service implementations.

## Read-only rules

Keep v1 strictly read-only:
- No post message
- No reply to thread
- No create/update/delete community
- No reactions
- No follow/unfollow
- No admin exports
- No file uploads

All tools must have `ReadOnly = true` on the `[McpServerTool]` attribute.

## Tool surface

Implemented tools:
- `get_current_user` — legacy REST
- `list_communities` — Graph
- `get_community` — Graph
- `list_feed_messages` — legacy REST
- `list_group_messages` — legacy REST
- `get_thread_messages` — legacy REST
- `search_viva_engage` — legacy REST
- `get_my_assigned_roles` — Graph
- `list_groups_for_user` — legacy REST

## DTO principles

- Use stable DTOs; never return raw Microsoft Graph SDK types or raw Yammer API payloads
- Map all upstream payloads in `Mcp/Mapping/`
- Keep DTOs in `Mcp/Contracts/`

## Pagination

- Graph: respect `@odata.nextLink` exactly; use `WithUrl` for next-page requests
- Legacy REST: use `older_than`, `newer_than`, `threaded`, `limit`, `page`, `num_per_page` parameters as documented

## Error handling

Use `VivaEngageServiceException` with a typed `VivaEngageServiceErrorKind` for all downstream failures.
Use `VivaEngageAuthException` for auth failures.
Never log tokens, raw auth headers, or sensitive PII.

## Testing

- Unit test config validation, DTO mapping, input validation, pagination, and read-only guardrails
- Mock `IVivaEngageGraphService` and `IVivaEngageRestService` — do not call real APIs in unit tests
- Use `MockBehavior.Strict` for mutation-safety tests

## Configuration

All options are in strongly typed options classes:
- `EntraOptions` — Entra/MSAL config
- `GraphOptions` — Graph scopes and base URL
- `VivaEngageOptions` — legacy REST base URL and token scope
- `McpOptions` — MCP endpoint path, server name/version

Never hardcode secrets. Use `dotnet user-secrets` for local development.
