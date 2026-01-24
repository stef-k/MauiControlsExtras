using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Sort direction for data grid columns.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Ascending sort order.
    /// </summary>
    Ascending,

    /// <summary>
    /// Descending sort order.
    /// </summary>
    Descending
}

/// <summary>
/// Grid lines visibility options.
/// </summary>
public enum GridLinesVisibility
{
    /// <summary>
    /// No grid lines.
    /// </summary>
    None,

    /// <summary>
    /// Horizontal grid lines only.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical grid lines only.
    /// </summary>
    Vertical,

    /// <summary>
    /// Both horizontal and vertical grid lines.
    /// </summary>
    Both
}

/// <summary>
/// Base class for data grid columns.
/// </summary>
public abstract class DataGridColumn : BindableObject, INotifyPropertyChanged
{
    private string _header = string.Empty;
    private double _width = -1; // -1 means auto
    private double _minWidth = 50;
    private double _maxWidth = double.MaxValue;
    private bool _canUserSort = true;
    private bool _canUserFilter = true;
    private bool _canUserEdit = true;
    private bool _isReadOnly;
    private bool _isVisible = true;
    private TextAlignment _textAlignment = TextAlignment.Start;
    private SortDirection? _sortDirection;
    private double _actualWidth;

    /// <summary>
    /// Gets or sets the column header text.
    /// </summary>
    public string Header
    {
        get => _header;
        set
        {
            if (_header != value)
            {
                _header = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the column width. Use -1 for auto, or a star value like 1* for proportional.
    /// </summary>
    public double Width
    {
        get => _width;
        set
        {
            if (Math.Abs(_width - value) > 0.001)
            {
                _width = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the minimum column width.
    /// </summary>
    public double MinWidth
    {
        get => _minWidth;
        set
        {
            if (Math.Abs(_minWidth - value) > 0.001)
            {
                _minWidth = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum column width.
    /// </summary>
    public double MaxWidth
    {
        get => _maxWidth;
        set
        {
            if (Math.Abs(_maxWidth - value) > 0.001)
            {
                _maxWidth = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the user can sort this column.
    /// </summary>
    public bool CanUserSort
    {
        get => _canUserSort;
        set
        {
            if (_canUserSort != value)
            {
                _canUserSort = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the user can filter this column.
    /// </summary>
    public bool CanUserFilter
    {
        get => _canUserFilter;
        set
        {
            if (_canUserFilter != value)
            {
                _canUserFilter = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the user can edit this column.
    /// </summary>
    public bool CanUserEdit
    {
        get => _canUserEdit;
        set
        {
            if (_canUserEdit != value)
            {
                _canUserEdit = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the column is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (_isReadOnly != value)
            {
                _isReadOnly = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the column is visible.
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text alignment for the column.
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => _textAlignment;
        set
        {
            if (_textAlignment != value)
            {
                _textAlignment = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the current sort direction.
    /// </summary>
    public SortDirection? SortDirection
    {
        get => _sortDirection;
        set
        {
            if (_sortDirection != value)
            {
                _sortDirection = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SortIndicator));
            }
        }
    }

    /// <summary>
    /// Gets the sort indicator text.
    /// </summary>
    public string SortIndicator => SortDirection switch
    {
        Controls.SortDirection.Ascending => "▲",
        Controls.SortDirection.Descending => "▼",
        _ => string.Empty
    };

    /// <summary>
    /// Gets or sets the actual calculated width.
    /// </summary>
    public double ActualWidth
    {
        get => _actualWidth;
        set
        {
            if (Math.Abs(_actualWidth - value) > 0.001)
            {
                _actualWidth = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the property path for data binding.
    /// </summary>
    public abstract string? PropertyPath { get; }

    /// <summary>
    /// Creates the cell content for this column.
    /// </summary>
    public abstract View CreateCellContent(object item);

    /// <summary>
    /// Creates the edit content for this column.
    /// </summary>
    public virtual View? CreateEditContent(object item) => null;

    /// <summary>
    /// Gets the cell value for sorting/filtering.
    /// </summary>
    public virtual object? GetCellValue(object item)
    {
        if (string.IsNullOrEmpty(PropertyPath))
            return null;

        var type = item.GetType();
        var property = type.GetProperty(PropertyPath);
        return property?.GetValue(item);
    }

    /// <summary>
    /// Sets the cell value after editing.
    /// </summary>
    public virtual void SetCellValue(object item, object? value)
    {
        if (string.IsNullOrEmpty(PropertyPath))
            return;

        var type = item.GetType();
        var property = type.GetProperty(PropertyPath);
        property?.SetValue(item, value);
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public new event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Text column for data grid.
/// </summary>
public class DataGridTextColumn : DataGridColumn
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
    /// Gets or sets the format string.
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
        var entry = new Entry
        {
            Text = value?.ToString() ?? string.Empty,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment
        };
        return entry;
    }

    private string FormatValue(object? value)
    {
        if (value == null)
            return string.Empty;

        if (!string.IsNullOrEmpty(Format) && value is IFormattable formattable)
        {
            return formattable.ToString(Format, null);
        }

        return value.ToString() ?? string.Empty;
    }
}

/// <summary>
/// Checkbox column for data grid.
/// </summary>
public class DataGridCheckBoxColumn : DataGridColumn
{
    private string _binding = string.Empty;

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

    /// <inheritdoc />
    public override string? PropertyPath => Binding;

    /// <inheritdoc />
    public override View CreateCellContent(object item)
    {
        var value = GetCellValue(item);
        var isChecked = value is bool b && b;

        return new Label
        {
            Text = isChecked ? "☑" : "☐",
            FontSize = 18,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };
    }

    /// <inheritdoc />
    public override View? CreateEditContent(object item)
    {
        var value = GetCellValue(item);
        var isChecked = value is bool b && b;

        var checkBox = new CheckBox
        {
            IsChecked = isChecked,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };
        return checkBox;
    }
}

/// <summary>
/// Image column for data grid.
/// </summary>
public class DataGridImageColumn : DataGridColumn
{
    private string _binding = string.Empty;
    private double _imageSize = 32;

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
    /// Gets or sets the image size.
    /// </summary>
    public double ImageSize
    {
        get => _imageSize;
        set
        {
            if (Math.Abs(_imageSize - value) > 0.001)
            {
                _imageSize = value;
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
        ImageSource? source = null;

        if (value is ImageSource imgSource)
        {
            source = imgSource;
        }
        else if (value is string str)
        {
            source = ImageSource.FromFile(str);
        }

        return new Image
        {
            Source = source,
            WidthRequest = ImageSize,
            HeightRequest = ImageSize,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Aspect = Aspect.AspectFit
        };
    }
}

/// <summary>
/// Template column for data grid.
/// </summary>
public class DataGridTemplateColumn : DataGridColumn
{
    private DataTemplate? _cellTemplate;
    private DataTemplate? _editTemplate;

    /// <summary>
    /// Gets or sets the cell template.
    /// </summary>
    public DataTemplate? CellTemplate
    {
        get => _cellTemplate;
        set
        {
            if (_cellTemplate != value)
            {
                _cellTemplate = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the edit template.
    /// </summary>
    public DataTemplate? EditTemplate
    {
        get => _editTemplate;
        set
        {
            if (_editTemplate != value)
            {
                _editTemplate = value;
                OnPropertyChanged();
            }
        }
    }

    /// <inheritdoc />
    public override string? PropertyPath => null;

    /// <inheritdoc />
    public override View CreateCellContent(object item)
    {
        if (CellTemplate?.CreateContent() is View view)
        {
            view.BindingContext = item;
            return view;
        }

        return new Label { Text = item?.ToString() ?? string.Empty };
    }

    /// <inheritdoc />
    public override View? CreateEditContent(object item)
    {
        if (EditTemplate?.CreateContent() is View view)
        {
            view.BindingContext = item;
            return view;
        }

        return null;
    }
}

/// <summary>
/// Collection of data grid columns.
/// </summary>
public class DataGridColumnCollection : List<DataGridColumn>
{
}
