# Qwertide

[![CI](https://github.com/ManuelMadu/Qwertide/actions/workflows/ci.yml/badge.svg)](https://github.com/ManuelMadu/Qwertide/actions/workflows/ci.yml)

**Live:** https://qwertide.azurewebsites.net

A browser-based typing-speed game built end to end in **C# / .NET 8**. A passage
appears, you type it, and Qwertide tracks your words-per-minute and accuracy live —
then drops your run onto a persistent, API-backed leaderboard.

The entire front-end and game loop are **Blazor WebAssembly** (the scoring engine
is pure C#; the only JavaScript is a 22-line caret-positioning helper). The
leaderboard is served by an **ASP.NET Core Web API** backed by **EF Core + SQLite**.
In production both ship as a **single Azure App Service**. It was built as a
focused, production-minded portfolio piece for a Junior C#/.NET role.

---

## Table of contents

- [Overview](#overview)
- [Key features](#key-features)
- [Architecture](#architecture)
- [Tech stack](#tech-stack)
- [API documentation](#api-documentation)
- [Security](#security)
- [Testing](#testing)
- [CI/CD](#cicd)
- [Deployment](#deployment)
- [Performance considerations](#performance-considerations)
- [Accessibility](#accessibility)
- [Project structure](#project-structure)
- [Local development](#local-development)
- [Environment variables](#environment-variables)
- [Engineering tradeoffs](#engineering-tradeoffs)
- [Known limitations](#known-limitations)
- [Future improvements](#future-improvements-not-yet-implemented)
- [Project status](#project-status)
- [License](#license)

---

## Overview

Qwertide is a full-stack, single-page typing test. The interesting engineering is
deliberately *not* in the UI: the words-per-minute and accuracy math is isolated in
a pure, dependency-free domain class so it can be unit-tested directly, and the
leaderboard is a small but properly-hardened REST API rather than a localStorage
hack. The project's goal is to demonstrate the complete C#/.NET stack — WASM
front-end, Web API, ORM with migrations, a tested domain layer, CI, and a public
cloud deployment — in one coherent, honestly-scoped app.

## Key features

- **Live per-keystroke feedback** — each character is highlighted correct/incorrect
  in place as you type, with a blinking caret on the current position.
- **Real-time metrics** — gross WPM (5 chars = 1 word) and accuracy update *while*
  you type, computed by a pure scoring engine.
- **Type-past-errors model** — wrong characters are marked and counted but don't
  block you, matching common typing-test convention.
- **Three passage lengths** — short warm-ups, full paragraphs, and real C# snippets.
- **Persistent leaderboard** — top runs are submitted to the API and stored in
  SQLite via EF Core, surviving restarts and redeploys.
- **Hardened public API** — input validation, per-IP rate limiting, security
  headers, and HSTS (see [Security](#security)).
- **Accessible, restrained design** — dark terminal-mono theme, WCAG AA text
  contrast, and motion gated behind `prefers-reduced-motion`.

## Architecture

One solution, three projects, with the scoring engine isolated so it is testable
without a browser and the client depending on the API only through an interface.

```
┌─────────────────────────────────────────────────────────────┐
│  Azure App Service (single Linux service)                     │
│                                                               │
│  ASP.NET Core Web API  ──────────────────────────────────┐   │
│   • GET/POST /api/scores            ┌──────────────────┐  │   │
│   • rate limiting, validation,      │  EF Core + SQLite │  │   │
│     security headers, HSTS  ───────▶│  (/home/qwertide  │  │   │
│   • serves the published WASM       │   .db, migrations)│  │   │
│     bundle + SPA fallback           └──────────────────┘  │   │
│                                                           │   │
│  Blazor WebAssembly client  ◀─── served as static files ──┘   │
│   • TypingSession (pure scoring engine)                       │
│   • ILeaderboardService ──▶ HttpClient ──▶ /api/scores        │
└─────────────────────────────────────────────────────────────┘
```

**Key decisions**

- **Pure domain layer.** All metric math lives in `TypingSession` as static,
  UI-free functions, so the test project references it directly and the Blazor
  component owns only rendering.
- **Interface-driven leaderboard.** The UI codes against `ILeaderboardService`;
  the active implementation (`ApiLeaderboardService`) is swapped in via DI without
  any UI changes. A second `localStorage` implementation exists to demonstrate the
  abstraction (it is not currently wired in as a runtime fallback).
- **Single-service hosting.** In production the API serves the published WASM
  client and falls back to `index.html` for client-side routes, so there is one
  deployable, one origin, and no production CORS.

### The scoring engine

`Services/TypingSession.cs` holds every calculation as plain, pure C#:

```csharp
// Gross WPM, guarding the zero-time edge case
TypingSession.GrossWpmFor(charsTyped: 50, elapsedSeconds: 60); // -> 10

// Accuracy = correct / total keystrokes, guarding 0/0
TypingSession.AccuracyFor(correctKeystrokes: 45, totalKeystrokes: 50); // -> 90
```

`CountKeystrokes` counts *every* character committed in a single input event
(a fast typist or IME can commit several between ticks), so the accuracy
denominator is never under-counted.

## Tech stack

| Layer        | Choice                                             |
| ------------ | -------------------------------------------------- |
| Language     | C# (.NET 8), nullable reference types enabled      |
| Front-end    | Blazor WebAssembly + MudBlazor 8                   |
| Back-end     | ASP.NET Core Web API                               |
| ORM / data   | EF Core 8 + SQLite (code-first migrations)         |
| Testing      | xUnit + FluentAssertions + coverlet                |
| CI           | GitHub Actions (build with warnings-as-errors)     |
| Code quality | `.editorconfig` + `dotnet format` (CI-enforced)    |
| Observability| Health checks (`/health`, EF Core DbContext check) |
| Hosting      | Azure App Service (Linux, single service)          |
| API docs     | Swagger / OpenAPI (Swashbuckle, Development only)  |

## API documentation

Base path `/api`. In Development, interactive Swagger UI is available at `/swagger`.

| Method | Route                  | Description                              | Success | Notes |
| ------ | ---------------------- | ---------------------------------------- | ------- | ----- |
| `GET`  | `/api/scores?top={n}`  | Top runs, ordered WPM↓, accuracy↓, time↑ | `200`   | `top` clamped to 1–100 |
| `GET`  | `/api/scores/{id:int}` | Single run by id                         | `200`   | `404` if not found |
| `POST` | `/api/scores`          | Submit a run                             | `201`   | Validated; rate-limited (`429`); returns the persisted row |

The POST body binds to a dedicated `ScoreRequest` DTO; server-owned fields
(`Id`, `CreatedAtUtc`) are intentionally absent so they cannot be set by the client.

A `GET /health` endpoint (outside `/api`) returns `Healthy`/`Unhealthy` and
verifies the database connection — suitable for platform health probes.

## Security

Implemented in this repository (`Program.cs`, `ScoresController.cs`, `ScoreRequest.cs`):

- **Over-posting protection** — a request DTO separate from the EF entity prevents
  clients from spoofing server-owned fields.
- **Input validation** — `[Required]`, `[StringLength]`, and `[Range]` attributes
  enforced automatically by `[ApiController]`.
- **Rate limiting** — fixed-window limiter, **5 submissions/minute per client IP**,
  returning `429`; partitioned by IP so one abuser can't lock everyone out.
- **Security response headers** — `X-Content-Type-Options: nosniff`,
  `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer` on every response.
- **HSTS** in production; HTTPS redirection enabled; HTTPS-only enforced at the
  platform.
- **Reverse-proxy awareness** — `X-Forwarded-*` handling for Azure's load balancer,
  **gated to Production** so a non-Azure host can't spoof scheme/client IP.
- **CORS** — a named policy with an explicit allow-list from configuration (no
  `AllowAnyOrigin`).
- **SPA fallback carve-out** — unmatched `/api/*` routes return a real `404`
  instead of the HTML shell.
- **SQL injection** — eliminated by EF Core parameterization.
- **Secrets** — none committed; the connection string is injected via an App
  Service setting (see [Environment variables](#environment-variables)).

> Authentication/authorization is intentionally **not** implemented — see
> [Engineering tradeoffs](#engineering-tradeoffs) and [Known limitations](#known-limitations).

## Testing

```bash
dotnet test
```

21 xUnit tests (with FluentAssertions) cover the scoring engine and the edge cases
that break naive implementations: zero elapsed time, all-errors, empty input,
multi-character input events, and derived-metric state. Tests target the pure
domain layer directly, so they run fast and need no browser or HTTP host.

**Scope, stated honestly:** test coverage is the scoring engine. API/controller
integration tests, component tests, and end-to-end tests are not yet present (see
[Future improvements](#future-improvements-not-yet-implemented)).

## CI/CD

GitHub Actions (`.github/workflows/ci.yml`) runs on every push and pull request to
`main`:

1. Restore (`dotnet restore`)
2. **Format check** (`dotnet format --verify-no-changes`) — style violations fail the build
3. **Build in Release with `-warnaserror`** — warnings fail the build
4. Test (`dotnet test`)

Code style is pinned by an `.editorconfig` and enforced by the format check above.
NuGet packages and GitHub Actions are kept current automatically via **Dependabot**
(`.github/dependabot.yml`, weekly).

This is **CI only**. Deployment is currently a documented manual step (see below);
there is no automated CD pipeline yet.

## Deployment

Live on **Azure App Service** (Linux) as a single service: the ASP.NET Core API
hosts both the leaderboard endpoints and the published Blazor client. The SQLite
file lives on the persistent `/home` share, so scores survive restarts and
redeploys. The complete, reproducible runbook — resource creation, connection
string, HTTPS-only, and the publish/zip/deploy commands — is in
[DEPLOY.md](DEPLOY.md).

## Performance considerations

- **Indexed leaderboard reads** — a DB index on `Wpm` backs the primary ordering.
- **Bounded queries** — `top` is clamped to 100 so a request can't ask for an
  unbounded result set.
- **Rate limiting** — protects the write path from abuse-driven load.
- **Single origin** — the WASM bundle is served as static files by the same
  service, avoiding cross-origin round-trips in production.
- **Release builds** verified in CI.

## Accessibility

- WCAG AA text contrast across the theme.
- All motion (caret blink, live counters, in-place colouring) gated behind
  `prefers-reduced-motion`.
- Semantic, single-accent design system documented in [DESIGN.md](DESIGN.md).

## Project structure

```
Qwertide.sln
├── .github/workflows/ci.yml        Build + test on push/PR (warnings-as-errors)
├── .config/dotnet-tools.json       Pinned local tool: dotnet-ef
├── DEPLOY.md                       Azure App Service runbook
├── DESIGN.md                       Design-system rationale
└── src/
    ├── Qwertide.Client/            Blazor WebAssembly UI + pure scoring engine
    │   ├── Services/TypingSession.cs   pure, UI-free scoring engine (test surface)
    │   ├── Services/ILeaderboardService.cs + ApiLeaderboardService.cs
    │   ├── Pages/ Components/ Layout/   game screens, per-glyph render, shell
    │   └── Theme/ wwwroot/css/          custom MudBlazor theme + design system
    ├── Qwertide.Api/               ASP.NET Core leaderboard API
    │   ├── Controllers/ScoresController.cs
    │   ├── Models/ (Score entity + ScoreRequest DTO)
    │   ├── Data/ (DbContext, seeder) + Migrations/
    │   └── Program.cs                   pipeline: rate limiting, headers, CORS, SPA host
    └── Qwertide.Tests/             xUnit tests for the scoring engine
```

## Local development

Requires the **.NET 8 SDK**.

```bash
git clone https://github.com/ManuelMadu/Qwertide.git
cd Qwertide
dotnet restore
```

**Run the API + client together (production-like single service):**

```bash
dotnet run --project src/Qwertide.Api
# open the printed http://localhost:5229 (https://localhost:7237) URL
```

**Run the client standalone against a separately-running API (dev):**

```bash
# terminal 1
dotnet run --project src/Qwertide.Api      # API on http://localhost:5229
# terminal 2
dotnet run --project src/Qwertide.Client   # client points at the API per appsettings.Development.json
```

Blazor WASM is served as a cached static bundle, so after a change rebuild and do a
hard refresh (Cmd/Ctrl + Shift + R) to clear the cached WASM.

**Database:** EF Core applies migrations automatically on startup and seeds a few
entries when empty — no manual `dotnet ef database update` needed for a fresh clone.

## Environment variables

Configuration is layered via `appsettings.json` + `appsettings.{Environment}.json`,
overridable by environment variables (double-underscore notation).

**API (`Qwertide.Api`)**

| Key | Purpose | Default |
| --- | --- | --- |
| `ConnectionStrings__Qwertide` | SQLite connection string | `Data Source=qwertide.db` (prod: `/home/qwertide.db`) |
| `Cors__AllowedOrigins__0` | Allowed CORS origin(s) | dev localhost origins |
| `ASPNETCORE_ENVIRONMENT` | Environment (gates Swagger, HSTS, proxy trust) | `Production` on Azure |

**Client (`Qwertide.Client/wwwroot`)**

| Key | Purpose | Default |
| --- | --- | --- |
| `Api:BaseUrl` | API base URL | empty = same origin (prod); `http://localhost:5229` in dev |

## Engineering tradeoffs

- **No authentication.** A typing game's leaderboard doesn't need accounts, so the
  API is anonymous. The consequence — accepted deliberately — is that scores are
  client-submitted and not server-verified; `[Range]` validation blocks absurd
  values but not a plausible fake. Real anti-cheat would require server-side
  gameplay validation or auth, which is out of scope for this piece.
- **SQLite over a managed SQL service.** Zero-cost, zero-ops, and ideal for a
  single-instance portfolio app, at the cost of horizontal scalability (see below).
- **Migrate-on-startup.** Convenient for a one-instance deploy; a controlled
  migration step would be preferable at production scale.
- **Single-service deploy.** Simpler ops and no prod CORS, traded against the
  ability to scale the API and static hosting independently.

## Known limitations

These are real gaps, listed so the scope is unambiguous:

- Scores are **not authenticated or server-verified** (spoofable by design).
- **SQLite is single-node** — the app cannot currently scale out to multiple App
  Service instances without changing the data store.
- **Tests cover the scoring engine only** — no API integration, component, or E2E
  tests yet; coverage is not published or gated.
- **No automated CD, no containerization** — deployment is a manual, documented
  zip deploy.
- **Limited observability** — a database-backed `/health` endpoint exists, but
  there is no Application Insights, structured logging, metrics, or alerting yet.
- **Fonts are loaded from the Google Fonts CDN** in production (self-hosting is
  noted as a TODO for the performance/privacy budget).
- The **`localStorage` leaderboard implementation is not wired in** as a runtime
  offline fallback; it exists to demonstrate the `ILeaderboardService` abstraction.

## Future improvements (not yet implemented)

Prioritised, and clearly separate from what exists today:

1. **API integration tests** with `WebApplicationFactory`, plus bUnit component
   tests and a Playwright E2E happy-path; publish coverage from CI.
2. **Continuous deployment** — extend the workflow to deploy on green `main`.
3. **Containerization** — a Dockerfile for reproducible builds and portability.
4. **Deeper observability** — Application Insights, structured logging, and
   metrics/alerting (a database-backed `/health` endpoint is already in place).
5. **Scale-ready persistence** — move to Azure SQL / PostgreSQL if multi-instance
   hosting is needed, with a controlled migration step.
6. **Self-hosted fonts** to remove the third-party CDN dependency.

## Project status

v1 is complete and deployed:

- [x] **M1** Core game: passage render, keystroke capture, live highlight, timer
- [x] **M2** Scoring engine wired into the results screen
- [x] **M3** xUnit tests for WPM, accuracy, and edge cases
- [x] **M4** Leaderboard API: ASP.NET Core + EF Core + SQLite
- [x] **M5** Connect the client to the API
- [x] **M6** Styling, difficulty levels, reduced-motion support
- [x] **M7** Deploy to Azure App Service (public URL)
- [x] **M8** API hardening: rate limiting, security headers, HSTS, SPA 404 carve-out
- [x] **M9** Ops & quality: `/health` check, CI format gate, Dependabot, MIT license

## License

Released under the [MIT License](LICENSE).
