using MauiControlsExtras.Base;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A popup for filtering data grid columns.
/// </summary>
public partial class DataGridFilterPopup : StyledControlBase
{
    // Sentinel used in _selectedValues and _checkboxMap to represent null cell values,
    // because HashSet<object>/Dictionary<object,T> cannot use null as a key.
    // Converted back to null at the emit boundary (OnApplyClicked).
    private static readonly object NullSentinel = new();

    private readonly HashSet<object> _selectedValues = new();
    private readonly Dictionary<object, CheckBox> _checkboxMap = new();
    private List<FilterItem> _allItems = new();
    private DataGridColumn? _column;

    /// <summary>
    /// Occurs when the filter is applied.
    /// </summary>
    public event EventHandler<FilterAppliedEventArgs>? FilterApplied;

    /// <summary>
    /// Occurs when the filter is cancelled.
    /// </summary>
    public event EventHandler? FilterCancelled;

    /// <summary>
    /// Gets or sets the column being filtered.
    /// </summary>
    public DataGridColumn? Column
    {
        get => _column;
        set
        {
            _column = value;
            UpdateHeader();
        }
    }

    /// <summary>
    /// Initializes a new instance of DataGridFilterPopup.
    /// </summary>
    public DataGridFilterPopup()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the available values for filtering.
    /// </summary>
    public void SetValues(IEnumerable<object?> distinctValues, IEnumerable<object>? currentlySelected = null)
    {
        _allItems.Clear();
        _selectedValues.Clear();
        _checkboxMap.Clear();

        // Add current selections (wrap null → NullSentinel for HashSet compatibility)
        if (currentlySelected != null)
        {
            foreach (var val in currentlySelected)
            {
                _selectedValues.Add(val ?? NullSentinel);
            }
        }

        // Build items list (store NullSentinel internally for null values)
        foreach (var value in distinctValues)
        {
            var wrappedValue = value ?? NullSentinel;
            var displayText = value?.ToString() ?? "(Empty)";
            var item = new FilterItem
            {
                Value = wrappedValue,
                DisplayText = displayText,
                IsSelected = _selectedValues.Contains(wrappedValue)
            };
            _allItems.Add(item);
        }

        // Sort alphabetically
        _allItems = _allItems.OrderBy(i => i.DisplayText).ToList();

        BuildCheckboxList(_allItems);
    }

    private void UpdateHeader()
    {
        headerLabel.Text = _column != null ? $"Filter: {_column.Header}" : "Filter";
    }

    private void BuildCheckboxList(IEnumerable<FilterItem> items)
    {
        checkboxContainer.Children.Clear();
        _checkboxMap.Clear();

        foreach (var item in items)
        {
            var row = new HorizontalStackLayout { Spacing = 8 };

            var checkBox = new CheckBox
            {
                IsChecked = item.IsSelected,
                VerticalOptions = LayoutOptions.Center
            };

            // Store reference for Select All / Clear (Value is never null; NullSentinel used for null cells)
            _checkboxMap[item.Value!] = checkBox;

            checkBox.CheckedChanged += (s, e) =>
            {
                if (e.Value)
                    _selectedValues.Add(item.Value!);
                else
                    _selectedValues.Remove(item.Value!);
            };

            var label = new Label
            {
                Text = item.DisplayText,
                VerticalOptions = LayoutOptions.Center
            };

            // Make the label tappable too
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => checkBox.IsChecked = !checkBox.IsChecked;
            label.GestureRecognizers.Add(tapGesture);

            row.Children.Add(checkBox);
            row.Children.Add(label);
            checkboxContainer.Children.Add(row);
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrEmpty(searchText))
        {
            BuildCheckboxList(_allItems);
        }
        else
        {
            var filtered = _allItems.Where(i =>
                i.DisplayText.ToLowerInvariant().Contains(searchText));
            BuildCheckboxList(filtered);
        }
    }

    private void OnSelectAllClicked(object? sender, EventArgs e)
    {
        foreach (var item in _allItems)
        {
            _selectedValues.Add(item.Value!);
            if (_checkboxMap.TryGetValue(item.Value!, out var checkBox))
            {
                checkBox.IsChecked = true;
            }
        }
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        _selectedValues.Clear();
        foreach (var checkBox in _checkboxMap.Values)
        {
            checkBox.IsChecked = false;
        }
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        FilterCancelled?.Invoke(this, EventArgs.Empty);
    }

    private void OnApplyClicked(object? sender, EventArgs e)
    {
        // Convert NullSentinel back to null for downstream consumers
        var values = _selectedValues.Select(v => v == NullSentinel ? null! : v).ToList();
        var args = new FilterAppliedEventArgs(_column, values, searchEntry.Text);
        FilterApplied?.Invoke(this, args);
    }

    private class FilterItem
    {
        // Value is never null internally; NullSentinel represents null cell values
        public object Value { get; set; } = null!;
        public string DisplayText { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}

/// <summary>
/// Event arguments for filter applied event.
/// </summary>
public class FilterAppliedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the column being filtered.
    /// </summary>
    public DataGridColumn? Column { get; }

    /// <summary>
    /// Gets the selected filter values.
    /// </summary>
    public IReadOnlyList<object> SelectedValues { get; }

    /// <summary>
    /// Gets the search text.
    /// </summary>
    public string? SearchText { get; }

    /// <summary>
    /// Initializes a new instance of FilterAppliedEventArgs.
    /// </summary>
    public FilterAppliedEventArgs(DataGridColumn? column, IReadOnlyList<object> selectedValues, string? searchText)
    {
        Column = column;
        SelectedValues = selectedValues;
        SearchText = searchText;
    }
}
