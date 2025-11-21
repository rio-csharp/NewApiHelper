using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NewApiHelper.Views;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace NewApiHelper.ViewModels;

public class MenuItemModel
{
    public string DisplayName { get; set; }
    public string PageKey { get; set; }

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

    private UserControl? _currentPage = null;

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
            new MenuItemModel { DisplayName = "渠道管理", PageKey = "ChannelManagement" },
            new MenuItemModel { DisplayName = "上游管理", PageKey = "CollectionConfig" },
            new MenuItemModel { DisplayName = "数据展示", PageKey = "DataDisplay" },
            new MenuItemModel { DisplayName = "同步日志", PageKey = "SyncLog" },
        };

        SelectedMenuItem = MenuItems.First();
    }

    private void UpdateCurrentPage()
    {
        if (SelectedMenuItem == null) return;

        switch (SelectedMenuItem.PageKey)
        {
            case "ChannelManagement":
                CurrentPage = _serviceProvider.GetRequiredService<ChannelManagementView>();
                break;

            case "CollectionConfig":
                CurrentPage = _serviceProvider.GetRequiredService<CollectionConfigView>();
                break;

            case "DataDisplay":
                CurrentPage = _serviceProvider.GetRequiredService<DataDisplayView>();
                break;

            case "SyncLog":
                CurrentPage = _serviceProvider.GetRequiredService<SyncLogView>();
                break;

            default:
                CurrentPage = null;
                break;
        }
    }
}