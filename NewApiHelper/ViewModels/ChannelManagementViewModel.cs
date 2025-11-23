using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using NewApiHelper.Data;
using NewApiHelper.Models;
using NewApiHelper.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace NewApiHelper.ViewModels;

public partial class ChannelManagementViewModel : ObservableObject
{
    private readonly IChannelService _channelService;
    private readonly IMessageService _messageService;
    private readonly AppDbContext _dbContext;

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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddModelToSelectedCommand))]
    private string _newModelText = string.Empty;

    [ObservableProperty]
    private bool _hasChannels;

    public ChannelManagementViewModel(IChannelService channelService, IMessageService messageService, AppDbContext dbContext)
    {
        _channelService = channelService ?? throw new ArgumentNullException(nameof(channelService));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _channels = new ObservableCollection<ChannelItemViewModel>();
        _channels.CollectionChanged += OnChannelsChanged;
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

    private void OnChannelsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        HasChannels = Channels.Any();
    }

    private void UpdateCommandStates()
    {
        LoadChannelsCommand.NotifyCanExecuteChanged();
        EditChannelCommand.NotifyCanExecuteChanged();
        StartEditChannelCommand.NotifyCanExecuteChanged();
        DeleteChannelCommand.NotifyCanExecuteChanged();
        TestChannelCommand.NotifyCanExecuteChanged();
        SaveChannelCommand.NotifyCanExecuteChanged();
        CancelEditCommand.NotifyCanExecuteChanged();
        AddModelToSelectedCommand.NotifyCanExecuteChanged();
        RemoveModelFromSelectedCommand.NotifyCanExecuteChanged();
        CopyChannelCommand.NotifyCanExecuteChanged();
    }

    private void ShowErrorMessage(string message)
    {
        _messageService.ShowError(message);
    }

    private bool ShowConfirmation(string message, string title = "确认")
    {
        return _messageService.ShowConfirmation(message, title);
    }

    [RelayCommand(CanExecute = nameof(CanAddModelToSelected))]
    public void AddModelToSelected(string model)
    {
        if (SelectedChannel == null || string.IsNullOrWhiteSpace(model)) return;
        SelectedChannel.AddModel(model);
        // clear the model input field
        NewModelText = string.Empty;
    }

    private bool CanAddModelToSelected(string model)
    {
        return SelectedChannel != null && SelectedChannel.IsEditing && !string.IsNullOrWhiteSpace(model);
    }

    [RelayCommand(CanExecute = nameof(CanRemoveModelFromSelected))]
    public void RemoveModelFromSelected(string model)
    {
        if (SelectedChannel == null || string.IsNullOrWhiteSpace(model)) return;
        SelectedChannel.RemoveModel(model);
    }

    private bool CanRemoveModelFromSelected(string model)
    {
        return SelectedChannel != null && SelectedChannel.IsEditing && !string.IsNullOrWhiteSpace(model);
    }

    private bool CanEditSelected() => SelectedChannel != null && SelectedChannel.IsEditing;

    partial void OnSelectedChannelChanged(ChannelItemViewModel? oldValue, ChannelItemViewModel? newValue)
    {
        // 当用户选择已存在的渠道，加载详细信息以便编辑
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= SelectedChannel_PropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += SelectedChannel_PropertyChanged;
            if (!newValue.IsNew)
            {
                _ = LoadChannelDetailsAsync(newValue);
            }
        }

        // Update commands availability
        UpdateCommandStates();
    }

    private void SelectedChannel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // 当 SelectedChannel 的编辑状态或其它导致命令可用性变化的属性改变时，刷新命令状态
        if (e.PropertyName == nameof(ChannelItemViewModel.IsEditing) || e.PropertyName == nameof(ChannelItemViewModel.IsDirty))
        {
            UpdateCommandStates();
        }
    }

    private async Task LoadChannelDetailsAsync(ChannelItemViewModel vm)
    {
        try
        {
            var response = await _channelService.GetChannelByIdAsync(vm.Id);
            if (response.Success && response.Data != null)
            {
                // 更新 vm 的字段
                var model = response.Data;
                // 直接替换底层字段 via reflection? We'll copy values
                vm.BaseUrl = model.BaseUrl;
                vm.Type = model.Type;
                vm.Group = model.Group;
                vm.Priority = model.Priority;
                vm.Weight = model.Weight;
                vm.TestStatusMessage = vm.TestStatusMessage; // keep existing
                vm.Key = model.Key;
                vm.ModelMapping = model.ModelMapping;
                // sync models list
                if (!string.IsNullOrWhiteSpace(model.Models))
                {
                    vm.ModelsList.Clear();
                    foreach (var m in model.Models.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries))
                    {
                        vm.ModelsList.Add(m);
                    }
                    // ensure underlying model string is set
                    vm.Models = model.Models;
                }
            }
            else
            {
                ShowErrorMessage(response.Message ?? "无法加载渠道详情");
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"加载渠道详情时发生异常: {ex.Message}");
        }
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
                    var vm = new ChannelItemViewModel(channel)
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
            else
            {
                // 显示错误信息
                ShowErrorMessage(apiResponse.Message ?? "加载渠道列表失败。");
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
        var newChannel = new Models.Channel
        {
            Id = 0,
            Name = "",
            Type = 0,
            Group = "default",
            Priority = 0,
            Weight = 0,
            BaseUrl = string.Empty,
            Models = string.Empty
        };

        var vm = new ChannelItemViewModel(newChannel)
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
        var newChannel = new Models.Channel
        {
            Id = 0,
            Name = src.Name + "_Copy",
            Type = src.Type,
            Group = src.Group,
            Priority = src.Priority,
            Weight = src.Weight,
            BaseUrl = src.BaseUrl,
            Models = src.Models,
            ModelMapping = src.ModelMapping,
            Key = src.Key
        };

        var vm = new ChannelItemViewModel(newChannel)
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
                if (string.IsNullOrWhiteSpace(SelectedChannel.Key))
                {
                    ShowErrorMessage("渠道 Key 是必需的");
                    return;
                }
                var addReq = SelectedChannel.ToAddRequest();
                var response = await _channelService.AddChannelAsync(addReq);
                if (response.Success)
                {
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
                    ShowErrorMessage(response.Message ?? "新增渠道失败");
                }
            }
            else
            {
                var updateReq = SelectedChannel.ToUpdateRequest();
                var response = await _channelService.UpdateChannelAsync(updateReq);
                if (response.Success)
                {
                    SelectedChannel.IsEditing = false;
                    SelectedChannel.IsDirty = false;
                    var updatedId = SelectedChannel.Id;
                    await LoadChannelsAsync();
                    SelectedChannel = Channels.FirstOrDefault(c => c.Id == updatedId);
                }
                else
                {
                    ShowErrorMessage(response.Message ?? "更新渠道失败");
                }
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
            await LoadChannelDetailsAsync(SelectedChannel);
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
            var response = await _channelService.DeleteChannelAsync(SelectedChannel.Id);
            if (response.Success)
            {
                Channels.Remove(SelectedChannel);
                SelectedChannel = Channels.FirstOrDefault();
            }
            else
            {
                ShowErrorMessage(response.Message ?? "删除失败。");
            }
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

    [RelayCommand]
    public async Task DeleteAllChannelsAsync()
    {
        if (!Channels.Any()) return;
        var result = ShowConfirmation("确定要删除所有渠道吗？此操作不可撤销。", "确认删除所有");
        if (!result) return;

        IsLoading = true;
        try
        {
            var ids = Channels.Select(c => c.Id).ToList();
            var response = await _channelService.DeleteChannelsAsync(ids);
            if (response.Success)
            {
                Channels.Clear();
                SelectedChannel = null;
                _messageService.ShowInfo($"成功删除了 {response.Data} 个渠道。");
            }
            else
            {
                ShowErrorMessage(response.Message ?? "删除所有渠道失败。");
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"删除所有渠道时发生异常: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ImportChannelsAsync()
    {
        // 获取所有现有渠道
        var existingResponse = await _channelService.GetChannelsAsync(1, 10000); // 假设足够大以获取所有
        if (!existingResponse.Success || existingResponse.Data == null)
        {
            ShowErrorMessage("无法获取现有渠道列表");
            return;
        }
        var existingChannels = existingResponse.Data.Items;
        var existingDict = existingChannels.ToDictionary(c => c.Name);

        var modelSyncs = _dbContext.ModelSyncs.Include(m => m.Upstream).Include(m => m.UpstreamGroup).Include(m => m.TestResults).ToList();
        var requests = _channelService.GenerateChannels(modelSyncs);

        int added = 0, updated = 0, unchanged = 0;
        var importedNames = new HashSet<string>();

        foreach (var request in requests)
        {
            importedNames.Add(request.Name);
            if (existingDict.TryGetValue(request.Name, out var existing))
            {
                // 比较模型
                var newModels = NormalizeModels(request.Models);
                var existingModels = NormalizeModels(existing.Models);
                if (newModels != existingModels)
                {
                    // 更新模型
                    var updateReq = new UpdateChannelRequest
                    {
                        Id = existing.Id,
                        Models = request.Models
                    };
                    var response = await _channelService.UpdateChannelAsync(updateReq);
                    if (response.Success)
                    {
                        updated++;
                    }
                    else
                    {
                        ShowErrorMessage($"更新渠道 '{request.Name}' 失败: {response.Message}");
                    }
                }
                else
                {
                    unchanged++;
                }
            }
            else
            {
                // 添加
                var response = await _channelService.AddChannelAsync(request);
                if (response.Success)
                {
                    added++;
                }
                else
                {
                    ShowErrorMessage($"添加渠道 '{request.Name}' 失败: {response.Message}");
                }
            }
        }

        // 找出多余的渠道
        var extraChannels = existingChannels.Where(c => !importedNames.Contains(c.Name)).Select(c => c.Name).ToList();

        // 重新加载渠道列表
        await LoadChannelsAsync();

        // 显示统计信息
        string message = $"添加了 {added} 个渠道，更新了 {updated} 个渠道，{unchanged} 个渠道模型未变化。";
        if (extraChannels.Any())
        {
            message += $" 多余的渠道: {string.Join(", ", extraChannels)}";
        }
        _messageService.ShowInfo(message);
    }

    private string NormalizeModels(string? models)
    {
        if (string.IsNullOrEmpty(models)) return "";
        return string.Join(",", models.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).OrderBy(m => m));
    }

    private bool CanTestChannel() => SelectedChannel != null && !SelectedChannel.IsEditing;

    [RelayCommand(CanExecute = nameof(CanTestChannel))]
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
        catch (Exception ex)
        {
            SelectedChannel.UpdateTestResult(0, false, $"测试异常: {ex.Message}");
        }
        finally
        {
            SelectedChannel.IsBusy = false;
        }
    }
}