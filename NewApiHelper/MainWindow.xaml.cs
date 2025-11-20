using NewApiHelper.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace NewApiHelper;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();
    }

    // 标题栏拖动窗口
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximizeRestore();
        }
        else
        {
            DragMove();
        }
    }
    // 点击最小化
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    // 点击最大化/还原
    private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximizeRestore();
    }
    private void ToggleMaximizeRestore()
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            MaxRestoreIcon.Text = "\u25A1"; // □ 还原图标
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaxRestoreIcon.Text = "\u2752"; // ▭ 最大化图标（你可以换成其他符号）
        }
    }
    // 关闭窗口
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}