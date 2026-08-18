---
title: Development
weight: 30
---

# Development

## Build and test

```bash
dotnet build Ssabba.slnx
dotnet test  Ssabba.slnx
```

## Running locally

Start the infrastructure, then the app from your machine:

```bash
cd deploy && docker compose up -d db keycloak
cd .. && dotnet watch --project src/Ssabba.Web
```

`appsettings.Development.json` points at `localhost` for both PostgreSQL and Keycloak.

## Database migrations

```bash
dotnet ef migrations add <Name> -p src/Ssabba.Infrastructure -s src/Ssabba.Web -o Migrations
dotnet ef database update      -p src/Ssabba.Infrastructure -s src/Ssabba.Web
```

The app also applies pending migrations at startup unless `Database:MigrateOnStartup` is `false`.

## Project layout

| Project                | Contains                                            |
| ---------------------- | --------------------------------------------------- |
| `Ssabba.Domain`        | Entities and rating maths, no dependencies           |
| `Ssabba.Shared`        | DTOs and route constants shared with the WASM client |
| `Ssabba.Infrastructure`| EF Core `DbContext`, configurations, migrations      |
| `Ssabba.Web`           | Blazor host, API endpoints, authentication           |
| `Ssabba.Web.Client`    | Components that can run in WebAssembly               |

## Render modes

The app uses **Auto** interactivity. Components that must run in the browser live in
`Ssabba.Web.Client` and reach the server through `/api/*`; server-rendered pages query the
`DbContext` directly through the same helpers in `MatchQueries`.

Authentication is a backend-for-frontend: tokens stay in the server's cookie, and the client only
receives the serialised authentication state.

## Who the request is acting as

Nearly everything Ssabba does is relative to a person: a rating is theirs, a match result is theirs
to amend or not. Two pieces answer "who is this", and they are deliberately separate.

**Provisioning happens once, at sign-in.** `PlayerProvisioner.EnsureAsync` runs from the OIDC
`OnTicketReceived` event — after the userinfo claims have arrived and before the cookie is issued —
and writes the `Player` carrying that `sub`. It is idempotent: a later sign-in finds the row and
changes nothing. See
[Identity and communities]({{< relref "../data-model/identity-and-communities" >}}) for what it does
and does not claim.

**Resolution happens per request, and is only ever a lookup.** Inject the scoped
`CurrentPlayerAccessor` and `await` its `GetAsync()`:

```csharp
group.MapPost("/", async (CurrentPlayerAccessor current, CancellationToken ct) =>
{
    if (await current.GetAsync(ct) is not { CommunityId: not null } me)
    {
        return Results.Forbid();
    }
    // me.PlayerId, me.CommunityMemberId, me.Role
});
```

`null` means the request is anonymous, or carries a subject we hold no player for. The membership
half is nullable too: an instance with no community yet has players who belong nowhere.

It reads `HttpContext`, so it serves the minimal API and server-rendered components. An
**interactive** Blazor circuit has no live request — pass such a component what it needs as a
parameter from a server-rendered parent rather than injecting the accessor into it.

**Which community is never a question.** `CommunityQueries.ResolveCommunityIdAsync`
(`src/Ssabba.Web/Endpoints/CommunityEndpoints.cs`) is the single answer: the instance's one
community, `null` before first run, and an exception when the database holds two — a state the app
has no way to tell apart and should not guess at. Endpoints and server-rendered pages call it and
404 on `null`; only `CommunityQueries.CreateFirstAsync` ever writes a `Community`, and it is also
what binds its creator as `Owner` — the one membership nobody grants.

`TestAuthHandler` stands in for the Keycloak handshake, and because that handler *is* a test's
sign-in, it calls `PlayerProvisioner.EnsureAsync` itself. So a test client created with
`CreateClientAs("ada")` is a real player on the roster, not a bare claims principal — worth
remembering when a roster assertion counts one more row than the test put there.

## What a request is allowed to do

Roles live on the membership, so authorization asks `CurrentPlayerAccessor`, never the `roles` claim
— Keycloak's realm roles say what somebody is on this instance, and `CommunityMember.Role` is what
they may do here. `CommunityRole` is ordered from Guest to Owner precisely so a policy can be a
comparison rather than a list of names to keep in step.

Two policies cover standing (`src/Ssabba.Web/Auth/CommunityAuthorization.cs`), and both require the
membership to be `Active` — a pending or suspended one carries nothing, whatever its role:

```csharp
group.MapPost("/", …).RequireAuthorization(CommunityPolicies.ActiveMember);   // Member and above
group.MapPut("/",  …).RequireAuthorization(CommunityPolicies.Administrator);  // Admin and above
```

A rung in between is one `CommunityRoleRequirement` away; add the policy when something needs it.

The third is about one particular row rather than about standing, so it is authorized against a
resource. Amending a match is the case: an organiser may amend any result, and everybody else only
one they played in, and only inside the correction window.

```csharp
var context = await MatchQueries.AmendContextAsync(db, communityId, id, ct);
var allowed = await authorization.AuthorizeAsync(http.User, context, CommunityPolicies.AmendMatch);
```

Reach for the resource form when the answer depends on the row — who is in it, how old it is, what
it configures. Reach for the standing form when it does not. A 404 for a row that is not there comes
before either, which gives nothing away as long as reading it needs no account.

## Writing docs

This wiki is Hugo. With the dev override running, `http://localhost:1313/docs/` live-reloads.
