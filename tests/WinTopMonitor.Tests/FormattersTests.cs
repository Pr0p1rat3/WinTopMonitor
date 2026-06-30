using WinTopMonitor.Utils;

namespace WinTopMonitor.Tests;

public sealed class FormattersTests
{
    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(1073741824, "1 GB")]
    public void BytesFormatsHumanReadableValues(double bytes, string expected)
    {
        Assert.Equal(expected, Formatters.Bytes(bytes));
    }

    [Fact]
    public void PercentFormatsOneDecimalPlace()
    {
        Assert.Equal("42.1%", Formatters.Percent(42.12));
    }
}

