# Qwertide - Design Notes (terminal-mono)

The frontend design layer for Qwertide, built directly in the real stack
(Blazor WebAssembly + MudBlazor). This advances PDD milestones **M1** (core game
shell), **M6** (styling), and the four screens in **§8**.

## Design read

> A focused single-player typing game (product UI, desktop-first) for recruiters
> evaluating a junior C#/.NET dev, with a dark terminal-mono language, built on
> Blazor WASM + MudBlazor with a custom dark theme and restrained keystroke motion.

Dials (overriding the 8/6/4 baseline because a typing test is a focus surface,
not a marketing page):

| Dial | Value | Why |
| --- | --- | --- |
| `DESIGN_VARIANCE` | 5 | The passage *is* the hero and must be centered for typing focus. Anti-center bias deliberately overridden. |
| `MOTION_INTENSITY` | 6 | Tactile micro-feedback: caret blink, live counters, in-place correct/incorrect colouring. All gated behind `prefers-reduced-motion`. |
| `VISUAL_DENSITY` | 3 | Airy game screen; `font-mono` + tabular-nums for every number. |

Reference anchor: Monkeytype (adjacent, not cloned).

## Locks (audited, per the anti-slop pre-flight)

- **Theme lock:** dark only, page-wide. No section inverts.
- **Color lock:** ONE accent, amber `#e2b340` (caret, primary CTA, active stat).
  Red `#e5484d` is reserved strictly for typing errors (real semantic state),
  never decoration. No second accent anywhere.
- **Shape lock:** interactive = 4px, panels = 8px, caret = 1px. One scale.
- **Type:** JetBrains Mono (passage, numbers, meta) + Space Grotesk (UI chrome).
  Inter avoided. Em-dash: zero on the page (hyphens only).
- **No AI tells:** no version eyebrows, no `00 / INDEX` section numbers, no scroll
  cues, no decorative dots, no locale/time strips, no div-based fake screenshots,
  no per-row top+bottom hairlines, believable handles + organic numbers.

## Files

```
src/Qwertide.Client/
  wwwroot/
    index.html                 fonts (JetBrains Mono + Space Grotesk), theme link
    css/qwertide.css           the design system (tokens, caret, stat bar, tables)
  Theme/QwertideTheme.cs        MudTheme mapped to the Qwertide tokens (no Material default)
  Layout/MainLayout.razor       terminal shell: nav (<=64px, one line) + footer
  Models/Score.cs, Passage.cs   mirror the future EF Core entities (PDD §7)
  Services/
    TypingSession.cs            pure scoring engine, UI-free, unit-test surface (PDD §5.4)
    PassageLibrary.cs           static passages for v1
    ILeaderboardService.cs      contract the API will implement at M5
    LocalLeaderboardService.cs  localStorage-backed stub (scores persist across refresh)
    RunResultState.cs           carries the finished run Play -> Results
  Components/
    PassageView.razor           per-glyph render + caret (pure)
    StatBar.razor               live WPM / accuracy / time
  Pages/
    Home.razor                  start screen: hero + length selector + CTA
    Play.razor                  the game: keystroke capture, live caret + stats
    Results.razor               big readout + save-to-leaderboard + play again
    Leaderboard.razor           top runs (skeleton loading + empty state)
```

## Decisions made (flag for your sign-off)

1. **Error model (resolves PDD §12 open question).** Implemented *type-past-errors*:
   wrong characters are marked red in place and counted as errors; the player is
   not forced to fix them to advance. This matches the in-place red highlighting
   the UI is built around and the common typing-test convention. The PDD draft
   leaned toward "require correct char to advance" - if you want that instead,
   it is a small change in `Play.OnInput`. See the comment in `TypingSession.cs`.

2. **Leaderboard is a localStorage stub for now.** It satisfies the "scores survive
   refresh" goal at the client level and gives the Leaderboard screen real,
   persistent data to render. M4/M5 swap `LocalLeaderboardService` for an
   `HttpClient`-backed one behind the same `ILeaderboardService` - no UI changes.

3. **Fonts via Google Fonts `<link>`** for dev convenience. For production, self-host
   JetBrains Mono + Space Grotesk (fontsource) to hit the perf budget. Noted in
   `index.html`.

## Run it

```bash
cd src/Qwertide.Client
dotnet run
# open the printed http://localhost:xxxx URL
```
