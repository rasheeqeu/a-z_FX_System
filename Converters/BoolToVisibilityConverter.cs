using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ForexTradingWorkspace.Converters;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isTrue = value is true;
        var isInverted = parameter?.ToString().Equals("Inverted", StringComparison.OrdinalIgnoreCase) ?? false;

        var shouldBeVisible = isInverted ? !isTrue : isTrue;
        return shouldBeVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isVisible = value is Visibility.Visible;
        var isInverted = parameter?.ToString().Equals("Inverted", StringComparison.OrdinalIgnoreCase) ?? false;
        return isInverted ? !isVisible : isVisible;
    }
}
