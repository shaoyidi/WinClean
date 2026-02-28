using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinClean.Helpers;
using WinClean.Localization;
using WinClean.Models;
using WinClean.Services;

namespace WinClean.ViewModels;

public partial class LargeFileScannerViewModel : ObservableObject
{
    private readonly ILargeFileScannerService _scanner;
    private readonly IFileOperationService _fileOps;
    private CancellationTokenSource? _cts;

    public ObservableCollection<FileItem> Files { get; } = [];
    public ObservableCollection<DriveDisplayItem> Drives { get; } = [];

    [ObservableProperty]
    private DriveDisplayItem? _selectedDrive;

    [ObservableProperty]
    private string _customPath = string.Empty;

    [ObservableProperty]
    private long _minSize = 100 * 1024 * 1024; // 100 MB

    [ObservableProperty]
    private int _minSizeMB = 100;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _statusText = LangProvider.S("LF_DefaultStatus");

    [ObservableProperty]
    private string _currentScanPath = string.Empty;

    [ObservableProperty]
    private int _resultCount;

    [ObservableProperty]
    private long _totalSize;

    public string TotalSizeText => FileSizeFormatter.Format(TotalSize);

    [ObservableProperty]
    private string _filterType = "全部";

    [ObservableProperty]
    private string _diskDriveName = string.Empty;

    [ObservableProperty]
    private double _diskUsedPercent;

    [ObservableProperty]
    private string _diskInfoText = string.Empty;

    [ObservableProperty]
    private bool _hasDiskInfo;

    public List<string> FilterTypes { get; } = ["全部", "视频", "压缩包", "安装包", "文档", "图片", "其他"];

    private string? _statusKey = "LF_DefaultStatus";

    public LargeFileScannerViewModel(ILargeFileScannerService scanner, IFileOperationService fileOps)
    {
        _scanner = scanner;
        _fileOps = fileOps;
        LoadDrives();
        LangProvider.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "Item[]" && _statusKey is not null)
                StatusText = LangProvider.S(_statusKey);
        };
    }

    private void RefreshDiskUsage()
    {
        var scanPath = string.IsNullOrWhiteSpace(CustomPath) ? SelectedDrive?.Path : CustomPath;
        if (string.IsNullOrWhiteSpace(scanPath)) return;

        try
        {
            var root = Path.GetPathRoot(scanPath);
            if (root is null) return;
            var drive = new DriveInfo(root);
            if (!drive.IsReady) return;
            var total = drive.TotalSize;
            var free = drive.AvailableFreeSpace;
            DiskDriveName = drive.Name;
            DiskUsedPercent = total > 0 ? (double)(total - free) / total * 100.0 : 0;
            DiskInfoText = LangProvider.F("LF_DiskInfo", FileSizeFormatter.Format(free), FileSizeFormatter.Format(total));
            HasDiskInfo = true;
        }
        catch
        {
            HasDiskInfo = false;
        }
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

    partial void OnMinSizeMBChanged(int value)
    {
        MinSize = (long)value * 1024 * 1024;
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        if (IsScanning) return;

        var scanPath = string.IsNullOrWhiteSpace(CustomPath)
            ? SelectedDrive?.Path
            : CustomPath;

        if (string.IsNullOrWhiteSpace(scanPath)) return;

        _cts = new CancellationTokenSource();
        IsScanning = true;
        Files.Clear();
        TotalSize = 0;
        ResultCount = 0;
        _statusKey = null;
        StatusText = LangProvider.S("LF_Scanning");

        try
        {
            var progress = new Progress<ScanProgress>(p => CurrentScanPath = p.CurrentPath);
            var results = await _scanner.ScanAsync(scanPath, MinSize, progress, _cts.Token);

            foreach (var file in results)
            {
                Files.Add(file);
                TotalSize += file.Size;
            }
            ResultCount = Files.Count;
            OnPropertyChanged(nameof(TotalSizeText));
            StatusText = LangProvider.F("LF_ScanComplete", ResultCount, TotalSizeText);
            RefreshDiskUsage();
        }
        catch (OperationCanceledException)
        {
            StatusText = LangProvider.S("LF_ScanCancelled");
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
    private void OpenInExplorer(FileItem? item)
    {
        if (item is not null) _fileOps.OpenInExplorer(item.FullPath);
    }

    public FileOperationResult DoDeleteToRecycleBin(FileItem item)
    {
        var result = _fileOps.MoveToRecycleBin(item.FullPath);
        if (result.Success)
        {
            TotalSize -= item.Size;
            Files.Remove(item);
            ResultCount = Files.Count;
            OnPropertyChanged(nameof(TotalSizeText));
            RefreshDiskUsage();
        }
        return result;
    }

    public FileOperationResult DoPermanentDelete(FileItem item)
    {
        var result = _fileOps.PermanentDelete(item.FullPath);
        if (result.Success)
        {
            TotalSize -= item.Size;
            Files.Remove(item);
            ResultCount = Files.Count;
            OnPropertyChanged(nameof(TotalSizeText));
            RefreshDiskUsage();
        }
        return result;
    }

    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "选择要扫描的文件夹"
        };
        if (dialog.ShowDialog() == true)
        {
            CustomPath = dialog.FolderName;
        }
    }
}

public class DriveDisplayItem
{
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public long TotalSize { get; init; }
    public long FreeSpace { get; init; }
    public double UsedPercent => TotalSize > 0 ? (double)(TotalSize - FreeSpace) / TotalSize * 100 : 0;
}
