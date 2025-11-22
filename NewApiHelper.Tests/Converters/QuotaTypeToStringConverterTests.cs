using NewApiHelper.Converters;
using NewApiHelper.Models;
using System.Globalization;

namespace NewApiHelper.Tests.Converters;

public class QuotaTypeToStringConverterTests
{
    private readonly QuotaTypeToStringConverter _converter = new();

    [Theory]
    [InlineData(QuotaType.PayAsYouGo, "按量付费")]
    [InlineData(QuotaType.PayPerUse, "按次计费")]
    [InlineData((QuotaType)999, "不支持")]
    public void Convert_ValidQuotaType_ReturnsCorrectString(QuotaType quotaType, string expected)
    {
        // Act
        var result = _converter.Convert(quotaType, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_NullValue_ReturnsUnknown()
    {
        // Act
        var result = _converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("未知", result);
    }

    [Fact]
    public void Convert_InvalidType_ReturnsUnknown()
    {
        // Act
        var result = _converter.Convert("invalid", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("未知", result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack("test", typeof(QuotaType), null!, CultureInfo.InvariantCulture));
    }
}