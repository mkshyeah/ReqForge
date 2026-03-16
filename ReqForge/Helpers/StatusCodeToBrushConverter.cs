using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ReqForge.Helpers;

public class StatusCodeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var statusText = value?.ToString() ?? "";

        // Пытаемся вытащить числовой код после "Status:"
        int code = 0;
        var parts = statusText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // ожидаем что формát: "Status:" "404" "(NotFound)" ...
        if (parts.Length >= 2 && int.TryParse(parts[1], out var parsed))
            code = parsed;

        if (code >= 200 && code < 300)
            return new SolidColorBrush(Color.FromRgb(73, 190, 109));    // зелёный

        if (code >= 300 && code < 400)
            return new SolidColorBrush(Color.FromRgb(255, 163, 26));   // оранжевый

        if (code >= 400 && code < 600 || statusText.Contains("Ошибка"))
            return new SolidColorBrush(Color.FromRgb(235, 87, 87));    // красный

        return new SolidColorBrush(Color.FromRgb(158, 158, 158));      // серый по умолчанию
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}