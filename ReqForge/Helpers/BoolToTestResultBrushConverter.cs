using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ReqForge.Helpers;

public class BoolToTestResultBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true
            ? new SolidColorBrush(Color.FromRgb(76, 175, 80))   // green
            : new SolidColorBrush(Color.FromRgb(244, 67, 54));  // red
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}