using System.Globalization;

namespace DemoApp.Converters;

public class IconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string iconName)
            return "\uE8B7"; // Default folder icon

        return iconName.ToLowerInvariant() switch
        {
            "folder" => "\uE8B7",
            "file" => "\uE8A5",
            "image" => "\uE8B9",
            "music" => "\uE8D6",
            "video" => "\uE8B2",
            "document" => "\uE8A5",
            _ => "\uE8B7"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
