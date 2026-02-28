using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace WinClean.Services;

public class FileOperationService : IFileOperationService
{
    private readonly ILogger<FileOperationService> _logger;

    public FileOperationService(ILogger<FileOperationService> logger)
    {
        _logger = logger;
    }

    public FileOperationResult MoveToRecycleBin(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return new FileOperationResult(false, $"文件不存在：{filePath}");

            var shf = new SHFILEOPSTRUCT
            {
                wFunc = FO_DELETE,
                pFrom = filePath + '\0' + '\0',
                fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_SILENT
            };

            int result = SHFileOperation(ref shf);
            return result == 0
                ? new FileOperationResult(true)
                : new FileOperationResult(false, $"移动到回收站失败（错误码：{result}）");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move file to recycle bin: {Path}", filePath);
            return new FileOperationResult(false, ex.Message);
        }
    }

    public FileOperationResult PermanentDelete(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return new FileOperationResult(true);
            }
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, recursive: true);
                return new FileOperationResult(true);
            }
            return new FileOperationResult(false, $"文件不存在：{filePath}");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Permission denied: {Path}", filePath);
            return new FileOperationResult(false, $"没有权限删除此文件，请尝试以管理员身份运行：\n{ex.Message}");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error deleting: {Path}", filePath);
            return new FileOperationResult(false, $"文件可能正在被其他程序占用：\n{ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete: {Path}", filePath);
            return new FileOperationResult(false, ex.Message);
        }
    }

    public void OpenInExplorer(string filePath)
    {
        if (File.Exists(filePath))
            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
    }

    public void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
            Process.Start("explorer.exe", $"\"{folderPath}\"");
    }

    #region Win32 Interop

    private const int FO_DELETE = 0x0003;
    private const int FOF_ALLOWUNDO = 0x0040;
    private const int FOF_NOCONFIRMATION = 0x0010;
    private const int FOF_NOERRORUI = 0x0400;
    private const int FOF_SILENT = 0x0004;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public int wFunc;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pFrom;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pTo;
        public ushort fFlags;
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpszProgressTitle;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

    #endregion
}
