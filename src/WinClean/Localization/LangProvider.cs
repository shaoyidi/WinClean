using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinClean.Localization;

public class LangProvider : INotifyPropertyChanged
{
    public static LangProvider Instance { get; } = new();

    public static readonly List<LanguageItem> Languages =
    [
        new("zh-CN", "简体中文"),
        new("zh-TW", "繁體中文"),
        new("en", "English"),
        new("ja", "日本語"),
        new("ko", "한국어"),
        new("de", "Deutsch")
    ];

    private Dictionary<string, string> _strings;
    private string _currentLang = "zh-CN";

    private static readonly Dictionary<string, Func<Dictionary<string, string>>> Loaders = new()
    {
        ["zh-CN"] = LangZhCN.Load,
        ["zh-TW"] = LangZhTW.Load,
        ["en"] = LangEn.Load,
        ["ja"] = LangJa.Load,
        ["ko"] = LangKo.Load,
        ["de"] = LangDe.Load,
    };

    public string this[string key] =>
        _strings.TryGetValue(key, out var val) ? val : key;

    public static string S(string key) => Instance[key];
    public static string F(string key, params object[] args) => string.Format(Instance[key], args);

    public string CurrentLanguage
    {
        get => _currentLang;
        set
        {
            if (_currentLang == value) return;
            _currentLang = value;
            _strings = Loaders.TryGetValue(value, out var loader) ? loader() : Loaders["zh-CN"]();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
        }
    }

    private LangProvider()
    {
        _strings = LangZhCN.Load();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public record LanguageItem(string Code, string DisplayName)
{
    public override string ToString() => DisplayName;
}

[ContentProperty(nameof(Key))]
[MarkupExtensionReturnType(typeof(object))]
public class LangExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public LangExtension() { }
    public LangExtension(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = LangProvider.Instance,
            Mode = BindingMode.OneWay
        };
        try
        {
            return binding.ProvideValue(serviceProvider);
        }
        catch
        {
            return LangProvider.Instance[Key];
        }
    }
}
