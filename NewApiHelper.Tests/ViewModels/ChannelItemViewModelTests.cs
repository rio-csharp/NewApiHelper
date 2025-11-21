using FluentAssertions;
using NewApiHelper.Models;
using NewApiHelper.ViewModels;

namespace NewApiHelper.Tests.ViewModels;

public class ChannelItemViewModelTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var channel = new Channel
        {
            Id = 1,
            Name = "Test Channel",
            Type = 1,
            Group = "test",
            Priority = 5,
            Weight = 10,
            BaseUrl = "https://api.test.com",
            Models = "gpt-3.5,gpt-4",
            Key = "test-key",
            ModelMapping = "mapping",
            Status = 1
        };

        // Act
        var vm = new ChannelItemViewModel(channel);

        // Assert
        vm.Id.Should().Be(1);
        vm.Name.Should().Be("Test Channel");
        vm.Type.Should().Be(1);
        vm.Group.Should().Be("test");
        vm.Priority.Should().Be(5);
        vm.Weight.Should().Be(10);
        vm.BaseUrl.Should().Be("https://api.test.com");
        vm.Key.Should().Be("test-key");
        vm.ModelMapping.Should().Be("mapping");
        vm.Status.Should().Be(1);
        vm.StatusText.Should().Be("启用");
        vm.ModelsList.Should().HaveCount(2);
        vm.ModelsList.Should().Contain("gpt-3.5");
        vm.ModelsList.Should().Contain("gpt-4");
    }

    [Fact]
    public void Name_PropertyChanged_ShouldSetIsDirty()
    {
        // Arrange
        var channel = new Channel { Name = "Original" };
        var vm = new ChannelItemViewModel(channel);

        // Act
        vm.Name = "Modified";

        // Assert
        vm.Name.Should().Be("Modified");
        vm.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void AddModel_ShouldAddModelToListAndUpdateModelsString()
    {
        // Arrange
        var channel = new Channel { Models = "gpt-3.5" };
        var vm = new ChannelItemViewModel(channel);

        // Act
        vm.AddModel("gpt-4");

        // Assert
        vm.ModelsList.Should().HaveCount(2);
        vm.ModelsList.Should().Contain("gpt-3.5");
        vm.ModelsList.Should().Contain("gpt-4");
        vm.Models.Should().Be("gpt-3.5,gpt-4");
        vm.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void RemoveModel_ShouldRemoveModelFromListAndUpdateModelsString()
    {
        // Arrange
        var channel = new Channel { Models = "gpt-3.5,gpt-4" };
        var vm = new ChannelItemViewModel(channel);

        // Act
        vm.RemoveModel("gpt-3.5");

        // Assert
        vm.ModelsList.Should().HaveCount(1);
        vm.ModelsList.Should().Contain("gpt-4");
        vm.Models.Should().Be("gpt-4");
        vm.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void ToAddRequest_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var channel = new Channel
        {
            Name = "Test Channel",
            Type = 1,
            Key = "test-key",
            BaseUrl = "https://api.test.com",
            Models = "gpt-3.5,gpt-4",
            ModelMapping = "mapping",
            Group = "test",
            Priority = 5,
            Weight = 10
        };
        var vm = new ChannelItemViewModel(channel);

        // Act
        var request = vm.ToAddRequest();

        // Assert
        request.Name.Should().Be("Test Channel");
        request.Type.Should().Be(1);
        request.Key.Should().Be("test-key");
        request.BaseUrl.Should().Be("https://api.test.com");
        request.Models.Should().Be("gpt-3.5,gpt-4");
        request.ModelMapping.Should().Be("mapping");
        request.Groups.Should().Contain("test");
        request.Priority.Should().Be(5);
        request.Weight.Should().Be(10);
    }

    [Fact]
    public void ToUpdateRequest_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var channel = new Channel
        {
            Id = 1,
            Name = "Test Channel",
            Priority = 5,
            Weight = 10,
            BaseUrl = "https://api.test.com",
            Models = "gpt-3.5,gpt-4",
            ModelMapping = "mapping",
            Key = "test-key",
            Group = "test"
        };
        var vm = new ChannelItemViewModel(channel);

        // Act
        var request = vm.ToUpdateRequest();

        // Assert
        request.Id.Should().Be(1);
        request.Name.Should().Be("Test Channel");
        request.Priority.Should().Be(5);
        request.Weight.Should().Be(10);
        request.BaseUrl.Should().Be("https://api.test.com");
        request.Models.Should().Be("gpt-3.5,gpt-4");
        request.ModelMapping.Should().Be("mapping");
        request.Key.Should().Be("test-key");
        request.Group.Should().Be("test");
    }

    [Fact]
    public void UpdateTestResult_ShouldUpdateProperties()
    {
        // Arrange
        var channel = new Channel();
        var vm = new ChannelItemViewModel(channel);

        // Act
        vm.UpdateTestResult(1500, true, "Success");

        // Assert
        vm.ResponseTime.Should().Be(1500);
        vm.TestStatusMessage.Should().Be("Success");
        vm.TestTimeDisplay.Should().NotBe("未测试");
    }
}