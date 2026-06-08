using System.Text.Json;
using Microsoft.JSInterop;
using Qwertide.Client.Models;

namespace Qwertide.Client.Services;

/// <summary>
/// DESIGN-LAYER STUB leaderboard backed by browser localStorage, so scores
/// survive refresh and the leaderboard screen has real, persistent data to
/// render. Seeded once with believable entries. Swap for an API-backed
/// implementation at M5 (same interface, no UI changes).
/// </summary>
public sealed class LocalLeaderboardService : ILeaderboardService
{
    private const string Key = "qwertide.scores.v1";
    private readonly IJSRuntime _js;

    public LocalLeaderboardService(IJSRuntime js) => _js = js;

    // Believable handles + organic numbers (no Jane Doe, no round-number slop).
    private static readonly List<Score> Seed = new()
    {
        new() { Id = 1, PlayerName = "kayl_okafor", Wpm = 138, Accuracy = 98.1, DurationSecs = 21.4, CreatedAtUtc = DateTime.UtcNow.AddDays(-2) },
        new() { Id = 2, PlayerName = "m.santoro",   Wpm = 121, Accuracy = 96.7, DurationSecs = 24.9, CreatedAtUtc = DateTime.UtcNow.AddDays(-5) },
        new() { Id = 3, PlayerName = "ferra",       Wpm = 117, Accuracy = 99.2, DurationSecs = 25.8, CreatedAtUtc = DateTime.UtcNow.AddHours(-9) },
        new() { Id = 4, PlayerName = "noah_si",     Wpm = 104, Accuracy = 94.3, DurationSecs = 29.1, CreatedAtUtc = DateTime.UtcNow.AddDays(-1) },
        new() { Id = 5, PlayerName = "tunde.dev",   Wpm = 99,  Accuracy = 97.5, DurationSecs = 30.6, CreatedAtUtc = DateTime.UtcNow.AddDays(-3) },
        new() { Id = 6, PlayerName = "p_renaud",    Wpm = 92,  Accuracy = 95.0, DurationSecs = 32.8, CreatedAtUtc = DateTime.UtcNow.AddHours(-30) },
        new() { Id = 7, PlayerName = "isla.k",      Wpm = 86,  Accuracy = 98.8, DurationSecs = 35.2, CreatedAtUtc = DateTime.UtcNow.AddDays(-6) },
    };

    private async Task<List<Score>> LoadAsync()
    {
        var raw = await _js.InvokeAsync<string?>("localStorage.getItem", Key);
        if (string.IsNullOrWhiteSpace(raw))
        {
            await SaveAsync(Seed);
            return new List<Score>(Seed);
        }
        try
        {
            return JsonSerializer.Deserialize<List<Score>>(raw) ?? new List<Score>(Seed);
        }
        catch
        {
            return new List<Score>(Seed);
        }
    }

    private async Task SaveAsync(List<Score> scores)
        => await _js.InvokeVoidAsync("localStorage.setItem", Key, JsonSerializer.Serialize(scores));

    public async Task<IReadOnlyList<Score>> GetTopAsync(int top = 10)
    {
        var all = await LoadAsync();
        return all.OrderByDescending(s => s.NetWpm).ThenByDescending(s => s.Accuracy).Take(top).ToList();
    }

    public async Task<Score> SubmitAsync(Score score)
    {
        var all = await LoadAsync();
        score.Id = (all.Count == 0 ? 0 : all.Max(s => s.Id)) + 1;
        score.CreatedAtUtc = DateTime.UtcNow;
        all.Add(score);
        await SaveAsync(all);
        return score;
    }
}
