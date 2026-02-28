using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WinClean.Helpers;
using WinClean.Models;

namespace WinClean.Services;

public class DuplicateFinderService : IDuplicateFinderService
{
    private readonly ILogger<DuplicateFinderService> _logger;
    private const int MaxParallelism = 4;

    public DuplicateFinderService(ILogger<DuplicateFinderService> logger)
    {
        _logger = logger;
    }

    public async Task<List<DuplicateGroup>> ScanAsync(IEnumerable<string> directories,
        IProgress<ScanProgress>? progress = null, CancellationToken ct = default)
    {
        // Phase 1: enumerate all files, group by extension
        progress?.Report(new ScanProgress("阶段 1/4：遍历文件并按类型分组...", 0, 0, 0));
        var extensionGroups = await EnumerateAndGroupByExtensionAsync(directories, progress, ct);

        // Phase 2: within each extension group, sub-group by file size
        progress?.Report(new ScanProgress("阶段 2/4：按文件大小筛选...", 0, 0, 20));
        var sizeGroups = GroupBySize(extensionGroups);

        // Phase 3: parallel partial hash comparison per extension group
        progress?.Report(new ScanProgress("阶段 3/4：多线程快速哈希比对...", 0, 0, 40));
        var partialHashGroups = await ParallelPartialHashAsync(sizeGroups, progress, ct);

        // Phase 4: parallel full hash comparison
        progress?.Report(new ScanProgress("阶段 4/4：多线程完整哈希比对...", 0, 0, 70));
        var results = await ParallelFullHashAsync(partialHashGroups, progress, ct);

        return results;
    }

    private async Task<Dictionary<string, List<FileEntry>>> EnumerateAndGroupByExtensionAsync(
        IEnumerable<string> directories, IProgress<ScanProgress>? progress, CancellationToken ct)
    {
        var groups = new Dictionary<string, List<FileEntry>>(StringComparer.OrdinalIgnoreCase);
        int scanned = 0;

        await Task.Run(() =>
        {
            foreach (var dir in directories)
            {
                if (ct.IsCancellationRequested) return;
                if (!Directory.Exists(dir)) continue;

                try
                {
                    foreach (var file in Directory.EnumerateFiles(dir, "*", new EnumerationOptions
                    {
                        IgnoreInaccessible = true,
                        RecurseSubdirectories = true,
                        AttributesToSkip = FileAttributes.System | FileAttributes.ReparsePoint
                    }))
                    {
                        if (ct.IsCancellationRequested) return;
                        try
                        {
                            var fi = new FileInfo(file);
                            if (!fi.Exists || fi.Length == 0) continue;

                            var ext = fi.Extension.ToLowerInvariant();
                            if (string.IsNullOrEmpty(ext)) ext = "(无扩展名)";

                            if (!groups.TryGetValue(ext, out var list))
                            {
                                list = [];
                                groups[ext] = list;
                            }
                            list.Add(new FileEntry(file, fi.Length));
                            scanned++;

                            if (scanned % 1000 == 0)
                                progress?.Report(new ScanProgress(file, scanned, 0, 10));
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cannot enumerate: {Dir}", dir);
                }
            }
        }, ct);

        progress?.Report(new ScanProgress($"已扫描 {scanned} 个文件，{groups.Count} 种文件类型", scanned, 0, 20));
        return groups;
    }

    private static List<List<FileEntry>> GroupBySize(Dictionary<string, List<FileEntry>> extensionGroups)
    {
        var result = new List<List<FileEntry>>();

        foreach (var (_, files) in extensionGroups)
        {
            var sizeMap = new Dictionary<long, List<FileEntry>>();
            foreach (var entry in files)
            {
                if (!sizeMap.TryGetValue(entry.Size, out var list))
                {
                    list = [];
                    sizeMap[entry.Size] = list;
                }
                list.Add(entry);
            }
            result.AddRange(sizeMap.Values.Where(g => g.Count > 1));
        }

        return result;
    }

    private async Task<List<List<FileEntry>>> ParallelPartialHashAsync(
        List<List<FileEntry>> sizeGroups, IProgress<ScanProgress>? progress, CancellationToken ct)
    {
        var result = new ConcurrentBag<List<FileEntry>>();
        int processed = 0;
        int total = sizeGroups.Sum(g => g.Count);

        await Parallel.ForEachAsync(sizeGroups,
            new ParallelOptions { MaxDegreeOfParallelism = MaxParallelism, CancellationToken = ct },
            async (group, token) =>
        {
            var hashMap = new Dictionary<string, List<FileEntry>>();

            foreach (var entry in group)
            {
                if (token.IsCancellationRequested) break;
                try
                {
                    var hash = await FileHashHelper.ComputePartialHashAsync(entry.Path, token);
                    if (!hashMap.TryGetValue(hash, out var list))
                    {
                        list = [];
                        hashMap[hash] = list;
                    }
                    list.Add(entry);

                    var count = Interlocked.Increment(ref processed);
                    if (count % 200 == 0)
                        progress?.Report(new ScanProgress(entry.Path, count, 0,
                            40 + (30.0 * count / Math.Max(total, 1))));
                }
                catch (Exception ex)
                {
                    _logger.LogTrace(ex, "Cannot partial-hash: {File}", entry.Path);
                }
            }

            foreach (var list in hashMap.Values.Where(g => g.Count > 1))
                result.Add(list);
        });

        return result.ToList();
    }

    private async Task<List<DuplicateGroup>> ParallelFullHashAsync(
        List<List<FileEntry>> partialGroups, IProgress<ScanProgress>? progress, CancellationToken ct)
    {
        var result = new ConcurrentBag<DuplicateGroup>();
        int processed = 0;
        int total = partialGroups.Sum(g => g.Count);

        await Parallel.ForEachAsync(partialGroups,
            new ParallelOptions { MaxDegreeOfParallelism = MaxParallelism, CancellationToken = ct },
            async (group, token) =>
        {
            var hashMap = new Dictionary<string, List<FileEntry>>();

            foreach (var entry in group)
            {
                if (token.IsCancellationRequested) break;
                try
                {
                    var hash = await FileHashHelper.ComputeFullHashAsync(entry.Path, token);
                    if (!hashMap.TryGetValue(hash, out var list))
                    {
                        list = [];
                        hashMap[hash] = list;
                    }
                    list.Add(entry);

                    var count = Interlocked.Increment(ref processed);
                    if (count % 100 == 0)
                        progress?.Report(new ScanProgress(entry.Path, count, 0,
                            70 + (30.0 * count / Math.Max(total, 1))));
                }
                catch (Exception ex)
                {
                    _logger.LogTrace(ex, "Cannot full-hash: {File}", entry.Path);
                }
            }

            foreach (var (hash, files) in hashMap.Where(kv => kv.Value.Count > 1))
            {
                var dupGroup = new DuplicateGroup { Hash = hash, FileSize = files[0].Size };
                bool first = true;
                foreach (var f in files)
                {
                    var fi = new FileInfo(f.Path);
                    dupGroup.Files.Add(new FileItem
                    {
                        FullPath = f.Path,
                        Size = f.Size,
                        LastModified = fi.Exists ? fi.LastWriteTime : default,
                        IsSelected = !first
                    });
                    first = false;
                }
                result.Add(dupGroup);
            }
        });

        return result.OrderByDescending(g => g.WastedSpace).ToList();
    }

    private record FileEntry(string Path, long Size);
}
