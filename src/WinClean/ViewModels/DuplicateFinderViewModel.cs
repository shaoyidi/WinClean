using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinClean.Helpers;
using WinClean.Localization;
using WinClean.Models;
using WinClean.Services;

namespace WinClean.ViewModels;

public partial class DuplicateFinderViewModel : ObservableObject
{
    private readonly IDuplicateFinderService _duplicateFinder;
    private readonly IFileOperationService _fileOps;
    private CancellationTokenSource? _cts;

    public ObservableCollection<DuplicateGroup> Groups { get; } = [];
    public ObservableCollection<string> ScanDirectories { get; } = [];

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _statusText = LangProvider.S("DF_DefaultStatus");

    [ObservableProperty]
    private string _currentScanPath = string.Empty;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private int _groupCount;

    [ObservableProperty]
    private long _wastedSpace;

    public string WastedSpaceText => FileSizeFormatter.Format(WastedSpace);

    public ObservableCollection<DriveCheckItem> AvailableDrives { get; } = [];

    [ObservableProperty]
    private bool _isDrivePopupOpen;

    private string? _statusKey = "DF_DefaultStatus";

    public DuplicateFinderViewModel(IDuplicateFinderService duplicateFinder, IFileOperationService fileOps)
    {
        _duplicateFinder = duplicateFinder;
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
            var used = drive.TotalSize - drive.AvailableFreeSpace;
            AvailableDrives.Add(new DriveCheckItem
            {
                DrivePath = drive.RootDirectory.FullName,
                DisplayName = $"{drive.Name} ({drive.VolumeLabel}) - {FileSizeFormatter.Format(used)}/{FileSizeFormatter.Format(drive.TotalSize)}",
                IsChecked = false
            });
        }
    }

    [RelayCommand]
    private void OpenDrivePopup()
    {
        foreach (var d in AvailableDrives)
            d.IsChecked = ScanDirectories.Contains(d.DrivePath);
        IsDrivePopupOpen = true;
    }

    [RelayCommand]
    private void ConfirmDriveSelection()
    {
        foreach (var d in AvailableDrives)
        {
            if (d.IsChecked && !ScanDirectories.Contains(d.DrivePath))
                ScanDirectories.Add(d.DrivePath);
            else if (!d.IsChecked && ScanDirectories.Contains(d.DrivePath))
                ScanDirectories.Remove(d.DrivePath);
        }
        IsDrivePopupOpen = false;
    }

    [RelayCommand]
    private void AddFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "选择要扫描的文件夹",
            Multiselect = true
        };
        if (dialog.ShowDialog() == true)
        {
            foreach (var folder in dialog.FolderNames)
            {
                if (!ScanDirectories.Contains(folder))
                    ScanDirectories.Add(folder);
            }
        }
    }

    [RelayCommand]
    private void RemoveFolder(string? path)
    {
        if (path is not null)
            ScanDirectories.Remove(path);
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        if (IsScanning || ScanDirectories.Count == 0) return;

        _cts = new CancellationTokenSource();
        IsScanning = true;
        Groups.Clear();
        WastedSpace = 0;
        GroupCount = 0;
        ProgressPercent = 0;
        _statusKey = null;
        StatusText = LangProvider.S("DF_Scanning");

        try
        {
            var progress = new Progress<ScanProgress>(p =>
            {
                CurrentScanPath = p.CurrentPath;
                if (p.ProgressPercent >= 0)
                    ProgressPercent = p.ProgressPercent;
            });

            var results = await _duplicateFinder.ScanAsync(ScanDirectories, progress, _cts.Token);

            foreach (var group in results)
            {
                Groups.Add(group);
                WastedSpace += group.WastedSpace;
            }

            GroupCount = Groups.Count;
            OnPropertyChanged(nameof(WastedSpaceText));
            ProgressPercent = 100;
            StatusText = LangProvider.F("DF_ScanComplete", GroupCount, WastedSpaceText);
        }
        catch (OperationCanceledException)
        {
            StatusText = LangProvider.S("DF_ScanCancelled");
        }
        finally
        {
            IsScanning = false;
            CurrentScanPath = string.Empty;
        }
    }

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();

    public (int deleted, int failed, long freed, List<string> errors) DoDeleteSelected()
    {
        var toDelete = Groups
            .SelectMany(g => g.Files.Where(f => f.IsSelected))
            .ToList();

        long freed = 0;
        int deleted = 0;
        var errors = new List<string>();

        foreach (var file in toDelete)
        {
            var result = _fileOps.MoveToRecycleBin(file.FullPath);
            if (result.Success)
            {
                freed += file.Size;
                deleted++;
                foreach (var group in Groups)
                    group.Files.Remove(file);
            }
            else
            {
                errors.Add($"{file.FileName}: {result.ErrorMessage}");
            }
        }

        var emptyGroups = Groups.Where(g => g.Files.Count <= 1).ToList();
        foreach (var g in emptyGroups)
            Groups.Remove(g);

        GroupCount = Groups.Count;
        WastedSpace = Groups.Sum(g => g.WastedSpace);
        OnPropertyChanged(nameof(WastedSpaceText));
        StatusText = LangProvider.F("DF_DeleteResult", deleted, FileSizeFormatter.Format(freed)) +
                     (errors.Count > 0 ? LangProvider.F("DF_DeleteFailed", errors.Count) : "");

        return (deleted, errors.Count, freed, errors);
    }

    [RelayCommand]
    private void OpenInExplorer(FileItem? item)
    {
        if (item is not null) _fileOps.OpenInExplorer(item.FullPath);
    }

    [RelayCommand]
    private void AutoSelectDuplicates()
    {
        foreach (var group in Groups)
        {
            bool first = true;
            foreach (var file in group.Files.OrderBy(f => f.LastModified))
            {
                file.IsSelected = !first;
                first = false;
            }
        }
    }
}

public class DriveCheckItem : ObservableObject
{
    public string DrivePath { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}
