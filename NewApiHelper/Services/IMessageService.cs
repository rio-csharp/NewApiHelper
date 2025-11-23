namespace NewApiHelper.Services;

public interface IMessageService
{
    void ShowError(string message);

    void ShowInfo(string message);

    bool ShowConfirmation(string message, string title = "确认");
}