using NewApiHelper.ViewModels;
using System.Windows.Controls;

namespace NewApiHelper.Views;

/// <summary>
/// Interaction logic for UpstreamGroupView.xaml
/// </summary>
public partial class UpstreamGroupView : UserControl
{
    private readonly UpstreamGroupViewModel _viewModel;

    public UpstreamGroupView(UpstreamGroupViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        // 自动加载分组列表
        _ = _viewModel.LoadGroupsAsync();
    }
}