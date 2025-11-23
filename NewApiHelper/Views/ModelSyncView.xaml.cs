using NewApiHelper.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace NewApiHelper.Views;

public partial class ModelSyncView : UserControl
{
    private readonly ModelSyncViewModel _viewModel;
    public ModelSyncView(ModelSyncViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
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

    private void SearchTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            // 确保绑定更新
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                var binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                binding?.UpdateSource();
            }
            
            _viewModel.SearchCommand.Execute(null);
        }
    }
}