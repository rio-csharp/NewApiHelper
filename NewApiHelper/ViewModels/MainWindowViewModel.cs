using CommunityToolkit.Mvvm.ComponentModel;
using NewApiHelper.Views;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace NewApiHelper.ViewModels;

public class MenuItemModel
{
    public string DisplayName { get; set; }
    public string PageKey { get; set; }  // 用于标识子页面，比如："ChannelManagement"
}

public class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<MenuItemModel> MenuItems { get; }

    private MenuItemModel _selectedMenuItem;
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

    private UserControl _currentPage;
    public UserControl CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public MainWindowViewModel()
    {
        MenuItems = new ObservableCollection<MenuItemModel>
        {
            new MenuItemModel { DisplayName = "渠道管理", PageKey = "ChannelManagement" },
            new MenuItemModel { DisplayName = "采集配置", PageKey = "CollectionConfig" },
            new MenuItemModel { DisplayName = "数据展示", PageKey = "DataDisplay" },
            new MenuItemModel { DisplayName = "同步日志", PageKey = "SyncLog" },
        };

        SelectedMenuItem = MenuItems.FirstOrDefault();
    }

    private void UpdateCurrentPage()
    {
        if (SelectedMenuItem == null) return;

        // 这里根据PageKey实例化对应的UserControl页面
        switch (SelectedMenuItem.PageKey)
        {
            case "ChannelManagement":
                CurrentPage = new ChannelManagementView();  // 你子页面UserControl
                break;
            case "CollectionConfig":
                CurrentPage = new CollectionConfigView();
                break;
            case "DataDisplay":
                CurrentPage = new DataDisplayView();
                break;
            case "SyncLog":
                CurrentPage = new SyncLogView();
                break;
            default:
                CurrentPage = null;
                break;
        }
    }
}
