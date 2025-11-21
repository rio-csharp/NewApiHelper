using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewApiHelper.Models;
using NewApiHelper.Services;
using System.Collections.ObjectModel;

namespace NewApiHelper.ViewModels;

public partial class UpStreamChannelManagementViewModel : ObservableObject
{
    private readonly IUpStreamChannelService _service;
    private readonly IMessageService _messageService;

    [ObservableProperty]
    private ObservableCollection<UpStreamChannelItemViewModel> _channels;

    [ObservableProperty]
    private UpStreamChannelItemViewModel? _selectedChannel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditChannelCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteChannelCommand))]
    private bool _isChannelSelected;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasChannels;

    [ObservableProperty]
    private bool _showAddButton;

    public UpStreamChannelManagementViewModel(IUpStreamChannelService service, IMessageService messageService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _channels = new ObservableCollection<UpStreamChannelItemViewModel>();
        _channels.CollectionChanged += (s, e) =>
        {
            HasChannels = _channels.Count > 0;
            ShowAddButton = _channels.Count == 0;
        };
        ShowAddButton = true; // 初始时显示
        // 监听 SelectedChannel 的变化来更新 IsChannelSelected
        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SelectedChannel))
            {
                IsChannelSelected = SelectedChannel != null;
                UpdateCommandStates();
            }
        };
    }

    private void UpdateCommandStates()
    {
        LoadChannelsCommand.NotifyCanExecuteChanged();
        EditChannelCommand.NotifyCanExecuteChanged();
        StartEditChannelCommand.NotifyCanExecuteChanged();
        DeleteChannelCommand.NotifyCanExecuteChanged();
        SaveChannelCommand.NotifyCanExecuteChanged();
        CancelEditCommand.NotifyCanExecuteChanged();
    }

    private void ShowErrorMessage(string message)
    {
        _messageService.ShowError(message);
    }

    private bool ShowConfirmation(string message, string title = "确认")
    {
        return _messageService.ShowConfirmation(message, title);
    }

    private partial void OnSelectedChannelChanged(UpStreamChannelItemViewModel? oldValue, UpStreamChannelItemViewModel? newValue)
    {
        // 当用户选择已存在的渠道，加载详细信息以便编辑
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= SelectedChannel_PropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += SelectedChannel_PropertyChanged;
        }

        // Update commands availability
        UpdateCommandStates();
    }

    private void SelectedChannel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // 当 SelectedChannel 的编辑状态或其它导致命令可用性变化的属性改变时，刷新命令状态
        if (e.PropertyName == nameof(UpStreamChannelItemViewModel.IsEditing) || e.PropertyName == nameof(UpStreamChannelItemViewModel.IsDirty))
        {
            UpdateCommandStates();
        }
    }

    // --- 命令 (Commands) ---
    [RelayCommand]
    public async Task LoadChannelsAsync()
    {
        IsLoading = true;
        try
        {
            var channels = await _service.GetAllAsync();
            Channels.Clear();
            foreach (var channel in channels)
            {
                var vm = new UpStreamChannelItemViewModel(channel)
                {
                    IsNew = false,
                    IsEditing = false,
                    IsDirty = false
                };
                Channels.Add(vm);
            }
            // 若没有选中项，则默认选择第一个
            if (Channels.Count > 0 && SelectedChannel == null)
            {
                SelectedChannel = Channels[0];
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"加载渠道列表时发生异常: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void AddChannel()
    {
        var newChannel = new UpStreamChannel
        {
            Id = 0,
            Name = "",
            Url = "",
            Multiplier = 1.0
        };

        var vm = new UpStreamChannelItemViewModel(newChannel)
        {
            IsNew = true,
            IsEditing = true
        };
        Channels.Insert(0, vm);
        SelectedChannel = vm;
    }

    [RelayCommand(CanExecute = nameof(CanCopyChannel))]
    public void CopyChannel()
    {
        if (SelectedChannel == null) return;

        var src = SelectedChannel.GetModel();
        var newChannel = new UpStreamChannel
        {
            Id = 0,
            Name = src.Name + "_Copy",
            Url = src.Url,
            Multiplier = src.Multiplier
        };

        var vm = new UpStreamChannelItemViewModel(newChannel)
        {
            IsNew = true,
            IsEditing = true
        };
        Channels.Insert(0, vm);
        SelectedChannel = vm;
    }

    private bool CanCopyChannel() => SelectedChannel != null;

    [RelayCommand(CanExecute = nameof(CanStartEdit))]
    public void StartEditChannel()
    {
        if (SelectedChannel == null) return;
        SelectedChannel.IsEditing = true;
    }

    [RelayCommand(CanExecute = nameof(CanSaveChannel))]
    public async Task SaveChannelAsync()
    {
        if (SelectedChannel == null) return;

        try
        {
            if (SelectedChannel.IsNew)
            {
                if (string.IsNullOrWhiteSpace(SelectedChannel.Name))
                {
                    ShowErrorMessage("渠道名称不能为空");
                    return;
                }
                if (string.IsNullOrWhiteSpace(SelectedChannel.Url))
                {
                    ShowErrorMessage("URL不能为空");
                    return;
                }
                var channel = SelectedChannel.GetModel();
                await _service.AddAsync(channel);
                SelectedChannel.IsNew = false;
                SelectedChannel.IsEditing = false;
                SelectedChannel.IsDirty = false;
                // Reload list and re-select new channel by name
                var createdName = SelectedChannel.Name;
                await LoadChannelsAsync();
                SelectedChannel = Channels.FirstOrDefault(c => c.Name == createdName);
            }
            else
            {
                var channel = SelectedChannel.GetModel();
                await _service.UpdateAsync(channel);
                SelectedChannel.IsEditing = false;
                SelectedChannel.IsDirty = false;
                var updatedId = SelectedChannel.Id;
                await LoadChannelsAsync();
                SelectedChannel = Channels.FirstOrDefault(c => c.Id == updatedId);
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"保存渠道时发生异常: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelEdit))]
    public async Task CancelEditAsync()
    {
        if (SelectedChannel == null) return;

        if (SelectedChannel.IsNew)
        {
            Channels.Remove(SelectedChannel);
            SelectedChannel = Channels.FirstOrDefault();
        }
        else
        {
            // reload details to undo changes
            var channel = await _service.GetByIdAsync(SelectedChannel.Id);
            if (channel != null)
            {
                SelectedChannel.Name = channel.Name;
                SelectedChannel.Url = channel.Url;
                SelectedChannel.Multiplier = channel.Multiplier;
            }
            SelectedChannel.IsEditing = false;
            SelectedChannel.IsDirty = false;
        }
    }

    private bool CanStartEdit() => SelectedChannel != null && !SelectedChannel.IsEditing;

    private bool CanSaveChannel() => SelectedChannel != null && SelectedChannel.IsEditing;

    private bool CanCancelEdit() => SelectedChannel != null && SelectedChannel.IsEditing;

    [RelayCommand(CanExecute = nameof(IsChannelSelected))]
    public void EditChannel()
    {
        if (SelectedChannel == null) return;
        SelectedChannel.IsEditing = true;
    }

    private bool CanDeleteChannel() => SelectedChannel != null && !SelectedChannel.IsEditing;

    [RelayCommand(CanExecute = nameof(CanDeleteChannel))]
    public async Task DeleteChannelAsync()
    {
        if (SelectedChannel == null) return;
        var result = ShowConfirmation($"确定要删除渠道 '{SelectedChannel.Name}' 吗？", "确认删除");
        if (!result) return;

        SelectedChannel.IsBusy = true;
        try
        {
            await _service.DeleteAsync(SelectedChannel.Id);
            Channels.Remove(SelectedChannel);
            SelectedChannel = Channels.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"删除时发生异常: {ex.Message}");
        }
        finally
        {
            if (SelectedChannel != null)
            {
                SelectedChannel.IsBusy = false;
            }
        }
    }
}