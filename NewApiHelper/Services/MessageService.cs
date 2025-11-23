using Microsoft.Extensions.Logging;
using System.Windows;

namespace NewApiHelper.Services;

public class MessageService : IMessageService
{
    private readonly Microsoft.Extensions.Logging.ILogger<MessageService> _logger;

    public MessageService(Microsoft.Extensions.Logging.ILogger<MessageService> logger)
    {
        _logger = logger;
    }

    public void ShowError(string message)
    {
        _logger.LogError("用户错误提示: {Message}", message);
        MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowInfo(string message)
    {
        _logger.LogInformation("用户信息提示: {Message}", message);
        MessageBox.Show(message, "信息", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool ShowConfirmation(string message, string title = "确认")
    {
        _logger.LogInformation("用户确认提示: {Title} - {Message}", title, message);
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }
}