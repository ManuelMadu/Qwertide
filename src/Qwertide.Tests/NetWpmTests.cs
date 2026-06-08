using FluentAssertions;
using Qwertide.Client.Services;

namespace Qwertide.Tests;

public class NetWpmTests
{
    [Fact]
    public void Perfect_accuracy_leaves_speed_untouched()
    {
        // 100% accuracy is a x1 weight: net == gross
        TypingSession.NetWpmFor(grossWpm: 90, accuracy: 100)
            .Should().Be(90);
    }

    [Theory]
    [InlineData(130, 55, 71.5)]   // the key-masher: fast but sloppy
    [InlineData(95, 99, 94.05)]   // the careful typist: nearly all of their speed survives
    [InlineData(100, 50, 50)]     // half the keys wrong -> half the score
    public void Speed_is_weighted_by_accuracy(double gross, double accuracy, double expected)
    {
        TypingSession.NetWpmFor(gross, accuracy)
            .Should().BeApproximately(expected, 0.001);
    }

    [Fact]
    public void Careful_typist_outranks_the_masher()
    {
        // The whole point: lower gross speed can still net higher when accurate.
        var masher = TypingSession.NetWpmFor(grossWpm: 130, accuracy: 55);
        var careful = TypingSession.NetWpmFor(grossWpm: 95, accuracy: 99);

        careful.Should().BeGreaterThan(masher);
    }

    [Theory]
    [InlineData(0, 100)]    // no speed
    [InlineData(90, 0)]     // nothing correct
    [InlineData(-10, 100)]  // guard against negatives
    [InlineData(90, -5)]
    public void Non_positive_inputs_return_zero(double gross, double accuracy)
    {
        TypingSession.NetWpmFor(gross, accuracy).Should().Be(0);
    }
}
