using Microsoft.Extensions.Logging;
using WinClean.Models;

namespace WinClean.Services;

public class SystemCleanerService : ICleanerService
{
    private readonly ILogger<SystemCleanerService> _logger;

    public SystemCleanerService(ILogger<SystemCleanerService> logger)
    {
        _logger = logger;
    }

    public async Task<List<CleanItem>> ScanAsync(IProgress<ScanProgress>? progress = null, CancellationToken ct = default)
    {
        var items = new List<CleanItem>
        {
            CreateItem("Windows 临时文件", "用户和系统临时文件", "系统", "FolderRemove"),
            CreateItem("回收站", "已删除文件的回收站", "系统", "DeleteCircle"),
            CreateItem("Windows 更新缓存", "Windows Update 下载的更新文件", "系统", "Update"),
            CreateItem("系统日志文件", "系统和应用程序日志文件", "系统", "FileDocument"),
            CreateItem("缩略图缓存", "图片和视频的缩略图缓存", "系统", "Image"),
            CreateItem("Chrome 缓存", "Google Chrome 浏览器缓存", "浏览器", "GoogleChrome"),
            CreateItem("Edge 缓存", "Microsoft Edge 浏览器缓存", "浏览器", "MicrosoftEdge"),
            CreateItem("Firefox 缓存", "Mozilla Firefox 浏览器缓存", "浏览器", "Firefox"),
        };

        foreach (var item in items)
        {
            if (ct.IsCancellationRequested) break;
            item.IsScanning = true;
            await Task.Run(() => ScanItem(item, progress, ct), ct);
            item.IsScanning = false;
        }

        return items;
    }

    public async Task<long> CleanAsync(IEnumerable<CleanItem> items, IProgress<ScanProgress>? progress = null, CancellationToken ct = default)
    {
        long totalCleaned = 0;

        foreach (var item in items.Where(i => i.IsSelected && i.FilePaths.Count > 0))
        {
            if (ct.IsCancellationRequested) break;

            if (item.Name == "回收站")
            {
                await Task.Run(() => EmptyRecycleBin(), ct);
                totalCleaned += item.Size;
                continue;
            }

            foreach (var path in item.FilePaths)
            {
                if (ct.IsCancellationRequested) break;
                try
                {
                    progress?.Report(new ScanProgress(path, 0, totalCleaned));
                    var fi = new FileInfo(path);
                    if (fi.Exists)
                    {
                        long size = fi.Length;
                        fi.Delete();
                        totalCleaned += size;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cannot delete: {Path}", path);
                }
            }
        }

        return totalCleaned;
    }

    private void ScanItem(CleanItem item, IProgress<ScanProgress>? progress, CancellationToken ct)
    {
        var paths = GetPathsForItem(item.Name);
        long totalSize = 0;
        int count = 0;

        foreach (var rootPath in paths)
        {
            if (ct.IsCancellationRequested) return;

            try
            {
                if (item.Name == "回收站")
                {
                    ScanRecycleBin(item);
                    return;
                }

                if (!Directory.Exists(rootPath)) continue;

                var files = Directory.EnumerateFiles(rootPath, "*", new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true,
                    AttributesToSkip = FileAttributes.System
                });

                foreach (var file in files)
                {
                    if (ct.IsCancellationRequested) return;
                    try
                    {
                        var fi = new FileInfo(file);
                        if (!fi.Exists) continue;
                        totalSize += fi.Length;
                        count++;
                        item.FilePaths.Add(file);
                        progress?.Report(new ScanProgress(file, count, totalSize));
                    }
                    catch { /* skip inaccessible */ }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error scanning path: {Path}", rootPath);
            }
        }

        item.Size = totalSize;
        item.FileCount = count;
    }

    private static List<string> GetPathsForItem(string name) => name switch
    {
        "Windows 临时文件" =>
        [
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
        ],
        "Windows 更新缓存" =>
        [
            @"C:\Windows\SoftwareDistribution\Download"
        ],
        "系统日志文件" =>
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrashDumps")
        ],
        "缩略图缓存" =>
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Windows\Explorer")
        ],
        "Chrome 缓存" =>
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default\Cache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default\Code Cache")
        ],
        "Edge 缓存" =>
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Edge\User Data\Default\Cache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Edge\User Data\Default\Code Cache")
        ],
        "Firefox 缓存" =>
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Mozilla\Firefox\Profiles")
        ],
        _ => []
    };

    private static void ScanRecycleBin(CleanItem item)
    {
        try
        {
            long size = 0;
            int count = 0;
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
            {
                var recyclePath = Path.Combine(drive.RootDirectory.FullName, "$Recycle.Bin");
                if (!Directory.Exists(recyclePath)) continue;
                try
                {
                    foreach (var file in Directory.EnumerateFiles(recyclePath, "*", new EnumerationOptions
                    {
                        IgnoreInaccessible = true,
                        RecurseSubdirectories = true,
                        AttributesToSkip = 0
                    }))
                    {
                        try
                        {
                            size += new FileInfo(file).Length;
                            count++;
                        }
                        catch { }
                    }
                }
                catch { }
            }
            item.Size = size;
            item.FileCount = count;
        }
        catch { }
    }

    private static void EmptyRecycleBin()
    {
        try
        {
            SHEmptyRecycleBin(IntPtr.Zero, null, 0x00000007);
        }
        catch { }
    }

    [System.Runtime.InteropServices.DllImport("shell32.dll")]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    private static CleanItem CreateItem(string name, string desc, string category, string icon) => new()
    {
        Name = name,
        Description = desc,
        Category = category,
        IconKind = icon
    };
}
