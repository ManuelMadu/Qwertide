using FluentAssertions;
using Qwertide.Client.Services;

namespace Qwertide.Tests;

public class AccuracyTests
{
    [Fact]
    public void All_correct_is_one_hundred_percent()
    {
        TypingSession.AccuracyFor(correctKeystrokes: 40, totalKeystrokes: 40)
            .Should().Be(100);
    }

    [Fact]
    public void All_errors_is_zero_percent()
    {
        // all-errors edge case: zero correct over a non-zero denominator
        TypingSession.AccuracyFor(correctKeystrokes: 0, totalKeystrokes: 40)
            .Should().Be(0);
    }

    [Theory]
    [InlineData(1, 2, 50)]
    [InlineData(3, 4, 75)]
    [InlineData(9, 10, 90)]
    public void Is_correct_over_total_as_a_percentage(int correct, int total, double expected)
    {
        TypingSession.AccuracyFor(correct, total).Should().Be(expected);
    }

    [Fact]
    public void Zero_keystrokes_returns_zero_not_NaN()
    {
        // empty-input edge case: guards 0/0
        TypingSession.AccuracyFor(correctKeystrokes: 0, totalKeystrokes: 0)
            .Should().Be(0);
    }
}
