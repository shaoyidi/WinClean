using System.Globalization;
using System.Windows.Data;
using WinClean.Helpers;

namespace WinClean.Converters;

public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
            return FileSizeFormatter.Format(bytes);
        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter is string s && s == "Invert";
        bool boolVal = value is bool b && b;
        if (invert) boolVal = !boolVal;
        return boolVal ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class PercentageToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is double percentage && values[1] is double totalWidth)
            return totalWidth * percentage / 100.0;
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class DiskUsagePercentToBrushConverter : IValueConverter
{
    private static readonly System.Windows.Media.SolidColorBrush GreenBrush =
        new(System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly System.Windows.Media.SolidColorBrush BlueBrush =
        new(System.Windows.Media.Color.FromRgb(0x21, 0x96, 0xF3));
    private static readonly System.Windows.Media.SolidColorBrush RedBrush =
        new(System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double usedPercent)
        {
            if (usedPercent < 50) return GreenBrush;
            if (usedPercent < 90) return BlueBrush;
            return RedBrush;
        }
        return BlueBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
