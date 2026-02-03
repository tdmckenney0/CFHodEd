using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace CFHodEd.UI;

/// <summary>
/// Converts a Color to a SolidColorBrush for binding.
/// </summary>
public class ColorToBrushConverter : IValueConverter
{
    public static readonly ColorToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
            return new SolidColorBrush(color);
        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
            return brush.Color;
        return Colors.Gray;
    }
}
