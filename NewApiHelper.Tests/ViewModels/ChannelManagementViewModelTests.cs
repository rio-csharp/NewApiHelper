using FluentAssertions;
using Moq;
using NewApiHelper.Models;
using NewApiHelper.Services;
using NewApiHelper.ViewModels;

namespace NewApiHelper.Tests.ViewModels;

public class ChannelManagementViewModelTests
{
    private readonly Mock<IChannelService> _mockChannelService;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly ChannelManagementViewModel _viewModel;

    public ChannelManagementViewModelTests()
    {
        _mockChannelService = new Mock<IChannelService>();
        _mockMessageService = new Mock<IMessageService>();
        _viewModel = new ChannelManagementViewModel(_mockChannelService.Object, _mockMessageService.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        _viewModel.Channels.Should().NotBeNull();
        _viewModel.Channels.Should().BeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.SelectedChannel.Should().BeNull();
        _viewModel.IsChannelSelected.Should().BeFalse();
        _viewModel.NewModelText.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadChannelsAsync_ShouldLoadChannelsSuccessfully()
    {
        // Arrange
        var channels = new List<Channel>
        {
            new Channel { Id = 1, Name = "Channel 1" },
            new Channel { Id = 2, Name = "Channel 2" }
        };
        var response = new ApiResponse<ChannelListResponseData>
        {
            Success = true,
            Data = new ChannelListResponseData { Items = channels, Total = 2 }
        };
        _mockChannelService.Setup(s => s.GetChannelsAsync(1, 100))
            .ReturnsAsync(response);

        // Act
        await _viewModel.LoadChannelsAsync();

        // Assert
        _viewModel.Channels.Should().HaveCount(2);
        _viewModel.Channels.First().Name.Should().Be("Channel 1");
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.SelectedChannel.Should().NotBeNull();
        _viewModel.IsChannelSelected.Should().BeTrue();
    }

    [Fact]
    public void AddChannel_ShouldAddNewChannel()
    {
        // Act
        _viewModel.AddChannel();

        // Assert
        _viewModel.Channels.Should().HaveCount(1);
        var newChannel = _viewModel.Channels.First();
        newChannel.Name.Should().BeEmpty();
        newChannel.IsNew.Should().BeTrue();
        newChannel.IsEditing.Should().BeTrue();
        _viewModel.SelectedChannel.Should().Be(newChannel);
    }

    [Fact]
    public void CopyChannel_ShouldCopySelectedChannel()
    {
        // Arrange
        var originalChannel = new Channel
        {
            Id = 1,
            Name = "Original",
            Type = 1,
            Group = "test",
            Priority = 5,
            Weight = 10,
            BaseUrl = "https://api.test.com",
            Models = "gpt-3.5",
            Key = "key",
            ModelMapping = "mapping"
        };
        var originalVm = new ChannelItemViewModel(originalChannel);
        _viewModel.Channels.Add(originalVm);
        _viewModel.SelectedChannel = originalVm;

        // Debug: check selected channel name
        _viewModel.SelectedChannel.Name.Should().Be("Original");

        // Act
        _viewModel.CopyChannel();

        // Assert
        _viewModel.Channels.Should().HaveCount(2);
        var copiedChannel = _viewModel.Channels.First(); // Insert(0) adds to the beginning
        copiedChannel.Name.Should().Be("Original_Copy");
        copiedChannel.Type.Should().Be(1);
        copiedChannel.Group.Should().Be("test");
        copiedChannel.IsNew.Should().BeTrue();
        copiedChannel.IsEditing.Should().BeTrue();
        _viewModel.SelectedChannel.Should().Be(copiedChannel);
    }

    [Fact]
    public void CopyChannel_ShouldNotExecute_WhenNoChannelSelected()
    {
        // Arrange
        _viewModel.SelectedChannel = null;

        // Act
        _viewModel.CopyChannel();

        // Assert
        _viewModel.Channels.Should().BeEmpty();
    }

    [Fact]
    public void StartEditChannel_ShouldSetIsEditingToTrue()
    {
        // Arrange
        var channel = new Channel { Id = 1, Name = "Test" };
        var vm = new ChannelItemViewModel(channel) { IsEditing = false };
        _viewModel.Channels.Add(vm);
        _viewModel.SelectedChannel = vm;

        // Act
        _viewModel.StartEditChannel();

        // Assert
        vm.IsEditing.Should().BeTrue();
    }

    [Fact]
    public void StartEditChannel_ShouldNotExecute_WhenNoChannelSelected()
    {
        // Arrange
        _viewModel.SelectedChannel = null;

        // Act
        _viewModel.StartEditChannel();

        // Assert - Should not throw exception
        _viewModel.Channels.Should().BeEmpty();
    }

    [Fact]
    public void CanCopyChannel_ShouldReturnTrue_WhenChannelSelected()
    {
        // Arrange
        var channel = new Channel { Id = 1, Name = "Test" };
        var vm = new ChannelItemViewModel(channel);
        _viewModel.Channels.Add(vm);
        _viewModel.SelectedChannel = vm;

        // Act
        var canExecute = _viewModel.GetType().GetMethod("CanCopyChannel",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (bool)canExecute!.Invoke(_viewModel, null)!;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanCopyChannel_ShouldReturnFalse_WhenNoChannelSelected()
    {
        // Arrange
        _viewModel.SelectedChannel = null;

        // Act
        var canExecute = _viewModel.GetType().GetMethod("CanCopyChannel",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (bool)canExecute!.Invoke(_viewModel, null)!;

        // Assert
        result.Should().BeFalse();
    }
}