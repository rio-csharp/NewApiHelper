using FluentAssertions;
using NewApiHelper.Utilities.Converters;
using System.Windows;

namespace NewApiHelper.Tests.Utilities.Converters;

public class BooleanToVisibilityConverterTests
{
    private readonly BooleanToVisibilityConverter _converter;

    public BooleanToVisibilityConverterTests()
    {
        _converter = new BooleanToVisibilityConverter();
    }

    [Theory]
    [InlineData(true, Visibility.Visible)]
    [InlineData(false, Visibility.Collapsed)]
    public void Convert_ShouldReturnCorrectVisibility_ForBooleanValue(bool input, Visibility expected)
    {
        // Act
        var result = _converter.Convert(input, typeof(Visibility), null, null);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_ShouldReturnCollapsed_ForNonBooleanValue()
    {
        // Act
        var result = _converter.Convert("not a boolean", typeof(Visibility), null!, null!);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_ShouldThrowNotImplementedException()
    {
        // Act & Assert
        _converter.Invoking(c => c.ConvertBack(null!, typeof(bool), null!, null!))
            .Should().Throw<NotImplementedException>();
    }
}