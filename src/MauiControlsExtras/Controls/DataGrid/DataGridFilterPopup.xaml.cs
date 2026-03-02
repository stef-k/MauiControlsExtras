using System.ComponentModel;
using MauiControlsExtras.Base;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A popup for filtering data grid columns.
/// </summary>
public partial class DataGridFilterPopup : StyledControlBase
{
    // Sentinel used internally to represent null cell values,
    // because null cannot be stored as a FilterItem.Value directly.
    // Converted back to null at the emit boundary (OnApplyClicked).
    private static readonly object NullSentinel = new();

    private List<FilterItem> _allItems = new();
    private DataGridColumn? _column;
    private CancellationTokenSource? _debounceTokenSource;

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
        SetupItemTemplate();
    }

    private void SetupItemTemplate()
    {
        filterItemsList.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(32)),
                    new ColumnDefinition(GridLength.Star)
                },
                ColumnSpacing = 4,
                Padding = new Thickness(0, 2)
            };

            var checkBox = new CheckBox
            {
                VerticalOptions = LayoutOptions.Center
            };
            checkBox.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(FilterItem.IsSelected), BindingMode.TwoWay));

            var label = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            label.SetBinding(Label.TextProperty, new Binding(nameof(FilterItem.DisplayText)));

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                if (label.BindingContext is FilterItem item)
                {
                    item.IsSelected = !item.IsSelected;
                }
            };
            label.GestureRecognizers.Add(tapGesture);

            Grid.SetColumn(checkBox, 0);
            Grid.SetColumn(label, 1);
            grid.Children.Add(checkBox);
            grid.Children.Add(label);

            return grid;
        });
    }

    /// <summary>
    /// Sets the available values for filtering.
    /// </summary>
    public void SetValues(IEnumerable<object?> distinctValues, IEnumerable<object>? currentlySelected = null)
    {
        _debounceTokenSource?.Cancel();
        _debounceTokenSource?.Dispose();
        _debounceTokenSource = null;
        _allItems.Clear();
        searchEntry.Text = string.Empty;

        // Build a set of currently selected values for O(1) lookup
        HashSet<object>? selectedSet = null;
        if (currentlySelected != null)
        {
            selectedSet = new HashSet<object>();
            foreach (var val in currentlySelected)
            {
                selectedSet.Add(val ?? NullSentinel);
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
                DisplayTextLower = displayText.ToLowerInvariant(),
                IsSelected = selectedSet?.Contains(wrappedValue) ?? false
            };
            _allItems.Add(item);
        }

        // Sort alphabetically
        _allItems = _allItems.OrderBy(i => i.DisplayText).ToList();

        ApplySearchFilter();
    }

    private void UpdateHeader()
    {
        headerLabel.Text = _column != null ? $"Filter: {_column.Header}" : "Filter";
    }

    private void ApplySearchFilter()
    {
        var searchText = searchEntry.Text?.ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrEmpty(searchText))
        {
            filterItemsList.ItemsSource = new List<FilterItem>(_allItems);
        }
        else
        {
            filterItemsList.ItemsSource = _allItems
                .Where(i => i.DisplayTextLower.Contains(searchText))
                .ToList();
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _debounceTokenSource?.Cancel();
        _debounceTokenSource?.Dispose();
        _debounceTokenSource = new CancellationTokenSource();

        var token = _debounceTokenSource.Token;
        Task.Delay(100, token).ContinueWith(_ =>
        {
            if (!token.IsCancellationRequested)
            {
                MainThread.BeginInvokeOnMainThread(ApplySearchFilter);
            }
        }, TaskScheduler.Default);
    }

    private void OnSelectAllClicked(object? sender, EventArgs e)
    {
        foreach (var item in _allItems)
        {
            item.IsSelected = true;
        }
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        foreach (var item in _allItems)
        {
            item.IsSelected = false;
        }
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        _debounceTokenSource?.Cancel();
        _debounceTokenSource?.Dispose();
        _debounceTokenSource = null;
        FilterCancelled?.Invoke(this, EventArgs.Empty);
    }

    private void OnApplyClicked(object? sender, EventArgs e)
    {
        _debounceTokenSource?.Cancel();
        _debounceTokenSource?.Dispose();
        _debounceTokenSource = null;
        // Derive selected values from the authoritative FilterItem.IsSelected state.
        // Convert NullSentinel back to null for downstream consumers.
        var values = _allItems
            .Where(i => i.IsSelected)
            .Select(i => i.Value == NullSentinel ? null! : i.Value)
            .ToList();
        var args = new FilterAppliedEventArgs(_column, values, searchEntry.Text);
        FilterApplied?.Invoke(this, args);
    }

    private class FilterItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        // Value is never null internally; NullSentinel represents null cell values
        public object Value { get; init; } = null!;
        public string DisplayText { get; init; } = string.Empty;
        public string DisplayTextLower { get; init; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
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
