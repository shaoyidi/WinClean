using Microsoft.Extensions.Logging;
using WinClean.Models;

namespace WinClean.Services;

public class LargeFileScannerService : ILargeFileScannerService
{
    private readonly ILogger<LargeFileScannerService> _logger;

    public LargeFileScannerService(ILogger<LargeFileScannerService> logger)
    {
        _logger = logger;
    }

    public async Task<List<FileItem>> ScanAsync(string rootPath, long minSize,
        IProgress<ScanProgress>? progress = null, CancellationToken ct = default)
    {
        var results = new List<FileItem>();

        await Task.Run(() =>
        {
            int scanned = 0;

            try
            {
                var files = Directory.EnumerateFiles(rootPath, "*", new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true,
                    AttributesToSkip = FileAttributes.System | FileAttributes.ReparsePoint
                });

                foreach (var filePath in files)
                {
                    if (ct.IsCancellationRequested) return;

                    try
                    {
                        var fi = new FileInfo(filePath);
                        if (!fi.Exists || fi.Length < minSize) continue;

                        scanned++;
                        results.Add(new FileItem
                        {
                            FullPath = fi.FullName,
                            Size = fi.Length,
                            LastModified = fi.LastWriteTime
                        });

                        progress?.Report(new ScanProgress(filePath, scanned, fi.Length));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogTrace(ex, "Cannot access: {Path}", filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning directory: {Path}", rootPath);
            }
        }, ct);

        return results.OrderByDescending(f => f.Size).ToList();
    }
}
