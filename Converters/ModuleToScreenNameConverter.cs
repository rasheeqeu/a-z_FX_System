using System.Globalization;
using System.Windows.Data;

namespace ForexTradingWorkspace.Converters;

public class ModuleToScreenNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Browser" => "A - Browser",
            "Journal" => "B - Journal",
            "Settings" => "C - Settings",
            "Dashboard" => "Dashboard",
            _ => "Unknown"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
