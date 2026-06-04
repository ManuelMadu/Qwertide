namespace Qwertide.Client.Models;

public enum Difficulty
{
    Short,   // a single crisp sentence
    Medium,  // a short paragraph
    Code     // a snippet, punctuation-heavy
}

/// <summary>A block of text the player types.</summary>
public sealed class Passage
{
    public int Id { get; init; }
    public string Text { get; init; } = "";
    public Difficulty Difficulty { get; init; }
    public string Source { get; init; } = "";
}
