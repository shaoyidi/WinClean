using WinClean.Models;

namespace WinClean.Services;

public interface ICleanerService
{
    Task<List<CleanItem>> ScanAsync(IProgress<ScanProgress>? progress = null, CancellationToken ct = default);
    Task<long> CleanAsync(IEnumerable<CleanItem> items, IProgress<ScanProgress>? progress = null, CancellationToken ct = default);
}

public interface ILargeFileScannerService
{
    Task<List<FileItem>> ScanAsync(string rootPath, long minSize, IProgress<ScanProgress>? progress = null, CancellationToken ct = default);
}

public interface IDuplicateFinderService
{
    Task<List<DuplicateGroup>> ScanAsync(IEnumerable<string> directories, IProgress<ScanProgress>? progress = null, CancellationToken ct = default);
}

public interface IDiskAnalyzerService
{
    Task<FolderNode> AnalyzeAsync(string rootPath, IProgress<ScanProgress>? progress = null, CancellationToken ct = default);
}

public record FileOperationResult(bool Success, string? ErrorMessage = null);

public interface IFileOperationService
{
    FileOperationResult MoveToRecycleBin(string filePath);
    FileOperationResult PermanentDelete(string filePath);
    void OpenInExplorer(string filePath);
    void OpenFolder(string folderPath);
}
