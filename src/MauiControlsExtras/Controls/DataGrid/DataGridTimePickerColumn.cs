namespace MauiControlsExtras.Controls;

/// <summary>
/// TimePicker column for data grid.
/// </summary>
public class DataGridTimePickerColumn : DataGridColumn
{
    private string _binding = string.Empty;
    private string? _format;

    /// <summary>
    /// Gets or sets the property binding path.
    /// </summary>
    public string Binding
    {
        get => _binding;
        set
        {
            if (_binding != value)
            {
                _binding = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the time format string (e.g., "t", "T", "HH:mm").
    /// </summary>
    public string? Format
    {
        get => _format;
        set
        {
            if (_format != value)
            {
                _format = value;
                OnPropertyChanged();
            }
        }
    }

    /// <inheritdoc />
    public override string? PropertyPath => Binding;

    /// <inheritdoc />
    public override View CreateCellContent(object item)
    {
        var value = GetCellValue(item);
        var displayText = FormatValue(value);

        return new Label
        {
            Text = displayText,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment,
            Padding = new Thickness(8, 4)
        };
    }

    /// <inheritdoc />
    public override View? CreateEditContent(object item)
    {
        var value = GetCellValue(item);
        TimeSpan time;
        if (value is TimeSpan ts)
            time = ts;
        else if (value is TimeOnly t)
            time = t.ToTimeSpan();
        else if (value is DateTime dateTime)
            time = dateTime.TimeOfDay;
        else
            time = TimeSpan.Zero;

        var timePicker = new TimePicker
        {
            Time = time,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };

        return timePicker;
    }

    /// <summary>
    /// Gets the value from the edit control.
    /// </summary>
    public TimeSpan GetValueFromEditControl(View editControl)
    {
        if (editControl is TimePicker timePicker)
        {
            return timePicker.Time ?? TimeSpan.Zero;
        }
        return TimeSpan.Zero;
    }

    private string FormatValue(object? value)
    {
        if (value == null)
            return string.Empty;

        TimeSpan? time = null;
        if (value is TimeSpan ts)
            time = ts;
        else if (value is TimeOnly t)
            time = t.ToTimeSpan();
        else if (value is DateTime dateTime)
            time = dateTime.TimeOfDay;

        if (time == null)
            return value.ToString() ?? string.Empty;

        if (!string.IsNullOrEmpty(Format))
        {
            // TimeSpan doesn't support format strings directly, convert to DateTime
            var formatted = DateTime.Today.Add(time.Value);
            return formatted.ToString(Format);
        }

        // Default short time format
        var defaultFormatted = DateTime.Today.Add(time.Value);
        return defaultFormatted.ToShortTimeString();
    }
}
