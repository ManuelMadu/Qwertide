# Qwertide

A browser-based typing-speed game built end to end in C# / .NET. A passage
appears, you type it, and Qwertide tracks your words-per-minute and accuracy
live before dropping your run onto a persistent leaderboard.

The entire front-end and game loop are written in **Blazor WebAssembly** (no
JavaScript game logic), and the leaderboard is served by a separate **ASP.NET
Core Web API** backed by **EF Core**. It is a focused, full-stack, deployed .NET
app built as a portfolio piece for a Junior Software Developer (C#/.NET) role.

## Features

- **Live per-keystroke feedback.** Each character is highlighted correct or
  incorrect in place as you type, with a blinking caret on the current position.
- **Real-time metrics.** Gross WPM (5 characters = 1 word) and accuracy update
  while you type, not just at the end.
- **Type-past-errors model.** Wrong characters are marked and counted, but you
  are not forced to stop and fix them, matching common typing-test convention.
- **Three passage lengths.** Short warm-ups, full paragraphs, and real C# code
  snippets.
- **Persistent leaderboard.** Top runs survive a refresh. (Client-side store
  today; swaps to the API at M5 behind one unchanged interface.)
- **Pure, unit-tested scoring engine.** All WPM and accuracy math lives in a
  UI-free C# class with xUnit coverage.
- **Accessible, restrained design.** A dark terminal-mono theme, WCAG AA text
  contrast, and motion gated behind `prefers-reduced-motion`.

## Tech stack

| Layer        | Choice                                             |
| ------------ | -------------------------------------------------- |
| Language     | C# (.NET 8)                                        |
| Front-end    | Blazor WebAssembly + MudBlazor                     |
| Back-end     | ASP.NET Core Web API *(in progress, M4)*           |
| ORM / data   | EF Core 8 + SQLite *(in progress, M4)*             |
| Testing      | xUnit + FluentAssertions                           |
| Hosting      | Azure App Service *(planned, M7)*                  |

## Architecture

One solution, structured so the scoring engine is testable in isolation and the
client depends on the API only through an interface:

```
Qwertide.sln
└── src/
    ├── Qwertide.Client   Blazor WebAssembly UI + pure scoring engine
    ├── Qwertide.Tests    xUnit tests for the scoring engine
    └── Qwertide.Api      ASP.NET Core leaderboard API  (planned, M4)
```

The leaderboard sits behind an `ILeaderboardService` contract. v1 ships a
`localStorage`-backed implementation; M5 swaps in an `HttpClient` one that talks
to the API, with no UI changes.

### The scoring engine

`Services/TypingSession.cs` holds every metric calculation as plain, pure C#
with zero UI dependencies, so it can be unit-tested directly:

```csharp
// gross WPM, guarding the zero-time edge case
TypingSession.GrossWpmFor(charsTyped: 50, elapsedSeconds: 60); // -> 10

// accuracy as correct / total keystrokes, guarding 0/0
TypingSession.AccuracyFor(correctKeystrokes: 45, totalKeystrokes: 50); // -> 90
```

Keystroke accounting (`CountKeystrokes`) counts every character committed in a
single input event, so fast typing never under-counts the accuracy denominator.

## Running locally

Requires the **.NET 8 SDK**.

```bash
git clone https://github.com/ManuelMadu/Qwertide.git
cd Qwertide

# run the game
cd src/Qwertide.Client
dotnet run
# open the printed http://localhost:xxxx URL
```

Blazor WebAssembly is served as a static bundle, so changes need a rebuild and a
hard refresh (Cmd/Ctrl + Shift + R) to clear the cached WASM.

## Tests

```bash
dotnet test
```

Covers the WPM and accuracy math plus the edge cases that break naive
implementations: zero elapsed time, all errors, empty input, and multi-character
input events.

## Project status

v1 is a work in progress. Done and in progress:

- [x] **M1** Core game: passage render, keystroke capture, live highlight, timer
- [x] **M2** Scoring engine wired into the results screen
- [x] **M3** xUnit tests for WPM, accuracy, and edge cases
- [ ] **M4** Leaderboard API: ASP.NET Core + EF Core + SQLite
- [ ] **M5** Connect the client to the API
- [x] **M6** Styling, difficulty levels, reduced-motion support
- [ ] **M7** Deploy to Azure App Service (public URL)

## Why this project

Qwertide demonstrates the full C#/.NET stack in one deployed app: a Blazor
WebAssembly front-end, an ASP.NET Core API, EF Core persistence with migrations,
a unit-tested domain layer, and a public Azure deployment, all under version
control with a clean history.
