using NewApiHelper.Models;
using NewApiHelper.ViewModels;
using System.Collections.ObjectModel;
using Xunit;

namespace NewApiHelper.Tests.ViewModels;

public class UpstreamGroupItemViewModelTests
{
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Arrange
        var upstreams = new ObservableCollection<Upstream>
        {
            new Upstream { Id = 1, Name = "Upstream1" },
            new Upstream { Id = 2, Name = "Upstream2" }
        };
        var group = new UpstreamGroup { Id = 1, Name = "Group1", UpstreamId = 1, GroupMultiplier = 1.5 };

        // Act
        var vm = new UpstreamGroupItemViewModel(group, upstreams);

        // Assert
        Assert.Equal(1, vm.Id);
        Assert.Equal("Group1", vm.Name);
        Assert.Equal(1, vm.UpstreamId);
        Assert.Equal(1.5, vm.GroupMultiplier);
        Assert.Equal(upstreams[0], vm.SelectedUpstream);
        Assert.Equal(upstreams, vm.AvailableUpstreams);
    }

    [Fact]
    public void SelectedUpstream_Set_UpdatesUpstreamId()
    {
        // Arrange
        var upstreams = new ObservableCollection<Upstream>
        {
            new Upstream { Id = 1, Name = "Upstream1" },
            new Upstream { Id = 2, Name = "Upstream2" }
        };
        var group = new UpstreamGroup { Id = 1, Name = "Group1", UpstreamId = 1 };
        var vm = new UpstreamGroupItemViewModel(group, upstreams);

        // Act
        vm.SelectedUpstream = upstreams[1];

        // Assert
        Assert.Equal(2, vm.UpstreamId);
        Assert.True(vm.IsDirty);
    }

    [Fact]
    public void Name_Set_MarksAsDirty()
    {
        // Arrange
        var upstreams = new ObservableCollection<Upstream>();
        var group = new UpstreamGroup { Id = 1, Name = "Group1" };
        var vm = new UpstreamGroupItemViewModel(group, upstreams);

        // Act
        vm.Name = "NewName";

        // Assert
        Assert.Equal("NewName", vm.Name);
        Assert.True(vm.IsDirty);
    }

    [Fact]
    public void GetModel_ReturnsCorrectModel()
    {
        // Arrange
        var upstreams = new ObservableCollection<Upstream>
        {
            new Upstream { Id = 1, Name = "Upstream1" }
        };
        var group = new UpstreamGroup { Id = 1, Name = "Group1", UpstreamId = 1, GroupMultiplier = 1.5, Key = "key1" };
        var vm = new UpstreamGroupItemViewModel(group, upstreams);

        // Act
        var model = vm.GetModel();

        // Assert
        Assert.Equal(1, model.Id);
        Assert.Equal("Group1", model.Name);
        Assert.Equal(1, model.UpstreamId);
        Assert.Equal(1.5, model.GroupMultiplier);
        Assert.Equal("key1", model.Key);
    }
}