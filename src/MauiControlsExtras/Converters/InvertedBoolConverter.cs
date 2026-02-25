using System.Globalization;
using Microsoft.Maui.Controls.Internals;

namespace MauiControlsExtras.Converters;

/// <summary>
/// Converts a boolean value to its inverse.
/// </summary>
[Preserve(AllMembers = true)]
public class InvertedBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to its inverse.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }

    /// <summary>
    /// Converts a boolean value back to its inverse.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }
}
