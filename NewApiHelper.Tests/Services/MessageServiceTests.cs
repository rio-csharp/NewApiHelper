using FluentAssertions;
using Moq;
using NewApiHelper.Services;

namespace NewApiHelper.Tests.Services;

public class MessageServiceTests
{
    private readonly Mock<IMessageService> _mockMessageService;

    public MessageServiceTests()
    {
        _mockMessageService = new Mock<IMessageService>();
    }

    // 注意：由于MessageService直接使用MessageBox.Show，这很难进行单元测试
    // 在实际项目中，可能需要重构MessageService以接受一个消息显示接口
    // 或者使用集成测试来测试UI行为

    [Fact]
    public void MessageService_InheritsFromIMessageService()
    {
        // Arrange
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<MessageService>>();
        // Act
        var service = new MessageService(mockLogger.Object);

        // Assert
        service.Should().BeAssignableTo<IMessageService>();
    }

    // 以下测试展示了如何在重构后进行测试
    // 如果MessageService被重构为接受IMessageBox接口，则可以这样测试：

    /*
    [Fact]
    public void ShowError_ValidMessage_CallsMessageBoxShow()
    {
        // Arrange
        var mockMessageBox = new Mock<IMessageBox>();
        var service = new MessageService(mockMessageBox.Object);
        var message = "Test error message";

        // Act
        service.ShowError(message);

        // Assert
        mockMessageBox.Verify(m => m.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error), Times.Once);
    }

    [Fact]
    public void ShowConfirmation_UserClicksYes_ReturnsTrue()
    {
        // Arrange
        var mockMessageBox = new Mock<IMessageBox>();
        mockMessageBox.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.YesNo, MessageBoxImage.Warning))
            .Returns(MessageBoxResult.Yes);
        var service = new MessageService(mockMessageBox.Object);

        // Act
        var result = service.ShowConfirmation("Test message");

        // Assert
        result.Should().BeTrue();
        mockMessageBox.Verify(m => m.Show("Test message", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning), Times.Once);
    }

    [Fact]
    public void ShowConfirmation_UserClicksNo_ReturnsFalse()
    {
        // Arrange
        var mockMessageBox = new Mock<IMessageBox>();
        mockMessageBox.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.YesNo, MessageBoxImage.Warning))
            .Returns(MessageBoxResult.No);
        var service = new MessageService(mockMessageBox.Object);

        // Act
        var result = service.ShowConfirmation("Test message");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShowConfirmation_CustomTitle_UsesCustomTitle()
    {
        // Arrange
        var mockMessageBox = new Mock<IMessageBox>();
        mockMessageBox.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.YesNo, MessageBoxImage.Warning))
            .Returns(MessageBoxResult.Yes);
        var service = new MessageService(mockMessageBox.Object);
        var customTitle = "Custom Title";

        // Act
        var result = service.ShowConfirmation("Test message", customTitle);

        // Assert
        result.Should().BeTrue();
        mockMessageBox.Verify(m => m.Show("Test message", customTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning), Times.Once);
    }
    */
}