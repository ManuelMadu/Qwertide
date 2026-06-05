using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwertide.Api.Data;
using Qwertide.Api.Models;

namespace Qwertide.Api.Controllers;

/// <summary>
/// Leaderboard endpoints (PDD §6):
///   GET  /api/scores?top=10  - top N entries by WPM, then accuracy
///   POST /api/scores         - submit a new score
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ScoresController : ControllerBase
{
    private const int MaxTop = 100;

    private readonly QwertideDbContext _db;

    public ScoresController(QwertideDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Score>>> GetTop([FromQuery] int top = 10)
    {
        top = Math.Clamp(top, 1, MaxTop);

        var scores = await _db.Scores
            .OrderByDescending(s => s.Wpm)
            .ThenByDescending(s => s.Accuracy)
            .ThenBy(s => s.DurationSecs)
            .Take(top)
            .ToListAsync();

        return Ok(scores);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Score>> GetById(int id)
    {
        var score = await _db.Scores.FindAsync(id);
        return score is null ? NotFound() : Ok(score);
    }

    [HttpPost]
    public async Task<ActionResult<Score>> Submit([FromBody] ScoreRequest request)
    {
        var score = new Score
        {
            PlayerName = request.PlayerName.Trim(),
            Wpm = request.Wpm,
            Accuracy = request.Accuracy,
            DurationSecs = request.DurationSecs,
            PassageId = request.PassageId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        _db.Scores.Add(score);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = score.Id }, score);
    }
}
