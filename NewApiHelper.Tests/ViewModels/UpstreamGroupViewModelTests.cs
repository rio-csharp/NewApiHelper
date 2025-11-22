using Moq;
using NewApiHelper.Models;
using NewApiHelper.Services;
using NewApiHelper.ViewModels;
using System.Collections.ObjectModel;

namespace NewApiHelper.Tests.ViewModels;

public class UpstreamGroupViewModelTests
{
    private readonly Mock<IUpstreamGroupService> _mockService;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<IUpstreamService> _mockUpstreamService;
    private readonly UpstreamGroupViewModel _vm;

    public UpstreamGroupViewModelTests()
    {
        _mockService = new Mock<IUpstreamGroupService>();
        _mockMessageService = new Mock<IMessageService>();
        _mockUpstreamService = new Mock<IUpstreamService>();
        _vm = new UpstreamGroupViewModel(_mockService.Object, _mockMessageService.Object, _mockUpstreamService.Object);
    }

    [Fact]
    public async Task LoadGroupsAsync_LoadsGroupsAndUpstreams()
    {
        // Arrange
        var upstreams = new List<Upstream>
        {
            new Upstream { Id = 1, Name = "Upstream1" }
        };
        var groups = new List<UpstreamGroup>
        {
            new UpstreamGroup { Id = 1, Name = "Group1", UpstreamId = 1 }
        };
        _mockUpstreamService.Setup(s => s.GetAllAsync()).ReturnsAsync(upstreams);
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(groups);

        // Act
        await _vm.LoadGroupsAsync();

        // Assert
        Assert.Single(_vm.Groups);
        Assert.Equal("Group1", _vm.Groups[0].Name);
        Assert.Single(_vm.AvailableUpstreams);
        Assert.Equal("Upstream1", _vm.AvailableUpstreams[0].Name);
    }

    [Fact]
    public void AddGroup_AddsNewGroup()
    {
        // Arrange
        _vm.AvailableUpstreams.Add(new Upstream { Id = 1, Name = "Upstream1" });

        // Act
        _vm.AddGroup();

        // Assert
        Assert.Single(_vm.Groups);
        Assert.True(_vm.Groups[0].IsNew);
        Assert.True(_vm.Groups[0].IsEditing);
        Assert.Equal(_vm.Groups[0], _vm.SelectedGroup);
    }

    [Fact]
    public async Task SaveGroupCommand_SavesNewGroup()
    {
        // Arrange
        var group = new UpstreamGroup { Id = 0, Name = "NewGroup", UpstreamId = 1, GroupRatio = 1.0, Key = "key" };
        var vm = new UpstreamGroupItemViewModel(group, new ObservableCollection<Upstream> { new Upstream { Id = 1, Name = "Upstream1" } })
        {
            IsNew = true,
            IsEditing = false
        };
        _vm.Groups.Add(vm);
        _vm.SelectedGroup = vm;
        _mockService.Setup(s => s.AddAsync(It.IsAny<UpstreamGroup>())).ReturnsAsync(group);

        // Act
        await _vm.SaveGroupCommand.ExecuteAsync(null);

        // Assert
        _mockService.Verify(s => s.AddAsync(It.Is<UpstreamGroup>(g => g.Name == "NewGroup")), Times.Once);
        Assert.False(vm.IsNew);
        Assert.False(vm.IsEditing);
        Assert.False(vm.IsDirty);
    }

    [Fact]
    public async Task DeleteGroupCommand_DeletesGroup()
    {
        // Arrange
        var group = new UpstreamGroup { Id = 1, Name = "Group1" };
        var vm = new UpstreamGroupItemViewModel(group, new ObservableCollection<Upstream>());
        _vm.Groups.Add(vm);
        _vm.SelectedGroup = vm;
        _mockMessageService.Setup(m => m.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        // Act
        await _vm.DeleteGroupCommand.ExecuteAsync(null);

        // Assert
        _mockService.Verify(s => s.DeleteAsync(1), Times.Once);
        Assert.Empty(_vm.Groups);
    }
}