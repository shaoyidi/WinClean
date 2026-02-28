using CommunityToolkit.Mvvm.ComponentModel;

namespace WinClean.Models;

public partial class FileItem : ObservableObject
{
    public string FullPath { get; init; } = string.Empty;
    public string FileName => Path.GetFileName(FullPath);
    public string Directory => Path.GetDirectoryName(FullPath) ?? string.Empty;
    public string Extension => Path.GetExtension(FullPath).ToLowerInvariant();
    public long Size { get; init; }
    public DateTime LastModified { get; init; }

    [ObservableProperty]
    private bool _isSelected;
}
