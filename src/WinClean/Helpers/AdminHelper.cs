using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace WinClean.Helpers;

public static class AdminHelper
{
    public static bool IsRunAsAdmin()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    /// <returns>true if restart was initiated, false if user cancelled or failed</returns>
    public static bool RestartAsAdmin()
    {
        var exePath = Environment.ProcessPath;
        if (exePath is null) return false;

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(startInfo);
            System.Windows.Application.Current.Shutdown();
            return true;
        }
        catch (Win32Exception)
        {
            // User cancelled UAC prompt or elevation not supported
            return false;
        }
        catch
        {
            return false;
        }
    }
}
