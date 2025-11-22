using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewApiHelper.Models;
using NewApiHelper.Services;
using System.Collections.ObjectModel;

namespace NewApiHelper.ViewModels;

public partial class UpstreamGroupViewModel : ObservableObject
{
    private readonly IUpstreamGroupService _service;
    private readonly IMessageService _messageService;
    private readonly IUpstreamService _upstreamService;

    [ObservableProperty]
    private ObservableCollection<UpstreamGroupItemViewModel> _groups;

    [ObservableProperty]
    private UpstreamGroupItemViewModel? _selectedGroup;

    [ObservableProperty]
    private ObservableCollection<Upstream> _availableUpstreams;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditGroupCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteGroupCommand))]
    private bool _isGroupSelected;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasGroups;

    [ObservableProperty]
    private bool _showAddButton;

    public UpstreamGroupViewModel(IUpstreamGroupService service, IMessageService messageService, IUpstreamService upstreamService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _upstreamService = upstreamService ?? throw new ArgumentNullException(nameof(upstreamService));
        _groups = new ObservableCollection<UpstreamGroupItemViewModel>();
        _availableUpstreams = new ObservableCollection<Upstream>();
        _groups.CollectionChanged += (s, e) =>
        {
            HasGroups = _groups.Count > 0;
            ShowAddButton = _groups.Count == 0;
        };
        ShowAddButton = true; // 初始时显示
        // 监听 SelectedGroup 的变化来更新 IsGroupSelected
        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SelectedGroup))
            {
                IsGroupSelected = SelectedGroup != null;
                UpdateCommandStates();
            }
        };
    }

    private void UpdateCommandStates()
    {
        LoadGroupsCommand.NotifyCanExecuteChanged();
        EditGroupCommand.NotifyCanExecuteChanged();
        StartEditGroupCommand.NotifyCanExecuteChanged();
        DeleteGroupCommand.NotifyCanExecuteChanged();
        SaveGroupCommand.NotifyCanExecuteChanged();
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

    partial void OnSelectedGroupChanged(UpstreamGroupItemViewModel? oldValue, UpstreamGroupItemViewModel? newValue)
    {
        // 当用户选择已存在的分组，加载详细信息以便编辑
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= SelectedGroup_PropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += SelectedGroup_PropertyChanged;
        }

        // Update commands availability
        UpdateCommandStates();
    }

    private void SelectedGroup_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // 当 SelectedGroup 的编辑状态或其它导致命令可用性变化的属性改变时，刷新命令状态
        if (e.PropertyName == nameof(UpstreamGroupItemViewModel.IsEditing) || e.PropertyName == nameof(UpstreamGroupItemViewModel.IsDirty))
        {
            UpdateCommandStates();
        }
    }

    // --- 命令 (Commands) ---
    [RelayCommand]
    public async Task LoadGroupsAsync()
    {
        IsLoading = true;
        try
        {
            // 加载上游列表
            var upstreams = await _upstreamService.GetAllAsync();
            AvailableUpstreams = new ObservableCollection<Upstream>(upstreams);

            var groups = await _service.GetAllAsync();
            Groups.Clear();
            foreach (var group in groups)
            {
                var vm = new UpstreamGroupItemViewModel(group, AvailableUpstreams)
                {
                    IsNew = false,
                    IsEditing = false,
                    IsDirty = false
                };
                Groups.Add(vm);
            }
            // 若没有选中项，则默认选择第一个
            if (Groups.Count > 0 && SelectedGroup == null)
            {
                SelectedGroup = Groups[0];
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"加载分组列表时发生异常: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void AddGroup()
    {
        var newGroup = new UpstreamGroup
        {
            Id = 0,
            Name = "",
            UpstreamId = 0,
            GroupMultiplier = 1.0,
            Key = ""
        };

        var vm = new UpstreamGroupItemViewModel(newGroup, AvailableUpstreams ?? new ObservableCollection<Upstream>())
        {
            IsNew = true,
            IsEditing = true
        };
        Groups.Insert(0, vm);
        SelectedGroup = vm;
    }

    [RelayCommand(CanExecute = nameof(CanCopyGroup))]
    public void CopyGroup()
    {
        if (SelectedGroup == null) return;

        var src = SelectedGroup.GetModel();
        var newGroup = new UpstreamGroup
        {
            Id = 0,
            Name = src.Name + "_Copy",
            UpstreamId = src.UpstreamId,
            GroupMultiplier = src.GroupMultiplier,
            Key = src.Key
        };

        var vm = new UpstreamGroupItemViewModel(newGroup, AvailableUpstreams ?? new ObservableCollection<Upstream>())
        {
            IsNew = true,
            IsEditing = true
        };
        Groups.Insert(0, vm);
        SelectedGroup = vm;
    }

    private bool CanCopyGroup() => SelectedGroup != null;

    [RelayCommand(CanExecute = nameof(CanStartEdit))]
    public void StartEditGroup()
    {
        if (SelectedGroup == null) return;
        SelectedGroup.IsEditing = true;
    }

    [RelayCommand(CanExecute = nameof(CanSaveGroup))]
    public async Task SaveGroupAsync()
    {
        if (SelectedGroup == null) return;

        try
        {
            if (SelectedGroup.IsNew)
            {
                if (string.IsNullOrWhiteSpace(SelectedGroup.Name))
                {
                    ShowErrorMessage("分组名称不能为空");
                    return;
                }
                if (string.IsNullOrWhiteSpace(SelectedGroup.Key))
                {
                    ShowErrorMessage("Key不能为空");
                    return;
                }
                var group = SelectedGroup.GetModel();
                await _service.AddAsync(group);
                SelectedGroup.IsNew = false;
                SelectedGroup.IsEditing = false;
                SelectedGroup.IsDirty = false;
                // Reload list and re-select new group by name
                var createdName = SelectedGroup.Name;
                await LoadGroupsAsync();
                SelectedGroup = Groups.FirstOrDefault(g => g.Name == createdName);
            }
            else
            {
                var group = SelectedGroup.GetModel();
                await _service.UpdateAsync(group);
                SelectedGroup.IsEditing = false;
                SelectedGroup.IsDirty = false;
                var updatedId = SelectedGroup.Id;
                await LoadGroupsAsync();
                SelectedGroup = Groups.FirstOrDefault(g => g.Id == updatedId);
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"保存分组时发生异常: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelEdit))]
    public async Task CancelEditAsync()
    {
        if (SelectedGroup == null) return;

        if (SelectedGroup.IsNew)
        {
            Groups.Remove(SelectedGroup);
            SelectedGroup = Groups.FirstOrDefault();
        }
        else
        {
            // reload details to undo changes
            var group = await _service.GetByIdAsync(SelectedGroup.Id);
            if (group != null)
            {
                SelectedGroup.Name = group.Name;
                SelectedGroup.UpstreamId = group.UpstreamId;
                SelectedGroup.GroupMultiplier = group.GroupMultiplier;
                SelectedGroup.Key = group.Key;
                // Re-select the upstream
                SelectedGroup.SelectedUpstream = AvailableUpstreams.FirstOrDefault(u => u.Id == group.UpstreamId);
            }
            SelectedGroup.IsEditing = false;
            SelectedGroup.IsDirty = false;
        }
    }

    private bool CanStartEdit() => SelectedGroup != null && !SelectedGroup.IsEditing;

    private bool CanSaveGroup() => SelectedGroup != null && SelectedGroup.IsEditing;

    private bool CanCancelEdit() => SelectedGroup != null && SelectedGroup.IsEditing;

    [RelayCommand(CanExecute = nameof(IsGroupSelected))]
    public void EditGroup()
    {
        if (SelectedGroup == null) return;
        SelectedGroup.IsEditing = true;
    }

    private bool CanDeleteGroup() => SelectedGroup != null && !SelectedGroup.IsEditing;

    [RelayCommand(CanExecute = nameof(CanDeleteGroup))]
    public async Task DeleteGroupAsync()
    {
        if (SelectedGroup == null) return;
        var result = ShowConfirmation($"确定要删除分组 '{SelectedGroup.Name}' 吗？", "确认删除");
        if (!result) return;

        SelectedGroup.IsBusy = true;
        try
        {
            await _service.DeleteAsync(SelectedGroup.Id);
            Groups.Remove(SelectedGroup);
            SelectedGroup = Groups.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"删除时发生异常: {ex.Message}");
        }
        finally
        {
            if (SelectedGroup != null)
            {
                SelectedGroup.IsBusy = false;
            }
        }
    }
}