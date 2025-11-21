using NewApiHelper.ViewModels;
using System.Windows.Controls;

namespace NewApiHelper.Views;

/// <summary>
/// Interaction logic for UpstreamManagementView.xaml
/// </summary>
public partial class UpstreamManagementView : UserControl
{
    private readonly UpStreamChannelManagementViewModel _viewModel;

    public UpstreamManagementView(UpStreamChannelManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        // 自动加载渠道列表
        _ = _viewModel.LoadChannelsAsync();
    }
}