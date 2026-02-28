using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinClean.Helpers;
using WinClean.Localization;
using WinClean.Models;
using WinClean.Services;

namespace WinClean.ViewModels;

public partial class DiskAnalyzerViewModel : ObservableObject
{
    private readonly IDiskAnalyzerService _analyzer;
    private readonly IFileOperationService _fileOps;
    private CancellationTokenSource? _cts;

    public ObservableCollection<DriveDisplayItem> Drives { get; } = [];
    public ObservableCollection<FolderNode> RootNodes { get; } = [];

    [ObservableProperty]
    private DriveDisplayItem? _selectedDrive;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _statusText = LangProvider.S("DA_DefaultStatus");

    [ObservableProperty]
    private string _currentScanPath = string.Empty;

    private string? _statusKey = "DA_DefaultStatus";

    public DiskAnalyzerViewModel(IDiskAnalyzerService analyzer, IFileOperationService fileOps)
    {
        _analyzer = analyzer;
        _fileOps = fileOps;
        LoadDrives();
        LangProvider.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "Item[]" && _statusKey is not null)
                StatusText = LangProvider.S(_statusKey);
        };
    }

    private void LoadDrives()
    {
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
        {
            Drives.Add(new DriveDisplayItem
            {
                Name = $"{drive.Name} ({drive.VolumeLabel})",
                Path = drive.RootDirectory.FullName,
                TotalSize = drive.TotalSize,
                FreeSpace = drive.AvailableFreeSpace
            });
        }
        if (Drives.Count > 0) SelectedDrive = Drives[0];
    }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        if (IsScanning || SelectedDrive is null) return;

        _cts = new CancellationTokenSource();
        IsScanning = true;
        RootNodes.Clear();
        _statusKey = null;
        StatusText = LangProvider.S("DA_Analyzing");

        try
        {
            var progress = new Progress<ScanProgress>(p => CurrentScanPath = p.CurrentPath);
            var root = await _analyzer.AnalyzeAsync(SelectedDrive.Path, progress, _cts.Token);

            RootNodes.Add(root);
            StatusText = LangProvider.F("DA_AnalyzeComplete", root.Name, FileSizeFormatter.Format(root.Size));
        }
        catch (OperationCanceledException)
        {
            StatusText = LangProvider.S("DA_AnalyzeCancelled");
        }
        finally
        {
            IsScanning = false;
            CurrentScanPath = string.Empty;
        }
    }

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();

    [RelayCommand]
    private void OpenFolder(FolderNode? node)
    {
        if (node is not null)
            _fileOps.OpenFolder(node.FullPath);
    }

    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = LangProvider.S("DA_SelectFolder")
        };
        if (dialog.ShowDialog() == true)
        {
            SelectedDrive = new DriveDisplayItem
            {
                Name = dialog.FolderName,
                Path = dialog.FolderName,
                TotalSize = 0,
                FreeSpace = 0
            };
        }
    }
}
