namespace MauiControlsExtras.Controls;

/// <summary>
/// DatePicker column for data grid.
/// </summary>
public class DataGridDatePickerColumn : DataGridColumn
{
    private string _binding = string.Empty;
    private string? _format;
    private DateTime? _minimumDate;
    private DateTime? _maximumDate;

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
    /// Gets or sets the date format string (e.g., "d", "D", "yyyy-MM-dd").
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

    /// <summary>
    /// Gets or sets the minimum selectable date.
    /// </summary>
    public DateTime? MinimumDate
    {
        get => _minimumDate;
        set
        {
            if (_minimumDate != value)
            {
                _minimumDate = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum selectable date.
    /// </summary>
    public DateTime? MaximumDate
    {
        get => _maximumDate;
        set
        {
            if (_maximumDate != value)
            {
                _maximumDate = value;
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
        DateTime date;
        if (value is DateTime dt)
            date = dt;
        else if (value is DateOnly d)
            date = d.ToDateTime(TimeOnly.MinValue);
        else
            date = DateTime.Today;

        var datePicker = new DatePicker
        {
            Date = date,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };

        if (MinimumDate.HasValue)
            datePicker.MinimumDate = MinimumDate.Value;

        if (MaximumDate.HasValue)
            datePicker.MaximumDate = MaximumDate.Value;

        return datePicker;
    }

    /// <summary>
    /// Gets the value from the edit control.
    /// </summary>
    public DateTime GetValueFromEditControl(View editControl)
    {
        if (editControl is DatePicker datePicker)
        {
            return datePicker.Date ?? DateTime.Today;
        }
        return DateTime.Today;
    }

    private string FormatValue(object? value)
    {
        if (value == null)
            return string.Empty;

        DateTime? date = null;
        if (value is DateTime dateTime)
            date = dateTime;
        else if (value is DateOnly dateOnly)
            date = dateOnly.ToDateTime(TimeOnly.MinValue);

        if (date == null)
            return value.ToString() ?? string.Empty;

        if (!string.IsNullOrEmpty(Format))
            return date.Value.ToString(Format);

        return date.Value.ToShortDateString();
    }
}
