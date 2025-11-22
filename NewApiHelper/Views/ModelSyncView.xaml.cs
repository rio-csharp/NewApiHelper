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

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ModelSyncViewModel viewModel && sender is DataGrid dataGrid)
        {
            viewModel.SelectedModelSyncs.Clear();
            foreach (var item in dataGrid.SelectedItems)
            {
                if (item is Models.ModelSync model)
                {
                    viewModel.SelectedModelSyncs.Add(model);
                }
            }
        }
    }
}