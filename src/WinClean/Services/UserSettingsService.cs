using System.IO;
using System.Text.Json;

namespace WinClean.Services;

public class UserSettings
{
    public bool IsDarkTheme { get; set; } = false;
    public string ThemeColor { get; set; } = "Blue";
    public string Language { get; set; } = "zh-CN";
    public bool DeleteToRecycleBin { get; set; } = true;
    public bool ConfirmBeforeDelete { get; set; } = true;
}

public class UserSettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinClean");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static UserSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<UserSettings>(json, JsonOptions) ?? new UserSettings();
            }
        }
        catch
        {
            // Fall back to defaults on any error
        }
        return new UserSettings();
    }

    public static void Save(UserSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Silently fail - non-critical
        }
    }
}
