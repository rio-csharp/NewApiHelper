using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using NewApiHelper.Data;
using NewApiHelper.Models;
using NewApiHelper.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace NewApiHelper.ViewModels;

public class ModelSyncViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly IModelSyncImportService _importService;
    private readonly ITestService _testService;

    public ObservableCollection<ModelSync> Items { get; } = new();
    public ObservableCollection<ModelSync> FilteredItems { get; } = new();
    public ObservableCollection<Upstream> Upstreams { get; } = new();
    public ObservableCollection<UpstreamGroup> UpstreamGroups { get; } = new();
    public ObservableCollection<UpstreamGroup> FilteredUpstreamGroups { get; } = new();

    private readonly Upstream allUpstream = new Upstream { Name = "All", Id = -1 };
    private readonly UpstreamGroup allUpstreamGroup = new UpstreamGroup { Name = "All", Id = -1, UpstreamId = -1 };

    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand ImportCommand { get; }
    public IRelayCommand SearchCommand { get; }
    public IRelayCommand TestCommand { get; }
    public IRelayCommand TestFailedCommand { get; }

    public ObservableCollection<ModelSync> SelectedModelSyncs { get; } = new();

    private bool _isTesting;

    public bool IsTesting
    {
        get => _isTesting;
        set => SetProperty(ref _isTesting, value);
    }

    private string _testStatus = string.Empty;

    public string TestStatus
    {
        get => _testStatus;
        set => SetProperty(ref _testStatus, value);
    }

    private Upstream? _selectedUpstream;

    public Upstream? SelectedUpstream
    {
        get => _selectedUpstream;
        set
        {
            SetProperty(ref _selectedUpstream, value);
            UpdateFilteredUpstreamGroups();
            UpdateFilteredItems();
            // 切换上游时，上游组默认选ALL
            SelectedUpstreamGroup = allUpstreamGroup;
        }
    }

    private UpstreamGroup? _selectedUpstreamGroup;

    public UpstreamGroup? SelectedUpstreamGroup
    {
        get => _selectedUpstreamGroup;
        set
        {
            SetProperty(ref _selectedUpstreamGroup, value);
            UpdateFilteredItems();
        }
    }

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            UpdateFilteredItems();
        }
    }

    public ModelSyncViewModel(AppDbContext context, IModelSyncImportService importService, ITestService testService)
    {
        _context = context;
        _importService = importService;
        _testService = testService;
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
        ImportCommand = new RelayCommand(async () => await ImportAsync());
        SearchCommand = new RelayCommand(UpdateFilteredItems);
        TestCommand = new RelayCommand(async () => await TestAsync());
        TestFailedCommand = new RelayCommand(async () => await TestFailedAsync());

        // 自动加载数据
        Task.Run(async () => await RefreshAsync());
    }

    private async Task RefreshAsync()
    {
        var items = await _context.ModelSyncs
            .Include(m => m.Upstream)
            .Include(m => m.UpstreamGroup)
            .Include(m => m.TestResults)
            .ToListAsync();

        var upstreams = await _context.UpStreams.ToListAsync();

        var upstreamGroups = await _context.UpstreamGroups.ToListAsync();

        Application.Current.Dispatcher.Invoke(() =>
        {
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            Upstreams.Clear();
            Upstreams.Add(allUpstream);
            foreach (var u in upstreams)
            {
                Upstreams.Add(u);
            }

            UpstreamGroups.Clear();
            UpstreamGroups.Add(allUpstreamGroup);
            foreach (var ug in upstreamGroups)
            {
                UpstreamGroups.Add(ug);
            }

            UpdateFilteredUpstreamGroups();
            UpdateFilteredItems();

            // 设置默认选择为 All
            SelectedUpstream = allUpstream;
            SelectedUpstreamGroup = allUpstreamGroup;
        });
    }

    private void UpdateFilteredItems()
    {
        var filtered = Items.Where(m =>
            (SelectedUpstream == null || SelectedUpstream.Id == -1 || m.UpstreamId == SelectedUpstream.Id) &&
            (SelectedUpstreamGroup == null || SelectedUpstreamGroup.Id == -1 || m.UpstreamGroupId == SelectedUpstreamGroup.Id) &&
            (string.IsNullOrEmpty(SearchText) || m.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        );
        FilteredItems.Clear();
        foreach (var item in filtered)
        {
            FilteredItems.Add(item);
        }
    }

    private void UpdateFilteredUpstreamGroups()
    {
        FilteredUpstreamGroups.Clear();
        FilteredUpstreamGroups.Add(allUpstreamGroup);
        if (SelectedUpstream != null && SelectedUpstream.Id != -1)
        {
            foreach (var ug in UpstreamGroups.Where(ug => ug.UpstreamId == SelectedUpstream.Id))
            {
                FilteredUpstreamGroups.Add(ug);
            }
        }
        else
        {
            foreach (var ug in UpstreamGroups.Where(ug => ug.Id != -1))
            {
                FilteredUpstreamGroups.Add(ug);
            }
        }
        if (SelectedUpstreamGroup != null && SelectedUpstreamGroup.Id != -1 && !FilteredUpstreamGroups.Contains(SelectedUpstreamGroup))
        {
            SelectedUpstreamGroup = null;
        }
    }

    private async Task ImportAsync()
    {
        var upstreams = await _context.UpStreams.ToListAsync();

        var tasks = upstreams.Select(async upstream =>
        {
            var upstreamGroups = await _context.UpstreamGroups
                .Where(ug => ug.UpstreamId == upstream.Id)
                .ToListAsync();
            await _importService.ImportAsync(upstream, upstreamGroups);
        }).ToList();

        await Task.WhenAll(tasks);

        await RefreshAsync();
    }

    private async Task TestAsync()
    {
        var modelsToTest = SelectedModelSyncs.ToList();
        if (!modelsToTest.Any()) return;

        IsTesting = true;
        int completed = 0;
        TestStatus = $"正在测试 0/{modelsToTest.Count}";

        var semaphore = new SemaphoreSlim(5); // 限制并发最多5个

        var tasks = modelsToTest.Select(async model =>
        {
            await semaphore.WaitAsync();
            try
            {
                await TestModelAsync(model, "Test");
            }
            finally
            {
                semaphore.Release();
            }
            int current = Interlocked.Increment(ref completed);
            TestStatus = $"正在测试 {current}/{modelsToTest.Count}";
        }).ToList();

        await Task.WhenAll(tasks);

        IsTesting = false;
        TestStatus = string.Empty;
    }

    private async Task TestFailedAsync()
    {
        var modelsToTest = SelectedModelSyncs.Where(m =>
        {
            var latestTest = m.TestResults.OrderByDescending(t => t.TestTime).FirstOrDefault();
            return latestTest == null || latestTest.Status == TestResultStatus.Failed;
        }).ToList();
        if (!modelsToTest.Any()) return;

        IsTesting = true;
        int completed = 0;
        TestStatus = $"正在测试失败 0/{modelsToTest.Count}";

        var semaphore = new SemaphoreSlim(5); // 限制并发最多5个

        var tasks = modelsToTest.Select(async model =>
        {
            await semaphore.WaitAsync();
            try
            {
                await TestModelAsync(model, "TestFailed");
            }
            finally
            {
                semaphore.Release();
            }
            int current = Interlocked.Increment(ref completed);
            TestStatus = $"正在测试失败 {current}/{modelsToTest.Count}";
        }).ToList();

        await Task.WhenAll(tasks);

        IsTesting = false;
        TestStatus = string.Empty;
    }

    private async Task TestModelAsync(ModelSync model, string testType)
    {
        TestResultStatus status;
        string? errorMessage = null;

        if (testType == "TestFailed")
        {
            var latestTest = model.TestResults.OrderByDescending(t => t.TestTime).FirstOrDefault();
            if (latestTest != null && latestTest.Status != TestResultStatus.Failed)
            {
                return;
            }
        }

        if (model.FinalPrice > 100 || model.QuotaType != QuotaType.PayAsYouGo)
        {
            status = TestResultStatus.Skipped;
            errorMessage = "跳过：不符合测试条件";
        }
        else
        {
            // 进行真正的测试
            var serviceResult = await _testService.TestModelAsync(model.Upstream!, model.UpstreamGroup!, model.Name);
            if (serviceResult.Success)
            {
                status = TestResultStatus.Success;
            }
            else
            {
                status = TestResultStatus.Failed;
                errorMessage = serviceResult.ErrorMessage;
            }
        }

        var testRecord = new ModelTestResult
        {
            ModelSyncId = model.Id,
            Status = status,
            TestType = testType,
            ErrorMessage = errorMessage
        };

        _context.ModelTestResults.Add(testRecord);
        await _context.SaveChangesAsync();

        // 更新内存中的TestResults
        model.TestResults.Add(testRecord);
        model.NotifyLatestTestResultChanged();
    }

}