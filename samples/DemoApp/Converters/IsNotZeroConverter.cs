using System.Globalization;

namespace DemoApp.Converters;

/// <summary>
/// Converts a numeric value to true if it's not zero, false otherwise.
/// </summary>
public class IsNotZeroConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            int i => i != 0,
            long l => l != 0,
            double d => d != 0,
            decimal dec => dec != 0,
            float f => f != 0,
            _ => false
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
