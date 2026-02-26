using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace MauiControlsExtras.Controls;

/// <summary>
/// An accordion control with expandable/collapsible sections.
/// Supports single or multiple expansion modes.
/// </summary>
[ContentProperty(nameof(Items))]
public partial class Accordion : HeaderedControlBase, IKeyboardNavigable, ISelectable
{
    #region Private Fields

    private readonly ObservableCollection<AccordionItem> _items = new();
    private int _selectedIndex = -1;
    private bool _hasKeyboardFocus;
    private VerticalStackLayout? itemsContainer;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="ExpandMode"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ExpandModeProperty = BindableProperty.Create(
        nameof(ExpandMode),
        typeof(AccordionExpandMode),
        typeof(Accordion),
        AccordionExpandMode.Single);

    /// <summary>
    /// Identifies the <see cref="IconPosition"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IconPositionProperty = BindableProperty.Create(
        nameof(IconPosition),
        typeof(ExpandIconPosition),
        typeof(Accordion),
        ExpandIconPosition.Left,
        propertyChanged: OnIconPositionChanged);

    /// <summary>
    /// Identifies the <see cref="ShowIcons"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowIconsProperty = BindableProperty.Create(
        nameof(ShowIcons),
        typeof(bool),
        typeof(Accordion),
        true,
        propertyChanged: OnShowIconsChanged);

    /// <summary>
    /// Identifies the <see cref="AnimateExpansion"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AnimateExpansionProperty = BindableProperty.Create(
        nameof(AnimateExpansion),
        typeof(bool),
        typeof(Accordion),
        true);

    /// <summary>
    /// Identifies the <see cref="AnimationDuration"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AnimationDurationProperty = BindableProperty.Create(
        nameof(AnimationDuration),
        typeof(uint),
        typeof(Accordion),
        (uint)200);

    /// <summary>
    /// Identifies the <see cref="ContentPadding"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContentPaddingProperty = BindableProperty.Create(
        nameof(ContentPadding),
        typeof(Thickness),
        typeof(Accordion),
        new Thickness(12, 8));

    /// <summary>
    /// Identifies the <see cref="ShowDividers"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowDividersProperty = BindableProperty.Create(
        nameof(ShowDividers),
        typeof(bool),
        typeof(Accordion),
        true,
        propertyChanged: OnShowDividersChanged);

    /// <summary>
    /// Identifies the <see cref="IsKeyboardNavigationEnabled"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsKeyboardNavigationEnabledProperty = BindableProperty.Create(
        nameof(IsKeyboardNavigationEnabled),
        typeof(bool),
        typeof(Accordion),
        true);

    #endregion

    #region Command Properties

    /// <summary>
    /// Identifies the <see cref="ItemExpandedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemExpandedCommandProperty = BindableProperty.Create(
        nameof(ItemExpandedCommand),
        typeof(ICommand),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="ItemExpandedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemExpandedCommandParameterProperty = BindableProperty.Create(
        nameof(ItemExpandedCommandParameter),
        typeof(object),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="ItemCollapsedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemCollapsedCommandProperty = BindableProperty.Create(
        nameof(ItemCollapsedCommand),
        typeof(ICommand),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="ItemCollapsedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemCollapsedCommandParameterProperty = BindableProperty.Create(
        nameof(ItemCollapsedCommandParameter),
        typeof(object),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandParameterProperty = BindableProperty.Create(
        nameof(GotFocusCommandParameter),
        typeof(object),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandParameterProperty = BindableProperty.Create(
        nameof(LostFocusCommandParameter),
        typeof(object),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandParameterProperty = BindableProperty.Create(
        nameof(KeyPressCommandParameter),
        typeof(object),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="SelectAllCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectAllCommandProperty = BindableProperty.Create(
        nameof(SelectAllCommand),
        typeof(ICommand),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="SelectAllCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectAllCommandParameterProperty = BindableProperty.Create(
        nameof(SelectAllCommandParameter),
        typeof(object),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="ClearSelectionCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearSelectionCommandProperty = BindableProperty.Create(
        nameof(ClearSelectionCommand),
        typeof(ICommand),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="ClearSelectionCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearSelectionCommandParameterProperty = BindableProperty.Create(
        nameof(ClearSelectionCommandParameter),
        typeof(object),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(
        nameof(SelectionChangedCommand),
        typeof(ICommand),
        typeof(Accordion));

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandParameterProperty = BindableProperty.Create(
        nameof(SelectionChangedCommandParameter),
        typeof(object),
        typeof(Accordion));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the expansion mode.
    /// </summary>
    public AccordionExpandMode ExpandMode
    {
        get => (AccordionExpandMode)GetValue(ExpandModeProperty);
        set => SetValue(ExpandModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the position of the expand icon.
    /// </summary>
    public ExpandIconPosition IconPosition
    {
        get => (ExpandIconPosition)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets whether expand icons are shown.
    /// </summary>
    public bool ShowIcons
    {
        get => (bool)GetValue(ShowIconsProperty);
        set => SetValue(ShowIconsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether expansion is animated.
    /// </summary>
    public bool AnimateExpansion
    {
        get => (bool)GetValue(AnimateExpansionProperty);
        set => SetValue(AnimateExpansionProperty, value);
    }

    /// <summary>
    /// Gets or sets the animation duration in milliseconds.
    /// </summary>
    public uint AnimationDuration
    {
        get => (uint)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets the content padding.
    /// </summary>
    public Thickness ContentPadding
    {
        get => (Thickness)GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether dividers are shown between items.
    /// </summary>
    public bool ShowDividers
    {
        get => (bool)GetValue(ShowDividersProperty);
        set => SetValue(ShowDividersProperty, value);
    }

    /// <summary>
    /// Gets the collection of accordion items.
    /// </summary>
    public ObservableCollection<AccordionItem> Items => _items;

    /// <summary>
    /// Gets the current border color based on focus state.
    /// </summary>
    public Color CurrentBorderColor =>
        _hasKeyboardFocus ? EffectiveFocusBorderColor : EffectiveBorderColor;

    /// <summary>
    /// Gets the currently selected/focused item index.
    /// </summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        private set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
    }

    /// <summary>
    /// Gets the currently selected/focused item.
    /// </summary>
    public AccordionItem? SelectedItem =>
        _selectedIndex >= 0 && _selectedIndex < _items.Count
            ? _items[_selectedIndex]
            : null;

    #endregion

    #region Command Properties Implementation

    /// <summary>
    /// Gets or sets the command executed when an item is expanded.
    /// </summary>
    public ICommand? ItemExpandedCommand
    {
        get => (ICommand?)GetValue(ItemExpandedCommandProperty);
        set => SetValue(ItemExpandedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ItemExpandedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ItemExpandedCommandParameter
    {
        get => GetValue(ItemExpandedCommandParameterProperty);
        set => SetValue(ItemExpandedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when an item is collapsed.
    /// </summary>
    public ICommand? ItemCollapsedCommand
    {
        get => (ICommand?)GetValue(ItemCollapsedCommandProperty);
        set => SetValue(ItemCollapsedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ItemCollapsedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ItemCollapsedCommandParameter
    {
        get => GetValue(ItemCollapsedCommandParameterProperty);
        set => SetValue(ItemCollapsedCommandParameterProperty, value);
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

    #endregion

    #region Events

    /// <summary>
    /// Occurs when an item is expanded.
    /// </summary>
    public event EventHandler<AccordionItemExpandedEventArgs>? ItemExpanded;

    /// <summary>
    /// Occurs when an item is collapsed.
    /// </summary>
    public event EventHandler<AccordionItemExpandedEventArgs>? ItemCollapsed;

    /// <summary>
    /// Occurs before an item expands/collapses (cancelable).
    /// </summary>
    public event EventHandler<AccordionItemExpandingEventArgs>? ItemExpanding;

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

    /// <inheritdoc/>
    public event EventHandler<Base.SelectionChangedEventArgs>? SelectionChanged;

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
            case "ArrowUp":
                SelectPreviousItem();
                return true;
            case "ArrowDown":
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
                ToggleSelectedItem();
                return true;
            case "ArrowRight":
                ExpandSelectedItem();
                return true;
            case "ArrowLeft":
                CollapseSelectedItem();
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts()
    {
        return new List<KeyboardShortcut>
        {
            new() { Key = "ArrowUp", Description = "Select previous item", Category = "Navigation" },
            new() { Key = "ArrowDown", Description = "Select next item", Category = "Navigation" },
            new() { Key = "Home", Description = "Select first item", Category = "Navigation" },
            new() { Key = "End", Description = "Select last item", Category = "Navigation" },
            new() { Key = "Enter", Description = "Toggle selected item", Category = "Actions" },
            new() { Key = "Space", Description = "Toggle selected item", Category = "Actions" },
            new() { Key = "ArrowRight", Description = "Expand selected item", Category = "Actions" },
            new() { Key = "ArrowLeft", Description = "Collapse selected item", Category = "Actions" }
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
            SelectedIndex = 0;
        }

        return true;
    }

    #endregion

    #region ISelectable Implementation

    /// <inheritdoc/>
    public bool HasSelection => _items.Any(i => i.IsExpanded);

    /// <inheritdoc/>
    public bool IsAllSelected
    {
        get
        {
            if (_items.Count == 0) return false;

            // In Single mode, having one expanded is "all" that can be selected
            if (ExpandMode == AccordionExpandMode.Single)
                return _items.Any(i => i.IsExpanded);

            return _items.All(i => i.IsExpanded);
        }
    }

    /// <inheritdoc/>
    public bool SupportsMultipleSelection => ExpandMode != AccordionExpandMode.Single;

    /// <inheritdoc/>
    void ISelectable.SelectAll()
    {
        var oldSelection = GetExpandedItems();
        ExpandAll();
        var newSelection = GetExpandedItems();
        OnSelectionChanged(oldSelection, newSelection);
    }

    /// <inheritdoc/>
    void ISelectable.ClearSelection()
    {
        var oldSelection = GetExpandedItems();
        CollapseAll();
        var newSelection = GetExpandedItems();
        OnSelectionChanged(oldSelection, newSelection);
    }

    /// <inheritdoc/>
    public object? GetSelection()
    {
        var expanded = GetExpandedItems();
        return expanded.Count switch
        {
            0 => null,
            1 => expanded[0],
            _ => expanded
        };
    }

    /// <inheritdoc/>
    public void SetSelection(object? selection)
    {
        var oldSelection = GetExpandedItems();

        if (selection is null)
        {
            CollapseAll();
        }
        else if (selection is AccordionItem item)
        {
            if (_items.Contains(item))
            {
                ExpandItem(item);
            }
        }
        else if (selection is IEnumerable<AccordionItem> items)
        {
            foreach (var i in items.Where(i => _items.Contains(i)))
            {
                ExpandItem(i);
            }
        }
        else if (selection is int index)
        {
            ExpandItem(index);
        }
        else if (selection is IEnumerable<int> indices)
        {
            foreach (var i in indices)
            {
                ExpandItem(i);
            }
        }
        else
        {
            throw new ArgumentException(
                $"Selection type {selection.GetType().Name} is not supported. " +
                "Use AccordionItem, IEnumerable<AccordionItem>, int, or IEnumerable<int>.",
                nameof(selection));
        }

        var newSelection = GetExpandedItems();
        OnSelectionChanged(oldSelection, newSelection);
    }

    private List<AccordionItem> GetExpandedItems() =>
        _items.Where(i => i.IsExpanded).ToList();

    private void OnSelectionChanged(List<AccordionItem> oldSelection, List<AccordionItem> newSelection)
    {
        // Convert to single item or list for event args
        object? oldValue = oldSelection.Count switch
        {
            0 => null,
            1 => oldSelection[0],
            _ => oldSelection
        };

        object? newValue = newSelection.Count switch
        {
            0 => null,
            1 => newSelection[0],
            _ => newSelection
        };

        // Only raise event if selection actually changed
        var oldSet = new HashSet<AccordionItem>(oldSelection);
        var newSet = new HashSet<AccordionItem>(newSelection);
        if (oldSet.SetEquals(newSet)) return;

        var args = new Base.SelectionChangedEventArgs(oldValue, newValue);
        SelectionChanged?.Invoke(this, args);
        SelectionChangedCommand?.Execute(SelectionChangedCommandParameter ?? newValue);

        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(IsAllSelected));
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Accordion"/> class.
    /// </summary>
    [DynamicDependency(nameof(CurrentBorderColor), typeof(Accordion))]
    public Accordion()
    {
        InitializeComponent();
        BuildVisualTree();
        _items.CollectionChanged += OnItemsCollectionChanged;
        Focused += OnControlFocused;
        Unfocused += OnControlUnfocused;

        // Initialize UI for items already added via XAML (before CollectionChanged was hooked up)
        if (_items.Count > 0)
        {
            RebuildUI();
        }
    }

    /// <summary>
    /// Builds the visual tree in code to avoid conflict with [ContentProperty(nameof(Items))].
    /// </summary>
    private void BuildVisualTree()
    {
        // Create the items container
        itemsContainer = new VerticalStackLayout { Spacing = 0 };

        // Create the ScrollView
        var scrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Vertical,
            VerticalScrollBarVisibility = ScrollBarVisibility.Default,
            Content = itemsContainer
        };

        // Create the corner radius shape
        var cornerRadiusShape = new RoundRectangle();
        cornerRadiusShape.SetBinding(
            RoundRectangle.CornerRadiusProperty,
            static (Accordion a) => a.EffectiveCornerRadius, source: this);

        // Create the border
        var border = new Border
        {
            StrokeShape = cornerRadiusShape,
            Content = scrollView
        };

        // Set up bindings for border
        border.SetBinding(
            Border.StrokeThicknessProperty,
            static (Accordion a) => a.EffectiveBorderThickness, source: this);
        border.SetBinding(
            Border.StrokeProperty,
            static (Accordion a) => a.CurrentBorderColor, source: this);

        // Set background color with theme support
        border.SetAppThemeColor(Border.BackgroundColorProperty,
            Color.FromArgb("#FFFFFF"),
            Color.FromArgb("#1E1E1E"));

        Content = border;
    }

    #endregion

    #region Navigation Methods

    private void SelectPreviousItem()
    {
        if (_items.Count == 0) return;
        SelectedIndex = _selectedIndex > 0 ? _selectedIndex - 1 : _items.Count - 1;
        UpdateItemHighlighting();
    }

    private void SelectNextItem()
    {
        if (_items.Count == 0) return;
        SelectedIndex = (_selectedIndex + 1) % _items.Count;
        UpdateItemHighlighting();
    }

    private void SelectFirstItem()
    {
        if (_items.Count == 0) return;
        SelectedIndex = 0;
        UpdateItemHighlighting();
    }

    private void SelectLastItem()
    {
        if (_items.Count == 0) return;
        SelectedIndex = _items.Count - 1;
        UpdateItemHighlighting();
    }

    private void ToggleSelectedItem()
    {
        if (SelectedItem != null)
        {
            ToggleItem(SelectedItem);
        }
    }

    private void ExpandSelectedItem()
    {
        if (SelectedItem != null && !SelectedItem.IsExpanded)
        {
            ExpandItem(SelectedItem);
        }
    }

    private void CollapseSelectedItem()
    {
        if (SelectedItem != null && SelectedItem.IsExpanded)
        {
            CollapseItem(SelectedItem);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Expands an item by index.
    /// </summary>
    public void ExpandItem(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            ExpandItem(_items[index]);
        }
    }

    /// <summary>
    /// Expands an item.
    /// </summary>
    public void ExpandItem(AccordionItem item)
    {
        if (!item.IsEnabled || item.IsExpanded) return;

        // Raise expanding event
        var expandingArgs = new AccordionItemExpandingEventArgs(item, item.Index, true);
        ItemExpanding?.Invoke(this, expandingArgs);
        if (expandingArgs.Cancel) return;

        // Collapse others in single mode
        if (ExpandMode == AccordionExpandMode.Single)
        {
            foreach (var other in _items.Where(i => i != item && i.IsExpanded))
            {
                CollapseItemInternal(other);
            }
        }

        ExpandItemInternal(item);
    }

    /// <summary>
    /// Collapses an item by index.
    /// </summary>
    public void CollapseItem(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            CollapseItem(_items[index]);
        }
    }

    /// <summary>
    /// Collapses an item.
    /// </summary>
    public void CollapseItem(AccordionItem item)
    {
        if (!item.IsExpanded) return;

        // Check AtLeastOne mode
        if (ExpandMode == AccordionExpandMode.AtLeastOne)
        {
            var expandedCount = _items.Count(i => i.IsExpanded);
            if (expandedCount <= 1) return;
        }

        // Raise expanding event (with isExpanding = false)
        var expandingArgs = new AccordionItemExpandingEventArgs(item, item.Index, false);
        ItemExpanding?.Invoke(this, expandingArgs);
        if (expandingArgs.Cancel) return;

        CollapseItemInternal(item);
    }

    /// <summary>
    /// Toggles an item's expansion state.
    /// </summary>
    public void ToggleItem(AccordionItem item)
    {
        if (item.IsExpanded)
        {
            CollapseItem(item);
        }
        else
        {
            ExpandItem(item);
        }
    }

    /// <summary>
    /// Expands all items.
    /// </summary>
    public void ExpandAll()
    {
        if (ExpandMode == AccordionExpandMode.Single)
        {
            // In single mode, just expand the first
            if (_items.Count > 0)
            {
                ExpandItem(_items[0]);
            }
        }
        else
        {
            foreach (var item in _items)
            {
                ExpandItemInternal(item);
            }
        }
    }

    /// <summary>
    /// Collapses all items.
    /// </summary>
    public void CollapseAll()
    {
        if (ExpandMode == AccordionExpandMode.AtLeastOne)
        {
            // Keep first one expanded
            for (int i = 1; i < _items.Count; i++)
            {
                CollapseItemInternal(_items[i]);
            }
        }
        else
        {
            foreach (var item in _items)
            {
                CollapseItemInternal(item);
            }
        }
    }

    #endregion

    #region UI Building

    private void RebuildUI()
    {
        if (itemsContainer == null)
            return;

        itemsContainer.Children.Clear();

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            item.Index = i;
            item.IsExpandedChanged += OnItemExpandedChanged;

            var itemView = CreateItemView(item, i);
            itemsContainer.Children.Add(itemView);
            System.Diagnostics.Trace.WriteLine($"[Accordion] Added item {i}: {item.Header}");
        }
    }

    private View CreateItemView(AccordionItem item, int index)
    {
        var container = new VerticalStackLayout { Spacing = 0 };

        // Header
        var header = CreateHeader(item, index);
        container.Children.Add(header);

        // Content container
        var contentWrapper = new Border
        {
            Padding = ContentPadding,
            Content = item,
            IsVisible = item.IsExpanded,
            BackgroundColor = Colors.Transparent
        };
        container.Children.Add(contentWrapper);

        // Divider
        if (ShowDividers && index < _items.Count - 1)
        {
            container.Children.Add(new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = EffectiveBorderColor,
                Opacity = 0.3
            });
        }

        // Store references for animations
        item.SetValue(ContentWrapperProperty, contentWrapper);

        return container;
    }

    private static readonly BindableProperty ContentWrapperProperty = BindableProperty.CreateAttached(
        "ContentWrapper",
        typeof(Border),
        typeof(Accordion),
        null);

    private View CreateHeader(AccordionItem item, int index)
    {
        var grid = new Grid
        {
            Padding = HeaderPadding,
            BackgroundColor = EffectiveHeaderBackgroundColor,
            ColumnSpacing = 8
        };

        // Setup column definitions based on icon position
        if (ShowIcons)
        {
            grid.ColumnDefinitions = IconPosition == ExpandIconPosition.Left
                ? new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star)
                }
                : new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                };
        }
        else
        {
            grid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            };
        }

        int col = 0;

        // Item icon (if any)
        if (!string.IsNullOrEmpty(item.Icon))
        {
            var iconLabel = new Label
            {
                Text = item.Icon,
                FontSize = HeaderFontSize,
                FontAttributes = HeaderFontAttributes,
                FontFamily = EffectiveHeaderFontFamily,
                VerticalOptions = LayoutOptions.Center,
                TextColor = EffectiveHeaderTextColor
            };
            grid.Add(iconLabel, col++, 0);
        }

        // Expand icon (left position)
        Label? expandIcon = null;
        if (ShowIcons && IconPosition == ExpandIconPosition.Left)
        {
            expandIcon = new Label
            {
                Text = item.ExpanderIcon,
                FontSize = 10,
                VerticalOptions = LayoutOptions.Center,
                TextColor = EffectiveHeaderTextColor
            };
            grid.Add(expandIcon, col++, 0);
        }

        // Header text or template
        View headerContent;
        if (item.HeaderTemplate != null)
        {
            headerContent = (View)item.HeaderTemplate.CreateContent();
            headerContent.BindingContext = item;
        }
        else
        {
            headerContent = new Label
            {
                Text = item.Header,
                FontSize = HeaderFontSize,
                FontAttributes = HeaderFontAttributes,
                FontFamily = EffectiveHeaderFontFamily,
                VerticalOptions = LayoutOptions.Center,
                TextColor = item.IsEnabled ? EffectiveHeaderTextColor : EffectiveDisabledColor
            };
        }
        grid.Add(headerContent, col++, 0);

        // Expand icon (right position)
        if (ShowIcons && IconPosition == ExpandIconPosition.Right)
        {
            expandIcon = new Label
            {
                Text = item.ExpanderIcon,
                FontSize = 10,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                TextColor = EffectiveHeaderTextColor
            };
            grid.Add(expandIcon, col, 0);
        }

        // Store expand icon reference
        if (expandIcon != null)
        {
            item.SetValue(ExpandIconProperty, expandIcon);
        }

        // Selection highlight
        if (index == _selectedIndex)
        {
            grid.BackgroundColor = MauiControlsExtrasTheme.Current.SelectedBackgroundColor;
        }

        // Click handler
        var tapGesture = new TapGestureRecognizer();
        var capturedIndex = index;
        tapGesture.Tapped += (s, e) =>
        {
            SelectedIndex = capturedIndex;
            ToggleItem(item);
        };
        grid.GestureRecognizers.Add(tapGesture);

        return grid;
    }

    private static readonly BindableProperty ExpandIconProperty = BindableProperty.CreateAttached(
        "ExpandIcon",
        typeof(Label),
        typeof(Accordion),
        null);

    private void UpdateItemHighlighting()
    {
        RebuildUI();
    }

    #endregion

    #region Expansion Methods

    private async void ExpandItemInternal(AccordionItem item)
    {
        item.IsExpanded = true;

        var contentWrapper = item.GetValue(ContentWrapperProperty) as Border;
        var expandIcon = item.GetValue(ExpandIconProperty) as Label;

        if (expandIcon != null)
        {
            expandIcon.Text = item.ExpanderIcon;
        }

        if (contentWrapper != null)
        {
            if (AnimateExpansion)
            {
                contentWrapper.IsVisible = true;
                contentWrapper.Opacity = 0;
                await contentWrapper.FadeToAsync(1, AnimationDuration);
            }
            else
            {
                contentWrapper.IsVisible = true;
            }
        }

        var args = new AccordionItemExpandedEventArgs(item, item.Index, true);
        ItemExpanded?.Invoke(this, args);
        ItemExpandedCommand?.Execute(ItemExpandedCommandParameter ?? args);

        // Update ISelectable state
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(IsAllSelected));
    }

    private async void CollapseItemInternal(AccordionItem item)
    {
        item.IsExpanded = false;

        var contentWrapper = item.GetValue(ContentWrapperProperty) as Border;
        var expandIcon = item.GetValue(ExpandIconProperty) as Label;

        if (expandIcon != null)
        {
            expandIcon.Text = item.ExpanderIcon;
        }

        if (contentWrapper != null)
        {
            if (AnimateExpansion)
            {
                await contentWrapper.FadeToAsync(0, AnimationDuration);
                contentWrapper.IsVisible = false;
            }
            else
            {
                contentWrapper.IsVisible = false;
            }
        }

        var args = new AccordionItemExpandedEventArgs(item, item.Index, false);
        ItemCollapsed?.Invoke(this, args);
        ItemCollapsedCommand?.Execute(ItemCollapsedCommandParameter ?? args);

        // Update ISelectable state
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(IsAllSelected));
    }

    #endregion

    #region Event Handlers

    private void OnControlFocused(object? sender, FocusEventArgs e)
    {
        _hasKeyboardFocus = true;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        OnPropertyChanged(nameof(CurrentBorderColor));
        KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
        GotFocusCommand?.Execute(GotFocusCommandParameter ?? this);

        if (_selectedIndex < 0 && _items.Count > 0)
        {
            SelectedIndex = 0;
        }
    }

    private void OnControlUnfocused(object? sender, FocusEventArgs e)
    {
        _hasKeyboardFocus = false;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        OnPropertyChanged(nameof(CurrentBorderColor));
        KeyboardFocusLost?.Invoke(this, new KeyboardFocusEventArgs(false));
        LostFocusCommand?.Execute(LostFocusCommandParameter ?? this);
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        System.Diagnostics.Trace.WriteLine($"[Accordion] OnItemsCollectionChanged: Action={e.Action}, Items.Count={_items.Count}");
        RebuildUI();

        // Ensure AtLeastOne mode has one expanded
        if (ExpandMode == AccordionExpandMode.AtLeastOne && _items.Count > 0)
        {
            if (!_items.Any(i => i.IsExpanded))
            {
                ExpandItemInternal(_items[0]);
            }
        }
    }

    private void OnItemExpandedChanged(AccordionItem item, bool isExpanded)
    {
        if (isExpanded)
        {
            if (ExpandMode == AccordionExpandMode.Single)
            {
                foreach (var other in _items.Where(i => i != item && i.IsExpanded))
                {
                    CollapseItemInternal(other);
                }
            }
        }
    }

    #endregion

    #region Property Changed Handlers

    private static void OnIconPositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Accordion accordion)
        {
            accordion.RebuildUI();
        }
    }

    private static void OnShowIconsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Accordion accordion)
        {
            accordion.RebuildUI();
        }
    }

    private static void OnShowDividersChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Accordion accordion)
        {
            accordion.RebuildUI();
        }
    }

    #endregion

    #region HeaderedControlBase Overrides

    /// <inheritdoc/>
    protected override void OnHeaderBackgroundColorChanged(Color? oldValue, Color? newValue)
        => RebuildUI();

    /// <inheritdoc/>
    protected override void OnHeaderTextColorChanged(Color? oldValue, Color? newValue)
        => RebuildUI();

    /// <inheritdoc/>
    protected override void OnHeaderFontSizeChanged(double oldValue, double newValue)
        => RebuildUI();

    /// <inheritdoc/>
    protected override void OnHeaderFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue)
        => RebuildUI();

    /// <inheritdoc/>
    protected override void OnHeaderFontFamilyChanged(string? oldValue, string? newValue)
        => RebuildUI();

    /// <inheritdoc/>
    protected override void OnHeaderPaddingChanged(Thickness oldValue, Thickness newValue)
        => RebuildUI();

    #endregion
}
