using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using NewApiHelper.Data;
using NewApiHelper.Models;
using NewApiHelper.Services;
using System.Collections.ObjectModel;

namespace NewApiHelper.ViewModels;

public class ModelSyncViewModel : ObservableObject
{
    private readonly AppDbContext _context;
    private readonly IModelSyncImportService _importService;

    public ObservableCollection<ModelSync> Items { get; } = new();

    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand ImportCommand { get; }

    public ModelSyncViewModel(AppDbContext context, IModelSyncImportService importService)
    {
        _context = context;
        _importService = importService;
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
        ImportCommand = new RelayCommand(async () => await ImportAsync());
    }

    private async Task RefreshAsync()
    {
        var items = await _context.ModelSyncs
            .Include(m => m.Upstream)
            .Include(m => m.UpstreamGroup)
            .ToListAsync();
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    private async Task ImportAsync()
    {
        // TODO: Implement import logic with file selection
        // For now, this is a placeholder
    }
}