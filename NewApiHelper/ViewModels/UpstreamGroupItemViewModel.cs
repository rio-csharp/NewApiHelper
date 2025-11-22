using CommunityToolkit.Mvvm.ComponentModel;
using NewApiHelper.Models;
using System.Collections.ObjectModel;

namespace NewApiHelper.ViewModels;

public partial class UpstreamGroupItemViewModel : ObservableObject
{
    private readonly UpstreamGroup _group;

    public UpstreamGroupItemViewModel(UpstreamGroup group, ObservableCollection<Upstream> availableUpstreams)
    {
        _group = group;
        _availableUpstreams = availableUpstreams;
        _selectedUpstream = _availableUpstreams.FirstOrDefault(u => u.Id == _group.UpstreamId);
    }

    public int Id => _group.Id;

    public string Name
    {
        get => _group.Name;
        set
        {
            if (_group.Name != value)
            {
                _group.Name = value ?? string.Empty;
                OnPropertyChanged(nameof(Name));
                IsDirty = true;
            }
        }
    }

    public int UpstreamId
    {
        get => _group.UpstreamId;
        set
        {
            if (_group.UpstreamId != value)
            {
                _group.UpstreamId = value;
                OnPropertyChanged(nameof(UpstreamId));
                IsDirty = true;
            }
        }
    }

    public double GroupMultiplier
    {
        get => _group.GroupRatio;
        set
        {
            if (_group.GroupRatio != value)
            {
                _group.GroupRatio = value;
                OnPropertyChanged(nameof(GroupMultiplier));
                IsDirty = true;
            }
        }
    }

    public string Key
    {
        get => _group.Key;
        set
        {
            if (_group.Key != value)
            {
                _group.Key = value ?? string.Empty;
                OnPropertyChanged(nameof(Key));
                IsDirty = true;
            }
        }
    }

    public DateTime CreatedAt => _group.CreatedAt;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isDirty;

    private readonly ObservableCollection<Upstream> _availableUpstreams;

    public ObservableCollection<Upstream> AvailableUpstreams => _availableUpstreams;

    private Upstream? _selectedUpstream;

    public Upstream? SelectedUpstream
    {
        get => _selectedUpstream;
        set
        {
            if (_selectedUpstream != value)
            {
                _selectedUpstream = value;
                if (_selectedUpstream != null)
                {
                    UpstreamId = _selectedUpstream.Id;
                }
                OnPropertyChanged();
                IsDirty = true;
            }
        }
    }

    public UpstreamGroup GetModel() => _group;
}