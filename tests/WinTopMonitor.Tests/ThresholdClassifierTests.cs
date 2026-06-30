using WinTopMonitor.Config;
using WinTopMonitor.Models;
using WinTopMonitor.Utils;

namespace WinTopMonitor.Tests;

public sealed class ThresholdClassifierTests
{
    [Theory]
    [InlineData(25, ThresholdLevel.Normal)]
    [InlineData(70, ThresholdLevel.Warning)]
    [InlineData(90, ThresholdLevel.Critical)]
    public void ClassifyReturnsExpectedLevel(double value, ThresholdLevel expected)
    {
        var threshold = new ResourceThreshold { Warning = 70, Critical = 90 };

        var actual = ThresholdClassifier.Classify(value, threshold);

        Assert.Equal(expected, actual);
    }
}

