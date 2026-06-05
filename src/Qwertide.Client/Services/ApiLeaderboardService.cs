using System.Net.Http.Json;
using Qwertide.Client.Models;

namespace Qwertide.Client.Services;

/// <summary>
/// Leaderboard backed by the ASP.NET Core API (M5). Talks to GET/POST
/// /api/scores over HTTP and implements the same <see cref="ILeaderboardService"/>
/// the UI already codes against, so swapping it in for the localStorage stub
/// needs no changes to any page or component.
/// </summary>
public sealed class ApiLeaderboardService : ILeaderboardService
{
    private readonly HttpClient _http;

    public ApiLeaderboardService(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<Score>> GetTopAsync(int top = 10)
    {
        var scores = await _http.GetFromJsonAsync<List<Score>>($"api/scores?top={top}");
        return scores ?? new List<Score>();
    }

    public async Task<Score> SubmitAsync(Score score)
    {
        // The API binds the body to its ScoreRequest DTO and ignores the
        // client-only Id/CreatedAtUtc fields, then returns the persisted row
        // (with the server-assigned Id) which the Results page uses to mark
        // "this is you" on the leaderboard.
        var response = await _http.PostAsJsonAsync("api/scores", score);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<Score>();
        return created ?? score;
    }
}
