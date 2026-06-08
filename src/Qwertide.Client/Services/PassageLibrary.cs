using Qwertide.Client.Models;

namespace Qwertide.Client.Services;

/// <summary>
/// Static passage source for v1 (PDD §6). Bundled in the client; a random
/// /api/passages endpoint is a stretch goal. Passages are deliberately ordinary
/// prose and real code, not motivational filler.
/// </summary>
public sealed class PassageLibrary
{
    private readonly Random _rng = new();

    private readonly Passage[] _passages =
    {
        new()
        {
            Id = 1, Difficulty = Difficulty.Short, Source = "warmup",
            Text = "The keyboard remembers what the hands forget."
        },
        new()
        {
            Id = 2, Difficulty = Difficulty.Short, Source = "warmup",
            Text = "Type the line, not the letters, and the speed arrives on its own."
        },
        new()
        {
            Id = 3, Difficulty = Difficulty.Medium, Source = "prose",
            Text = "A good typist does not chase the next key. They settle into a rhythm, " +
                   "let the words come in clean groups, and trust the fingers to land where " +
                   "they have landed a thousand times before."
        },
        new()
        {
            Id = 4, Difficulty = Difficulty.Medium, Source = "prose",
            Text = "Most of the time the cursor sits still, waiting, and the only sound in " +
                   "the room is the small click of a thought turning into a sentence that " +
                   "someone, somewhere, will eventually read."
        },
        new()
        {
            Id = 5, Difficulty = Difficulty.Code, Source = "csharp",
            Text = "public static double GrossWpm(int chars, double secs) => secs <= 0 ? 0 : (chars / 5.0) / (secs / 60.0);"
        },
        new()
        {
            Id = 6, Difficulty = Difficulty.Code, Source = "csharp",
            Text = "var top = scores.OrderByDescending(s => s.Wpm).Take(10).ToList();"
        },
    };

    public Passage Random(Difficulty difficulty)
    {
        var pool = _passages.Where(p => p.Difficulty == difficulty).ToArray();
        if (pool.Length == 0)
        {
            pool = _passages;
        }

        return pool[_rng.Next(pool.Length)];
    }

    public Passage ById(int id) => _passages.FirstOrDefault(p => p.Id == id) ?? _passages[0];
}
