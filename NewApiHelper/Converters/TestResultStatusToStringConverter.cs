using System.Globalization;
using System.Windows.Data;

namespace NewApiHelper.Converters;

public class TestResultStatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Models.TestResultStatus status)
        {
            return status switch
            {
                Models.TestResultStatus.Untested => "○ 未测试",
                Models.TestResultStatus.Success => "✓ 成功",
                Models.TestResultStatus.Failed => "✗ 失败",
                Models.TestResultStatus.Skipped => "⊘ 跳过",
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