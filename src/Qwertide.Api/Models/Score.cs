namespace Qwertide.Api.Models;

/// <summary>
/// A persisted leaderboard entry (PDD §7). This is the EF Core entity; the
/// Blazor client mirrors its shape in its own <c>Score</c> model so the two
/// stay in lockstep without sharing a project.
/// </summary>
public sealed class Score
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = "";
    public int Wpm { get; set; }
    public double Accuracy { get; set; }
    public double DurationSecs { get; set; }
    public int? PassageId { get; set; }

    /// <summary>Server-set on insert; never trusted from the client.</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
