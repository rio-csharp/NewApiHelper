using FluentAssertions;
using Moq;
using NewApiHelper.ViewModels;
using NewApiHelper.Views;

namespace NewApiHelper.Tests.ViewModels;

public class TestServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        // Return null to avoid creating UI components in tests
        return null;
    }
}

public class MainWindowViewModelTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly MainWindowViewModel _viewModel;

    public MainWindowViewModelTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _viewModel = new MainWindowViewModel(_mockServiceProvider.Object);
    }

    [Fact]
    public void Constructor_InitializesMenuItems()
    {
        // Assert
        _viewModel.MenuItems.Should().NotBeNull();
        _viewModel.MenuItems.Should().HaveCount(6);
        _viewModel.MenuItems.Should().Contain(m => m.DisplayName == "渠道管理" && m.PageKey == "ChannelManagement");
        _viewModel.MenuItems.Should().Contain(m => m.DisplayName == "上游管理" && m.PageKey == "UpstreamManagement");
        _viewModel.MenuItems.Should().Contain(m => m.DisplayName == "上游分组" && m.PageKey == "UpstreamGroup");
        _viewModel.MenuItems.Should().Contain(m => m.DisplayName == "模型同步" && m.PageKey == "ModelSync");
        _viewModel.MenuItems.Should().Contain(m => m.DisplayName == "数据展示" && m.PageKey == "DataDisplay");
        _viewModel.MenuItems.Should().Contain(m => m.DisplayName == "同步日志" && m.PageKey == "SyncLog");
    }

    [Fact]
    public void SelectedMenuItem_SetToChannelManagement_LoadsChannelManagementView()
    {
        // Arrange
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(ChannelManagementView)))
            .Returns((object?)null);

        // Act
        _viewModel.SelectedMenuItem = _viewModel.MenuItems.First(m => m.PageKey == "ChannelManagement");

        // Assert
        _mockServiceProvider.Verify(sp => sp.GetService(typeof(ChannelManagementView)), Times.Once);
        _viewModel.CurrentPage.Should().BeNull();
    }

    [Fact]
    public void SelectedMenuItem_SetToUpstreamManagement_LoadsUpstreamManagementView()
    {
        // Arrange
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(UpstreamManagementView)))
            .Returns((object?)null);

        // Act
        _viewModel.SelectedMenuItem = _viewModel.MenuItems.First(m => m.PageKey == "UpstreamManagement");

        // Assert
        _mockServiceProvider.Verify(sp => sp.GetService(typeof(UpstreamManagementView)), Times.Once);
        _viewModel.CurrentPage.Should().BeNull();
    }

    [Fact]
    public void SelectedMenuItem_SetToDataDisplay_LoadsDataDisplayView()
    {
        // Arrange
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(DataDisplayView)))
            .Returns((object?)null);

        // Act
        _viewModel.SelectedMenuItem = _viewModel.MenuItems.First(m => m.PageKey == "DataDisplay");

        // Assert
        _mockServiceProvider.Verify(sp => sp.GetService(typeof(DataDisplayView)), Times.Once);
        _viewModel.CurrentPage.Should().BeNull();
    }

    [Fact]
    public void SelectedMenuItem_SetToSyncLog_LoadsSyncLogView()
    {
        // Arrange
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(SyncLogView)))
            .Returns((object?)null);

        // Act
        _viewModel.SelectedMenuItem = _viewModel.MenuItems.First(m => m.PageKey == "SyncLog");

        // Assert
        _mockServiceProvider.Verify(sp => sp.GetService(typeof(SyncLogView)), Times.Once);
        _viewModel.CurrentPage.Should().BeNull();
    }

    [Fact]
    public void SelectedMenuItem_SetToInvalidPageKey_SetsCurrentPageToNull()
    {
        // Arrange
        var invalidMenuItem = new MenuItemModel("Invalid", "InvalidKey");

        // Act
        _viewModel.SelectedMenuItem = invalidMenuItem;

        // Assert
        _viewModel.CurrentPage.Should().BeNull();
    }

    [Fact]
    public void CurrentPage_Initially_IsNull()
    {
        // Assert
        _viewModel.CurrentPage.Should().BeNull();
    }

    [Fact]
    public void MenuItemModel_Constructor_SetsProperties()
    {
        // Act
        var menuItem = new MenuItemModel("Test Display", "TestKey");

        // Assert
        menuItem.DisplayName.Should().Be("Test Display");
        menuItem.PageKey.Should().Be("TestKey");
    }

    [Fact]
    public void MenuItemModel_DefaultConstructor_SetsEmptyProperties()
    {
        // Act
        var menuItem = new MenuItemModel();

        // Assert
        menuItem.DisplayName.Should().BeEmpty();
        menuItem.PageKey.Should().BeEmpty();
    }
}