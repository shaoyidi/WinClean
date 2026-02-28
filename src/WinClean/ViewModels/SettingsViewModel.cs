using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignColors;
using WinClean.Helpers;
using WinClean.Localization;
using WinClean.Services;

namespace WinClean.ViewModels;

public record ThemeColorItem(string Name, PrimaryColor Primary, SecondaryColor Secondary, string Hex);

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _deleteToRecycleBin;

    [ObservableProperty]
    private bool _confirmBeforeDelete;

    [ObservableProperty]
    private string _appVersion = "1.0.0";

    [ObservableProperty]
    private LanguageItem _selectedLanguage;

    [ObservableProperty]
    private ThemeColorItem _selectedColor;

    public List<LanguageItem> AvailableLanguages => LangProvider.Languages;

    public static List<ThemeColorItem> AvailableColors { get; } =
    [
        new("Red",        PrimaryColor.Red,        SecondaryColor.Red,        "#F44336"),
        new("Pink",       PrimaryColor.Pink,       SecondaryColor.Pink,       "#E91E63"),
        new("Purple",     PrimaryColor.Purple,     SecondaryColor.Purple,     "#9C27B0"),
        new("DeepPurple", PrimaryColor.DeepPurple, SecondaryColor.DeepPurple, "#673AB7"),
        new("Indigo",     PrimaryColor.Indigo,     SecondaryColor.Indigo,     "#3F51B5"),
        new("Blue",       PrimaryColor.Blue,       SecondaryColor.Blue,       "#2196F3"),
        new("LightBlue",  PrimaryColor.LightBlue,  SecondaryColor.LightBlue,  "#03A9F4"),
        new("Cyan",       PrimaryColor.Cyan,       SecondaryColor.Cyan,       "#00BCD4"),
        new("Teal",       PrimaryColor.Teal,       SecondaryColor.Teal,       "#009688"),
        new("Green",      PrimaryColor.Green,      SecondaryColor.Green,      "#4CAF50"),
        new("LightGreen", PrimaryColor.LightGreen, SecondaryColor.LightGreen, "#8BC34A"),
        new("Lime",       PrimaryColor.Lime,       SecondaryColor.Lime,       "#CDDC39"),
        new("Yellow",     PrimaryColor.Yellow,     SecondaryColor.Yellow,     "#FFEB3B"),
        new("Amber",      PrimaryColor.Amber,      SecondaryColor.Amber,      "#FFC107"),
        new("Orange",     PrimaryColor.Orange,     SecondaryColor.Orange,     "#FF9800"),
        new("DeepOrange", PrimaryColor.DeepOrange, SecondaryColor.DeepOrange, "#FF5722"),
    ];

    private bool _isInitializing = true;

    public SettingsViewModel()
    {
        IsAdmin = AdminHelper.IsRunAsAdmin();

        var settings = UserSettingsService.Load();

        _isDarkTheme = settings.IsDarkTheme;
        _deleteToRecycleBin = settings.DeleteToRecycleBin;
        _confirmBeforeDelete = settings.ConfirmBeforeDelete;

        _selectedLanguage = LangProvider.Languages.FirstOrDefault(l => l.Code == settings.Language)
                            ?? LangProvider.Languages.First();

        _selectedColor = AvailableColors.FirstOrDefault(c => c.Name == settings.ThemeColor)
                         ?? AvailableColors.First(c => c.Name == "Blue");

        ApplyTheme();
        ApplyLanguage();
        ApplyColor();

        _isInitializing = false;
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        ApplyTheme();
        SaveSettings();
    }

    partial void OnSelectedLanguageChanged(LanguageItem value)
    {
        if (value is null) return;
        ApplyLanguage();
        SaveSettings();
    }

    partial void OnSelectedColorChanged(ThemeColorItem value)
    {
        if (value is null) return;
        ApplyColor();
        SaveSettings();
    }

    partial void OnDeleteToRecycleBinChanged(bool value) => SaveSettings();
    partial void OnConfirmBeforeDeleteChanged(bool value) => SaveSettings();

    private void ApplyTheme()
    {
        var bundledTheme = GetBundledTheme();
        if (bundledTheme is not null)
        {
            bundledTheme.BaseTheme = IsDarkTheme
                ? MaterialDesignThemes.Wpf.BaseTheme.Dark
                : MaterialDesignThemes.Wpf.BaseTheme.Light;
        }
    }

    private void ApplyLanguage()
    {
        if (SelectedLanguage is null) return;
        LangProvider.Instance.CurrentLanguage = SelectedLanguage.Code;

        var window = System.Windows.Application.Current.MainWindow;
        if (window is not null)
            window.Title = LangProvider.S("App_Title");
    }

    private void ApplyColor()
    {
        if (SelectedColor is null) return;
        var bundledTheme = GetBundledTheme();
        if (bundledTheme is not null)
        {
            bundledTheme.PrimaryColor = SelectedColor.Primary;
            bundledTheme.SecondaryColor = SelectedColor.Secondary;
        }
    }

    private void SaveSettings()
    {
        if (_isInitializing) return;
        UserSettingsService.Save(new UserSettings
        {
            IsDarkTheme = IsDarkTheme,
            ThemeColor = SelectedColor?.Name ?? "Green",
            Language = SelectedLanguage?.Code ?? "zh-CN",
            DeleteToRecycleBin = DeleteToRecycleBin,
            ConfirmBeforeDelete = ConfirmBeforeDelete,
        });
    }

    private static MaterialDesignThemes.Wpf.BundledTheme? GetBundledTheme()
        => System.Windows.Application.Current.Resources.MergedDictionaries
            .OfType<MaterialDesignThemes.Wpf.BundledTheme>()
            .FirstOrDefault();

    [RelayCommand]
    private void RestartAsAdmin()
    {
        AdminHelper.RestartAsAdmin();
    }
}
