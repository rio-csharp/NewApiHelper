using System.Globalization;
using System.Windows.Data;

namespace NewApiHelper.Converters;

public class QuotaTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Models.QuotaType quotaType)
        {
            return quotaType switch
            {
                Models.QuotaType.PayAsYouGo => "按量付费",
                Models.QuotaType.PayPerUse => "按次计费",
                Models.QuotaType.NotSupported => "不支持",
                _ => "未知"
            };
        }
        return "未知";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}