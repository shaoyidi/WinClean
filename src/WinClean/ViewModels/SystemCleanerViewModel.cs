using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinClean.Helpers;
using WinClean.Localization;
using WinClean.Models;
using WinClean.Services;

namespace WinClean.ViewModels;

public partial class SystemCleanerViewModel : ObservableObject
{
    private readonly ICleanerService _cleanerService;
    private CancellationTokenSource? _cts;

    public ObservableCollection<CleanItem> CleanItems { get; } = [];

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _isCleaning;

    [ObservableProperty]
    private string _statusText = LangProvider.S("SC_DefaultStatus");

    [ObservableProperty]
    private string _currentScanPath = string.Empty;

    [ObservableProperty]
    private long _totalSize;

    [ObservableProperty]
    private int _totalFiles;

    public string TotalSizeText => FileSizeFormatter.Format(TotalSize);

    private string? _statusKey = "SC_DefaultStatus";

    public SystemCleanerViewModel(ICleanerService cleanerService)
    {
        _cleanerService = cleanerService;
        LangProvider.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "Item[]" && _statusKey is not null)
                StatusText = LangProvider.S(_statusKey);
        };
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        if (IsScanning) return;

        _cts = new CancellationTokenSource();
        IsScanning = true;
        CleanItems.Clear();
        TotalSize = 0;
        TotalFiles = 0;
        _statusKey = null;
        StatusText = LangProvider.S("SC_Scanning");

        try
        {
            var progress = new Progress<ScanProgress>(p =>
            {
                CurrentScanPath = p.CurrentPath;
            });

            var items = await _cleanerService.ScanAsync(progress, _cts.Token);

            foreach (var item in items)
            {
                CleanItems.Add(item);
                TotalSize += item.Size;
                TotalFiles += item.FileCount;
            }

            OnPropertyChanged(nameof(TotalSizeText));
            StatusText = LangProvider.F("SC_ScanComplete", TotalFiles, TotalSizeText);
        }
        catch (OperationCanceledException)
        {
            StatusText = LangProvider.S("SC_ScanCancelled");
        }
        catch (Exception ex)
        {
            StatusText = LangProvider.F("SC_ScanError", ex.Message);
        }
        finally
        {
            IsScanning = false;
            CurrentScanPath = string.Empty;
        }
    }

    [RelayCommand]
    private async Task CleanAsync()
    {
        if (IsCleaning || CleanItems.Count == 0) return;

        _cts = new CancellationTokenSource();
        IsCleaning = true;
        StatusText = LangProvider.S("SC_Cleaning");

        try
        {
            var progress = new Progress<ScanProgress>(p =>
            {
                CurrentScanPath = p.CurrentPath;
            });

            long cleaned = await _cleanerService.CleanAsync(CleanItems, progress, _cts.Token);
            StatusText = LangProvider.F("SC_CleanComplete", FileSizeFormatter.Format(cleaned));
            CleanItems.Clear();
            TotalSize = 0;
            TotalFiles = 0;
            OnPropertyChanged(nameof(TotalSizeText));
        }
        catch (OperationCanceledException)
        {
            StatusText = LangProvider.S("SC_CleanCancelled");
        }
        catch (Exception ex)
        {
            StatusText = LangProvider.F("SC_CleanError", ex.Message);
        }
        finally
        {
            IsCleaning = false;
            CurrentScanPath = string.Empty;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in CleanItems)
            item.IsSelected = true;
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var item in CleanItems)
            item.IsSelected = false;
    }
}
