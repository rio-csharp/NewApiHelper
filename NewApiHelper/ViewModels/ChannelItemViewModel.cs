using CommunityToolkit.Mvvm.ComponentModel;
using NewApiHelper.Models;

namespace NewApiHelper.ViewModels;

public partial class ChannelItemViewModel : ObservableObject
{
    // 核心数据模型
    private readonly Channel _channel;

    public int Id => _channel.Id;

    // 可编辑属性：直接读写底层 _channel 并通知变更
    public string Name
    {
        get => _channel.Name;
        set
        {
            if (_channel.Name != value)
            {
                _channel.Name = value ?? string.Empty;
                OnPropertyChanged(nameof(Name));
                IsDirty = true;
            }
        }
    }

    public int Type
    {
        get => _channel.Type;
        set
        {
            if (_channel.Type != value)
            {
                _channel.Type = value;
                OnPropertyChanged(nameof(Type));
                IsDirty = true;
            }
        }
    }

    public string Group
    {
        get => _channel.Group;
        set
        {
            if (_channel.Group != value)
            {
                _channel.Group = value ?? string.Empty;
                OnPropertyChanged(nameof(Group));
                IsDirty = true;
            }
        }
    }

    public int Priority
    {
        get => _channel.Priority;
        set
        {
            if (_channel.Priority != value)
            {
                _channel.Priority = value;
                OnPropertyChanged(nameof(Priority));
                IsDirty = true;
            }
        }
    }

    // --- UI相关属性和计算属性 ---

    [ObservableProperty]
    private bool _isBusy; // 用于表示此行项目是否正在执行操作（如测试、删除）

    [ObservableProperty]
    private string? _testStatusMessage; // 用于显示测试结果

    // 编辑相关标志
    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isDirty;

    public int Status => _channel.Status;

    // 将状态码转换为可读文本
    public string StatusText => Status switch
    {
        1 => "启用",
        2 => "禁用",
        _ => "未知"
    };

    public int ResponseTime => _channel.ResponseTime;

    public string? Key
    {
        get => _channel.Key;
        set
        {
            if (_channel.Key != value)
            {
                _channel.Key = value;
                OnPropertyChanged(nameof(Key));
                IsDirty = true;
            }
        }
    }

    public string? BaseUrl
    {
        get => _channel.BaseUrl;
        set
        {
            if (_channel.BaseUrl != value)
            {
                _channel.BaseUrl = value;
                OnPropertyChanged(nameof(BaseUrl));
                IsDirty = true;
            }
        }
    }

    public string? Models
    {
        get => _channel.Models;
        set
        {
            if (_channel.Models != value)
            {
                _channel.Models = value ?? string.Empty;
                OnPropertyChanged(nameof(Models));
                // sync ModelsList
                ModelsList.Clear();
                if (!string.IsNullOrWhiteSpace(_channel.Models))
                {
                    foreach (var m in _channel.Models.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        ModelsList.Add(m);
                    }
                }
                IsDirty = true;
            }
        }
    }

    public int Weight
    {
        get => _channel.Weight;
        set
        {
            if (_channel.Weight != value)
            {
                _channel.Weight = value;
                OnPropertyChanged(nameof(Weight));
                IsDirty = true;
            }
        }
    }

    public string? ModelMapping
    {
        get => _channel.ModelMapping;
        set
        {
            if (_channel.ModelMapping != value)
            {
                _channel.ModelMapping = value;
                OnPropertyChanged(nameof(ModelMapping));
                IsDirty = true;
            }
        }
    }

    public int StatusInt
    {
        get => _channel.Status;
        set
        {
            if (_channel.Status != value)
            {
                _channel.Status = value;
                OnPropertyChanged(nameof(StatusInt));
                OnPropertyChanged(nameof(StatusText));
                IsDirty = true;
            }
        }
    }

    // 将Unix时间戳转换为可读的本地时间字符串
    public string TestTimeDisplay => (_channel.TestTime > 0)
        ? DateTimeOffset.FromUnixTimeSeconds(_channel.TestTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
        : "未测试";

    public ChannelItemViewModel(Channel channel)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        ModelsList = new System.Collections.ObjectModel.ObservableCollection<string>();
        if (!string.IsNullOrWhiteSpace(_channel.Models))
        {
            foreach (var m in _channel.Models.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                ModelsList.Add(m);
            }
        }
    }

    public System.Collections.ObjectModel.ObservableCollection<string> ModelsList { get; }

    public void AddModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model)) return;
        ModelsList.Add(model.Trim());
        _channel.Models = string.Join(",", ModelsList);
        IsDirty = true;
        OnPropertyChanged(nameof(ModelsList));
    }

    public void RemoveModel(string model)
    {
        if (ModelsList.Remove(model))
        {
            _channel.Models = string.Join(",", ModelsList);
            IsDirty = true;
            OnPropertyChanged(nameof(ModelsList));
        }
    }

    /// <summary>
    /// 用于构造新增请求
    /// </summary>
    /// <returns></returns>
    public AddChannelRequest ToAddRequest()
    {
        return new AddChannelRequest
        {
            Name = _channel.Name,
            Type = _channel.Type,
            Key = _channel.Key ?? string.Empty,
            BaseUrl = _channel.BaseUrl,
            Models = _channel.Models,
            ModelMapping = _channel.ModelMapping,
            Groups = new System.Collections.Generic.List<string> { _channel.Group ?? "default" },
            Priority = _channel.Priority,
            Weight = _channel.Weight
        };
    }

    public UpdateChannelRequest ToUpdateRequest()
    {
        return new UpdateChannelRequest
        {
            Id = _channel.Id,
            Name = string.IsNullOrWhiteSpace(_channel.Name) ? null : _channel.Name,
            Priority = _channel.Priority,
            Weight = _channel.Weight
            ,
            BaseUrl = _channel.BaseUrl,
            Models = string.IsNullOrWhiteSpace(_channel.Models) ? null : _channel.Models,
            ModelMapping = string.IsNullOrWhiteSpace(_channel.ModelMapping) ? null : _channel.ModelMapping,
            Key = string.IsNullOrWhiteSpace(_channel.Key) ? null : _channel.Key,
            Group = string.IsNullOrWhiteSpace(_channel.Group) ? null : _channel.Group
        };
    }

    /// <summary>
    /// 获取用于更新或编辑的完整Channel对象。
    /// </summary>
    /// <returns></returns>
    public Channel GetModel() => _channel;

    /// <summary>
    /// 当测试完成后，更新UI相关的属性。
    /// </summary>
    public void UpdateTestResult(int responseTime, bool success, string message)
    {
        _channel.ResponseTime = responseTime;
        // 因为 ResponseTime 和 TestStatusMessage 是可观察的，所以需要通知UI更新
        OnPropertyChanged(nameof(ResponseTime));
        TestStatusMessage = message;
        _channel.TestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        OnPropertyChanged(nameof(TestTimeDisplay));
    }
}