using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewApiHelper.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace NewApiHelper.ViewModels;

public partial class ChannelManagementViewModel : ObservableObject
{
    private readonly IChannelService _channelService;

    [ObservableProperty]
    private ObservableCollection<ChannelItemViewModel> _channels;

    [ObservableProperty]
    private ChannelItemViewModel? _selectedChannel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditChannelCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteChannelCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestChannelCommand))]
    private bool _isChannelSelected;

    [ObservableProperty]
    private bool _isLoading;

    public ChannelManagementViewModel(IChannelService channelService)
    {
        _channelService = channelService;
        _channels = new ObservableCollection<ChannelItemViewModel>();
        // 监听 SelectedChannel 的变化来更新 IsChannelSelected
        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SelectedChannel))
            {
                IsChannelSelected = SelectedChannel != null;
            }
        };
    }

    // --- 命令 (Commands) ---
    [RelayCommand]
    public async Task LoadChannelsAsync()
    {
        IsLoading = true;
        try
        {
            var apiResponse = await _channelService.GetChannelsAsync(1, 100); // 暂时硬编码分页
            if (apiResponse.Success && apiResponse.Data != null)
            {
                Channels.Clear();
                foreach (var channel in apiResponse.Data.Items)
                {
                    Channels.Add(new ChannelItemViewModel(channel));
                }
            }
            else
            {
                // 显示错误信息
                MessageBox.Show(apiResponse.Message ?? "加载渠道列表失败。");
            }
        }
        catch (System.Exception ex)
        {
            // 记录日志并显示错误
            MessageBox.Show($"发生异常: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void AddChannel()
    {
        // TODO: 此处应打开一个新增/编辑渠道的对话框或导航到新页面
        // var addEditVM = new AddEditChannelViewModel();
        // _dialogService.ShowDialog(addEditVM);
        // if(addEditVM.DialogResult == true) { await LoadChannelsAsync(); }
        MessageBox.Show("此处将打开新增渠道对话框。");
    }

    [RelayCommand(CanExecute = nameof(IsChannelSelected))]
    public void EditChannel()
    {
        if (SelectedChannel == null) return;
        // TODO: 打开对话框，并传入SelectedChannel的数据
        // var addEditVM = new AddEditChannelViewModel(SelectedChannel.GetModel());
        // _dialogService.ShowDialog(addEditVM);
        // if(addEditVM.DialogResult == true) { await LoadChannelsAsync(); }
        MessageBox.Show($"编辑渠道: {SelectedChannel.Name}");
    }

    [RelayCommand(CanExecute = nameof(IsChannelSelected))]
    public async Task DeleteChannelAsync()
    {
        if (SelectedChannel == null) return;
        if (MessageBox.Show($"确定要删除渠道 '{SelectedChannel.Name}' 吗？", "确认删除", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            SelectedChannel.IsBusy = true;
            try
            {
                var response = await _channelService.DeleteChannelAsync(SelectedChannel.Id);
                if (response.Success)
                {
                    Channels.Remove(SelectedChannel);
                }
                else
                {
                    MessageBox.Show(response.Message ?? "删除失败。");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"删除时发生异常: {ex.Message}");
            }
            finally
            {
                // 在 finally 中总是将 IsBusy 设置为 false，即使SelectedChannel可能已被删除
                // 这是一个简化处理，更复杂的场景可能需要不同策略
            }
        }
    }

    [RelayCommand(CanExecute = nameof(IsChannelSelected))]
    public async Task TestChannelAsync()
    {
        if (SelectedChannel == null) return;
        SelectedChannel.IsBusy = true;
        SelectedChannel.TestStatusMessage = "测试中...";
        try
        {
            var response = await _channelService.TestChannelAsync(SelectedChannel.Id);
            if (response.Success)
            {
                SelectedChannel.UpdateTestResult((int)(response.Time * 1000), true, $"成功，耗时: {response.Time * 1000:F0}ms");
            }
            else
            {
                SelectedChannel.UpdateTestResult(0, false, $"失败: {response.Message}");
            }
        }
        catch (System.Exception ex)
        {
            SelectedChannel.UpdateTestResult(0, false, $"测试异常: {ex.Message}");
        }
        finally
        {
            SelectedChannel.IsBusy = false;
        }
    }
}