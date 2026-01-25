using System.Collections;

namespace MauiControlsExtras.Controls;

/// <summary>
/// ComboBox/Picker column for data grid.
/// </summary>
public class DataGridComboBoxColumn : DataGridColumn
{
    private string _binding = string.Empty;
    private IEnumerable? _itemsSource;
    private string? _displayMemberPath;
    private string? _selectedValuePath;

    /// <summary>
    /// Gets or sets the property binding path for the selected value.
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
    /// Gets or sets the collection of items for the dropdown.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => _itemsSource;
        set
        {
            if (_itemsSource != value)
            {
                _itemsSource = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the property path to display in the dropdown.
    /// </summary>
    public string? DisplayMemberPath
    {
        get => _displayMemberPath;
        set
        {
            if (_displayMemberPath != value)
            {
                _displayMemberPath = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the property path for the selected value.
    /// </summary>
    public string? SelectedValuePath
    {
        get => _selectedValuePath;
        set
        {
            if (_selectedValuePath != value)
            {
                _selectedValuePath = value;
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
        var displayText = GetDisplayText(value);

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
        var currentValue = GetCellValue(item);
        var items = GetPickerItems();

        var picker = new Picker
        {
            ItemsSource = items,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };

        // Set the selected index
        if (currentValue != null && items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var itemValue = GetItemValue(items[i]);
                if (Equals(itemValue, currentValue))
                {
                    picker.SelectedIndex = i;
                    break;
                }
            }
        }

        return picker;
    }

    /// <summary>
    /// Gets the value from the edit control.
    /// </summary>
    public object? GetValueFromEditControl(View editControl)
    {
        if (editControl is Picker picker && picker.SelectedIndex >= 0)
        {
            var items = GetPickerItems();
            if (items != null && picker.SelectedIndex < items.Count)
            {
                return GetItemValue(items[picker.SelectedIndex]);
            }
        }
        return null;
    }

    private List<object>? GetPickerItems()
    {
        if (ItemsSource == null)
            return null;

        var items = new List<object>();
        foreach (var sourceItem in ItemsSource)
        {
            if (sourceItem != null)
            {
                items.Add(sourceItem);
            }
        }
        return items;
    }

    private string GetDisplayText(object? value)
    {
        if (value == null)
            return string.Empty;

        // If we have a display member path, find the matching item and get its display text
        if (!string.IsNullOrEmpty(DisplayMemberPath) && ItemsSource != null)
        {
            foreach (var sourceItem in ItemsSource)
            {
                if (sourceItem == null)
                    continue;

                var itemValue = GetItemValue(sourceItem);
                if (Equals(itemValue, value))
                {
                    return GetDisplayValue(sourceItem);
                }
            }
        }

        return value.ToString() ?? string.Empty;
    }

    private object? GetItemValue(object item)
    {
        if (string.IsNullOrEmpty(SelectedValuePath))
            return item;

        var property = item.GetType().GetProperty(SelectedValuePath);
        return property?.GetValue(item);
    }

    private string GetDisplayValue(object item)
    {
        if (string.IsNullOrEmpty(DisplayMemberPath))
            return item.ToString() ?? string.Empty;

        var property = item.GetType().GetProperty(DisplayMemberPath);
        return property?.GetValue(item)?.ToString() ?? string.Empty;
    }
}
