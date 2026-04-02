using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReqForge.Helpers;

public class StringMatchToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && parameter is string param)
        {
            var matches = param.Split('|');
            return matches.Any(m => m.Equals(str, StringComparison.OrdinalIgnoreCase))
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
