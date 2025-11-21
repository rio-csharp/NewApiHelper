using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NewApiHelper.Utilities.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int status)
        {
            return status switch
            {
                1 => Brushes.Green, // 启用
                2 => Brushes.Red,   // 禁用
                _ => Brushes.Gray   // 未知
            };
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}