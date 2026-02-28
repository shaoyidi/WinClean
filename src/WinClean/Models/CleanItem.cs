using CommunityToolkit.Mvvm.ComponentModel;

namespace WinClean.Models;

public partial class CleanItem : ObservableObject
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string IconKind { get; init; } = "Delete";

    [ObservableProperty]
    private bool _isSelected = true;

    [ObservableProperty]
    private long _size;

    [ObservableProperty]
    private int _fileCount;

    [ObservableProperty]
    private bool _isScanning;

    public List<string> FilePaths { get; set; } = [];
}
