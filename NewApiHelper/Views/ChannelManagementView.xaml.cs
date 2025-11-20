using NewApiHelper.ViewModels;
using System.Windows.Controls;

namespace NewApiHelper.Views;

/// <summary>
/// Interaction logic for ChannelManagementView.xaml
/// </summary>
public partial class ChannelManagementView : UserControl
{
    private readonly ChannelManagementViewModel _viewModel;

    public ChannelManagementView(ChannelManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }
}