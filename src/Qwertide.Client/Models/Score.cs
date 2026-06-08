namespace Qwertide.Client.Models;

/// <summary>
/// A single leaderboard entry. Mirrors the API's Score shape (PDD §7) so the
/// client model and the future EF Core entity stay in lockstep.
/// </summary>
public sealed class Score
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = "";
    public int Wpm { get; set; }
    public double Accuracy { get; set; }
    public double DurationSecs { get; set; }
    public int? PassageId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Speed weighted by accuracy - the leaderboard's ranking metric.</summary>
    public double NetWpm => Wpm * Accuracy / 100.0;
}
