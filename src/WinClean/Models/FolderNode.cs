using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WinClean.Models;

public partial class FolderNode : ObservableObject
{
    public string Name { get; init; } = string.Empty;
    public string FullPath { get; init; } = string.Empty;

    [ObservableProperty]
    private long _size;

    [ObservableProperty]
    private double _percentage;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isLoading;

    public int FileCount { get; set; }
    public int SubFolderCount { get; set; }

    public ObservableCollection<FolderNode> Children { get; } = [];
}
