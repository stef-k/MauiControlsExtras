using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;
using MauiControlsExtras.Converters;
using MauiControlsExtras.ContextMenu;
using MauiControlsExtras.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A dropdown control that allows selecting multiple items, displaying selections as removable chips.
/// </summary>
public partial class MultiSelectComboBox : TextStyledControlBase, IValidatable, Base.IKeyboardNavigable, Base.IClipboardSupport, Base.IContextMenuSupport
{
    #region Fields

    private bool _isExpanded;
    private bool _isUpdatingSelection;
    private CancellationTokenSource? _debounceTokenSource;
    private readonly List<string> _validationErrors = new();
    private readonly Dictionary<object, CheckBox> _itemCheckboxes = new();
    private int _highlightedIndex = -1;
    private bool _isKeyboardNavigationEnabled = true;
    private static readonly List<Base.KeyboardShortcut> _keyboardShortcuts = new();
    private readonly ContextMenuItemCollection _contextMenuItems = new();
    private CancellationTokenSource? _longPressCts;

    #endregion

    #region Bindable Properties

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(MultiSelectComboBox),
        default(IEnumerable),
        propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(
        nameof(SelectedItems),
        typeof(IList),
        typeof(MultiSelectComboBox),
        default(IList),
        BindingMode.TwoWay,
        propertyChanged: OnSelectedItemsChanged);

    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath),
        typeof(string),
        typeof(MultiSelectComboBox),
        default(string),
        propertyChanged: OnDisplayMemberPathChanged);

    public static readonly BindableProperty ValueMemberPathProperty = BindableProperty.Create(
        nameof(ValueMemberPath),
        typeof(string),
        typeof(MultiSelectComboBox),
        default(string));

    public static readonly BindableProperty IconMemberPathProperty = BindableProperty.Create(
        nameof(IconMemberPath),
        typeof(string),
        typeof(MultiSelectComboBox),
        default(string),
        propertyChanged: OnIconMemberPathChanged);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(MultiSelectComboBox),
        "Select items...");

    public static readonly BindableProperty MaxSelectionsProperty = BindableProperty.Create(
        nameof(MaxSelections),
        typeof(int?),
        typeof(MultiSelectComboBox),
        default(int?),
        propertyChanged: OnMaxSelectionsChanged);

    public static readonly BindableProperty IsSearchableProperty = BindableProperty.Create(
        nameof(IsSearchable),
        typeof(bool),
        typeof(MultiSelectComboBox),
        true);

    public static readonly BindableProperty SelectAllOptionProperty = BindableProperty.Create(
        nameof(SelectAllOption),
        typeof(bool),
        typeof(MultiSelectComboBox),
        false);

    public static readonly BindableProperty VisibleItemCountProperty = BindableProperty.Create(
        nameof(VisibleItemCount),
        typeof(int),
        typeof(MultiSelectComboBox),
        5);

    public static readonly BindableProperty FilteredItemsProperty = BindableProperty.Create(
        nameof(FilteredItems),
        typeof(ObservableCollection<object>),
        typeof(MultiSelectComboBox),
        default(ObservableCollection<object>));

    public static readonly BindableProperty ListMaxHeightProperty = BindableProperty.Create(
        nameof(ListMaxHeight),
        typeof(double),
        typeof(MultiSelectComboBox),
        200.0);

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate),
        typeof(DataTemplate),
        typeof(MultiSelectComboBox),
        null,
        propertyChanged: OnItemTemplateChanged);

    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(MultiSelectComboBox),
        false);

    public static readonly BindableProperty RequiredErrorMessageProperty = BindableProperty.Create(
        nameof(RequiredErrorMessage),
        typeof(string),
        typeof(MultiSelectComboBox),
        "At least one selection is required");

    public static readonly BindableProperty MinSelectionsProperty = BindableProperty.Create(
        nameof(MinSelections),
        typeof(int),
        typeof(MultiSelectComboBox),
        0);

    #endregion

    #region Command Bindable Properties

    public static readonly BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(
        nameof(SelectionChangedCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox),
        default(ICommand));

    public static readonly BindableProperty ItemSelectedCommandProperty = BindableProperty.Create(
        nameof(ItemSelectedCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox),
        default(ICommand));

    public static readonly BindableProperty ItemDeselectedCommandProperty = BindableProperty.Create(
        nameof(ItemDeselectedCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox),
        default(ICommand));

    public static readonly BindableProperty OpenedCommandProperty = BindableProperty.Create(
        nameof(OpenedCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox),
        default(ICommand));

    public static readonly BindableProperty ClosedCommandProperty = BindableProperty.Create(
        nameof(ClosedCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox),
        default(ICommand));

    public static readonly BindableProperty ValidateCommandProperty = BindableProperty.Create(
        nameof(ValidateCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox),
        default(ICommand));

    public static readonly BindableProperty CopyCommandProperty = BindableProperty.Create(
        nameof(CopyCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox),
        default(ICommand));

    public static readonly BindableProperty CutCommandProperty = BindableProperty.Create(
        nameof(CutCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox),
        default(ICommand));

    public static readonly BindableProperty PasteCommandProperty = BindableProperty.Create(
        nameof(PasteCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox),
        default(ICommand));

    /// <summary>
    /// Identifies the <see cref="ShowDefaultContextMenu"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowDefaultContextMenuProperty = BindableProperty.Create(
        nameof(ShowDefaultContextMenu),
        typeof(bool),
        typeof(MultiSelectComboBox),
        true);

    /// <summary>
    /// Identifies the <see cref="ContextMenuOpeningCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContextMenuOpeningCommandProperty = BindableProperty.Create(
        nameof(ContextMenuOpeningCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the collection of available items.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of selected items.
    /// </summary>
    public IList? SelectedItems
    {
        get => (IList?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path for display text.
    /// </summary>
    public string? DisplayMemberPath
    {
        get => (string?)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path for the value.
    /// </summary>
    public string? ValueMemberPath
    {
        get => (string?)GetValue(ValueMemberPathProperty);
        set => SetValue(ValueMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path for item icons.
    /// </summary>
    public string? IconMemberPath
    {
        get => (string?)GetValue(IconMemberPathProperty);
        set => SetValue(IconMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of selections allowed.
    /// </summary>
    public int? MaxSelections
    {
        get => (int?)GetValue(MaxSelectionsProperty);
        set => SetValue(MaxSelectionsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether searching is enabled.
    /// </summary>
    public bool IsSearchable
    {
        get => (bool)GetValue(IsSearchableProperty);
        set => SetValue(IsSearchableProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show the Select All option.
    /// </summary>
    public bool SelectAllOption
    {
        get => (bool)GetValue(SelectAllOptionProperty);
        set => SetValue(SelectAllOptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of visible items without scrolling.
    /// </summary>
    public int VisibleItemCount
    {
        get => (int)GetValue(VisibleItemCountProperty);
        set => SetValue(VisibleItemCountProperty, value);
    }

    /// <summary>
    /// Gets the filtered items collection.
    /// </summary>
    public ObservableCollection<object> FilteredItems
    {
        get => (ObservableCollection<object>)GetValue(FilteredItemsProperty);
        private set => SetValue(FilteredItemsProperty, value);
    }

    /// <summary>
    /// Gets the maximum height for the dropdown list.
    /// </summary>
    public double ListMaxHeight
    {
        get => (double)GetValue(ListMaxHeightProperty);
        private set => SetValue(ListMaxHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets a custom template for dropdown items.
    /// When set, overrides the default template created from DisplayMemberPath.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets whether selection is required.
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets the required error message.
    /// </summary>
    public string RequiredErrorMessage
    {
        get => (string)GetValue(RequiredErrorMessageProperty);
        set => SetValue(RequiredErrorMessageProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum number of selections required.
    /// </summary>
    public int MinSelections
    {
        get => (int)GetValue(MinSelectionsProperty);
        set => SetValue(MinSelectionsProperty, value);
    }

    /// <summary>
    /// Gets whether the dropdown is expanded.
    /// </summary>
    public bool IsExpanded => _isExpanded;

    /// <summary>
    /// Gets the count of selected items.
    /// </summary>
    public int SelectedCount => SelectedItems?.Count ?? 0;

    /// <summary>
    /// Gets whether the max selection limit has been reached.
    /// </summary>
    public bool IsMaxReached => MaxSelections.HasValue && SelectedCount >= MaxSelections.Value;

    #endregion

    #region Command Properties

    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    public ICommand? ItemSelectedCommand
    {
        get => (ICommand?)GetValue(ItemSelectedCommandProperty);
        set => SetValue(ItemSelectedCommandProperty, value);
    }

    public ICommand? ItemDeselectedCommand
    {
        get => (ICommand?)GetValue(ItemDeselectedCommandProperty);
        set => SetValue(ItemDeselectedCommandProperty, value);
    }

    public ICommand? OpenedCommand
    {
        get => (ICommand?)GetValue(OpenedCommandProperty);
        set => SetValue(OpenedCommandProperty, value);
    }

    public ICommand? ClosedCommand
    {
        get => (ICommand?)GetValue(ClosedCommandProperty);
        set => SetValue(ClosedCommandProperty, value);
    }

    public ICommand? ValidateCommand
    {
        get => (ICommand?)GetValue(ValidateCommandProperty);
        set => SetValue(ValidateCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? CopyCommand
    {
        get => (ICommand?)GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? CutCommand
    {
        get => (ICommand?)GetValue(CutCommandProperty);
        set => SetValue(CutCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? PasteCommand
    {
        get => (ICommand?)GetValue(PasteCommandProperty);
        set => SetValue(PasteCommandProperty, value);
    }

    #endregion

    #region IClipboardSupport

    /// <inheritdoc />
    public bool CanCopy => IsEnabled && searchEntry?.Text?.Length > 0;

    /// <inheritdoc />
    public bool CanCut => CanCopy;

    /// <inheritdoc />
    public bool CanPaste => IsEnabled;

    /// <inheritdoc />
    public void Copy()
    {
        if (!CanCopy) return;
        var content = GetClipboardContent();
        if (content is string text)
            Clipboard.Default.SetTextAsync(text).ConfigureAwait(false);
        CopyCommand?.Execute(content);
    }

    /// <inheritdoc />
    public void Cut()
    {
        if (!CanCut) return;
        var content = GetClipboardContent();
        if (content is string text)
            Clipboard.Default.SetTextAsync(text).ConfigureAwait(false);
        searchEntry.Text = string.Empty;
        CutCommand?.Execute(content);
    }

    /// <inheritdoc />
    public void Paste()
    {
        if (!CanPaste) return;
        var task = Clipboard.Default.GetTextAsync();
        task.ContinueWith(t =>
        {
            if (t.Result is string text)
                MainThread.BeginInvokeOnMainThread(() => searchEntry.Text = text);
        }, TaskScheduler.Default);
        PasteCommand?.Execute(null);
    }

    /// <inheritdoc />
    public object? GetClipboardContent() => searchEntry?.Text;

    #endregion

    #region IContextMenuSupport

    /// <inheritdoc />
    public ContextMenuItemCollection ContextMenuItems => _contextMenuItems;

    /// <summary>
    /// Gets or sets whether to show default context menu items (Copy, Cut, Paste, Select All, Clear).
    /// </summary>
    public bool ShowDefaultContextMenu
    {
        get => (bool)GetValue(ShowDefaultContextMenuProperty);
        set => SetValue(ShowDefaultContextMenuProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the context menu is opening.
    /// </summary>
    public ICommand? ContextMenuOpeningCommand
    {
        get => (ICommand?)GetValue(ContextMenuOpeningCommandProperty);
        set => SetValue(ContextMenuOpeningCommandProperty, value);
    }

    /// <inheritdoc />
    public event EventHandler<ContextMenuOpeningEventArgs>? ContextMenuOpening;

    /// <inheritdoc />
    public void ShowContextMenu(Point? position = null)
    {
        _ = ShowContextMenuAsync(position);
    }

    /// <summary>
    /// Asynchronously shows the context menu.
    /// </summary>
    /// <param name="position">The position to show the menu at.</param>
    public async Task ShowContextMenuAsync(Point? position = null)
    {
        var menuItems = new ContextMenuItemCollection();

        // Add custom items first
        foreach (var item in _contextMenuItems)
        {
            menuItems.Add(item);
        }

        // Add separator if we have custom items and will add default items
        if (_contextMenuItems.Count > 0 && ShowDefaultContextMenu)
        {
            menuItems.AddSeparator();
        }

        // Add default clipboard and editing items
        if (ShowDefaultContextMenu)
        {
            var copyItem = ContextMenuItem.Create(
                "Copy",
                Copy,
                "\uE8C8",
                GetPlatformShortcutText("C"));
            copyItem.IsEnabled = CanCopy;
            menuItems.Add(copyItem);

            var cutItem = ContextMenuItem.Create(
                "Cut",
                Cut,
                "\uE8C6",
                GetPlatformShortcutText("X"));
            cutItem.IsEnabled = CanCut;
            menuItems.Add(cutItem);

            var pasteItem = ContextMenuItem.Create(
                "Paste",
                Paste,
                "\uE77F",
                GetPlatformShortcutText("V"));
            pasteItem.IsEnabled = CanPaste;
            menuItems.Add(pasteItem);

            menuItems.AddSeparator();

            var selectAllItem = ContextMenuItem.Create(
                "Select All",
                SelectAllText,
                "\uE8B3",
                GetPlatformShortcutText("A"));
            selectAllItem.IsEnabled = searchEntry?.Text?.Length > 0;
            menuItems.Add(selectAllItem);

            menuItems.AddSeparator();

            var clearItem = ContextMenuItem.Create(
                "Clear",
                ClearSelection,
                "\uE74D");
            clearItem.IsEnabled = SelectedCount > 0;
            menuItems.Add(clearItem);
        }

        if (menuItems.Count == 0) return;

        // Raise opening event
        var eventArgs = new ContextMenuOpeningEventArgs(
            menuItems,
            position ?? Point.Zero,
            this);

        ContextMenuOpening?.Invoke(this, eventArgs);

        if (ContextMenuOpeningCommand?.CanExecute(eventArgs) == true)
        {
            ContextMenuOpeningCommand.Execute(eventArgs);
        }

        if (eventArgs.Cancel || eventArgs.Handled) return;

        await ContextMenuService.Current.ShowAsync(this, menuItems.ToList(), position);
    }

    private void SelectAllText()
    {
        if (searchEntry?.Text?.Length > 0)
        {
            searchEntry.CursorPosition = 0;
            searchEntry.SelectionLength = searchEntry.Text.Length;
        }
    }

    private static string GetPlatformShortcutText(string key)
    {
#if MACCATALYST || IOS
        return $"⌘{key}";
#else
        return $"Ctrl+{key}";
#endif
    }

    #endregion

    #region IValidatable

    public bool IsValid => _validationErrors.Count == 0;

    public IReadOnlyList<string> ValidationErrors => _validationErrors.AsReadOnly();

    public ValidationResult Validate()
    {
        _validationErrors.Clear();

        var count = SelectedCount;

        if (IsRequired && count == 0)
        {
            _validationErrors.Add(RequiredErrorMessage);
        }

        if (MinSelections > 0 && count < MinSelections)
        {
            _validationErrors.Add($"At least {MinSelections} selection(s) required.");
        }

        if (MaxSelections.HasValue && count > MaxSelections.Value)
        {
            _validationErrors.Add($"Maximum {MaxSelections.Value} selection(s) allowed.");
        }

        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(ValidationErrors));

        var result = _validationErrors.Count == 0
            ? ValidationResult.Success
            : ValidationResult.Failure(_validationErrors);

        if (ValidateCommand?.CanExecute(result) == true)
        {
            ValidateCommand.Execute(result);
        }

        return result;
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler<IList?>? SelectionChanged;

    /// <summary>
    /// Occurs when an item is selected.
    /// </summary>
    public event EventHandler<object>? ItemSelected;

    /// <summary>
    /// Occurs when an item is deselected.
    /// </summary>
    public event EventHandler<object>? ItemDeselected;

    /// <summary>
    /// Occurs when the dropdown opens.
    /// </summary>
    public event EventHandler? Opened;

    /// <summary>
    /// Occurs when the dropdown closes.
    /// </summary>
    public event EventHandler? Closed;

    #endregion

    #region Constructor

    public MultiSelectComboBox()
    {
        InitializeComponent();
        FilteredItems = new ObservableCollection<object>();
        UpdateListMaxHeight();
        SetupItemTemplate();
        UpdateChipsDisplay();
        searchEntry.HandlerChanged += OnSearchEntryHandlerChanged;

        SetupContextMenuGestures();
    }

    private void OnSearchEntryHandlerChanged(object? sender, EventArgs e)
    {
        if (searchEntry.Handler?.PlatformView == null) return;

#if WINDOWS
        if (searchEntry.Handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            textBox.KeyDown += OnWindowsTextBoxKeyDown;
        }
#endif

        MobileClipboardBridge.Setup(searchEntry, this);
    }

#if WINDOWS
    private void OnWindowsTextBoxKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (!_isExpanded) return;

        switch (e.Key)
        {
            case Windows.System.VirtualKey.Down:
                if (FilteredItems.Count > 0)
                {
                    _highlightedIndex = (_highlightedIndex + 1) % FilteredItems.Count;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.Up:
                if (FilteredItems.Count > 0)
                {
                    _highlightedIndex = _highlightedIndex <= 0 ? FilteredItems.Count - 1 : _highlightedIndex - 1;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.Space:
                if (_highlightedIndex >= 0 && _highlightedIndex < FilteredItems.Count)
                {
                    var item = FilteredItems[_highlightedIndex];
                    if (IsItemSelected(item))
                        DeselectItem(item);
                    else
                        SelectItem(item);
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.Escape:
                Close();
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.Home:
                if (FilteredItems.Count > 0)
                {
                    _highlightedIndex = 0;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;

            case Windows.System.VirtualKey.End:
                if (FilteredItems.Count > 0)
                {
                    _highlightedIndex = FilteredItems.Count - 1;
                    UpdateHighlightVisual();
                    e.Handled = true;
                }
                break;
        }
    }
#endif

    #endregion

    #region Property Changed Handlers

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MultiSelectComboBox control)
        {
            control.UpdateFilteredItems(string.Empty);
            control.SetupItemTemplate();
        }
    }

    private static void OnSelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MultiSelectComboBox control)
        {
            // Unsubscribe from old collection
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= control.OnSelectedItemsCollectionChanged;
            }

            // Subscribe to new collection
            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += control.OnSelectedItemsCollectionChanged;
            }

            control.UpdateChipsDisplay();
            control.UpdateCheckboxStates();
            control.UpdateSelectAllState();
            control.OnPropertyChanged(nameof(SelectedCount));
            control.OnPropertyChanged(nameof(IsMaxReached));
        }
    }

    private void OnSelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isUpdatingSelection) return;

        UpdateChipsDisplay();
        UpdateCheckboxStates();
        UpdateSelectAllState();
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(IsMaxReached));
        RaiseSelectionChanged();
    }

    private static void OnDisplayMemberPathChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MultiSelectComboBox control)
        {
            control.SetupItemTemplate();
            control.UpdateChipsDisplay();
        }
    }

    private static void OnIconMemberPathChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MultiSelectComboBox control)
        {
            control.SetupItemTemplate();
        }
    }

    private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MultiSelectComboBox control)
        {
            control.SetupItemTemplate();
        }
    }

    private static void OnMaxSelectionsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MultiSelectComboBox control)
        {
            control.OnPropertyChanged(nameof(IsMaxReached));
            control.UpdateCheckboxStates();
        }
    }

    #endregion

    #region Event Handlers

    private void OnCollapsedTapped(object? sender, TappedEventArgs e)
    {
        ToggleDropdown();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        clearSearchButton.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);

        _debounceTokenSource?.Cancel();
        _debounceTokenSource = new CancellationTokenSource();

        var token = _debounceTokenSource.Token;
        Task.Delay(100, token).ContinueWith(_ =>
        {
            if (!token.IsCancellationRequested)
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateFilteredItems(e.NewTextValue));
            }
        }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    private void OnClearSearchTapped(object? sender, TappedEventArgs e)
    {
        searchEntry.Text = string.Empty;
        UpdateFilteredItems(string.Empty);
    }

    private void OnSelectAllTapped(object? sender, TappedEventArgs e)
    {
        selectAllCheckBox.IsChecked = !selectAllCheckBox.IsChecked;
    }

    private void OnSelectAllCheckChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (_isUpdatingSelection) return;

        _isUpdatingSelection = true;
        try
        {
            if (e.Value)
            {
                SelectAll();
            }
            else
            {
                ClearSelection();
            }
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    private void OnItemCheckChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (_isUpdatingSelection) return;

        if (sender is CheckBox checkBox && checkBox.BindingContext is { } item)
        {
            if (e.Value)
            {
                SelectItem(item);
            }
            else
            {
                DeselectItem(item);
            }
        }
    }

    private void OnChipRemoveTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Label label && label.BindingContext is { } item)
        {
            DeselectItem(item);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Selects all available items.
    /// </summary>
    public void SelectAll()
    {
        if (ItemsSource == null) return;

        EnsureSelectedItemsList();

        _isUpdatingSelection = true;
        try
        {
            var maxToSelect = MaxSelections ?? int.MaxValue;
            var currentCount = SelectedCount;

            foreach (var item in ItemsSource)
            {
                if (item == null) continue;
                if (currentCount >= maxToSelect) break;

                if (!IsItemSelected(item))
                {
                    SelectedItems!.Add(item);
                    currentCount++;
                    RaiseItemSelected(item);
                }
            }

            UpdateChipsDisplay();
            UpdateCheckboxStates();
            UpdateSelectAllState();
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(IsMaxReached));
            RaiseSelectionChanged();
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    /// <summary>
    /// Clears all selections.
    /// </summary>
    public void ClearSelection()
    {
        if (SelectedItems == null || SelectedItems.Count == 0) return;

        _isUpdatingSelection = true;
        try
        {
            var items = SelectedItems.Cast<object>().ToList();
            SelectedItems.Clear();

            foreach (var item in items)
            {
                RaiseItemDeselected(item);
            }

            UpdateChipsDisplay();
            UpdateCheckboxStates();
            UpdateSelectAllState();
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(IsMaxReached));
            RaiseSelectionChanged();
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    /// <summary>
    /// Selects a specific item.
    /// </summary>
    public void SelectItem(object item)
    {
        if (item == null) return;
        if (IsMaxReached) return;
        if (IsItemSelected(item)) return;

        EnsureSelectedItemsList();

        _isUpdatingSelection = true;
        try
        {
            SelectedItems!.Add(item);
            UpdateChipsDisplay();
            UpdateCheckboxStates();
            UpdateSelectAllState();
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(IsMaxReached));
            RaiseItemSelected(item);
            RaiseSelectionChanged();
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    /// <summary>
    /// Deselects a specific item.
    /// </summary>
    public void DeselectItem(object item)
    {
        if (item == null || SelectedItems == null) return;
        if (!IsItemSelected(item)) return;

        _isUpdatingSelection = true;
        try
        {
            SelectedItems.Remove(item);

            // Immediately update the checkbox for this item if it's in the dictionary
            if (_itemCheckboxes.TryGetValue(item, out var checkBox))
            {
                checkBox.IsChecked = false;
                checkBox.IsEnabled = true;
                checkBox.Opacity = 1.0;
            }

            UpdateChipsDisplay();
            UpdateCheckboxStates();
            UpdateSelectAllState();
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(IsMaxReached));
        }
        finally
        {
            _isUpdatingSelection = false;
        }

        RaiseItemDeselected(item);
        RaiseSelectionChanged();
    }

    /// <summary>
    /// Opens the dropdown.
    /// </summary>
    public void Open()
    {
        if (!_isExpanded) ToggleDropdown();
    }

    /// <summary>
    /// Closes the dropdown.
    /// </summary>
    public void Close()
    {
        if (_isExpanded) ToggleDropdown();
    }

    #endregion

    #region Private Methods

    private void EnsureSelectedItemsList()
    {
        if (SelectedItems == null)
        {
            SelectedItems = new ObservableCollection<object>();
        }
    }

    private bool IsItemSelected(object item)
    {
        if (SelectedItems == null) return false;

        foreach (var selected in SelectedItems)
        {
            if (Equals(selected, item)) return true;
        }
        return false;
    }

    private void ToggleDropdown()
    {
        _isExpanded = !_isExpanded;

        if (_isExpanded)
        {
            UpdateFilteredItems(string.Empty);
            expandedBorder.IsVisible = true;
            dropdownArrow.Text = "▲";
            collapsedBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(EffectiveCornerRadius, EffectiveCornerRadius, 0, 0) };
            collapsedBorder.Stroke = EffectiveFocusBorderColor;
            expandedBorder.Stroke = EffectiveFocusBorderColor;
            _highlightedIndex = -1;
            UpdateCheckboxStates();
            UpdateSelectAllState();
            RaiseOpened();
#if ANDROID
            AndroidBackButtonHandler.Register(this, Close);
#endif

            // Focus the search entry when dropdown opens
            if (IsSearchable)
            {
                Dispatcher.Dispatch(() => searchEntry?.Focus());
            }
        }
        else
        {
            expandedBorder.IsVisible = false;
            dropdownArrow.Text = "▼";
            collapsedBorder.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(EffectiveCornerRadius) };
            collapsedBorder.Stroke = EffectiveBorderColor;
            searchEntry.Text = string.Empty;
            searchEntry.Unfocus();
#if ANDROID
            AndroidBackButtonHandler.Unregister(this);
#endif
            RaiseClosed();
            Validate();
        }
    }

    private void UpdateFilteredItems(string? searchText)
    {
        FilteredItems.Clear();
        _itemCheckboxes.Clear();

        if (ItemsSource == null) return;

        var search = searchText?.Trim() ?? string.Empty;
        var hasSearch = !string.IsNullOrWhiteSpace(search);

        foreach (var item in ItemsSource)
        {
            if (item == null) continue;

            var displayText = GetDisplayText(item);
            if (!hasSearch || displayText.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                FilteredItems.Add(item);
            }
        }
    }

    private void SetupItemTemplate()
    {
        // If a custom ItemTemplate is provided, wrap it with checkbox support
        if (ItemTemplate != null)
        {
            itemsList.ItemTemplate = new DataTemplate(() =>
            {
                var grid = new Grid
                {
                    Padding = new Thickness(12, 8),
                    ColumnSpacing = 8,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(new GridLength(32)),
                        new ColumnDefinition(GridLength.Star)
                    }
                };

                // Checkbox
                var checkBox = new CheckBox
                {
                    VerticalOptions = LayoutOptions.Center
                };
                checkBox.SetBinding(CheckBox.ColorProperty, new Binding(nameof(EffectiveAccentColor), source: this));
                checkBox.CheckedChanged += OnItemCheckChanged;
                Grid.SetColumn(checkBox, 0);
                grid.Add(checkBox);

                // Store reference for later updates
                grid.Loaded += (s, e) =>
                {
                    if (grid.BindingContext != null)
                    {
                        _itemCheckboxes[grid.BindingContext] = checkBox;
                        checkBox.IsChecked = IsItemSelected(grid.BindingContext);

                        if (IsMaxReached && !checkBox.IsChecked)
                        {
                            checkBox.IsEnabled = false;
                            checkBox.Opacity = 0.5;
                        }
                    }
                };

                // Custom template content
                var contentView = new ContentView
                {
                    VerticalOptions = LayoutOptions.Center
                };
                var templateContent = ItemTemplate.CreateContent();
                if (templateContent is View view)
                {
                    contentView.Content = view;
                }
                Grid.SetColumn(contentView, 1);
                grid.Add(contentView);

                return grid;
            });
            return;
        }

        // Default template based on DisplayMemberPath/IconMemberPath
        var displayMemberPath = DisplayMemberPath;
        var iconMemberPath = IconMemberPath;
        var hasIcon = !string.IsNullOrEmpty(iconMemberPath);

        itemsList.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid
            {
                Padding = new Thickness(12, 8),
                ColumnSpacing = 8
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(32)));
            if (hasIcon)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(28)));
            }
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            // Checkbox
            var checkBox = new CheckBox
            {
                VerticalOptions = LayoutOptions.Center
            };
            checkBox.SetBinding(CheckBox.ColorProperty, new Binding(nameof(EffectiveAccentColor), source: this));
            checkBox.CheckedChanged += OnItemCheckChanged;
            Grid.SetColumn(checkBox, 0);
            grid.Add(checkBox);

            // Store reference for later updates
            grid.Loaded += (s, e) =>
            {
                if (grid.BindingContext != null)
                {
                    _itemCheckboxes[grid.BindingContext] = checkBox;
                    checkBox.IsChecked = IsItemSelected(grid.BindingContext);

                    // Disable if max reached and not selected
                    if (IsMaxReached && !checkBox.IsChecked)
                    {
                        checkBox.IsEnabled = false;
                        checkBox.Opacity = 0.5;
                    }
                }
            };

            var columnIndex = 1;

            // Icon
            if (hasIcon)
            {
                var image = new Image
                {
                    WidthRequest = 28,
                    HeightRequest = 28,
                    VerticalOptions = LayoutOptions.Center,
                    Aspect = Aspect.AspectFit
                };
                image.SetBinding(Image.SourceProperty, new Binding(iconMemberPath, converter: new MauiAssetImageConverter()));
                Grid.SetColumn(image, columnIndex++);
                grid.Add(image);
            }

            // Label
            var label = new Label
            {
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center
            };
            label.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#212121"), Colors.White);

            if (!string.IsNullOrEmpty(displayMemberPath))
            {
                label.SetBinding(Label.TextProperty, new Binding(displayMemberPath));
            }
            else
            {
                label.SetBinding(Label.TextProperty, new Binding("."));
            }

            Grid.SetColumn(label, columnIndex);
            grid.Add(label);

            // Tap to toggle
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                if (grid.BindingContext != null && _itemCheckboxes.TryGetValue(grid.BindingContext, out var cb))
                {
                    if (cb.IsEnabled)
                    {
                        cb.IsChecked = !cb.IsChecked;
                    }
                }
            };
            grid.GestureRecognizers.Add(tapGesture);

            return grid;
        });
    }

    private void UpdateChipsDisplay()
    {
        chipsContainer.Children.Clear();

        if (SelectedItems == null || SelectedItems.Count == 0)
        {
            // Show placeholder
            var placeholder = new Label
            {
                Text = Placeholder,
                TextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#9CA3AF")
                    : Color.FromArgb("#6B7280"),
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center
            };
            chipsContainer.Children.Add(placeholder);
            return;
        }

        foreach (var item in SelectedItems)
        {
            if (item == null) continue;
            var chip = CreateChip(item);
            chipsContainer.Children.Add(chip);
        }
    }

    private View CreateChip(object item)
    {
        var displayText = GetDisplayText(item);

        var chipBorder = new Border
        {
            BackgroundColor = EffectiveAccentColor.WithAlpha(0.15f),
            StrokeThickness = 0,
            Padding = new Thickness(8, 4),
            Margin = new Thickness(2),
            StrokeShape = new RoundRectangle { CornerRadius = 12 }
        };

        var chipGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 6
        };

        var textLabel = new Label
        {
            Text = displayText,
            FontSize = 12,
            TextColor = EffectiveAccentColor,
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(textLabel, 0);
        chipGrid.Add(textLabel);

        var removeLabel = new Label
        {
            Text = "✕",
            FontSize = 10,
            TextColor = EffectiveAccentColor.WithAlpha(0.7f),
            VerticalOptions = LayoutOptions.Center,
            BindingContext = item
        };
        var removeTap = new TapGestureRecognizer();
        removeTap.Tapped += OnChipRemoveTapped;
        removeLabel.GestureRecognizers.Add(removeTap);
        Grid.SetColumn(removeLabel, 1);
        chipGrid.Add(removeLabel);

        chipBorder.Content = chipGrid;
        return chipBorder;
    }

    private void UpdateCheckboxStates()
    {
        var maxReached = IsMaxReached;

        // Update all tracked checkboxes
        foreach (var kvp in _itemCheckboxes.ToList()) // ToList to avoid modification issues
        {
            var item = kvp.Key;
            var checkBox = kvp.Value;

            // Skip if checkbox is no longer valid
            if (checkBox.Parent == null) continue;

            var isSelected = IsItemSelected(item);

            _isUpdatingSelection = true;
            try
            {
                checkBox.IsChecked = isSelected;
            }
            finally
            {
                _isUpdatingSelection = false;
            }

            checkBox.IsEnabled = isSelected || !maxReached;
            checkBox.Opacity = checkBox.IsEnabled ? 1.0 : 0.5;
        }
    }

    private void UpdateSelectAllState()
    {
        if (!SelectAllOption) return;
        if (_isUpdatingSelection) return;

        _isUpdatingSelection = true;
        try
        {
            var allCount = ItemsSource?.Cast<object>().Count() ?? 0;
            var selectedCount = SelectedCount;

            selectAllCheckBox.IsChecked = allCount > 0 && selectedCount == allCount;
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    private void UpdateListMaxHeight()
    {
        ListMaxHeight = VisibleItemCount * 44;
    }

    private string GetDisplayText(object item)
    {
        if (item == null) return string.Empty;

        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            var property = item.GetType().GetProperty(DisplayMemberPath);
            return property?.GetValue(item)?.ToString() ?? string.Empty;
        }

        return item.ToString() ?? string.Empty;
    }

    private void RaiseSelectionChanged()
    {
        SelectionChanged?.Invoke(this, SelectedItems);

        if (SelectionChangedCommand?.CanExecute(SelectedItems) == true)
        {
            SelectionChangedCommand.Execute(SelectedItems);
        }
    }

    private void RaiseItemSelected(object item)
    {
        ItemSelected?.Invoke(this, item);

        if (ItemSelectedCommand?.CanExecute(item) == true)
        {
            ItemSelectedCommand.Execute(item);
        }
    }

    private void RaiseItemDeselected(object item)
    {
        ItemDeselected?.Invoke(this, item);

        if (ItemDeselectedCommand?.CanExecute(item) == true)
        {
            ItemDeselectedCommand.Execute(item);
        }
    }

    private void RaiseOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);

        if (OpenedCommand?.CanExecute(null) == true)
        {
            OpenedCommand.Execute(null);
        }
    }

    private void RaiseClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);

        if (ClosedCommand?.CanExecute(null) == true)
        {
            ClosedCommand.Execute(null);
        }
    }

    #endregion

    #region IKeyboardNavigable Implementation

    /// <inheritdoc />
    public bool CanReceiveFocus => IsEnabled && IsVisible;

    /// <inheritdoc />
    public bool IsKeyboardNavigationEnabled
    {
        get => _isKeyboardNavigationEnabled;
        set
        {
            _isKeyboardNavigationEnabled = value;
            OnPropertyChanged(nameof(IsKeyboardNavigationEnabled));
        }
    }

    /// <inheritdoc />
    public bool HasKeyboardFocus => IsFocused;

    /// <summary>
    /// Identifies the GotFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox));

    /// <summary>
    /// Identifies the LostFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox));

    /// <summary>
    /// Identifies the KeyPressCommand bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(MultiSelectComboBox));

    /// <inheritdoc />
    public ICommand? GotFocusCommand
    {
        get => (ICommand?)GetValue(GotFocusCommandProperty);
        set => SetValue(GotFocusCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? KeyPressCommand
    {
        get => (ICommand?)GetValue(KeyPressCommandProperty);
        set => SetValue(KeyPressCommandProperty, value);
    }

    /// <inheritdoc />
    public event EventHandler<Base.KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <inheritdoc />
#pragma warning disable CS0067 // Event is never used (raised by platform-specific handlers)
    public event EventHandler<Base.KeyboardFocusEventArgs>? KeyboardFocusLost;
#pragma warning restore CS0067

    /// <inheritdoc />
    public event EventHandler<Base.KeyEventArgs>? KeyPressed;

    /// <inheritdoc />
#pragma warning disable CS0067 // Event is never used (raised by platform-specific handlers)
    public event EventHandler<Base.KeyEventArgs>? KeyReleased;
#pragma warning restore CS0067

    /// <inheritdoc />
    public bool HandleKeyPress(Base.KeyEventArgs e)
    {
        if (!IsKeyboardNavigationEnabled) return false;

        // Raise event first
        KeyPressed?.Invoke(this, e);
        if (e.Handled) return true;

        // Execute command if set
        if (KeyPressCommand?.CanExecute(e) == true)
        {
            KeyPressCommand.Execute(e);
            if (e.Handled) return true;
        }

        return e.Key switch
        {
            "ArrowDown" => HandleDownKey(),
            "ArrowUp" => HandleUpKey(),
            "Enter" => HandleEnterKey(),
            "Space" => HandleSpaceKey(),
            "Escape" => HandleEscapeKey(),
            "Home" => HandleHomeKey(),
            "End" => HandleEndKey(),
            "A" when e.IsPlatformCommandPressed => HandleSelectAllKey(),
            "C" when e.IsPlatformCommandPressed => HandleCopyKey(),
            "X" when e.IsPlatformCommandPressed => HandleCutKey(),
            "V" when e.IsPlatformCommandPressed => HandlePasteKey(),
            _ => false
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<Base.KeyboardShortcut> GetKeyboardShortcuts()
    {
        if (_keyboardShortcuts.Count == 0)
        {
            _keyboardShortcuts.AddRange(new[]
            {
                new Base.KeyboardShortcut { Key = "ArrowDown", Description = "Open dropdown / Move to next item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "ArrowUp", Description = "Move to previous item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Enter", Description = "Open dropdown", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Space", Description = "Toggle selection on highlighted item", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Escape", Description = "Close dropdown", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Home", Description = "Move to first item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "End", Description = "Move to last item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "A", Modifiers = "Ctrl", Description = "Select all", Category = "Selection" },
                new Base.KeyboardShortcut { Key = "Ctrl+C", Description = "Copy", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "Ctrl+X", Description = "Cut", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "Ctrl+V", Description = "Paste", Category = "Clipboard" },
            });
        }
        return _keyboardShortcuts;
    }

    /// <inheritdoc />
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;

        var result = base.Focus();
        if (result)
        {
            KeyboardFocusGained?.Invoke(this, new Base.KeyboardFocusEventArgs(true));
            GotFocusCommand?.Execute(this);
        }
        return result;
    }

    #region Keyboard Handlers

    private bool HandleDownKey()
    {
        if (!_isExpanded)
        {
            Open();
            _highlightedIndex = 0;
            return true;
        }

        if (FilteredItems.Count > 0)
        {
            _highlightedIndex = (_highlightedIndex + 1) % FilteredItems.Count;
            UpdateHighlightVisual();
        }
        return true;
    }

    private bool HandleUpKey()
    {
        if (!_isExpanded) return false;

        if (FilteredItems.Count > 0)
        {
            _highlightedIndex = _highlightedIndex <= 0 ? FilteredItems.Count - 1 : _highlightedIndex - 1;
            UpdateHighlightVisual();
        }
        return true;
    }

    private bool HandleEnterKey()
    {
        if (!_isExpanded)
        {
            Open();
            return true;
        }
        return false;
    }

    private bool HandleSpaceKey()
    {
        if (_isExpanded && _highlightedIndex >= 0 && _highlightedIndex < FilteredItems.Count)
        {
            var item = FilteredItems[_highlightedIndex];
            if (IsItemSelected(item))
            {
                DeselectItem(item);
            }
            else
            {
                SelectItem(item);
            }
            return true;
        }
        else if (!_isExpanded)
        {
            Open();
            return true;
        }
        return false;
    }

    private bool HandleEscapeKey()
    {
        if (_isExpanded)
        {
            Close();
            return true;
        }
        return false;
    }

    private bool HandleHomeKey()
    {
        if (_isExpanded && FilteredItems.Count > 0)
        {
            _highlightedIndex = 0;
            UpdateHighlightVisual();
            return true;
        }
        return false;
    }

    private bool HandleEndKey()
    {
        if (_isExpanded && FilteredItems.Count > 0)
        {
            _highlightedIndex = FilteredItems.Count - 1;
            UpdateHighlightVisual();
            return true;
        }
        return false;
    }

    private bool HandleSelectAllKey()
    {
        if (SelectAllOption)
        {
            SelectAll();
            return true;
        }
        return false;
    }

    private bool HandleCopyKey() { Copy(); return CanCopy; }
    private bool HandleCutKey() { Cut(); return CanCut; }
    private bool HandlePasteKey() { Paste(); return CanPaste; }

    private void UpdateHighlightVisual()
    {
        // Scroll the highlighted item into view
        if (_highlightedIndex >= 0 && _highlightedIndex < FilteredItems.Count)
        {
            itemsList.ScrollTo(_highlightedIndex, position: ScrollToPosition.MakeVisible, animate: false);
        }
    }

    #endregion

    #endregion

    #region Context Menu Gestures

    private void SetupContextMenuGestures()
    {
        // Platform-specific context menu setup happens in HandlerChanged
        this.HandlerChanged += OnHandlerChangedForContextMenu;

        // Add long-press gesture to the collapsed border (mobile)
        AddLongPressGesture(collapsedBorder);
    }

    private void OnHandlerChangedForContextMenu(object? sender, EventArgs e)
    {
#if WINDOWS
        SetupWindowsContextMenu();
#elif MACCATALYST
        SetupMacContextMenu();
#endif
    }

#if WINDOWS
    private void SetupWindowsContextMenu()
    {
        if (Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
        {
            element.RightTapped += OnWindowsRightTapped;
        }
    }

    private void OnWindowsRightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        var position = e.GetPosition(sender as Microsoft.UI.Xaml.UIElement);
        var mauiPosition = new Point(position.X, position.Y);
        e.Handled = true;
        _longPressCts?.Cancel();
        _ = ShowContextMenuAsync(mauiPosition);
    }
#endif

#if MACCATALYST
    private void SetupMacContextMenu()
    {
        if (Handler?.PlatformView is UIKit.UIView view)
        {
            var tapRecognizer = new UIKit.UITapGestureRecognizer(OnMacSecondaryClick);
            tapRecognizer.ButtonMaskRequired = UIKit.UIEventButtonMask.Secondary;
            view.AddGestureRecognizer(tapRecognizer);
        }
    }

    private void OnMacSecondaryClick(UIKit.UITapGestureRecognizer recognizer)
    {
        var location = recognizer.LocationInView(recognizer.View);
        var mauiPosition = new Point(location.X, location.Y);
        _longPressCts?.Cancel();
        _ = ShowContextMenuAsync(mauiPosition);
    }
#endif

    private void AddLongPressGesture(View target)
    {
        Point? pressPosition = null;

        var pointerGesture = new PointerGestureRecognizer();

        pointerGesture.PointerPressed += (s, e) =>
        {
            pressPosition = e.GetPosition(target);
            _longPressCts?.Cancel();
            _longPressCts = new CancellationTokenSource();
            _ = DetectLongPressAsync(_longPressCts.Token, pressPosition);
        };

        pointerGesture.PointerMoved += (s, e) =>
        {
            if (pressPosition == null || _longPressCts == null) return;
            var currentPosition = e.GetPosition(target);
            if (currentPosition == null) return;

            var dx = currentPosition.Value.X - pressPosition.Value.X;
            var dy = currentPosition.Value.Y - pressPosition.Value.Y;
            if (Math.Sqrt(dx * dx + dy * dy) > 10)
            {
                _longPressCts?.Cancel();
                _longPressCts = null;
            }
        };

        pointerGesture.PointerReleased += (s, e) =>
        {
            _longPressCts?.Cancel();
            _longPressCts = null;
        };

        target.GestureRecognizers.Add(pointerGesture);
    }

    private async Task DetectLongPressAsync(CancellationToken cancellationToken, Point? position)
    {
        try
        {
            await Task.Delay(500, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await ShowContextMenuAsync(position);
                });
            }
        }
        catch (TaskCanceledException)
        {
            // Long press was cancelled (pointer released or moved)
        }
    }

    #endregion
}
