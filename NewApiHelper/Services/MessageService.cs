using System.Windows;

namespace NewApiHelper.Services;

public class MessageService : IMessageService
{
    public void ShowError(string message)
    {
        MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool ShowConfirmation(string message, string title = "确认")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }
}