using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A breadcrumb navigation control showing the current location in a hierarchy.
/// </summary>
public partial class Breadcrumb : StyledControlBase, IKeyboardNavigable, ISelectable
{
    #region Private Fields

    private readonly ObservableCollection<BreadcrumbItem> _items = new();
    private int _selectedIndex = -1;
    private bool _hasKeyboardFocus;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="Separator"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SeparatorProperty = BindableProperty.Create(
        nameof(Separator),
        typeof(string),
        typeof(Breadcrumb),
        "/",
        propertyChanged: OnSeparatorChanged);

    /// <summary>
    /// Identifies the <see cref="SeparatorTemplate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SeparatorTemplateProperty = BindableProperty.Create(
        nameof(SeparatorTemplate),
        typeof(DataTemplate),
        typeof(Breadcrumb),
        null,
        propertyChanged: OnSeparatorChanged);

    /// <summary>
    /// Identifies the <see cref="ItemTemplate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate),
        typeof(DataTemplate),
        typeof(Breadcrumb),
        null,
        propertyChanged: OnItemTemplateChanged);

    /// <summary>
    /// Identifies the <see cref="ShowHomeIcon"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowHomeIconProperty = BindableProperty.Create(
        nameof(ShowHomeIcon),
        typeof(bool),
        typeof(Breadcrumb),
        true,
        propertyChanged: OnShowHomeIconChanged);

    /// <summary>
    /// Identifies the <see cref="HomeIcon"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HomeIconProperty = BindableProperty.Create(
        nameof(HomeIcon),
        typeof(string),
        typeof(Breadcrumb),
        "🏠",
        propertyChanged: OnShowHomeIconChanged);

    /// <summary>
    /// Identifies the <see cref="ItemSpacing"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemSpacingProperty = BindableProperty.Create(
        nameof(ItemSpacing),
        typeof(double),
        typeof(Breadcrumb),
        8.0,
        propertyChanged: OnItemSpacingChanged);

    /// <summary>
    /// Identifies the <see cref="FontSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(Breadcrumb),
        14.0);

    /// <summary>
    /// Identifies the <see cref="ActiveItemColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ActiveItemColorProperty = BindableProperty.Create(
        nameof(ActiveItemColor),
        typeof(Color),
        typeof(Breadcrumb),
        null);

    /// <summary>
    /// Identifies the <see cref="InactiveItemColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty InactiveItemColorProperty = BindableProperty.Create(
        nameof(InactiveItemColor),
        typeof(Color),
        typeof(Breadcrumb),
        null);

    /// <summary>
    /// Identifies the <see cref="SeparatorColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SeparatorColorProperty = BindableProperty.Create(
        nameof(SeparatorColor),
        typeof(Color),
        typeof(Breadcrumb),
        null);

    /// <summary>
    /// Identifies the <see cref="HoverUnderline"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HoverUnderlineProperty = BindableProperty.Create(
        nameof(HoverUnderline),
        typeof(bool),
        typeof(Breadcrumb),
        true);

    /// <summary>
    /// Identifies the <see cref="MaxVisibleItems"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MaxVisibleItemsProperty = BindableProperty.Create(
        nameof(MaxVisibleItems),
        typeof(int),
        typeof(Breadcrumb),
        0,
        propertyChanged: OnMaxVisibleItemsChanged);

    /// <summary>
    /// Identifies the <see cref="CollapsedIndicator"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CollapsedIndicatorProperty = BindableProperty.Create(
        nameof(CollapsedIndicator),
        typeof(string),
        typeof(Breadcrumb),
        "...");

    /// <summary>
    /// Identifies the <see cref="IsKeyboardNavigationEnabled"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsKeyboardNavigationEnabledProperty = BindableProperty.Create(
        nameof(IsKeyboardNavigationEnabled),
        typeof(bool),
        typeof(Breadcrumb),
        true);

    #endregion

    #region Command Properties

    /// <summary>
    /// Identifies the <see cref="ItemClickedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemClickedCommandProperty = BindableProperty.Create(
        nameof(ItemClickedCommand),
        typeof(ICommand),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="ItemClickedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemClickedCommandParameterProperty = BindableProperty.Create(
        nameof(ItemClickedCommandParameter),
        typeof(object),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="HomeClickedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HomeClickedCommandProperty = BindableProperty.Create(
        nameof(HomeClickedCommand),
        typeof(ICommand),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="HomeClickedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HomeClickedCommandParameterProperty = BindableProperty.Create(
        nameof(HomeClickedCommandParameter),
        typeof(object),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandParameterProperty = BindableProperty.Create(
        nameof(GotFocusCommandParameter),
        typeof(object),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandParameterProperty = BindableProperty.Create(
        nameof(LostFocusCommandParameter),
        typeof(object),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandParameterProperty = BindableProperty.Create(
        nameof(KeyPressCommandParameter),
        typeof(object),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="SelectAllCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectAllCommandProperty = BindableProperty.Create(
        nameof(SelectAllCommand),
        typeof(ICommand),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="SelectAllCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectAllCommandParameterProperty = BindableProperty.Create(
        nameof(SelectAllCommandParameter),
        typeof(object),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="ClearSelectionCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearSelectionCommandProperty = BindableProperty.Create(
        nameof(ClearSelectionCommand),
        typeof(ICommand),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="ClearSelectionCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearSelectionCommandParameterProperty = BindableProperty.Create(
        nameof(ClearSelectionCommandParameter),
        typeof(object),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(
        nameof(SelectionChangedCommand),
        typeof(ICommand),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandParameterProperty = BindableProperty.Create(
        nameof(SelectionChangedCommandParameter),
        typeof(object),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="NavigatingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty NavigatingCommandProperty = BindableProperty.Create(
        nameof(NavigatingCommand),
        typeof(ICommand),
        typeof(Breadcrumb));

    /// <summary>
    /// Identifies the <see cref="NavigatingCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty NavigatingCommandParameterProperty = BindableProperty.Create(
        nameof(NavigatingCommandParameter),
        typeof(object),
        typeof(Breadcrumb));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the separator string between items.
    /// </summary>
    public string Separator
    {
        get => (string)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    /// <summary>
    /// Gets or sets a custom template for the separator.
    /// </summary>
    public DataTemplate? SeparatorTemplate
    {
        get => (DataTemplate?)GetValue(SeparatorTemplateProperty);
        set => SetValue(SeparatorTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a custom template for items.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the home icon is shown.
    /// </summary>
    public bool ShowHomeIcon
    {
        get => (bool)GetValue(ShowHomeIconProperty);
        set => SetValue(ShowHomeIconProperty, value);
    }

    /// <summary>
    /// Gets or sets the home icon.
    /// </summary>
    public string HomeIcon
    {
        get => (string)GetValue(HomeIconProperty);
        set => SetValue(HomeIconProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between items.
    /// </summary>
    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size.
    /// </summary>
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the color for the current (active) item.
    /// </summary>
    public Color? ActiveItemColor
    {
        get => (Color?)GetValue(ActiveItemColorProperty);
        set => SetValue(ActiveItemColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color for inactive (clickable) items.
    /// </summary>
    public Color? InactiveItemColor
    {
        get => (Color?)GetValue(InactiveItemColorProperty);
        set => SetValue(InactiveItemColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the separator color.
    /// </summary>
    public Color? SeparatorColor
    {
        get => (Color?)GetValue(SeparatorColorProperty);
        set => SetValue(SeparatorColorProperty, value);
    }

    /// <summary>
    /// Gets or sets whether items show underline on hover.
    /// </summary>
    public bool HoverUnderline
    {
        get => (bool)GetValue(HoverUnderlineProperty);
        set => SetValue(HoverUnderlineProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum visible items (0 = show all).
    /// When exceeded, middle items are collapsed.
    /// </summary>
    public int MaxVisibleItems
    {
        get => (int)GetValue(MaxVisibleItemsProperty);
        set => SetValue(MaxVisibleItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the indicator shown for collapsed items.
    /// </summary>
    public string CollapsedIndicator
    {
        get => (string)GetValue(CollapsedIndicatorProperty);
        set => SetValue(CollapsedIndicatorProperty, value);
    }

    /// <summary>
    /// Gets the collection of breadcrumb items.
    /// </summary>
    public ObservableCollection<BreadcrumbItem> Items => _items;

    /// <summary>
    /// Gets the effective active item color.
    /// </summary>
    public Color EffectiveActiveItemColor =>
        ActiveItemColor ?? EffectiveForegroundColor;

    /// <summary>
    /// Gets the effective inactive item color.
    /// </summary>
    public Color EffectiveInactiveItemColor =>
        InactiveItemColor ?? EffectiveAccentColor;

    /// <summary>
    /// Gets the effective separator color.
    /// </summary>
    public Color EffectiveSeparatorColor =>
        SeparatorColor ?? MauiControlsExtrasTheme.GetForegroundColor().WithAlpha(0.5f);

    /// <summary>
    /// Gets the current border color based on focus state.
    /// </summary>
    public Color CurrentBorderColor =>
        _hasKeyboardFocus ? EffectiveFocusBorderColor : EffectiveBorderColor;

    #endregion

    #region Command Properties Implementation

    /// <summary>
    /// Gets or sets the command executed when an item is clicked.
    /// </summary>
    public ICommand? ItemClickedCommand
    {
        get => (ICommand?)GetValue(ItemClickedCommandProperty);
        set => SetValue(ItemClickedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ItemClickedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ItemClickedCommandParameter
    {
        get => GetValue(ItemClickedCommandParameterProperty);
        set => SetValue(ItemClickedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when the home icon is clicked.
    /// </summary>
    public ICommand? HomeClickedCommand
    {
        get => (ICommand?)GetValue(HomeClickedCommandProperty);
        set => SetValue(HomeClickedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="HomeClickedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? HomeClickedCommandParameter
    {
        get => GetValue(HomeClickedCommandParameterProperty);
        set => SetValue(HomeClickedCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? GotFocusCommand
    {
        get => (ICommand?)GetValue(GotFocusCommandProperty);
        set => SetValue(GotFocusCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="GotFocusCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? GotFocusCommandParameter
    {
        get => GetValue(GotFocusCommandParameterProperty);
        set => SetValue(GotFocusCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="LostFocusCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? LostFocusCommandParameter
    {
        get => GetValue(LostFocusCommandParameterProperty);
        set => SetValue(LostFocusCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? KeyPressCommand
    {
        get => (ICommand?)GetValue(KeyPressCommandProperty);
        set => SetValue(KeyPressCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="KeyPressCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? KeyPressCommandParameter
    {
        get => GetValue(KeyPressCommandParameterProperty);
        set => SetValue(KeyPressCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? SelectAllCommand
    {
        get => (ICommand?)GetValue(SelectAllCommandProperty);
        set => SetValue(SelectAllCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="SelectAllCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? SelectAllCommandParameter
    {
        get => GetValue(SelectAllCommandParameterProperty);
        set => SetValue(SelectAllCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? ClearSelectionCommand
    {
        get => (ICommand?)GetValue(ClearSelectionCommandProperty);
        set => SetValue(ClearSelectionCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ClearSelectionCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ClearSelectionCommandParameter
    {
        get => GetValue(ClearSelectionCommandParameterProperty);
        set => SetValue(ClearSelectionCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="SelectionChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? SelectionChangedCommandParameter
    {
        get => GetValue(SelectionChangedCommandParameterProperty);
        set => SetValue(SelectionChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed before navigating to an item.
    /// The command parameter defaults to <see cref="BreadcrumbNavigatingEventArgs"/> unless
    /// <see cref="NavigatingCommandParameter"/> is set.
    /// </summary>
    public ICommand? NavigatingCommand
    {
        get => (ICommand?)GetValue(NavigatingCommandProperty);
        set => SetValue(NavigatingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets an optional parameter for <see cref="NavigatingCommand"/>.
    /// When null, the <see cref="BreadcrumbNavigatingEventArgs"/> is used as the parameter.
    /// </summary>
    public object? NavigatingCommandParameter
    {
        get => GetValue(NavigatingCommandParameterProperty);
        set => SetValue(NavigatingCommandParameterProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when an item is clicked.
    /// </summary>
    public event EventHandler<BreadcrumbItemClickedEventArgs>? ItemClicked;

    /// <summary>
    /// Occurs before navigating to an item (cancelable).
    /// </summary>
    public event EventHandler<BreadcrumbNavigatingEventArgs>? Navigating;

    /// <summary>
    /// Occurs when the home icon is clicked.
    /// </summary>
    public event EventHandler? HomeClicked;

    /// <inheritdoc/>
    public event EventHandler<Base.SelectionChangedEventArgs>? SelectionChanged;

    /// <inheritdoc/>
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <inheritdoc/>
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusLost;

    /// <inheritdoc/>
    public event EventHandler<KeyEventArgs>? KeyPressed;

    /// <inheritdoc/>
#pragma warning disable CS0067
    public event EventHandler<KeyEventArgs>? KeyReleased;
#pragma warning restore CS0067

    #endregion

    #region IKeyboardNavigable Implementation

    /// <inheritdoc/>
    public bool CanReceiveFocus => IsEnabled && IsVisible;

    /// <inheritdoc/>
    public bool IsKeyboardNavigationEnabled
    {
        get => (bool)GetValue(IsKeyboardNavigationEnabledProperty);
        set => SetValue(IsKeyboardNavigationEnabledProperty, value);
    }

    /// <inheritdoc/>
    public bool HasKeyboardFocus => _hasKeyboardFocus;

    /// <inheritdoc/>
    public bool HandleKeyPress(KeyEventArgs e)
    {
        if (!IsKeyboardNavigationEnabled) return false;

        KeyPressed?.Invoke(this, e);
        if (e.Handled) return true;

        if (KeyPressCommand?.CanExecute(e) == true)
        {
            KeyPressCommand.Execute(KeyPressCommandParameter ?? e);
            if (e.Handled) return true;
        }

        switch (e.Key)
        {
            case "ArrowLeft":
                SelectPreviousItem();
                return true;
            case "ArrowRight":
                SelectNextItem();
                return true;
            case "Home":
                SelectFirstItem();
                return true;
            case "End":
                SelectLastItem();
                return true;
            case "Enter":
            case " ":
                ActivateSelectedItem();
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts()
    {
        return new List<KeyboardShortcut>
        {
            new() { Key = "ArrowLeft", Description = "Select previous item", Category = "Navigation" },
            new() { Key = "ArrowRight", Description = "Select next item", Category = "Navigation" },
            new() { Key = "Home", Description = "Select first item", Category = "Navigation" },
            new() { Key = "End", Description = "Select last item", Category = "Navigation" },
            new() { Key = "Enter", Description = "Navigate to selected item", Category = "Actions" },
            new() { Key = "Space", Description = "Navigate to selected item", Category = "Actions" }
        };
    }

    /// <inheritdoc/>
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;
        _hasKeyboardFocus = true;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
        GotFocusCommand?.Execute(GotFocusCommandParameter ?? this);

        if (_selectedIndex < 0 && _items.Count > 0)
        {
            _selectedIndex = 0;
        }

        RebuildUI();
        return true;
    }

    #endregion

    #region ISelectable Implementation

    /// <inheritdoc/>
    /// <remarks>
    /// For Breadcrumb, returns true when there are items in the trail.
    /// The "selection" is the current location (last item).
    /// </remarks>
    public bool HasSelection => _items.Count > 0;

    /// <inheritdoc/>
    /// <remarks>
    /// For Breadcrumb, always returns false as multi-selection is not supported.
    /// </remarks>
    public bool IsAllSelected => false;

    /// <inheritdoc/>
    /// <remarks>
    /// Breadcrumb only supports single selection (the current location).
    /// </remarks>
    public bool SupportsMultipleSelection => false;

    /// <inheritdoc/>
    /// <remarks>
    /// For Breadcrumb, SelectAll is a no-op as it doesn't make sense
    /// to select all items in a navigation trail.
    /// </remarks>
    public void SelectAll()
    {
        // No-op for breadcrumb - selecting all items doesn't make sense
        // for a navigation trail where only one location is "current"
        SelectAllCommand?.Execute(SelectAllCommandParameter);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// For Breadcrumb, ClearSelection navigates to the root (first item)
    /// by removing all items after it.
    /// </remarks>
    public void ClearSelection()
    {
        if (_items.Count <= 1)
        {
            ClearSelectionCommand?.Execute(ClearSelectionCommandParameter);
            return;
        }

        var oldSelection = GetSelection();

        // Navigate to root (first item)
        NavigateTo(0);

        var newSelection = GetSelection();

        // Only raise if selection actually changed (NavigateTo might be cancelled)
        if (!ReferenceEquals(oldSelection, newSelection))
        {
            RaiseSelectionChanged(oldSelection, newSelection);
        }

        ClearSelectionCommand?.Execute(ClearSelectionCommandParameter);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the current location (last item in the breadcrumb trail),
    /// or null if the trail is empty.
    /// </remarks>
    public object? GetSelection()
    {
        return _items.Count > 0 ? _items[^1] : null;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// For Breadcrumb, SetSelection navigates to the specified item if it exists in the trail.
    /// Pass a BreadcrumbItem or an index (int) to navigate.
    /// Pass null to navigate to root.
    /// </remarks>
    public void SetSelection(object? selection)
    {
        var oldSelection = GetSelection();

        if (selection == null)
        {
            // Navigate to root
            if (_items.Count > 1)
            {
                NavigateTo(0);
            }
        }
        else if (selection is int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                NavigateTo(index);
            }
        }
        else if (selection is BreadcrumbItem item)
        {
            var itemIndex = _items.IndexOf(item);
            if (itemIndex >= 0)
            {
                NavigateTo(itemIndex);
            }
        }

        var newSelection = GetSelection();
        if (!ReferenceEquals(oldSelection, newSelection))
        {
            RaiseSelectionChanged(oldSelection, newSelection);
        }
    }

    private void RaiseSelectionChanged(object? oldSelection, object? newSelection)
    {
        var args = new Base.SelectionChangedEventArgs(oldSelection, newSelection);
        SelectionChanged?.Invoke(this, args);

        if (SelectionChangedCommand?.CanExecute(newSelection) == true)
        {
            SelectionChangedCommand.Execute(SelectionChangedCommandParameter ?? newSelection);
        }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Breadcrumb"/> class.
    /// </summary>
    [DynamicDependency(nameof(CurrentBorderColor), typeof(Breadcrumb))]
    public Breadcrumb()
    {
        InitializeComponent();
        _items.CollectionChanged += OnItemsCollectionChanged;
        Focused += OnControlFocused;
        Unfocused += OnControlUnfocused;
    }

    #endregion

    #region Navigation Methods

    private void SelectPreviousItem()
    {
        if (_items.Count == 0) return;
        _selectedIndex = _selectedIndex > 0 ? _selectedIndex - 1 : _items.Count - 1;
        RebuildUI();
    }

    private void SelectNextItem()
    {
        if (_items.Count == 0) return;
        _selectedIndex = (_selectedIndex + 1) % _items.Count;
        RebuildUI();
    }

    private void SelectFirstItem()
    {
        if (_items.Count == 0) return;
        _selectedIndex = 0;
        RebuildUI();
    }

    private void SelectLastItem()
    {
        if (_items.Count == 0) return;
        _selectedIndex = _items.Count - 1;
        RebuildUI();
    }

    private void ActivateSelectedItem()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
        {
            OnItemClicked(_items[_selectedIndex], _selectedIndex);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a breadcrumb item.
    /// </summary>
    public void AddItem(string text, object? tag = null)
    {
        _items.Add(new BreadcrumbItem { Text = text, Tag = tag });
    }

    /// <summary>
    /// Adds a breadcrumb item with icon.
    /// </summary>
    public void AddItem(string text, string icon, object? tag = null)
    {
        _items.Add(new BreadcrumbItem { Text = text, Icon = icon, Tag = tag });
    }

    /// <summary>
    /// Navigates to an item, removing all items after it.
    /// </summary>
    public void NavigateTo(int index)
    {
        if (index < 0 || index >= _items.Count) return;

        var item = _items[index];

        // Raise navigating event and command
        var navArgs = new BreadcrumbNavigatingEventArgs(item, index);
        Navigating?.Invoke(this, navArgs);

        var parameter = NavigatingCommandParameter ?? navArgs;
        if (NavigatingCommand?.CanExecute(parameter) == true)
            NavigatingCommand.Execute(parameter);

        if (navArgs.Cancel) return;

        // Remove items after clicked index
        while (_items.Count > index + 1)
        {
            _items.RemoveAt(_items.Count - 1);
        }
    }

    /// <summary>
    /// Clears all breadcrumb items.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }

    #endregion

    #region UI Building

    private void RebuildUI()
    {
        itemsContainer.Children.Clear();

        // Update item indices and current state
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].Index = i;
            _items[i].IsCurrent = i == _items.Count - 1;
        }

        // Home icon
        if (ShowHomeIcon)
        {
            var homeView = CreateHomeIcon();
            itemsContainer.Children.Add(homeView);

            if (_items.Count > 0)
            {
                itemsContainer.Children.Add(CreateSeparator());
            }
        }

        // Determine visible items
        var visibleItems = GetVisibleItems();

        for (int i = 0; i < visibleItems.Count; i++)
        {
            var item = visibleItems[i];

            if (item == null)
            {
                // Collapsed indicator
                var collapsed = CreateCollapsedIndicator();
                itemsContainer.Children.Add(collapsed);
            }
            else
            {
                var itemView = CreateItemView(item);
                itemsContainer.Children.Add(itemView);
            }

            // Add separator (except after last item)
            if (i < visibleItems.Count - 1)
            {
                itemsContainer.Children.Add(CreateSeparator());
            }
        }
    }

    private List<BreadcrumbItem?> GetVisibleItems()
    {
        if (MaxVisibleItems <= 0 || _items.Count <= MaxVisibleItems)
        {
            return _items.ToList<BreadcrumbItem?>();
        }

        // Show first item, collapsed indicator, then last (MaxVisibleItems - 2) items
        var result = new List<BreadcrumbItem?>();
        result.Add(_items[0]); // First item
        result.Add(null); // Collapsed indicator

        var startIndex = _items.Count - (MaxVisibleItems - 2);
        for (int i = startIndex; i < _items.Count; i++)
        {
            result.Add(_items[i]);
        }

        return result;
    }

    private View CreateHomeIcon()
    {
        var label = new Label
        {
            Text = HomeIcon,
            FontSize = FontSize,
            VerticalOptions = LayoutOptions.Center,
            TextColor = EffectiveInactiveItemColor
        };

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            HomeClicked?.Invoke(this, EventArgs.Empty);
            HomeClickedCommand?.Execute(HomeClickedCommandParameter);
        };
        label.GestureRecognizers.Add(tapGesture);

        return label;
    }

    private View CreateItemView(BreadcrumbItem item)
    {
        View content;

        if (ItemTemplate != null)
        {
            content = (View)ItemTemplate.CreateContent();
            content.BindingContext = item;
        }
        else
        {
            var stack = new HorizontalStackLayout { Spacing = 4 };

            if (!string.IsNullOrEmpty(item.Icon))
            {
                stack.Children.Add(new Label
                {
                    Text = item.Icon,
                    FontSize = FontSize,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = GetItemColor(item)
                });
            }

            var textLabel = new Label
            {
                Text = item.Text,
                FontSize = FontSize,
                VerticalOptions = LayoutOptions.Center,
                TextColor = GetItemColor(item),
                TextDecorations = item.IsCurrent ? TextDecorations.None :
                    (HoverUnderline ? TextDecorations.Underline : TextDecorations.None)
            };

            // Selection highlight
            if (_hasKeyboardFocus && item.Index == _selectedIndex)
            {
                textLabel.BackgroundColor = MauiControlsExtrasTheme.Current.SelectedBackgroundColor;
            }

            stack.Children.Add(textLabel);
            content = stack;
        }

        // Click handler (only for non-current items)
        if (!item.IsCurrent && item.IsEnabled)
        {
            var tapGesture = new TapGestureRecognizer();
            var capturedItem = item;
            var capturedIndex = item.Index;
            tapGesture.Tapped += (s, e) => OnItemClicked(capturedItem, capturedIndex);
            content.GestureRecognizers.Add(tapGesture);
        }

        return content;
    }

    private View CreateSeparator()
    {
        if (SeparatorTemplate != null)
        {
            return (View)SeparatorTemplate.CreateContent();
        }

        return new Label
        {
            Text = Separator,
            FontSize = FontSize,
            VerticalOptions = LayoutOptions.Center,
            TextColor = EffectiveSeparatorColor,
            Margin = new Thickness(ItemSpacing, 0)
        };
    }

    private View CreateCollapsedIndicator()
    {
        return new Label
        {
            Text = CollapsedIndicator,
            FontSize = FontSize,
            VerticalOptions = LayoutOptions.Center,
            TextColor = EffectiveSeparatorColor
        };
    }

    private Color GetItemColor(BreadcrumbItem item)
    {
        if (!item.IsEnabled) return EffectiveDisabledColor;
        return item.IsCurrent ? EffectiveActiveItemColor : EffectiveInactiveItemColor;
    }

    #endregion

    #region Event Handlers

    private void OnItemClicked(BreadcrumbItem item, int index)
    {
        // Execute item command
        if (item.Command?.CanExecute(item.CommandParameter) == true)
        {
            item.Command.Execute(item.CommandParameter);
        }

        var args = new BreadcrumbItemClickedEventArgs(item, index);
        ItemClicked?.Invoke(this, args);
        ItemClickedCommand?.Execute(ItemClickedCommandParameter ?? args);
    }

    private void OnControlFocused(object? sender, FocusEventArgs e)
    {
        _hasKeyboardFocus = true;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        OnPropertyChanged(nameof(CurrentBorderColor));
        KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
        GotFocusCommand?.Execute(GotFocusCommandParameter ?? this);

        if (_selectedIndex < 0 && _items.Count > 0)
        {
            _selectedIndex = 0;
        }

        RebuildUI();
    }

    private void OnControlUnfocused(object? sender, FocusEventArgs e)
    {
        _hasKeyboardFocus = false;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        OnPropertyChanged(nameof(CurrentBorderColor));
        KeyboardFocusLost?.Invoke(this, new KeyboardFocusEventArgs(false));
        LostFocusCommand?.Execute(LostFocusCommandParameter ?? this);
        RebuildUI();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildUI();
    }

    #endregion

    #region Property Changed Handlers

    private static void OnSeparatorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Breadcrumb breadcrumb)
        {
            breadcrumb.RebuildUI();
        }
    }

    private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Breadcrumb breadcrumb)
        {
            breadcrumb.RebuildUI();
        }
    }

    private static void OnShowHomeIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Breadcrumb breadcrumb)
        {
            breadcrumb.RebuildUI();
        }
    }

    private static void OnItemSpacingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Breadcrumb breadcrumb)
        {
            breadcrumb.RebuildUI();
        }
    }

    private static void OnMaxVisibleItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Breadcrumb breadcrumb)
        {
            breadcrumb.RebuildUI();
        }
    }

    #endregion
}
