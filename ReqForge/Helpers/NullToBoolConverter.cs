using System.Globalization;
using System.Windows.Data;

namespace ReqForge.Helpers;

public class NullToBoolConverter : IValueConverter
{
    // Если объект не null — возвращаем true, если null — false
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}