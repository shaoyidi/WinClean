using Microsoft.Extensions.Logging;
using WinClean.Models;

namespace WinClean.Services;

public class DiskAnalyzerService : IDiskAnalyzerService
{
    private readonly ILogger<DiskAnalyzerService> _logger;

    public DiskAnalyzerService(ILogger<DiskAnalyzerService> logger)
    {
        _logger = logger;
    }

    public async Task<FolderNode> AnalyzeAsync(string rootPath,
        IProgress<ScanProgress>? progress = null, CancellationToken ct = default)
    {
        var root = new FolderNode
        {
            Name = Path.GetFileName(rootPath) is { Length: > 0 } name ? name : rootPath,
            FullPath = rootPath,
            IsExpanded = true
        };

        await Task.Run(() => AnalyzeDirectory(root, rootPath, progress, ct, 0), ct);

        CalculatePercentages(root);

        return root;
    }

    private void AnalyzeDirectory(FolderNode node, string path,
        IProgress<ScanProgress>? progress, CancellationToken ct, int depth)
    {
        if (ct.IsCancellationRequested) return;

        long totalSize = 0;
        int fileCount = 0;

        try
        {
            foreach (var file in Directory.EnumerateFiles(path, "*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = false,
                AttributesToSkip = FileAttributes.System | FileAttributes.ReparsePoint
            }))
            {
                if (ct.IsCancellationRequested) return;
                try
                {
                    totalSize += new FileInfo(file).Length;
                    fileCount++;
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            _logger.LogTrace(ex, "Cannot enumerate files in: {Path}", path);
        }

        node.FileCount = fileCount;

        try
        {
            var subDirs = Directory.EnumerateDirectories(path, "*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = false,
                AttributesToSkip = FileAttributes.System | FileAttributes.ReparsePoint
            });

            foreach (var subDir in subDirs)
            {
                if (ct.IsCancellationRequested) return;

                var dirName = Path.GetFileName(subDir);
                var child = new FolderNode
                {
                    Name = dirName,
                    FullPath = subDir
                };

                if (depth < 2)
                {
                    AnalyzeDirectory(child, subDir, progress, ct, depth + 1);
                }
                else
                {
                    child.Size = GetDirectorySize(subDir, ct);
                }

                totalSize += child.Size;
                node.Children.Add(child);
                node.SubFolderCount++;

                progress?.Report(new ScanProgress(subDir, fileCount, totalSize));
            }
        }
        catch (Exception ex)
        {
            _logger.LogTrace(ex, "Cannot enumerate dirs in: {Path}", path);
        }

        node.Size = totalSize;
        SortChildren(node);
    }

    private long GetDirectorySize(string path, CancellationToken ct)
    {
        long size = 0;
        try
        {
            foreach (var file in Directory.EnumerateFiles(path, "*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                AttributesToSkip = FileAttributes.System | FileAttributes.ReparsePoint
            }))
            {
                if (ct.IsCancellationRequested) return size;
                try { size += new FileInfo(file).Length; } catch { }
            }
        }
        catch { }
        return size;
    }

    private static void CalculatePercentages(FolderNode node)
    {
        if (node.Size == 0) return;

        foreach (var child in node.Children)
        {
            child.Percentage = node.Size > 0 ? (double)child.Size / node.Size * 100 : 0;
            CalculatePercentages(child);
        }
    }

    private static void SortChildren(FolderNode node)
    {
        var sorted = node.Children.OrderByDescending(c => c.Size).ToList();
        node.Children.Clear();
        foreach (var child in sorted)
            node.Children.Add(child);
    }
}
