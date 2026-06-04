using Qwertide.Client.Models;

namespace Qwertide.Client.Services;

/// <summary>
/// Leaderboard contract. The client codes against this interface so M4/M5 can
/// drop in an HttpClient-backed implementation hitting the ASP.NET Core API
/// (GET /api/scores?top=10, POST /api/scores) without touching the UI.
/// </summary>
public interface ILeaderboardService
{
    Task<IReadOnlyList<Score>> GetTopAsync(int top = 10);
    Task<Score> SubmitAsync(Score score);
}
