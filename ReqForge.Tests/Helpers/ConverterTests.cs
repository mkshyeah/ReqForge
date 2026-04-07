using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ReqForge.Helpers;

namespace ReqForge.Tests.Helpers;

public class MethodToColorConverterTests
{
    private readonly MethodToColorConverter _converter = new();

    [Theory]
    [InlineData("GET", 73, 190, 109)]
    [InlineData("POST", 255, 163, 26)]
    [InlineData("PUT", 66, 133, 244)]
    [InlineData("DELETE", 235, 87, 87)]
    [InlineData("PATCH", 155, 89, 182)]
    public void Convert_HttpMethod_ReturnsCorrectColor(string method, byte r, byte g, byte b)
    {
        var result = _converter.Convert(method, typeof(Brush), null!, CultureInfo.InvariantCulture);

        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(r, g, b), brush.Color);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("OPTIONS")]
    [InlineData("HEAD")]
    public void Convert_UnknownMethod_ReturnsGray(string? method)
    {
        var result = _converter.Convert(method!, typeof(Brush), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Brushes.Gray, result);
    }
}

public class BoolToTestResultBrushConverterTests
{
    private readonly BoolToTestResultBrushConverter _converter = new();

    [Fact]
    public void Convert_True_ReturnsGreen()
    {
        var result = _converter.Convert(true, typeof(Brush), null!, CultureInfo.InvariantCulture);

        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(76, 175, 80), brush.Color);
    }

    [Fact]
    public void Convert_False_ReturnsRed()
    {
        var result = _converter.Convert(false, typeof(Brush), null!, CultureInfo.InvariantCulture);

        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(244, 67, 54), brush.Color);
    }

    [Fact]
    public void Convert_Null_ReturnsRed()
    {
        var result = _converter.Convert(null!, typeof(Brush), null!, CultureInfo.InvariantCulture);

        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(244, 67, 54), brush.Color);
    }
}

public class NullToBoolConverterTests
{
    private readonly NullToBoolConverter _converter = new();

    [Fact]
    public void Convert_NonNull_ReturnsTrue()
    {
        var result = _converter.Convert("something", typeof(bool), null!, CultureInfo.InvariantCulture);

        Assert.Equal(true, result);
    }

    [Fact]
    public void Convert_Null_ReturnsFalse()
    {
        var result = _converter.Convert(null!, typeof(bool), null!, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }
}

public class InverseBoolToVisibilityConverterTests
{
    private readonly InverseBoolToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_True_ReturnsCollapsed()
    {
        var result = _converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_False_ReturnsVisible()
    {
        var result = _converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_NonBool_ReturnsVisible()
    {
        var result = _converter.Convert("not a bool", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }
}

public class StringMatchToVisibilityConverterTests
{
    private readonly StringMatchToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_ExactMatch_ReturnsVisible()
    {
        var result = _converter.Convert("json", typeof(Visibility), "json", CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_CaseInsensitive_ReturnsVisible()
    {
        var result = _converter.Convert("JSON", typeof(Visibility), "json", CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_PipeDelimited_MatchesAny()
    {
        var result = _converter.Convert("raw", typeof(Visibility), "json|raw", CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_NoMatch_ReturnsCollapsed()
    {
        var result = _converter.Convert("form-data", typeof(Visibility), "json|raw", CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_NullValue_ReturnsCollapsed()
    {
        var result = _converter.Convert(null!, typeof(Visibility), "json", CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_NullParameter_ReturnsCollapsed()
    {
        var result = _converter.Convert("json", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Collapsed, result);
    }
}

public class StatusCodeToBrushConverterTests
{
    private readonly StatusCodeToBrushConverter _converter = new();

    [Theory]
    [InlineData("Status: 200 (OK)", 73, 190, 109)]
    [InlineData("Status: 201 (Created)", 73, 190, 109)]
    [InlineData("Status: 301 (MovedPermanently)", 255, 163, 26)]
    [InlineData("Status: 404 (NotFound)", 235, 87, 87)]
    [InlineData("Status: 500 (InternalServerError)", 235, 87, 87)]
    public void Convert_StatusText_ReturnsCorrectColor(string statusText, byte r, byte g, byte b)
    {
        var result = _converter.Convert(statusText, typeof(Brush), null!, CultureInfo.InvariantCulture);

        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(r, g, b), brush.Color);
    }

    [Fact]
    public void Convert_ErrorText_ReturnsRed()
    {
        var result = _converter.Convert("Ошибка подключения", typeof(Brush), null!, CultureInfo.InvariantCulture);

        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(235, 87, 87), brush.Color);
    }

    [Fact]
    public void Convert_EmptyString_ReturnsGray()
    {
        var result = _converter.Convert("", typeof(Brush), null!, CultureInfo.InvariantCulture);

        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(158, 158, 158), brush.Color);
    }
}
