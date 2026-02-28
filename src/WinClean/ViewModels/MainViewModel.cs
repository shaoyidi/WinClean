using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WinClean.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public SystemCleanerViewModel SystemCleaner { get; }
    public LargeFileScannerViewModel LargeFileScanner { get; }
    public DuplicateFinderViewModel DuplicateFinder { get; }
    public DiskAnalyzerViewModel DiskAnalyzer { get; }
    public SettingsViewModel Settings { get; }

    [ObservableProperty]
    private object _currentView;

    [ObservableProperty]
    private int _selectedNavIndex;

    [ObservableProperty]
    private string _title = "WinClean";

    public MainViewModel(
        SystemCleanerViewModel systemCleaner,
        LargeFileScannerViewModel largeFileScanner,
        DuplicateFinderViewModel duplicateFinder,
        DiskAnalyzerViewModel diskAnalyzer,
        SettingsViewModel settings)
    {
        SystemCleaner = systemCleaner;
        LargeFileScanner = largeFileScanner;
        DuplicateFinder = duplicateFinder;
        DiskAnalyzer = diskAnalyzer;
        Settings = settings;
        _currentView = systemCleaner;
    }

    partial void OnSelectedNavIndexChanged(int value)
    {
        CurrentView = value switch
        {
            0 => SystemCleaner,
            1 => LargeFileScanner,
            2 => DuplicateFinder,
            3 => DiskAnalyzer,
            4 => Settings,
            _ => SystemCleaner
        };
    }

    [RelayCommand]
    private void NavigateTo(string target)
    {
        SelectedNavIndex = target switch
        {
            "SystemCleaner" => 0,
            "LargeFileScanner" => 1,
            "DuplicateFinder" => 2,
            "DiskAnalyzer" => 3,
            "Settings" => 4,
            _ => 0
        };
    }
}
