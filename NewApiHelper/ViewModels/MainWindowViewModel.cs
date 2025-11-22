using CommunityToolkit.Mvvm.ComponentModel;
using NewApiHelper.Views;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace NewApiHelper.ViewModels;

public static class PageKeys
{
    public const string ChannelManagement = "ChannelManagement";
    public const string UpstreamManagement = "UpstreamManagement";
    public const string UpstreamGroup = "UpstreamGroup";
    public const string DataDisplay = "DataDisplay";
    public const string SyncLog = "SyncLog";
}

public class MenuItemModel
{
    public string DisplayName { get; set; } = string.Empty;
    public string PageKey { get; set; } = string.Empty;

    public MenuItemModel(string displayName = "", string pageKey = "")
    {
        DisplayName = displayName;
        PageKey = pageKey;
    }
}

public class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<MenuItemModel> MenuItems { get; }

    private MenuItemModel _selectedMenuItem = null!;

    public MenuItemModel SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            if (SetProperty(ref _selectedMenuItem, value))
            {
                UpdateCurrentPage();
            }
        }
    }

    private UserControl? _currentPage;

    public UserControl? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    private readonly IServiceProvider _serviceProvider;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        MenuItems = new ObservableCollection<MenuItemModel>
        {
            new MenuItemModel { DisplayName = "渠道管理", PageKey = PageKeys.ChannelManagement },
            new MenuItemModel { DisplayName = "上游管理", PageKey = PageKeys.UpstreamManagement },
            new MenuItemModel { DisplayName = "上游分组", PageKey = PageKeys.UpstreamGroup },
            new MenuItemModel { DisplayName = "数据展示", PageKey = PageKeys.DataDisplay },
            new MenuItemModel { DisplayName = "同步日志", PageKey = PageKeys.SyncLog },
        };
        // 默认选中第一个菜单项
        if (MenuItems.Count > 0)
            SelectedMenuItem = MenuItems[0];
    }

    private void UpdateCurrentPage()
    {
        if (SelectedMenuItem == null)
        {
            CurrentPage = null;
            return;
        }

        object? page = null;
        switch (SelectedMenuItem.PageKey)
        {
            case PageKeys.ChannelManagement:
                page = _serviceProvider.GetService(typeof(ChannelManagementView));
                break;

            case PageKeys.UpstreamManagement:
                page = _serviceProvider.GetService(typeof(UpstreamManagementView));
                break;

            case PageKeys.UpstreamGroup:
                page = _serviceProvider.GetService(typeof(UpstreamGroupView));
                break;

            case PageKeys.DataDisplay:
                page = _serviceProvider.GetService(typeof(DataDisplayView));
                break;

            case PageKeys.SyncLog:
                page = _serviceProvider.GetService(typeof(SyncLogView));
                break;

            default:
                page = null;
                break;
        }
        CurrentPage = page as UserControl;
    }
}