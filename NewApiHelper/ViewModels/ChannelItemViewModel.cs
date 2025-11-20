using CommunityToolkit.Mvvm.ComponentModel;
using NewApiHelper.Models;

namespace NewApiHelper.ViewModels;

public partial class ChannelItemViewModel : ObservableObject
{
    // 核心数据模型
    private readonly Channel _channel;

    public int Id => _channel.Id;
    public string Name => _channel.Name;
    public int Type => _channel.Type;
    public string Group => _channel.Group;
    public int Priority => _channel.Priority;

    // --- UI相关属性和计算属性 ---

    [ObservableProperty]
    private bool _isBusy; // 用于表示此行项目是否正在执行操作（如测试、删除）

    [ObservableProperty]
    private string? _testStatusMessage; // 用于显示测试结果

    public int Status => _channel.Status;

    // 将状态码转换为可读文本
    public string StatusText => Status switch
    {
        1 => "启用",
        2 => "禁用",
        _ => "未知"
    };

    public int ResponseTime => _channel.ResponseTime;

    // 将Unix时间戳转换为可读的本地时间字符串
    public string TestTimeDisplay => (_channel.TestTime > 0)
        ? DateTimeOffset.FromUnixTimeSeconds(_channel.TestTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
        : "未测试";

    public ChannelItemViewModel(Channel channel)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
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