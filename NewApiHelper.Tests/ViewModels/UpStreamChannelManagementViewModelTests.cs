using FluentAssertions;
using Moq;
using NewApiHelper.Models;
using NewApiHelper.Services;
using NewApiHelper.ViewModels;

namespace NewApiHelper.Tests.ViewModels;

public class UpStreamChannelManagementViewModelTests
{
    private readonly Mock<IUpstreamService> _mockService;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly UpStreamManagementViewModel _viewModel;

    public UpStreamChannelManagementViewModelTests()
    {
        _mockService = new Mock<IUpstreamService>();
        _mockMessageService = new Mock<IMessageService>();
        _viewModel = new UpStreamManagementViewModel(_mockService.Object, _mockMessageService.Object);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Assert
        _viewModel.Channels.Should().NotBeNull();
        _viewModel.Channels.Should().BeEmpty();
        _viewModel.SelectedChannel.Should().BeNull();
        _viewModel.IsChannelSelected.Should().BeFalse();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.HasChannels.Should().BeFalse();
        _viewModel.ShowAddButton.Should().BeTrue();
    }

    [Fact]
    public async Task LoadChannelsAsync_EmptyList_SetsPropertiesCorrectly()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Upstream>());

        // Act
        await _viewModel.LoadChannelsAsync();

        // Assert
        _viewModel.Channels.Should().BeEmpty();
        _viewModel.HasChannels.Should().BeFalse();
        _viewModel.ShowAddButton.Should().BeTrue();
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadChannelsAsync_WithChannels_SetsPropertiesCorrectly()
    {
        // Arrange
        var channels = new List<Upstream>
        {
            new Upstream { Id = 1, Name = "Channel 1", Url = "https://api1.com", Multiplier = 1.0, CreatedAt = DateTime.Now },
            new Upstream { Id = 2, Name = "Channel 2", Url = "https://api2.com", Multiplier = 2.0, CreatedAt = DateTime.Now }
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(channels);

        // Act
        await _viewModel.LoadChannelsAsync();

        // Assert
        _viewModel.Channels.Should().HaveCount(2);
        _viewModel.HasChannels.Should().BeTrue();
        _viewModel.ShowAddButton.Should().BeFalse();
        _viewModel.IsLoading.Should().BeFalse();

        _viewModel.Channels[0].Name.Should().Be("Channel 1");
        _viewModel.Channels[1].Name.Should().Be("Channel 2");
    }

    [Fact]
    public async Task LoadChannelsAsync_SetsLoadingState()
    {
        // Arrange
        var channels = new List<Upstream>
        {
            new Upstream { Id = 1, Name = "Channel 1", Url = "https://api1.com", Multiplier = 1.0, CreatedAt = DateTime.Now }
        };

        // Setup mock to return after a delay to ensure we can capture the loading state
        _mockService.Setup(s => s.GetAllAsync()).Returns(async () =>
        {
            await Task.Delay(10); // Small delay to allow loading state to be observed
            return channels;
        });

        // Act
        var loadTask = _viewModel.LoadChannelsAsync();

        // Assert - Loading should be true during operation
        _viewModel.IsLoading.Should().BeTrue();

        await loadTask;

        // Assert - Loading should be false after completion
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void SelectedChannel_SetToValidChannel_SetsIsChannelSelectedToTrue()
    {
        // Arrange
        var channel = new Upstream { Id = 1, Name = "Test", Url = "https://test.com", Multiplier = 1.0, CreatedAt = DateTime.Now };
        var channelVm = new UpstreamItemViewModel(channel);
        _viewModel.Channels.Add(channelVm);

        // Act
        _viewModel.SelectedChannel = channelVm;

        // Assert
        _viewModel.IsChannelSelected.Should().BeTrue();
    }

    [Fact]
    public void SelectedChannel_SetToNull_SetsIsChannelSelectedToFalse()
    {
        // Arrange
        var channel = new Upstream { Id = 1, Name = "Test", Url = "https://test.com", Multiplier = 1.0, CreatedAt = DateTime.Now };
        var channelVm = new UpstreamItemViewModel(channel);
        _viewModel.Channels.Add(channelVm);
        _viewModel.SelectedChannel = channelVm; // First set to a channel

        // Act
        _viewModel.SelectedChannel = null;

        // Assert
        _viewModel.IsChannelSelected.Should().BeFalse();
    }

    [Fact]
    public void AddChannel_ValidChannel_AddsToCollection()
    {
        // Arrange
        var initialCount = _viewModel.Channels.Count;

        // Act
        _viewModel.AddChannel();

        // Assert
        _viewModel.Channels.Should().HaveCount(initialCount + 1);
        var addedChannel = _viewModel.Channels[0];
        addedChannel.Name.Should().BeEmpty();
        addedChannel.Url.Should().BeEmpty();
        addedChannel.Multiplier.Should().Be(1.0);
        addedChannel.IsNew.Should().BeTrue();
        addedChannel.IsEditing.Should().BeTrue();
        _viewModel.SelectedChannel.Should().Be(addedChannel);
    }

    [Fact]
    public async Task DeleteChannelAsync_ExistingChannel_RemovesFromCollection()
    {
        // Arrange
        var channel = new Upstream { Id = 1, Name = "Test", Url = "https://test.com", Multiplier = 1.0, CreatedAt = DateTime.Now };
        var channelVm = new UpstreamItemViewModel(channel);
        _viewModel.Channels.Add(channelVm);
        _viewModel.SelectedChannel = channelVm;

        _mockService.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);
        _mockMessageService.Setup(m => m.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        // Act
        await _viewModel.DeleteChannelAsync();

        // Assert
        _viewModel.Channels.Should().BeEmpty();
        _viewModel.HasChannels.Should().BeFalse();
        _viewModel.ShowAddButton.Should().BeTrue();
        _viewModel.SelectedChannel.Should().BeNull();
        _viewModel.IsChannelSelected.Should().BeFalse();
    }

    [Fact]
    public async Task SaveChannelAsync_ValidChannel_SavesSuccessfully()
    {
        // Arrange
        var channel = new Upstream { Id = 1, Name = "Original", Url = "https://original.com", Multiplier = 1.0, CreatedAt = DateTime.Now };
        var channelVm = new UpstreamItemViewModel(channel);
        _viewModel.Channels.Add(channelVm);
        _viewModel.SelectedChannel = channelVm;

        // Modify the channel
        channelVm.Name = "Modified";
        channelVm.IsEditing = true;

        var updatedChannel = new Upstream { Id = 1, Name = "Modified", Url = "https://original.com", Multiplier = 1.0, CreatedAt = DateTime.Now };
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Upstream>())).ReturnsAsync(updatedChannel);

        // Act
        await _viewModel.SaveChannelAsync();

        // Assert
        channelVm.IsEditing.Should().BeFalse();
        channelVm.IsDirty.Should().BeFalse();
        _mockService.Verify(s => s.UpdateAsync(It.Is<Upstream>(c => c.Name == "Modified")), Times.Once);
    }

    [Fact]
    public void CancelEditCommand_ResetsChannelState()
    {
        // Arrange
        var originalChannel = new Upstream { Id = 1, Name = "Original", Url = "https://original.com", Multiplier = 1.0, CreatedAt = DateTime.Now };
        var channelVm = new UpstreamItemViewModel(originalChannel);
        _viewModel.Channels.Add(channelVm);
        _viewModel.SelectedChannel = channelVm;

        // Modify and set editing
        channelVm.Name = "Modified";
        channelVm.IsEditing = true;

        // Act
        _viewModel.CancelEditCommand.Execute(null);

        // Assert
        channelVm.IsEditing.Should().BeFalse();
        // Note: Cancel doesn't reset the dirty state or revert changes in this implementation
    }

    [Fact]
    public void Channels_CollectionChanged_UpdatesHasChannelsAndShowAddButton()
    {
        // Act - Add first channel
        var channel1 = new UpstreamItemViewModel(new Upstream { Id = 1, Name = "Channel 1", Url = "https://api1.com", Multiplier = 1.0, CreatedAt = DateTime.Now });
        _viewModel.Channels.Add(channel1);

        // Assert
        _viewModel.HasChannels.Should().BeTrue();
        _viewModel.ShowAddButton.Should().BeFalse();

        // Act - Remove all channels
        _viewModel.Channels.Clear();

        // Assert
        _viewModel.HasChannels.Should().BeFalse();
        _viewModel.ShowAddButton.Should().BeTrue();
    }
}