using System.Globalization;
using System.Windows.Data;

namespace ForexTradingWorkspace.Converters;

public sealed class UrlToUriConverter : IValueConverter
{
    private static readonly Uri DefaultUri = new("https://www.tradingview.com/chart/");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text))
        {
            return DefaultUri;
        }

        if (!text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            text = $"https://{text}";
        }

        return Uri.TryCreate(text, UriKind.Absolute, out var uri) ? uri : DefaultUri;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Uri uri ? uri.ToString() : DefaultUri.ToString();
    }
}
