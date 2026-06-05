using Qwertide.Api.Models;

namespace Qwertide.Api.Data;

/// <summary>
/// Seeds a handful of believable leaderboard entries the first time the database
/// comes up empty, so a fresh clone or deploy has something to render instead of
/// a blank table. Real player scores are added on top through POST /api/scores.
/// </summary>
public static class DbSeeder
{
    public static void Seed(QwertideDbContext db)
    {
        if (db.Scores.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;
        db.Scores.AddRange(
            new Score { PlayerName = "kayl_okafor", Wpm = 138, Accuracy = 98.1, DurationSecs = 21.4, CreatedAtUtc = now.AddDays(-2) },
            new Score { PlayerName = "m.santoro",   Wpm = 121, Accuracy = 96.7, DurationSecs = 24.9, CreatedAtUtc = now.AddDays(-5) },
            new Score { PlayerName = "ferra",       Wpm = 117, Accuracy = 99.2, DurationSecs = 25.8, CreatedAtUtc = now.AddHours(-9) },
            new Score { PlayerName = "noah_si",     Wpm = 104, Accuracy = 94.3, DurationSecs = 29.1, CreatedAtUtc = now.AddDays(-1) },
            new Score { PlayerName = "tunde.dev",   Wpm = 99,  Accuracy = 97.5, DurationSecs = 30.6, CreatedAtUtc = now.AddDays(-3) },
            new Score { PlayerName = "p_renaud",    Wpm = 92,  Accuracy = 95.0, DurationSecs = 32.8, CreatedAtUtc = now.AddHours(-30) },
            new Score { PlayerName = "isla.k",      Wpm = 86,  Accuracy = 98.8, DurationSecs = 35.2, CreatedAtUtc = now.AddDays(-6) });

        db.SaveChanges();
    }
}
