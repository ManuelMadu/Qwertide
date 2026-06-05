using System.ComponentModel.DataAnnotations;

namespace Qwertide.Api.Models;

/// <summary>
/// The shape a client is allowed to submit on POST /api/scores. Server-owned
/// fields (Id, CreatedAtUtc) are deliberately absent so they cannot be spoofed;
/// validation attributes are enforced by [ApiController] model binding.
/// </summary>
public sealed class ScoreRequest
{
    [Required]
    [StringLength(30, MinimumLength = 1)]
    public string PlayerName { get; set; } = "";

    [Range(0, 500)]
    public int Wpm { get; set; }

    [Range(0, 100)]
    public double Accuracy { get; set; }

    [Range(0, double.MaxValue)]
    public double DurationSecs { get; set; }

    public int? PassageId { get; set; }
}
