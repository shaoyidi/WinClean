using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WinClean.Models;

public partial class DuplicateGroup : ObservableObject
{
    public string Hash { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public int Count => Files.Count;

    public long WastedSpace => FileSize * (Files.Count - 1);

    public ObservableCollection<FileItem> Files { get; } = [];

    [ObservableProperty]
    private bool _isExpanded = true;
}
