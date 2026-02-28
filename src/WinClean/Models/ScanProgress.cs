namespace WinClean.Models;

public record ScanProgress(
    string CurrentPath,
    int FilesScanned,
    long TotalSize,
    double ProgressPercent = -1
);
