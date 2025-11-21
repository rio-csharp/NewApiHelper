using CommunityToolkit.Mvvm.ComponentModel;
using NewApiHelper.Models;

namespace NewApiHelper.ViewModels;

public partial class UpStreamChannelItemViewModel : ObservableObject
{
    private readonly UpStreamChannel _channel;

    public UpStreamChannelItemViewModel(UpStreamChannel channel)
    {
        _channel = channel;
    }

    public int Id => _channel.Id;

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

    public string Url
    {
        get => _channel.Url;
        set
        {
            if (_channel.Url != value)
            {
                _channel.Url = value ?? string.Empty;
                OnPropertyChanged(nameof(Url));
                IsDirty = true;
            }
        }
    }

    public double Multiplier
    {
        get => _channel.Multiplier;
        set
        {
            if (_channel.Multiplier != value)
            {
                _channel.Multiplier = value;
                OnPropertyChanged(nameof(Multiplier));
                IsDirty = true;
            }
        }
    }

    public DateTime CreatedAt => _channel.CreatedAt;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isDirty;

    public UpStreamChannel GetModel() => _channel;
}