using FluentAssertions;
using NewApiHelper.Utilities.Converters;
using System.Windows.Media;

namespace NewApiHelper.Tests.Utilities.Converters;

public class StatusToColorConverterTests
{
    private readonly StatusToColorConverter _converter;

    public StatusToColorConverterTests()
    {
        _converter = new StatusToColorConverter();
    }

    [Theory]
    [InlineData(1, "#FF008000")]
    [InlineData(2, "#FFFF0000")]
    [InlineData(0, "#FF808080")]
    [InlineData(3, "#FF808080")]
    [InlineData(-1, "#FF808080")]
    public void Convert_ShouldReturnCorrectBrush_ForStatusValue(int status, string expectedColor)
    {
        // Act
        var result = _converter.Convert(status, typeof(Brush), null!, null!) as SolidColorBrush;

        // Assert
        result.Should().NotBeNull();
        result!.Color.ToString().Should().Be(expectedColor);
    }

    [Fact]
    public void Convert_ShouldReturnGrayBrush_ForNonIntegerValue()
    {
        // Act
        var result = _converter.Convert("not an integer", typeof(Brush), null!, null!) as SolidColorBrush;

        // Assert
        result.Should().NotBeNull();
        result!.Color.Should().Be(Colors.Gray);
    }

    [Fact]
    public void ConvertBack_ShouldThrowNotImplementedException()
    {
        // Act & Assert
        _converter.Invoking(c => c.ConvertBack(null!, typeof(int), null!, null!))
            .Should().Throw<NotImplementedException>();
    }
}