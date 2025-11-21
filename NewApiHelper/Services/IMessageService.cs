namespace NewApiHelper.Services;

public interface IMessageService
{
    void ShowError(string message);

    bool ShowConfirmation(string message, string title = "确认");
}