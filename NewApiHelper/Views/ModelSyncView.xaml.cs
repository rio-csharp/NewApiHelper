using NewApiHelper.ViewModels;
using System.Windows.Controls;

namespace NewApiHelper.Views;

public partial class ModelSyncView : UserControl
{
    public ModelSyncView(ModelSyncViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}