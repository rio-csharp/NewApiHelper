using NewApiHelper.ViewModels;
using System.Windows.Controls;

namespace NewApiHelper.Views;

/// <summary>
/// Interaction logic for CollectionConfigView.xaml
/// </summary>
public partial class CollectionConfigView : UserControl
{
    private readonly UpStreamChannelManagementViewModel _viewModel;

    public CollectionConfigView(UpStreamChannelManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        // 自动加载渠道列表
        _ = _viewModel.LoadChannelsAsync();
    }
}