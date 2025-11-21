using FluentAssertions;
using NewApiHelper.Utilities.Converters;
using System.Windows;

namespace NewApiHelper.Tests.Utilities.Converters;

public class InvertedBooleanToVisibilityConverterTests
{
    private readonly InvertedBooleanToVisibilityConverter _converter;

    public InvertedBooleanToVisibilityConverterTests()
    {
        _converter = new InvertedBooleanToVisibilityConverter();
    }

    [Theory]
    [InlineData(true, Visibility.Collapsed)]
    [InlineData(false, Visibility.Visible)]
    public void Convert_ShouldReturnCorrectVisibility_ForBooleanValue(bool input, Visibility expected)
    {
        // Act
        var result = _converter.Convert(input, typeof(Visibility), null, null);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_ShouldReturnVisible_ForNonBooleanValue()
    {
        // Act
        var result = _converter.Convert("not a boolean", typeof(Visibility), null!, null!);

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void ConvertBack_ShouldThrowNotImplementedException()
    {
        // Act & Assert
        _converter.Invoking(c => c.ConvertBack(null!, typeof(bool), null!, null!))
            .Should().Throw<NotImplementedException>();
    }
}