using System.Collections;
using System.Diagnostics.CodeAnalysis;
using MauiControlsExtras.Helpers;

namespace MauiControlsExtras.Controls;

/// <summary>
/// ComboBox column for data grid using the library's custom ComboBox control.
/// Provides searchable/filterable dropdown with keyboard navigation.
/// </summary>
public class DataGridComboBoxColumn : DataGridColumn
{
    private string _binding = string.Empty;
    private IEnumerable? _itemsSource;
    private string? _displayMemberPath;
    private string? _selectedValuePath;
    private string? _placeholder;
    private int _visibleItemCount = 6;

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

    /// <summary>
    /// Gets or sets an AOT-safe function to extract the display text from combo items.
    /// When set, takes priority over <see cref="DisplayMemberPath"/>.
    /// </summary>
    public Func<object, string?>? DisplayMemberFunc { get; set; }

    /// <summary>
    /// Gets or sets an AOT-safe function to extract the value from combo items.
    /// When set, takes priority over <see cref="SelectedValuePath"/>.
    /// </summary>
    public Func<object, object?>? SelectedValueFunc { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text shown when no item is selected.
    /// </summary>
    public string? Placeholder
    {
        get => _placeholder;
        set
        {
            if (_placeholder != value)
            {
                _placeholder = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of visible items in the dropdown (default: 6).
    /// </summary>
    public int VisibleItemCount
    {
        get => _visibleItemCount;
        set
        {
            if (_visibleItemCount != value)
            {
                _visibleItemCount = value;
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

        // Use ComboBox with PopupMode for DataGrid editing
        // This allows filtering while avoiding clipping issues in constrained cells
        var comboBox = new ComboBox
        {
            ItemsSource = items,
            DisplayMemberPath = DisplayMemberPath,
            DisplayMemberFunc = DisplayMemberFunc,
            SelectedItem = FindSelectedItem(currentValue, items),
            PopupMode = true,
            Placeholder = Placeholder ?? "Search...",
            VisibleItemCount = VisibleItemCount,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };

        if (!string.IsNullOrEmpty(SelectedValuePath))
            comboBox.ValueMemberPath = SelectedValuePath;

        if (SelectedValueFunc != null)
            comboBox.ValueMemberFunc = SelectedValueFunc;

        return comboBox;
    }

    private object? FindSelectedItem(object? currentValue, List<object>? items)
    {
        if (currentValue == null || items == null)
            return null;

        foreach (var item in items)
        {
            var itemValue = GetItemValue(item);
            if (Equals(itemValue, currentValue))
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the value from the edit control.
    /// </summary>
    public object? GetValueFromEditControl(View editControl)
    {
        if (editControl is ComboBox comboBox)
        {
            var selectedItem = comboBox.SelectedItem;
            if (selectedItem != null)
            {
                return GetItemValue(selectedItem);
            }
            return null;
        }

        // Legacy support for Picker (in case it's used elsewhere)
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

    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
        Justification = "Reflection fallback for non-AOT scenarios. Use SelectedValueFunc for AOT compatibility.")]
    private object? GetItemValue(object item)
    {
        if (SelectedValueFunc != null)
            return SelectedValueFunc(item);

        if (string.IsNullOrEmpty(SelectedValuePath))
            return item;

        return PropertyAccessor.GetValue(item, SelectedValuePath);
    }

    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
        Justification = "Reflection fallback for non-AOT scenarios. Use DisplayMemberFunc for AOT compatibility.")]
    private string GetDisplayValue(object item)
    {
        if (DisplayMemberFunc != null)
            return DisplayMemberFunc(item) ?? string.Empty;

        if (string.IsNullOrEmpty(DisplayMemberPath))
            return item.ToString() ?? string.Empty;

        return PropertyAccessor.GetValue(item, DisplayMemberPath)?.ToString() ?? string.Empty;
    }
}
