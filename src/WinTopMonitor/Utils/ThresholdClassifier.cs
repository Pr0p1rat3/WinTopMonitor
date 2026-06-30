using WinTopMonitor.Config;
using WinTopMonitor.Models;

namespace WinTopMonitor.Utils;

public static class ThresholdClassifier
{
    public static ThresholdLevel Classify(double value, ResourceThreshold threshold)
    {
        if (value >= threshold.Critical)
        {
            return ThresholdLevel.Critical;
        }

        if (value >= threshold.Warning)
        {
            return ThresholdLevel.Warning;
        }

        return ThresholdLevel.Normal;
    }
}

