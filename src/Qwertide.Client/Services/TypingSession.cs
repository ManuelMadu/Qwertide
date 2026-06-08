namespace Qwertide.Client.Services;

/// <summary>
/// The scoring engine. Pure C#, zero UI dependencies (PDD §5.4) so it can be
/// unit-tested directly in Qwertide.Tests (M3). The Blazor component feeds it
/// counts + elapsed time and renders what comes back; it owns no rendering.
///
/// Error model (resolves open question PDD §12): the player types FREELY and may
/// type past a wrong character. Wrong characters are marked in place and counted
/// as errors; WPM is gross (every advanced position counts), accuracy is
/// correct keystrokes over total keystrokes. This matches the in-place red
/// highlighting the UI is designed around, and the common typing-test convention.
/// </summary>
public sealed class TypingSession
{
    /// <summary>Number of glyph positions advanced through the passage.</summary>
    public int CharsTyped { get; private set; }

    /// <summary>Every key the player committed, including corrections of mistakes.</summary>
    public int TotalKeystrokes { get; private set; }

    /// <summary>Keystrokes that matched the expected character at that position.</summary>
    public int CorrectKeystrokes { get; private set; }

    public double ElapsedSeconds { get; private set; }

    /// <summary>Standard convention: one "word" = 5 characters.</summary>
    public const double CharsPerWord = 5.0;

    /// <summary>Gross WPM. Guards divide-by-zero (zero-time edge case).</summary>
    public double GrossWpm => GrossWpmFor(CharsTyped, ElapsedSeconds);

    /// <summary>Accuracy as a percentage 0-100. Guards zero keystrokes.</summary>
    public double Accuracy => AccuracyFor(CorrectKeystrokes, TotalKeystrokes);

    public int ErrorCount => TotalKeystrokes - CorrectKeystrokes;

    /// <summary>Feed the live state computed by the component each input tick.</summary>
    public void Update(int charsTyped, int correctKeystrokes, int totalKeystrokes, double elapsedSeconds)
    {
        CharsTyped = Math.Max(0, charsTyped);
        CorrectKeystrokes = Math.Max(0, correctKeystrokes);
        TotalKeystrokes = Math.Max(0, totalKeystrokes);
        ElapsedSeconds = Math.Max(0, elapsedSeconds);
    }

    // ---- Pure functions (the unit-test surface) ----------------------------

    public static double GrossWpmFor(int charsTyped, double elapsedSeconds)
    {
        if (elapsedSeconds <= 0 || charsTyped <= 0)
        {
            return 0;
        }

        var minutes = elapsedSeconds / 60.0;
        return (charsTyped / CharsPerWord) / minutes;
    }

    public static double AccuracyFor(int correctKeystrokes, int totalKeystrokes)
    {
        if (totalKeystrokes <= 0)
        {
            return 0;
        }

        return (double)correctKeystrokes / totalKeystrokes * 100.0;
    }

    /// <summary>
    /// Keystroke accounting for one input event. Counts EVERY new character appended
    /// to <paramref name="previousTyped"/> in this event - not just one - comparing
    /// each against the expected character in <paramref name="target"/>. A fast typist
    /// (or autocorrect / IME) can commit several characters between input ticks; each
    /// of those is a real keystroke and must count toward the accuracy denominator.
    ///
    /// Only a forward extension of what was already typed counts: backspaces (the value
    /// shrank) and edits that don't continue the prior text contribute nothing. Positions
    /// past the end of <paramref name="target"/> are ignored. Returns the keystrokes to
    /// add to the running totals.
    /// </summary>
    public static (int Total, int Correct) CountKeystrokes(string previousTyped, string newValue, string target)
    {
        if (newValue.Length <= previousTyped.Length)
        {
            return (0, 0);
        }

        if (!newValue.StartsWith(previousTyped, StringComparison.Ordinal))
        {
            return (0, 0);
        }

        var end = Math.Min(newValue.Length, target.Length);
        var total = 0;
        var correct = 0;
        for (var pos = previousTyped.Length; pos < end; pos++)
        {
            total++;
            if (newValue[pos] == target[pos])
            {
                correct++;
            }
        }
        return (total, correct);
    }
}
