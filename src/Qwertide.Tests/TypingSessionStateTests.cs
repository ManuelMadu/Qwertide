using FluentAssertions;
using Qwertide.Client.Services;

namespace Qwertide.Tests;

public class TypingSessionStateTests
{
    [Fact]
    public void Update_exposes_derived_metrics()
    {
        var session = new TypingSession();
        session.Update(charsTyped: 50, correctKeystrokes: 45, totalKeystrokes: 50, elapsedSeconds: 60);

        session.CharsTyped.Should().Be(50);
        session.GrossWpm.Should().Be(10);     // (50/5)/1
        session.Accuracy.Should().Be(90);     // 45/50
        session.ErrorCount.Should().Be(5);    // 50 - 45
    }

    [Fact]
    public void Update_clamps_negative_inputs_to_zero()
    {
        var session = new TypingSession();
        session.Update(charsTyped: -5, correctKeystrokes: -1, totalKeystrokes: -1, elapsedSeconds: -10);

        session.CharsTyped.Should().Be(0);
        session.CorrectKeystrokes.Should().Be(0);
        session.TotalKeystrokes.Should().Be(0);
        session.ElapsedSeconds.Should().Be(0);
        session.ErrorCount.Should().Be(0);
    }

    [Fact]
    public void A_run_typed_in_bursts_keeps_an_honest_accuracy_denominator()
    {
        // Simulate the input loop: the same string arrives across a few events, some
        // events carrying more than one new character. Every committed key must land
        // in the denominator regardless of how it was chunked.
        const string target = "hello world";
        var typed = "";
        var total = 0;
        var correct = 0;

        foreach (var value in new[] { "he", "hel", "hello ", "hello world" })
        {
            var (added, c) = TypingSession.CountKeystrokes(typed, value, target);
            total += added;
            correct += c;
            typed = value;
        }

        total.Should().Be(target.Length);   // every character counted exactly once
        correct.Should().Be(target.Length); // and all were correct
        TypingSession.AccuracyFor(correct, total).Should().Be(100);
    }
}
