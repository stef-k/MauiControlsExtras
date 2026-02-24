using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using MauiControlsExtras.Base;
using MauiControlsExtras.Helpers;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Standalone popup content for ComboBox dropdown with search and list.
/// Used by DataGridView for ComboBox editing in cells.
/// </summary>
public partial class ComboBoxPopupContent : StyledControlBase
{
    private readonly ObservableCollection<object> _filteredItems = new();
    private List<object>? _allItems;
    private string? _displayMemberPath;
    private Func<object, string?>? _displayMemberFunc;
    private object? _selectedItem;
    private int _highlightedIndex = -1;
    private bool _isUpdatingHighlight;
    private Grid? _dismissOverlay;

    /// <summary>
    /// Occurs when an item is selected.
    /// </summary>
    public event EventHandler<object?>? ItemSelected;

    /// <summary>
    /// Occurs when the popup is cancelled (e.g., Escape key).
    /// </summary>
#pragma warning disable CS0067 // Event is never used (raised by platform-specific handlers)
    public event EventHandler? Cancelled;
#pragma warning restore CS0067

    /// <summary>
    /// Gets or sets the items source for the dropdown.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => _allItems;
        set
        {
            // Cache items as a list for filtering
            _allItems = value?.Cast<object>().Where(x => x != null).ToList();
            RefreshFilteredItems();
        }
    }

    /// <summary>
    /// Gets or sets the property path to use for display text.
    /// </summary>
    public string? DisplayMemberPath
    {
        get => _displayMemberPath;
        set
        {
            if (_displayMemberPath != value)
            {
                _displayMemberPath = value;
                SetupItemTemplate();
            }
        }
    }

    /// <summary>
    /// Gets or sets an AOT-safe function to extract display text from items.
    /// When set, takes priority over <see cref="DisplayMemberPath"/>.
    /// </summary>
    public Func<object, string?>? DisplayMemberFunc
    {
        get => _displayMemberFunc;
        set
        {
            if (_displayMemberFunc != value)
            {
                _displayMemberFunc = value;
                SetupItemTemplate();
            }
        }
    }

    private bool _isSearchVisible = true;

    /// <summary>
    /// Gets or sets whether the search input is visible in the popup.
    /// </summary>
    public bool IsSearchVisible
    {
        get => _isSearchVisible;
        set
        {
            if (_isSearchVisible != value)
            {
                _isSearchVisible = value;
                if (searchBorder != null)
                    searchBorder.IsVisible = value;
                if (keyboardCaptureEntry != null)
                    keyboardCaptureEntry.IsVisible = !value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the currently selected item.
    /// </summary>
    public object? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            // Highlight selected item if visible
            if (_selectedItem != null)
            {
                var index = _filteredItems.IndexOf(_selectedItem);
                if (index >= 0)
                {
                    _highlightedIndex = index;
                    UpdateHighlightVisual();
                }
            }
        }
    }

    /// <summary>
    /// Gets the filtered items collection.
    /// </summary>
    public ObservableCollection<object> FilteredItems => _filteredItems;

    /// <summary>
    /// Initializes a new instance of ComboBoxPopupContent.
    /// </summary>
    public ComboBoxPopupContent()
    {
        InitializeComponent();
        itemsList.ItemsSource = _filteredItems;
        SetupItemTemplate();
        WireUpKeyboardEvents();
    }

    /// <summary>
    /// Sets the position of the popup using translation.
    /// </summary>
    public void SetPosition(double x, double y)
    {
        TranslationX = x;
        TranslationY = y;
    }

    /// <summary>
    /// Focuses the search entry if visible, otherwise focuses the hidden keyboard capture entry.
    /// </summary>
    public new void Focus()
    {
        if (_isSearchVisible)
        {
            Dispatcher.Dispatch(() => searchEntry?.Focus());
        }
        else
        {
#if WINDOWS || MACCATALYST
            Dispatcher.Dispatch(() => keyboardCaptureEntry?.Focus());
#endif
        }
    }

    /// <summary>
    /// Resets the popup state for reuse.
    /// </summary>
    public void Reset()
    {
        if (_isSearchVisible && searchEntry != null)
        {
            searchEntry.Text = string.Empty;
        }
        // Initialize highlight to first item when search is hidden
        _highlightedIndex = _filteredItems.Count > 0 ? 0 : -1;
        UpdateHighlightVisual();
    }

    /// <summary>
    /// Gets or sets an optional anchor view for standalone popup positioning.
    /// </summary>
    public View? AnchorView { get; set; }

    /// <summary>
    /// Shows the popup anchored to the specified view (or <see cref="AnchorView"/> if not provided).
    /// Returns true if the popup was shown successfully.
    /// </summary>
    public bool ShowAnchored(View? anchor = null, PopupPlacement placement = PopupPlacement.Auto)
    {
        var target = anchor ?? AnchorView;
        if (target == null)
            return false;

        if (_dismissOverlay != null)
            return false; // Already showing

        _dismissOverlay = PopupOverlayHelper.ShowAnchored(target, this, placement, Hide);
        return true;
    }

    /// <summary>
    /// Hides the popup and removes the overlay.
    /// </summary>
    public void Hide()
    {
        if (_dismissOverlay != null)
        {
            PopupOverlayHelper.Dismiss(_dismissOverlay);
            _dismissOverlay = null;
        }
    }

    /// <summary>
    /// Refreshes the filtered items from the current search text.
    /// </summary>
    private void RefreshFilteredItems()
    {
        var searchText = searchEntry?.Text;
        UpdateFilteredItems(searchText);
    }

    private void SetupItemTemplate()
    {
        var displayMemberPath = _displayMemberPath;
        var func = _displayMemberFunc;

        itemsList.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid
            {
                Padding = new Thickness(12, 10),
                BackgroundColor = Colors.Transparent
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnItemTapped;
            grid.GestureRecognizers.Add(tapGesture);

            var label = new Label
            {
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center
            };
            label.SetAppThemeColor(Label.TextColorProperty,
                Color.FromArgb("#212121"),
                Colors.White);

            if (func != null)
            {
                label.SetBinding(Label.TextProperty, new Binding(".") { Converter = new FuncDisplayConverter(func) });
            }
            else if (!string.IsNullOrEmpty(displayMemberPath))
            {
                label.SetBinding(Label.TextProperty, new Binding(displayMemberPath));
            }
            else
            {
                label.SetBinding(Label.TextProperty, new Binding("."));
            }

            grid.Add(label);

            VisualStateManager.SetVisualStateGroups(grid, new VisualStateGroupList
            {
                new VisualStateGroup
                {
                    Name = "CommonStates",
                    States =
                    {
                        new VisualState
                        {
                            Name = "Normal",
                            Setters = { new Setter { Property = Grid.BackgroundColorProperty, Value = Colors.Transparent } }
                        },
                        new VisualState
                        {
                            Name = "PointerOver",
                            Setters =
                            {
                                new Setter
                                {
                                    Property = Grid.BackgroundColorProperty,
                                    Value = MauiControlsExtras.Theming.MauiControlsExtrasTheme.Current.HoverColor
                                }
                            }
                        },
                        new VisualState
                        {
                            Name = "Selected",
                            Setters =
                            {
                                new Setter
                                {
                                    Property = Grid.BackgroundColorProperty,
                                    Value = MauiControlsExtras.Theming.MauiControlsExtrasTheme.Current.SelectedBackgroundColor
                                }
                            }
                        }
                    }
                }
            });

            return grid;
        });
    }

    private void WireUpKeyboardEvents()
    {
        searchEntry.Completed += OnSearchEntryCompleted;
        searchEntry.HandlerChanged += OnSearchEntryHandlerChanged;
        keyboardCaptureEntry.HandlerChanged += OnKeyboardCaptureEntryHandlerChanged;
    }

    private void OnSearchEntryHandlerChanged(object? sender, EventArgs e)
    {
        if (searchEntry.Handler?.PlatformView == null) return;

#if WINDOWS
        if (searchEntry.Handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            textBox.KeyDown += OnWindowsTextBoxKeyDown;
            textBox.PreviewKeyDown += OnWindowsTextBoxPreviewKeyDown;
        }
#endif
    }

    private void OnKeyboardCaptureEntryHandlerChanged(object? sender, EventArgs e)
    {
        if (keyboardCaptureEntry.Handler?.PlatformView == null) return;

#if WINDOWS
        if (keyboardCaptureEntry.Handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            textBox.KeyDown += OnWindowsKeyboardCaptureKeyDown;
            textBox.PreviewKeyDown += OnWindowsKeyboardCapturePreviewKeyDown;
        }
#endif
    }

#if WINDOWS
    private void OnWindowsTextBoxPreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        HandleWindowsPreviewKeyDown(e);
    }

    private void OnWindowsTextBoxKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        HandleWindowsKeyDown(e);
    }

    private void OnWindowsKeyboardCapturePreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        HandleWindowsPreviewKeyDown(e);
    }

    private void OnWindowsKeyboardCaptureKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        HandleWindowsKeyDown(e);
    }

    private void HandleWindowsPreviewKeyDown(Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // Handle Tab key for autocomplete
        if (e.Key == Windows.System.VirtualKey.Tab)
        {
            if (_filteredItems.Count == 1)
            {
                SelectItem(_filteredItems[0]);
                e.Handled = true;
            }
            else if (_highlightedIndex >= 0 && _highlightedIndex < _filteredItems.Count)
            {
                SelectItem(_filteredItems[_highlightedIndex]);
                e.Handled = true;
            }
        }
    }

    private void HandleWindowsKeyDown(Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Down:
                if (_filteredItems.Count > 0)
                {
                    _highlightedIndex = (_highlightedIndex + 1) % _filteredItems.Count;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.Up:
                if (_filteredItems.Count > 0)
                {
                    _highlightedIndex = _highlightedIndex <= 0 ? _filteredItems.Count - 1 : _highlightedIndex - 1;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.Enter:
                // Select the highlighted item or single filtered item
                if (_highlightedIndex >= 0 && _highlightedIndex < _filteredItems.Count)
                {
                    SelectItem(_filteredItems[_highlightedIndex]);
                    e.Handled = true;
                }
                else if (_filteredItems.Count == 1)
                {
                    SelectItem(_filteredItems[0]);
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.Escape:
                Cancelled?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.Home:
                if (_filteredItems.Count > 0)
                {
                    _highlightedIndex = 0;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.End:
                if (_filteredItems.Count > 0)
                {
                    _highlightedIndex = _filteredItems.Count - 1;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;
        }
    }
#endif

    private void OnSearchEntryCompleted(object? sender, EventArgs e)
    {
        // Enter pressed - select highlighted item or single filtered item
        if (_filteredItems.Count == 1)
        {
            SelectItem(_filteredItems[0]);
        }
        else if (_highlightedIndex >= 0 && _highlightedIndex < _filteredItems.Count)
        {
            SelectItem(_filteredItems[_highlightedIndex]);
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        clearSearchButton.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);
        UpdateFilteredItems(e.NewTextValue);

        // Auto-highlight first result
        if (_filteredItems.Count > 0)
        {
            _highlightedIndex = 0;
        }
        else
        {
            _highlightedIndex = -1;
        }
        UpdateHighlightVisual();
    }

    private void OnClearSearchTapped(object? sender, TappedEventArgs e)
    {
        searchEntry.Text = string.Empty;
        UpdateFilteredItems(string.Empty);
    }

    private void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is { } item)
        {
            SelectItem(item);
        }
    }

    private void OnItemsListSelectionChanged(object? sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        // Ignore programmatic selection changes for highlighting
        if (_isUpdatingHighlight) return;

        // When user clicks an item in the list, select it
        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is { } item)
        {
            SelectItem(item);
        }
    }

    private void SelectItem(object item)
    {
        _selectedItem = item;
        ItemSelected?.Invoke(this, item);
    }

    private void UpdateFilteredItems(string? searchText)
    {
        _filteredItems.Clear();

        if (_allItems == null || _allItems.Count == 0)
            return;

        var search = searchText?.Trim() ?? string.Empty;
        var hasSearch = !string.IsNullOrWhiteSpace(search);

        foreach (var item in _allItems)
        {
            var displayText = GetDisplayText(item);

            if (!hasSearch || displayText.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                _filteredItems.Add(item);
            }
        }

        // Reset highlight index
        _highlightedIndex = _filteredItems.Count > 0 ? 0 : -1;
    }

    private string GetDisplayText(object item)
    {
        if (item == null)
            return string.Empty;

        if (_displayMemberFunc != null)
            return _displayMemberFunc(item) ?? string.Empty;

        if (!string.IsNullOrEmpty(_displayMemberPath))
        {
            var value = GetPropertyValueFallback(item, _displayMemberPath);
            return value?.ToString() ?? string.Empty;
        }

        return item.ToString() ?? string.Empty;
    }

    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
        Justification = "Reflection fallback for non-AOT scenarios. Use DisplayMemberFunc for AOT compatibility.")]
    private static object? GetPropertyValueFallback(object item, string propertyPath)
    {
        return PropertyAccessor.GetValue(item, propertyPath);
    }

    private void UpdateHighlightVisual()
    {
        _isUpdatingHighlight = true;
        try
        {
            // Scroll the highlighted item into view and update visual selection
            if (_highlightedIndex >= 0 && _highlightedIndex < _filteredItems.Count)
            {
                var highlightedItem = _filteredItems[_highlightedIndex];
                itemsList.SelectedItem = highlightedItem;
                itemsList.ScrollTo(_highlightedIndex, position: ScrollToPosition.MakeVisible, animate: false);
            }
            else
            {
                itemsList.SelectedItem = null;
            }
        }
        finally
        {
            _isUpdatingHighlight = false;
        }
    }

    private sealed class FuncDisplayConverter : IValueConverter
    {
        private readonly Func<object, string?> _func;
        public FuncDisplayConverter(Func<object, string?> func) => _func = func;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value != null ? _func(value) ?? string.Empty : string.Empty;
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
