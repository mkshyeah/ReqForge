using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ReqForge.Helpers;

public class MethodToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "GET" => new SolidColorBrush(Color.FromRgb(73, 190, 109)),
            "POST" => new SolidColorBrush(Color.FromRgb(255, 163, 26)),
            "PUT" => new SolidColorBrush(Color.FromRgb(66, 133, 244)),
            "DELETE" => new SolidColorBrush(Color.FromRgb(235, 87, 87)),
            "PATCH" => new SolidColorBrush(Color.FromRgb(155, 89, 182)),
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}