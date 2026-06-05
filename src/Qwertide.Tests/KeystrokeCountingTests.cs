using FluentAssertions;
using Qwertide.Client.Services;

namespace Qwertide.Tests;

public class KeystrokeCountingTests
{
    private const string Target = "the quick brown fox";

    [Fact]
    public void Single_correct_append_counts_one_correct_keystroke()
    {
        TypingSession.CountKeystrokes(previousTyped: "the", newValue: "the ", Target)
            .Should().Be((1, 1));
    }

    [Fact]
    public void Single_wrong_append_counts_a_keystroke_but_not_a_correct_one()
    {
        // "the" + 'X' where the expected char is a space
        TypingSession.CountKeystrokes(previousTyped: "the", newValue: "theX", Target)
            .Should().Be((1, 0));
    }

    [Fact]
    public void Multiple_chars_in_one_event_are_all_counted()
    {
        // Regression test for the deferred finding: a fast typist can commit several
        // characters between input ticks. The old code only counted when exactly one
        // char was appended, so these would advance the passage uncounted and leave
        // the accuracy denominator short. All four must count here.
        TypingSession.CountKeystrokes(previousTyped: "the ", newValue: "the quic", Target)
            .Should().Be((4, 4));
    }

    [Fact]
    public void Multiple_chars_in_one_event_count_their_errors_too()
    {
        // appends "Xui" against expected "qui" -> 3 keystrokes, 2 correct
        TypingSession.CountKeystrokes(previousTyped: "the ", newValue: "the Xui", Target)
            .Should().Be((3, 2));
    }

    [Fact]
    public void Backspace_counts_nothing()
    {
        TypingSession.CountKeystrokes(previousTyped: "the ", newValue: "the", Target)
            .Should().Be((0, 0));
    }

    [Fact]
    public void Unchanged_value_counts_nothing()
    {
        TypingSession.CountKeystrokes(previousTyped: "the", newValue: "the", Target)
            .Should().Be((0, 0));
    }

    [Fact]
    public void Non_append_edit_that_does_not_continue_prior_text_counts_nothing()
    {
        // value grew but does not start with what was already typed (e.g. a jump/paste
        // replacing the field) -> not real forward keystrokes
        TypingSession.CountKeystrokes(previousTyped: "the", newValue: "xyz!", Target)
            .Should().Be((0, 0));
    }

    [Fact]
    public void Characters_past_the_end_of_the_target_are_ignored()
    {
        var target = "abcd";
        // "ab" -> "abcdef": positions 2,3 are inside the target ('c','d'); 4,5 overflow
        TypingSession.CountKeystrokes(previousTyped: "ab", newValue: "abcdef", target)
            .Should().Be((2, 2));
    }

    [Fact]
    public void Append_starting_at_the_target_end_counts_nothing()
    {
        var target = "abc";
        TypingSession.CountKeystrokes(previousTyped: "abc", newValue: "abcd", target)
            .Should().Be((0, 0));
    }
}
