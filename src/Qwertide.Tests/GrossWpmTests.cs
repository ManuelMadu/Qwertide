using FluentAssertions;
using Qwertide.Client.Services;

namespace Qwertide.Tests;

public class GrossWpmTests
{
    [Fact]
    public void Standard_run_uses_five_chars_per_word()
    {
        // 50 chars in 60s -> (50 / 5) / 1 minute = 10 WPM
        TypingSession.GrossWpmFor(charsTyped: 50, elapsedSeconds: 60)
            .Should().Be(10);
    }

    [Theory]
    [InlineData(100, 60, 20)]   // (100/5)/1
    [InlineData(250, 60, 50)]   // (250/5)/1
    [InlineData(100, 30, 40)]   // (100/5)/0.5
    [InlineData(5, 60, 1)]      // one word in a minute
    public void Scales_with_chars_and_time(int chars, double seconds, double expectedWpm)
    {
        TypingSession.GrossWpmFor(chars, seconds).Should().Be(expectedWpm);
    }

    [Fact]
    public void Zero_elapsed_time_returns_zero_not_infinity()
    {
        // zero-time edge case: guards the divide-by-zero
        TypingSession.GrossWpmFor(charsTyped: 50, elapsedSeconds: 0)
            .Should().Be(0);
    }

    [Fact]
    public void Empty_input_returns_zero()
    {
        // empty-input edge case
        TypingSession.GrossWpmFor(charsTyped: 0, elapsedSeconds: 60)
            .Should().Be(0);
    }

    [Theory]
    [InlineData(-10, 60)]
    [InlineData(50, -60)]
    public void Negative_inputs_return_zero(int chars, double seconds)
    {
        TypingSession.GrossWpmFor(chars, seconds).Should().Be(0);
    }
}
