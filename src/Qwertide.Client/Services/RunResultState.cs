using Qwertide.Client.Models;

namespace Qwertide.Client.Services;

/// <summary>
/// Hands the finished run from Play -> Results (scoped, so it lives for the
/// session). Null when the user lands on Results without playing.
/// </summary>
public sealed class RunResultState
{
    public int Wpm { get; set; }
    public double Accuracy { get; set; }
    public double DurationSecs { get; set; }
    public int Errors { get; set; }
    public int CharCount { get; set; }
    public int PassageId { get; set; }
    public bool HasResult { get; set; }

    /// <summary>Id of the player's most recent saved run, so the leaderboard can mark "you".</summary>
    public int? LastSavedScoreId { get; set; }

    public void Clear() => HasResult = false;
}
