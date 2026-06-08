using System.ComponentModel.DataAnnotations.Schema;

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

    /// <summary>
    /// Speed weighted by accuracy - the leaderboard's ranking metric. Derived from
    /// <see cref="Wpm"/> and <see cref="Accuracy"/> rather than stored, so it stays
    /// consistent and needs no schema change; it serializes into the API response.
    /// </summary>
    [NotMapped]
    public double NetWpm => Wpm * Accuracy / 100.0;

    /// <summary>Server-set on insert; never trusted from the client.</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
