using System.Globalization;

namespace MauiControlsExtras.Helpers;

/// <summary>
/// IValueConverter that uses a <see cref="Func{T, TResult}"/> to convert items to display strings.
/// Shared by ComboBox, MultiSelectComboBox, and ComboBoxPopupContent for Func-based display bindings.
/// </summary>
internal sealed class FuncDisplayConverter : IValueConverter
{
    private readonly Func<object, string?> _func;

    public FuncDisplayConverter(Func<object, string?> func) => _func = func;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;
        try
        {
            return _func(value) ?? string.Empty;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            System.Diagnostics.Debug.WriteLine($"FuncDisplayConverter: {ex.Message}");
            return string.Empty;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
