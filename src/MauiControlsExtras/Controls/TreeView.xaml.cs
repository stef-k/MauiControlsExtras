using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiControlsExtras.Base;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Selection mode for the TreeView.
/// </summary>
public enum TreeViewSelectionMode
{
    /// <summary>
    /// No selection allowed.
    /// </summary>
    None,

    /// <summary>
    /// Single item selection.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple item selection.
    /// </summary>
    Multiple
}

/// <summary>
/// Checkbox behavior mode for the TreeView.
/// </summary>
public enum CheckBoxMode
{
    /// <summary>
    /// Checkboxes are independent - parent doesn't affect children.
    /// </summary>
    Independent,

    /// <summary>
    /// Cascading - checking parent checks all children.
    /// </summary>
    Cascade,

    /// <summary>
    /// Tri-state - parent shows partial state when some children are checked.
    /// </summary>
    TriState
}

/// <summary>
/// Check state for tree view items.
/// </summary>
public enum CheckState
{
    /// <summary>
    /// Unchecked state.
    /// </summary>
    Unchecked,

    /// <summary>
    /// Checked state.
    /// </summary>
    Checked,

    /// <summary>
    /// Indeterminate state (some children checked).
    /// </summary>
    Indeterminate
}

/// <summary>
/// Event arguments for tree view item events.
/// </summary>
public class TreeViewItemEventArgs : EventArgs
{
    /// <summary>
    /// Gets the data item.
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the tree view node.
    /// </summary>
    public TreeViewNode Node { get; }

    /// <summary>
    /// Initializes a new instance of TreeViewItemEventArgs.
    /// </summary>
    public TreeViewItemEventArgs(object item, TreeViewNode node)
    {
        Item = item;
        Node = node;
    }
}

/// <summary>
/// Event arguments for cancelable tree view item events.
/// </summary>
public class TreeViewItemCancelEventArgs : TreeViewItemEventArgs
{
    /// <summary>
    /// Gets or sets whether to cancel the operation.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of TreeViewItemCancelEventArgs.
    /// </summary>
    public TreeViewItemCancelEventArgs(object item, TreeViewNode node)
        : base(item, node)
    {
    }
}

/// <summary>
/// Represents a node in the flattened tree view.
/// </summary>
public class TreeViewNode : INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isSelected;
    private CheckState _checkState = CheckState.Unchecked;
    private View? _content;
    private Color? _backgroundColor;

    /// <summary>
    /// Gets the underlying data item.
    /// </summary>
    public object DataItem { get; }

    /// <summary>
    /// Gets the parent node.
    /// </summary>
    public TreeViewNode? Parent { get; }

    /// <summary>
    /// Gets the depth level in the tree.
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Gets the child nodes.
    /// </summary>
    public ObservableCollection<TreeViewNode> Children { get; } = new();

    /// <summary>
    /// Gets or sets whether this node is expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExpanderIcon));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether this node is selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
                UpdateBackgroundColor();
            }
        }
    }

    /// <summary>
    /// Gets or sets the check state.
    /// </summary>
    public CheckState CheckState
    {
        get => _checkState;
        set
        {
            if (_checkState != value)
            {
                _checkState = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CheckBoxIcon));
            }
        }
    }

    /// <summary>
    /// Gets the icon for this item.
    /// </summary>
    public ImageSource? Icon { get; set; }

    /// <summary>
    /// Gets whether this item has an icon.
    /// </summary>
    public bool HasIcon => Icon != null;

    /// <summary>
    /// Gets whether this node has children.
    /// </summary>
    public bool HasChildren => Children.Count > 0 || HasPotentialChildren;

    /// <summary>
    /// Gets the inverse of HasChildren for XAML binding.
    /// </summary>
    public bool HasNoChildren => !HasChildren;

    /// <summary>
    /// Gets or sets whether this node potentially has children (for lazy loading).
    /// </summary>
    public bool HasPotentialChildren { get; set; }

    /// <summary>
    /// Gets or sets whether children have been loaded (for lazy loading).
    /// </summary>
    public bool ChildrenLoaded { get; set; } = true;

    /// <summary>
    /// Gets the expander icon based on expansion state.
    /// </summary>
    public string ExpanderIcon => IsExpanded ? "▼" : "▶";

    /// <summary>
    /// Gets the checkbox icon based on check state.
    /// </summary>
    public string CheckBoxIcon => CheckState switch
    {
        CheckState.Checked => "☑",
        CheckState.Indeterminate => "▣",
        _ => "☐"
    };

    /// <summary>
    /// Gets the indent margin based on level.
    /// </summary>
    public Thickness IndentMargin { get; set; }

    /// <summary>
    /// Gets or sets the content view for this node.
    /// </summary>
    public View? Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public Color? BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor != value)
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Reference to the parent TreeView control.
    /// </summary>
    internal TreeView? TreeView { get; set; }

    /// <summary>
    /// Initializes a new instance of TreeViewNode.
    /// </summary>
    public TreeViewNode(object dataItem, TreeViewNode? parent, int level)
    {
        DataItem = dataItem;
        Parent = parent;
        Level = level;
    }

    private void UpdateBackgroundColor()
    {
        BackgroundColor = IsSelected
            ? Color.FromArgb("#E3F2FD")
            : Colors.Transparent;
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Notifies that HasChildren property has changed.
    /// </summary>
    internal void NotifyHasChildrenChanged()
    {
        OnPropertyChanged(nameof(HasChildren));
        OnPropertyChanged(nameof(HasNoChildren));
    }
}

/// <summary>
/// A hierarchical list control for displaying and interacting with tree-structured data.
/// </summary>
public partial class TreeView : Base.ListStyledControlBase, Base.IKeyboardNavigable, Base.ISelectable
{
    #region Private Fields

    private readonly ObservableCollection<TreeViewNode> _flattenedItems = new();
    private readonly Dictionary<object, TreeViewNode> _nodeMap = new();
    private bool _isUpdating;
    private int _focusedIndex = -1;
    private bool _isKeyboardNavigationEnabled = true;
    private static readonly List<Base.KeyboardShortcut> _keyboardShortcuts = new();

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(TreeView),
        null,
        propertyChanged: OnItemsSourceChanged);

    /// <summary>
    /// Identifies the <see cref="ChildrenPath"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ChildrenPathProperty = BindableProperty.Create(
        nameof(ChildrenPath),
        typeof(string),
        typeof(TreeView),
        "Children");

    /// <summary>
    /// Identifies the <see cref="DisplayMemberPath"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath),
        typeof(string),
        typeof(TreeView),
        null);

    /// <summary>
    /// Identifies the <see cref="IconMemberPath"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IconMemberPathProperty = BindableProperty.Create(
        nameof(IconMemberPath),
        typeof(string),
        typeof(TreeView),
        null);

    /// <summary>
    /// Identifies the <see cref="SelectedItem"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(object),
        typeof(TreeView),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedItemChanged);

    /// <summary>
    /// Identifies the <see cref="SelectedItems"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(
        nameof(SelectedItems),
        typeof(IList),
        typeof(TreeView),
        null,
        BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="SelectionMode"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(
        nameof(SelectionMode),
        typeof(TreeViewSelectionMode),
        typeof(TreeView),
        TreeViewSelectionMode.Single,
        propertyChanged: OnSelectionModeChanged);

    /// <summary>
    /// Identifies the <see cref="IndentSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IndentSizeProperty = BindableProperty.Create(
        nameof(IndentSize),
        typeof(double),
        typeof(TreeView),
        20.0);

    /// <summary>
    /// Identifies the <see cref="ShowLines"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowLinesProperty = BindableProperty.Create(
        nameof(ShowLines),
        typeof(bool),
        typeof(TreeView),
        false);

    /// <summary>
    /// Identifies the <see cref="ShowCheckBoxes"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowCheckBoxesProperty = BindableProperty.Create(
        nameof(ShowCheckBoxes),
        typeof(bool),
        typeof(TreeView),
        false);

    /// <summary>
    /// Identifies the <see cref="CheckBoxMode"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CheckBoxModeProperty = BindableProperty.Create(
        nameof(CheckBoxMode),
        typeof(CheckBoxMode),
        typeof(TreeView),
        CheckBoxMode.Independent);

    /// <summary>
    /// Identifies the <see cref="ItemTemplate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate),
        typeof(DataTemplate),
        typeof(TreeView),
        null,
        propertyChanged: OnItemTemplateChanged);

    /// <summary>
    /// Identifies the <see cref="LoadChildrenCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LoadChildrenCommandProperty = BindableProperty.Create(
        nameof(LoadChildrenCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="IsExpandedPath"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsExpandedPathProperty = BindableProperty.Create(
        nameof(IsExpandedPath),
        typeof(string),
        typeof(TreeView),
        null);

    /// <summary>
    /// Identifies the <see cref="HasChildrenPath"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HasChildrenPathProperty = BindableProperty.Create(
        nameof(HasChildrenPath),
        typeof(string),
        typeof(TreeView),
        null);

    #endregion

    #region Command Bindable Properties

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(
        nameof(SelectionChangedCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandParameterProperty = BindableProperty.Create(
        nameof(SelectionChangedCommandParameter),
        typeof(object),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="ItemExpandedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemExpandedCommandProperty = BindableProperty.Create(
        nameof(ItemExpandedCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="ItemCollapsedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemCollapsedCommandProperty = BindableProperty.Create(
        nameof(ItemCollapsedCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="ItemTappedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemTappedCommandProperty = BindableProperty.Create(
        nameof(ItemTappedCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="ItemDoubleTappedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemDoubleTappedCommandProperty = BindableProperty.Create(
        nameof(ItemDoubleTappedCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="ItemCheckedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemCheckedCommandProperty = BindableProperty.Create(
        nameof(ItemCheckedCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="ItemCollapsingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemCollapsingCommandProperty = BindableProperty.Create(
        nameof(ItemCollapsingCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="ItemDeselectedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemDeselectedCommandProperty = BindableProperty.Create(
        nameof(ItemDeselectedCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="SelectAllCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectAllCommandProperty = BindableProperty.Create(
        nameof(SelectAllCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the <see cref="ClearSelectionCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearSelectionCommandProperty = BindableProperty.Create(
        nameof(ClearSelectionCommand),
        typeof(ICommand),
        typeof(TreeView));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the root items collection.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path to child items.
    /// </summary>
    public string ChildrenPath
    {
        get => (string)GetValue(ChildrenPathProperty);
        set => SetValue(ChildrenPathProperty, value);
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
    /// Gets or sets the property path for item icons.
    /// </summary>
    public string? IconMemberPath
    {
        get => (string?)GetValue(IconMemberPathProperty);
        set => SetValue(IconMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected item.
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected items collection for multi-select mode.
    /// </summary>
    public IList? SelectedItems
    {
        get => (IList?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the selection mode.
    /// </summary>
    public TreeViewSelectionMode SelectionMode
    {
        get => (TreeViewSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the indent size in pixels per level.
    /// </summary>
    public double IndentSize
    {
        get => (double)GetValue(IndentSizeProperty);
        set => SetValue(IndentSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show tree connector lines.
    /// </summary>
    public bool ShowLines
    {
        get => (bool)GetValue(ShowLinesProperty);
        set => SetValue(ShowLinesProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show selection checkboxes.
    /// </summary>
    public bool ShowCheckBoxes
    {
        get => (bool)GetValue(ShowCheckBoxesProperty);
        set => SetValue(ShowCheckBoxesProperty, value);
    }

    /// <summary>
    /// Gets or sets the checkbox behavior mode.
    /// </summary>
    public CheckBoxMode CheckBoxMode
    {
        get => (CheckBoxMode)GetValue(CheckBoxModeProperty);
        set => SetValue(CheckBoxModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the custom item template.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the command for lazy loading children.
    /// </summary>
    public ICommand? LoadChildrenCommand
    {
        get => (ICommand?)GetValue(LoadChildrenCommandProperty);
        set => SetValue(LoadChildrenCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path to bind expanded state.
    /// </summary>
    public string? IsExpandedPath
    {
        get => (string?)GetValue(IsExpandedPathProperty);
        set => SetValue(IsExpandedPathProperty, value);
    }

    /// <summary>
    /// Gets or sets the property path to determine if item has children (for lazy loading).
    /// </summary>
    public string? HasChildrenPath
    {
        get => (string?)GetValue(HasChildrenPathProperty);
        set => SetValue(HasChildrenPathProperty, value);
    }

    #endregion

    #region Command Properties

    /// <summary>
    /// Gets or sets the command to execute when selection changes.
    /// </summary>
    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for SelectionChangedCommand.
    /// </summary>
    public object? SelectionChangedCommandParameter
    {
        get => GetValue(SelectionChangedCommandParameterProperty);
        set => SetValue(SelectionChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when an item is expanded.
    /// </summary>
    public ICommand? ItemExpandedCommand
    {
        get => (ICommand?)GetValue(ItemExpandedCommandProperty);
        set => SetValue(ItemExpandedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when an item is collapsed.
    /// </summary>
    public ICommand? ItemCollapsedCommand
    {
        get => (ICommand?)GetValue(ItemCollapsedCommandProperty);
        set => SetValue(ItemCollapsedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when an item is tapped.
    /// </summary>
    public ICommand? ItemTappedCommand
    {
        get => (ICommand?)GetValue(ItemTappedCommandProperty);
        set => SetValue(ItemTappedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when an item is double-tapped.
    /// </summary>
    public ICommand? ItemDoubleTappedCommand
    {
        get => (ICommand?)GetValue(ItemDoubleTappedCommandProperty);
        set => SetValue(ItemDoubleTappedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when an item's check state changes.
    /// </summary>
    public ICommand? ItemCheckedCommand
    {
        get => (ICommand?)GetValue(ItemCheckedCommandProperty);
        set => SetValue(ItemCheckedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the cancelable command to execute before an item is collapsed.
    /// The command parameter is <see cref="TreeViewItemCancelEventArgs"/>.
    /// Set Cancel = true to prevent the collapse.
    /// </summary>
    public ICommand? ItemCollapsingCommand
    {
        get => (ICommand?)GetValue(ItemCollapsingCommandProperty);
        set => SetValue(ItemCollapsingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when an item is deselected.
    /// </summary>
    public ICommand? ItemDeselectedCommand
    {
        get => (ICommand?)GetValue(ItemDeselectedCommandProperty);
        set => SetValue(ItemDeselectedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute for select all operations.
    /// </summary>
    public ICommand? SelectAllCommand
    {
        get => (ICommand?)GetValue(SelectAllCommandProperty);
        set => SetValue(SelectAllCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute for clear selection operations.
    /// </summary>
    public ICommand? ClearSelectionCommand
    {
        get => (ICommand?)GetValue(ClearSelectionCommandProperty);
        set => SetValue(ClearSelectionCommandProperty, value);
    }

    #endregion

    #region Display Properties

    /// <summary>
    /// Gets the flattened items collection for display.
    /// </summary>
    public ObservableCollection<TreeViewNode> FlattenedItems => _flattenedItems;

    /// <summary>
    /// Gets the collection view selection mode based on TreeView selection mode.
    /// </summary>
    public SelectionMode CollectionSelectionMode => SelectionMode switch
    {
        TreeViewSelectionMode.None => Microsoft.Maui.Controls.SelectionMode.None,
        TreeViewSelectionMode.Single => Microsoft.Maui.Controls.SelectionMode.Single,
        TreeViewSelectionMode.Multiple => Microsoft.Maui.Controls.SelectionMode.Multiple,
        _ => Microsoft.Maui.Controls.SelectionMode.Single
    };

    /// <summary>
    /// Gets the expander color.
    /// </summary>
    public Color ExpanderColor => EffectiveForegroundColor;

    /// <summary>
    /// Gets the checkbox color.
    /// </summary>
    public Color CheckBoxColor => EffectiveAccentColor;

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler<Base.SelectionChangedEventArgs>? SelectionChanged;

    /// <summary>
    /// Occurs when an item is expanded.
    /// </summary>
    public event EventHandler<TreeViewItemEventArgs>? ItemExpanded;

    /// <summary>
    /// Occurs when an item is collapsed.
    /// </summary>
    public event EventHandler<TreeViewItemEventArgs>? ItemCollapsed;

    /// <summary>
    /// Occurs when an item is tapped.
    /// </summary>
    public event EventHandler<TreeViewItemEventArgs>? ItemTapped;

    /// <summary>
    /// Occurs when an item is double-tapped.
    /// </summary>
    public event EventHandler<TreeViewItemEventArgs>? ItemDoubleTapped;

    /// <summary>
    /// Occurs when an item's check state changes.
    /// </summary>
    public event EventHandler<TreeViewItemEventArgs>? ItemChecked;

    /// <summary>
    /// Occurs before an item is collapsed (cancelable).
    /// </summary>
    public event EventHandler<TreeViewItemCancelEventArgs>? ItemCollapsing;

    /// <summary>
    /// Occurs when an item is deselected.
    /// </summary>
    public event EventHandler<TreeViewItemEventArgs>? ItemDeselected;

    #endregion

    #region ISelectable Implementation

    /// <inheritdoc />
    public bool HasSelection => _nodeMap.Values.Any(n => n.IsSelected);

    /// <inheritdoc />
    public bool IsAllSelected
    {
        get
        {
            if (_flattenedItems.Count == 0)
                return false;

            return _flattenedItems.All(n => n.IsSelected);
        }
    }

    /// <inheritdoc />
    public bool SupportsMultipleSelection => SelectionMode == TreeViewSelectionMode.Multiple;

    /// <inheritdoc />
    public void SelectAll()
    {
        if (SelectionMode == TreeViewSelectionMode.None)
            return;

        if (SelectAllCommand?.CanExecute(null) == true)
        {
            SelectAllCommand.Execute(null);
            return;
        }

        var oldSelection = GetSelection();

        if (SelectionMode == TreeViewSelectionMode.Single)
        {
            // In single selection mode, select the first visible item
            if (_flattenedItems.Count > 0)
            {
                foreach (var n in _nodeMap.Values)
                {
                    if (n.IsSelected && n != _flattenedItems[0])
                    {
                        n.IsSelected = false;
                        RaiseItemDeselected(n);
                    }
                }
                _flattenedItems[0].IsSelected = true;
                SelectedItem = _flattenedItems[0].DataItem;
            }
        }
        else if (SelectionMode == TreeViewSelectionMode.Multiple)
        {
            // In multiple selection mode, select all visible items
            foreach (var node in _flattenedItems)
            {
                node.IsSelected = true;
            }

            if (SelectedItems != null)
            {
                SelectedItems.Clear();
                foreach (var node in _flattenedItems)
                {
                    SelectedItems.Add(node.DataItem);
                }
            }
        }

        RaiseSelectionChanged(oldSelection);
    }

    /// <inheritdoc />
    public void ClearSelection()
    {
        if (ClearSelectionCommand?.CanExecute(null) == true)
        {
            ClearSelectionCommand.Execute(null);
            return;
        }

        var oldSelection = GetSelection();

        foreach (var node in _nodeMap.Values)
        {
            if (node.IsSelected)
            {
                node.IsSelected = false;
                RaiseItemDeselected(node);
            }
        }

        SelectedItem = null;
        SelectedItems?.Clear();

        RaiseSelectionChanged(oldSelection);
    }

    /// <inheritdoc />
    public object? GetSelection()
    {
        if (SelectionMode == TreeViewSelectionMode.Single)
        {
            return SelectedItem;
        }
        else if (SelectionMode == TreeViewSelectionMode.Multiple)
        {
            var selectedItems = _nodeMap.Values
                .Where(n => n.IsSelected)
                .Select(n => n.DataItem)
                .ToList();
            return selectedItems.Count > 0 ? selectedItems : null;
        }

        return null;
    }

    /// <inheritdoc />
    public void SetSelection(object? selection)
    {
        if (selection == null)
        {
            ClearSelection();
            return;
        }

        var oldSelection = GetSelection();

        if (SelectionMode == TreeViewSelectionMode.Single)
        {
            // Clear current selection
            foreach (var n in _nodeMap.Values)
            {
                if (n.IsSelected)
                {
                    n.IsSelected = false;
                    RaiseItemDeselected(n);
                }
            }

            // Select the specified item
            if (_nodeMap.TryGetValue(selection, out var node))
            {
                node.IsSelected = true;
                SelectedItem = selection;
            }
        }
        else if (SelectionMode == TreeViewSelectionMode.Multiple)
        {
            // Clear current selection
            foreach (var n in _nodeMap.Values)
            {
                if (n.IsSelected)
                {
                    n.IsSelected = false;
                    RaiseItemDeselected(n);
                }
            }

            SelectedItems?.Clear();

            // Handle single item or collection
            if (selection is System.Collections.IEnumerable items && selection is not string)
            {
                foreach (var item in items)
                {
                    if (_nodeMap.TryGetValue(item, out var node))
                    {
                        node.IsSelected = true;
                        SelectedItems?.Add(item);
                    }
                }
            }
            else if (_nodeMap.TryGetValue(selection, out var node))
            {
                node.IsSelected = true;
                SelectedItems?.Add(selection);
            }
        }

        RaiseSelectionChanged(oldSelection);
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the TreeView control.
    /// </summary>
    public TreeView()
    {
        InitializeComponent();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Expands all nodes in the tree.
    /// </summary>
    public void ExpandAll()
    {
        _isUpdating = true;
        foreach (var node in _nodeMap.Values)
        {
            if (node.HasChildren)
            {
                node.IsExpanded = true;
            }
        }
        _isUpdating = false;
        RebuildFlattenedList();
    }

    /// <summary>
    /// Collapses all nodes in the tree.
    /// </summary>
    public void CollapseAll()
    {
        _isUpdating = true;
        foreach (var node in _nodeMap.Values)
        {
            node.IsExpanded = false;
        }
        _isUpdating = false;
        RebuildFlattenedList();
    }

    /// <summary>
    /// Expands the path to a specific item.
    /// </summary>
    /// <param name="item">The item to expand to.</param>
    public void ExpandTo(object item)
    {
        if (!_nodeMap.TryGetValue(item, out var node))
            return;

        _isUpdating = true;
        var current = node.Parent;
        while (current != null)
        {
            current.IsExpanded = true;
            current = current.Parent;
        }
        _isUpdating = false;
        RebuildFlattenedList();
    }

    /// <summary>
    /// Scrolls to make an item visible.
    /// </summary>
    /// <param name="item">The item to scroll to.</param>
    public void ScrollTo(object item)
    {
        ExpandTo(item);

        if (_nodeMap.TryGetValue(item, out var node))
        {
            var index = _flattenedItems.IndexOf(node);
            if (index >= 0)
            {
                flattenedList.ScrollTo(index, position: ScrollToPosition.MakeVisible, animate: true);
            }
        }
    }

    /// <summary>
    /// Gets all checked items.
    /// </summary>
    public IEnumerable<object> GetCheckedItems()
    {
        return _nodeMap.Values
            .Where(n => n.CheckState == CheckState.Checked)
            .Select(n => n.DataItem);
    }

    /// <summary>
    /// Sets the check state for an item.
    /// </summary>
    public void SetChecked(object item, bool isChecked)
    {
        if (_nodeMap.TryGetValue(item, out var node))
        {
            SetNodeCheckState(node, isChecked ? CheckState.Checked : CheckState.Unchecked);
        }
    }

    #endregion

    #region Private Methods - Tree Building

    private void BuildTree()
    {
        _nodeMap.Clear();
        _flattenedItems.Clear();

        if (ItemsSource == null)
            return;

        foreach (var item in ItemsSource)
        {
            var node = CreateNode(item, null, 0);
            AddNodeToFlatList(node);
        }
    }

    private TreeViewNode CreateNode(object item, TreeViewNode? parent, int level)
    {
        var node = new TreeViewNode(item, parent, level)
        {
            TreeView = this,
            IndentMargin = new Thickness(level * IndentSize, 0, 0, 0)
        };

        _nodeMap[item] = node;

        // Set up content
        node.Content = CreateItemContent(item);

        // Set up icon
        if (!string.IsNullOrEmpty(IconMemberPath))
        {
            var iconValue = GetPropertyValue(item, IconMemberPath);
            if (iconValue is ImageSource imageSource)
            {
                node.Icon = imageSource;
            }
            else if (iconValue is string iconString)
            {
                node.Icon = ImageSource.FromFile(iconString);
            }
        }

        // Check for initial expanded state
        if (!string.IsNullOrEmpty(IsExpandedPath))
        {
            var expandedValue = GetPropertyValue(item, IsExpandedPath);
            if (expandedValue is bool isExpanded)
            {
                node.IsExpanded = isExpanded;
            }
        }

        // Check for potential children (lazy loading)
        if (!string.IsNullOrEmpty(HasChildrenPath))
        {
            var hasChildrenValue = GetPropertyValue(item, HasChildrenPath);
            if (hasChildrenValue is bool hasChildren)
            {
                node.HasPotentialChildren = hasChildren;
                node.ChildrenLoaded = false;
            }
        }

        // Load children
        var children = GetChildren(item);
        if (children != null)
        {
            foreach (var child in children)
            {
                var childNode = CreateNode(child, node, level + 1);
                node.Children.Add(childNode);
            }
            node.ChildrenLoaded = true;
        }

        node.NotifyHasChildrenChanged();

        return node;
    }

    private View CreateItemContent(object item)
    {
        if (ItemTemplate != null)
        {
            var content = ItemTemplate.CreateContent() as View;
            if (content != null)
            {
                content.BindingContext = item;
                return content;
            }
        }

        // Default: show display member or ToString
        var displayText = GetDisplayText(item);
        return new Label
        {
            Text = displayText,
            VerticalOptions = LayoutOptions.Center,
            TextColor = EffectiveForegroundColor
        };
    }

    private string GetDisplayText(object item)
    {
        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            var value = GetPropertyValue(item, DisplayMemberPath);
            return value?.ToString() ?? string.Empty;
        }
        return item.ToString() ?? string.Empty;
    }

    private IEnumerable? GetChildren(object item)
    {
        if (string.IsNullOrEmpty(ChildrenPath))
            return null;

        var value = GetPropertyValue(item, ChildrenPath);
        return value as IEnumerable;
    }

    private static object? GetPropertyValue(object item, string propertyPath)
    {
        var type = item.GetType();
        var property = type.GetProperty(propertyPath);
        return property?.GetValue(item);
    }

    private void AddNodeToFlatList(TreeViewNode node)
    {
        _flattenedItems.Add(node);

        if (node.IsExpanded)
        {
            foreach (var child in node.Children)
            {
                AddNodeToFlatList(child);
            }
        }
    }

    private void RebuildFlattenedList()
    {
        if (_isUpdating)
            return;

        _flattenedItems.Clear();

        if (ItemsSource == null)
            return;

        foreach (var item in ItemsSource)
        {
            if (_nodeMap.TryGetValue(item, out var node))
            {
                AddNodeToFlatList(node);
            }
        }
    }

    #endregion

    #region Private Methods - Expand/Collapse

    private void ToggleExpand(TreeViewNode node)
    {
        if (!node.HasChildren)
            return;

        // Check if collapsing should be cancelled
        if (node.IsExpanded)
        {
            if (RaiseItemCollapsing(node))
            {
                return; // Collapse was cancelled
            }
        }

        // Handle lazy loading
        if (!node.ChildrenLoaded && LoadChildrenCommand != null)
        {
            if (LoadChildrenCommand.CanExecute(node.DataItem))
            {
                LoadChildrenCommand.Execute(node.DataItem);
                // After loading, rebuild children
                ReloadNodeChildren(node);
            }
        }

        node.IsExpanded = !node.IsExpanded;

        if (node.IsExpanded)
        {
            RaiseItemExpanded(node);
        }
        else
        {
            RaiseItemCollapsed(node);
        }

        RebuildFlattenedList();
    }

    private void ReloadNodeChildren(TreeViewNode node)
    {
        node.Children.Clear();
        var children = GetChildren(node.DataItem);
        if (children != null)
        {
            foreach (var child in children)
            {
                var childNode = CreateNode(child, node, node.Level + 1);
                node.Children.Add(childNode);
            }
        }
        node.ChildrenLoaded = true;
        node.NotifyHasChildrenChanged();
    }

    #endregion

    #region Private Methods - Selection

    private void SelectNode(TreeViewNode node)
    {
        if (SelectionMode == TreeViewSelectionMode.None)
            return;

        var oldSelection = GetSelection();

        if (SelectionMode == TreeViewSelectionMode.Single)
        {
            // Deselect all others and raise deselected events
            foreach (var n in _nodeMap.Values)
            {
                if (n.IsSelected && n != node)
                {
                    n.IsSelected = false;
                    RaiseItemDeselected(n);
                }
            }
            node.IsSelected = true;
            SelectedItem = node.DataItem;
        }
        else if (SelectionMode == TreeViewSelectionMode.Multiple)
        {
            var wasSelected = node.IsSelected;
            node.IsSelected = !node.IsSelected;

            if (SelectedItems != null)
            {
                if (node.IsSelected)
                {
                    if (!SelectedItems.Contains(node.DataItem))
                    {
                        SelectedItems.Add(node.DataItem);
                    }
                }
                else
                {
                    SelectedItems.Remove(node.DataItem);
                }
            }

            // Raise deselected event if item was deselected
            if (wasSelected && !node.IsSelected)
            {
                RaiseItemDeselected(node);
            }
        }

        RaiseSelectionChanged(oldSelection);
    }

    #endregion

    #region Private Methods - Checkboxes

    private void ToggleCheckBox(TreeViewNode node)
    {
        var newState = node.CheckState == CheckState.Checked
            ? CheckState.Unchecked
            : CheckState.Checked;

        SetNodeCheckState(node, newState);
    }

    private void SetNodeCheckState(TreeViewNode node, CheckState state)
    {
        node.CheckState = state;

        if (CheckBoxMode == CheckBoxMode.Cascade || CheckBoxMode == CheckBoxMode.TriState)
        {
            // Propagate to children
            SetChildrenCheckState(node, state);
        }

        if (CheckBoxMode == CheckBoxMode.TriState)
        {
            // Update parents
            UpdateParentCheckState(node.Parent);
        }

        RaiseItemChecked(node);
    }

    private void SetChildrenCheckState(TreeViewNode node, CheckState state)
    {
        foreach (var child in node.Children)
        {
            child.CheckState = state;
            SetChildrenCheckState(child, state);
        }
    }

    private void UpdateParentCheckState(TreeViewNode? parent)
    {
        while (parent != null)
        {
            var allChecked = parent.Children.All(c => c.CheckState == CheckState.Checked);
            var allUnchecked = parent.Children.All(c => c.CheckState == CheckState.Unchecked);

            if (allChecked)
            {
                parent.CheckState = CheckState.Checked;
            }
            else if (allUnchecked)
            {
                parent.CheckState = CheckState.Unchecked;
            }
            else
            {
                parent.CheckState = CheckState.Indeterminate;
            }

            parent = parent.Parent;
        }
    }

    #endregion

    #region Event Handlers

    private void OnExpanderTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is TreeViewNode node)
        {
            ToggleExpand(node);
        }
    }

    private void OnCheckBoxTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is TreeViewNode node)
        {
            ToggleCheckBox(node);
        }
    }

    private void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is TreeViewNode node)
        {
            SelectNode(node);
            RaiseItemTapped(node);
        }
    }

    private void OnItemDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is TreeViewNode node)
        {
            ToggleExpand(node);
            RaiseItemDoubleTapped(node);
        }
    }

    private void OnCollectionSelectionChanged(object? sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
    {
        // Sync with our internal selection state
    }

    #endregion

    #region Property Changed Handlers

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TreeView treeView)
        {
            // Unsubscribe from old collection
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= treeView.OnItemsSourceCollectionChanged;
            }

            // Subscribe to new collection
            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += treeView.OnItemsSourceCollectionChanged;
            }

            treeView.BuildTree();
        }
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // For simplicity, rebuild the entire tree on any change
        // A more sophisticated implementation would handle incremental updates
        BuildTree();
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TreeView treeView && newValue != null)
        {
            if (treeView._nodeMap.TryGetValue(newValue, out var node))
            {
                foreach (var n in treeView._nodeMap.Values)
                {
                    n.IsSelected = false;
                }
                node.IsSelected = true;
            }
        }
    }

    private static void OnSelectionModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TreeView treeView)
        {
            treeView.OnPropertyChanged(nameof(CollectionSelectionMode));
        }
    }

    private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TreeView treeView)
        {
            // Rebuild to apply new template
            treeView.BuildTree();
        }
    }

    #endregion

    #region Event Raising Methods

    private void RaiseSelectionChanged(object? oldSelection = null)
    {
        var newSelection = GetSelection();
        var args = new Base.SelectionChangedEventArgs(oldSelection, newSelection);
        SelectionChanged?.Invoke(this, args);

        var parameter = SelectionChangedCommandParameter ?? newSelection;
        if (SelectionChangedCommand?.CanExecute(parameter) == true)
        {
            SelectionChangedCommand.Execute(parameter);
        }
    }

    private void RaiseItemExpanded(TreeViewNode node)
    {
        var args = new TreeViewItemEventArgs(node.DataItem, node);
        ItemExpanded?.Invoke(this, args);

        if (ItemExpandedCommand?.CanExecute(node.DataItem) == true)
        {
            ItemExpandedCommand.Execute(node.DataItem);
        }
    }

    private void RaiseItemCollapsed(TreeViewNode node)
    {
        var args = new TreeViewItemEventArgs(node.DataItem, node);
        ItemCollapsed?.Invoke(this, args);

        if (ItemCollapsedCommand?.CanExecute(node.DataItem) == true)
        {
            ItemCollapsedCommand.Execute(node.DataItem);
        }
    }

    private void RaiseItemTapped(TreeViewNode node)
    {
        var args = new TreeViewItemEventArgs(node.DataItem, node);
        ItemTapped?.Invoke(this, args);

        if (ItemTappedCommand?.CanExecute(node.DataItem) == true)
        {
            ItemTappedCommand.Execute(node.DataItem);
        }
    }

    private void RaiseItemDoubleTapped(TreeViewNode node)
    {
        var args = new TreeViewItemEventArgs(node.DataItem, node);
        ItemDoubleTapped?.Invoke(this, args);

        if (ItemDoubleTappedCommand?.CanExecute(node.DataItem) == true)
        {
            ItemDoubleTappedCommand.Execute(node.DataItem);
        }
    }

    private void RaiseItemChecked(TreeViewNode node)
    {
        var args = new TreeViewItemEventArgs(node.DataItem, node);
        ItemChecked?.Invoke(this, args);

        if (ItemCheckedCommand?.CanExecute(node.DataItem) == true)
        {
            ItemCheckedCommand.Execute(node.DataItem);
        }
    }

    private bool RaiseItemCollapsing(TreeViewNode node)
    {
        var args = new TreeViewItemCancelEventArgs(node.DataItem, node);
        ItemCollapsing?.Invoke(this, args);

        if (ItemCollapsingCommand?.CanExecute(args) == true)
        {
            ItemCollapsingCommand.Execute(args);
        }

        return args.Cancel;
    }

    private void RaiseItemDeselected(TreeViewNode node)
    {
        var args = new TreeViewItemEventArgs(node.DataItem, node);
        ItemDeselected?.Invoke(this, args);

        if (ItemDeselectedCommand?.CanExecute(node.DataItem) == true)
        {
            ItemDeselectedCommand.Execute(node.DataItem);
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
        typeof(TreeView));

    /// <summary>
    /// Identifies the LostFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(TreeView));

    /// <summary>
    /// Identifies the KeyPressCommand bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(TreeView));

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

        // Handle navigation and action keys
        return e.Key switch
        {
            "Up" => HandleUpKey(e),
            "Down" => HandleDownKey(e),
            "Left" => HandleLeftKey(e),
            "Right" => HandleRightKey(e),
            "Home" => HandleHomeKey(e),
            "End" => HandleEndKey(e),
            "PageUp" => HandlePageUpKey(),
            "PageDown" => HandlePageDownKey(),
            "Enter" => HandleEnterKey(),
            "Space" => HandleSpaceKey(),
            "Add" or "+" => HandleExpandKey(),
            "Subtract" or "-" => HandleCollapseKey(),
            "Multiply" or "*" => HandleExpandAllKey(),
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
                new Base.KeyboardShortcut { Key = "Up", Description = "Move to previous item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Down", Description = "Move to next item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Left", Description = "Collapse node or move to parent", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Right", Description = "Expand node or move to first child", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Home", Description = "Move to first item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "End", Description = "Move to last item", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "PageUp", Description = "Move up one page", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "PageDown", Description = "Move down one page", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Enter", Description = "Expand/collapse node", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Space", Description = "Toggle selection or checkbox", Category = "Action" },
                new Base.KeyboardShortcut { Key = "+", Description = "Expand current node", Category = "Action" },
                new Base.KeyboardShortcut { Key = "-", Description = "Collapse current node", Category = "Action" },
                new Base.KeyboardShortcut { Key = "*", Description = "Expand current node and all children", Category = "Action" },
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

            // If no focused item, select first
            if (_focusedIndex < 0 && _flattenedItems.Count > 0)
            {
                _focusedIndex = 0;
                UpdateFocusVisual();
            }
        }
        return result;
    }

    /// <summary>
    /// Gets the currently focused node.
    /// </summary>
    public TreeViewNode? FocusedNode => _focusedIndex >= 0 && _focusedIndex < _flattenedItems.Count
        ? _flattenedItems[_focusedIndex]
        : null;

    #region Keyboard Navigation Handlers

    private bool HandleUpKey(Base.KeyEventArgs e)
    {
        if (_flattenedItems.Count == 0) return false;

        var newIndex = _focusedIndex <= 0 ? _flattenedItems.Count - 1 : _focusedIndex - 1;

        if (e.IsShiftPressed && SelectionMode == TreeViewSelectionMode.Multiple)
        {
            // Extend selection
            var oldSelection = GetSelection();
            _focusedIndex = newIndex;
            var node = _flattenedItems[newIndex];
            node.IsSelected = true;
            UpdateSelectedItems(oldSelection);
        }
        else
        {
            SetFocusedIndex(newIndex, !e.IsPlatformCommandPressed);
        }

        ScrollToFocused();
        return true;
    }

    private bool HandleDownKey(Base.KeyEventArgs e)
    {
        if (_flattenedItems.Count == 0) return false;

        var newIndex = _focusedIndex >= _flattenedItems.Count - 1 ? 0 : _focusedIndex + 1;

        if (e.IsShiftPressed && SelectionMode == TreeViewSelectionMode.Multiple)
        {
            // Extend selection
            var oldSelection = GetSelection();
            _focusedIndex = newIndex;
            var node = _flattenedItems[newIndex];
            node.IsSelected = true;
            UpdateSelectedItems(oldSelection);
        }
        else
        {
            SetFocusedIndex(newIndex, !e.IsPlatformCommandPressed);
        }

        ScrollToFocused();
        return true;
    }

    private bool HandleLeftKey(Base.KeyEventArgs e)
    {
        if (_focusedIndex < 0 || _focusedIndex >= _flattenedItems.Count) return false;

        var node = _flattenedItems[_focusedIndex];

        if (node.IsExpanded && node.HasChildren)
        {
            // Collapse the node
            ToggleExpand(node);
            return true;
        }
        else if (node.Parent != null)
        {
            // Move to parent
            var parentIndex = _flattenedItems.IndexOf(node.Parent);
            if (parentIndex >= 0)
            {
                SetFocusedIndex(parentIndex, !e.IsPlatformCommandPressed);
                ScrollToFocused();
            }
            return true;
        }

        return false;
    }

    private bool HandleRightKey(Base.KeyEventArgs e)
    {
        if (_focusedIndex < 0 || _focusedIndex >= _flattenedItems.Count) return false;

        var node = _flattenedItems[_focusedIndex];

        if (!node.IsExpanded && node.HasChildren)
        {
            // Expand the node
            ToggleExpand(node);
            return true;
        }
        else if (node.IsExpanded && node.Children.Count > 0)
        {
            // Move to first child
            var childIndex = _flattenedItems.IndexOf(node.Children[0]);
            if (childIndex >= 0)
            {
                SetFocusedIndex(childIndex, !e.IsPlatformCommandPressed);
                ScrollToFocused();
            }
            return true;
        }

        return false;
    }

    private bool HandleHomeKey(Base.KeyEventArgs e)
    {
        if (_flattenedItems.Count == 0) return false;

        SetFocusedIndex(0, !e.IsPlatformCommandPressed);
        ScrollToFocused();
        return true;
    }

    private bool HandleEndKey(Base.KeyEventArgs e)
    {
        if (_flattenedItems.Count == 0) return false;

        SetFocusedIndex(_flattenedItems.Count - 1, !e.IsPlatformCommandPressed);
        ScrollToFocused();
        return true;
    }

    private bool HandlePageUpKey()
    {
        if (_flattenedItems.Count == 0) return false;

        // Move approximately one page up (10 items)
        var newIndex = Math.Max(0, _focusedIndex - 10);
        SetFocusedIndex(newIndex, true);
        ScrollToFocused();
        return true;
    }

    private bool HandlePageDownKey()
    {
        if (_flattenedItems.Count == 0) return false;

        // Move approximately one page down (10 items)
        var newIndex = Math.Min(_flattenedItems.Count - 1, _focusedIndex + 10);
        SetFocusedIndex(newIndex, true);
        ScrollToFocused();
        return true;
    }

    private bool HandleEnterKey()
    {
        if (_focusedIndex < 0 || _focusedIndex >= _flattenedItems.Count) return false;

        var node = _flattenedItems[_focusedIndex];
        if (node.HasChildren)
        {
            ToggleExpand(node);
        }
        return true;
    }

    private bool HandleSpaceKey()
    {
        if (_focusedIndex < 0 || _focusedIndex >= _flattenedItems.Count) return false;

        var node = _flattenedItems[_focusedIndex];

        if (ShowCheckBoxes)
        {
            ToggleCheckBox(node);
        }
        else if (SelectionMode != TreeViewSelectionMode.None)
        {
            SelectNode(node);
        }

        return true;
    }

    private bool HandleExpandKey()
    {
        if (_focusedIndex < 0 || _focusedIndex >= _flattenedItems.Count) return false;

        var node = _flattenedItems[_focusedIndex];
        if (node.HasChildren && !node.IsExpanded)
        {
            ToggleExpand(node);
            return true;
        }
        return false;
    }

    private bool HandleCollapseKey()
    {
        if (_focusedIndex < 0 || _focusedIndex >= _flattenedItems.Count) return false;

        var node = _flattenedItems[_focusedIndex];
        if (node.IsExpanded)
        {
            ToggleExpand(node);
            return true;
        }
        return false;
    }

    private bool HandleExpandAllKey()
    {
        if (_focusedIndex < 0 || _focusedIndex >= _flattenedItems.Count) return false;

        var node = _flattenedItems[_focusedIndex];
        ExpandNodeRecursively(node);
        RebuildFlattenedList();
        return true;
    }

    private void ExpandNodeRecursively(TreeViewNode node)
    {
        if (node.HasChildren)
        {
            node.IsExpanded = true;
            foreach (var child in node.Children)
            {
                ExpandNodeRecursively(child);
            }
        }
    }

    private void SetFocusedIndex(int index, bool select)
    {
        if (index < 0 || index >= _flattenedItems.Count) return;

        _focusedIndex = index;
        UpdateFocusVisual();

        if (select)
        {
            var node = _flattenedItems[index];
            SelectNode(node);
        }
    }

    private void UpdateFocusVisual()
    {
        // Update visual focus indicator (could be done via binding or direct manipulation)
        for (int i = 0; i < _flattenedItems.Count; i++)
        {
            var node = _flattenedItems[i];
            // The selection visual already serves as focus indicator
            // If we want a separate focus ring, we'd need to add a FocusedBackgroundColor property
        }
    }

    private void ScrollToFocused()
    {
        if (_focusedIndex >= 0 && _focusedIndex < _flattenedItems.Count)
        {
            flattenedList.ScrollTo(_focusedIndex, position: ScrollToPosition.MakeVisible, animate: true);
        }
    }

    private void UpdateSelectedItems(object? oldSelection = null)
    {
        if (SelectedItems != null)
        {
            SelectedItems.Clear();
            foreach (var node in _flattenedItems.Where(n => n.IsSelected))
            {
                SelectedItems.Add(node.DataItem);
            }
        }
        RaiseSelectionChanged(oldSelection);
    }

    #endregion

    #endregion
}
