using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Selection mode for the data grid.
/// </summary>
public enum DataGridSelectionMode
{
    /// <summary>
    /// No selection allowed.
    /// </summary>
    None,

    /// <summary>
    /// Single row selection.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple row selection.
    /// </summary>
    Multiple
}

/// <summary>
/// Event arguments for cell events.
/// </summary>
public class DataGridCellEventArgs : EventArgs
{
    /// <summary>
    /// Gets the row item.
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the column.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Gets the row index.
    /// </summary>
    public int RowIndex { get; }

    /// <summary>
    /// Gets the column index.
    /// </summary>
    public int ColumnIndex { get; }

    /// <summary>
    /// Initializes a new instance of DataGridCellEventArgs.
    /// </summary>
    public DataGridCellEventArgs(object item, DataGridColumn column, int rowIndex, int columnIndex)
    {
        Item = item;
        Column = column;
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
    }
}

/// <summary>
/// Event arguments for sorting events.
/// </summary>
public class DataGridSortingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the column being sorted.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Gets or sets whether the sorting should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridSortingEventArgs.
    /// </summary>
    public DataGridSortingEventArgs(DataGridColumn column)
    {
        Column = column;
    }
}

/// <summary>
/// A data grid control with column sorting, selection, editing, filtering, and data binding.
/// </summary>
[ContentProperty(nameof(Columns))]
public partial class DataGridView : Base.ListStyledControlBase, Base.IUndoRedo, Base.IClipboardSupport, Base.IKeyboardNavigable
{
    #region Private Fields

    private readonly ObservableCollection<DataGridColumn> _columns = new();
    private readonly List<object> _sortedItems = new();
    private readonly List<object> _filteredItems = new();
    private readonly HashSet<object> _selectedItems = new();
    private readonly Dictionary<DataGridColumn, DataGridColumnFilter> _activeFilters = new();
    private readonly List<DataGridSortDescription> _sortDescriptions = new();
    private readonly List<DataGridColumn> _editedColumnsInRow = new();
    private readonly Dictionary<(int, int), ValidationResult> _validationErrors = new();

    // Undo/Redo state
    private readonly Stack<IUndoableOperation> _undoStack = new();
    private readonly Stack<IUndoableOperation> _redoStack = new();
    private int _undoLimit = 100;
    private int _batchNestingLevel;
    private List<IUndoableOperation>? _currentBatch;
    private string? _currentBatchDescription;

    // Virtualization state
    private VirtualizingDataGridPanel? _virtualizingPanel;
    private VirtualizingDataGridPanel? _frozenVirtualizingPanel;

    private bool _isUpdating;
    private bool _isSyncingScroll;
    private DataGridColumn? _currentSortColumn;
    private object? _editingItem;
    private DataGridColumn? _editingColumn;
    private int _editingRowIndex = -1;
    private int _editingColumnIndex = -1;
    private View? _currentEditControl;
    private object? _originalEditValue;
    private View? _originalCellContent;
    private int _focusedRowIndex = -1;
    private int _focusedColumnIndex = -1;

    // Resize state
    private int _resizingColumnIndex = -1;
    private double _resizeStartWidth;

    // Column reorder state
    private int _draggingColumnIndex = -1;

    // Row reorder state
    private int _draggingRowIndex = -1;

    // Row details state
    private readonly HashSet<object> _expandedItems = new();

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(DataGridView),
        null,
        propertyChanged: OnItemsSourceChanged);

    /// <summary>
    /// Identifies the <see cref="AutoGenerateColumns"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AutoGenerateColumnsProperty = BindableProperty.Create(
        nameof(AutoGenerateColumns),
        typeof(bool),
        typeof(DataGridView),
        true);

    /// <summary>
    /// Identifies the <see cref="SelectedItem"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(object),
        typeof(DataGridView),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedItemChanged);

    /// <summary>
    /// Identifies the <see cref="SelectionMode"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(
        nameof(SelectionMode),
        typeof(DataGridSelectionMode),
        typeof(DataGridView),
        DataGridSelectionMode.Single);

    /// <summary>
    /// Identifies the <see cref="CanUserSort"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CanUserSortProperty = BindableProperty.Create(
        nameof(CanUserSort),
        typeof(bool),
        typeof(DataGridView),
        true,
        propertyChanged: OnCanUserSortChanged);

    /// <summary>
    /// Identifies the <see cref="CanUserFilter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CanUserFilterProperty = BindableProperty.Create(
        nameof(CanUserFilter),
        typeof(bool),
        typeof(DataGridView),
        true,
        propertyChanged: OnCanUserFilterChanged);

    /// <summary>
    /// Identifies the <see cref="CanUserEdit"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CanUserEditProperty = BindableProperty.Create(
        nameof(CanUserEdit),
        typeof(bool),
        typeof(DataGridView),
        false,
        propertyChanged: OnCanUserEditChanged);

    /// <summary>
    /// Identifies the <see cref="CanUserResize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CanUserResizeProperty = BindableProperty.Create(
        nameof(CanUserResize),
        typeof(bool),
        typeof(DataGridView),
        true);

    /// <summary>
    /// Identifies the <see cref="CanUserReorder"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CanUserReorderProperty = BindableProperty.Create(
        nameof(CanUserReorder),
        typeof(bool),
        typeof(DataGridView),
        false);

    /// <summary>
    /// Identifies the <see cref="RowHeight"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowHeightProperty = BindableProperty.Create(
        nameof(RowHeight),
        typeof(double),
        typeof(DataGridView),
        44.0);

    /// <summary>
    /// Identifies the <see cref="HeaderHeight"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderHeightProperty = BindableProperty.Create(
        nameof(HeaderHeight),
        typeof(double),
        typeof(DataGridView),
        48.0);

    /// <summary>
    /// Identifies the <see cref="GridLinesVisibility"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GridLinesVisibilityProperty = BindableProperty.Create(
        nameof(GridLinesVisibility),
        typeof(GridLinesVisibility),
        typeof(DataGridView),
        Controls.GridLinesVisibility.Both);

    /// <summary>
    /// Identifies the <see cref="GridLineColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GridLineColorProperty = BindableProperty.Create(
        nameof(GridLineColor),
        typeof(Color),
        typeof(DataGridView),
        Color.FromArgb("#E0E0E0"));

    /// <summary>
    /// Identifies the <see cref="SelectedRowColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedRowColorProperty = BindableProperty.Create(
        nameof(SelectedRowColor),
        typeof(Color),
        typeof(DataGridView),
        Color.FromArgb("#E3F2FD"));

    /// <summary>
    /// Identifies the <see cref="FocusedCellColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FocusedCellColorProperty = BindableProperty.Create(
        nameof(FocusedCellColor),
        typeof(Color),
        typeof(DataGridView),
        Color.FromArgb("#BBDEFB"));

    /// <summary>
    /// Identifies the <see cref="SelectedTextColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedTextColorProperty = BindableProperty.Create(
        nameof(SelectedTextColor),
        typeof(Color),
        typeof(DataGridView),
        Color.FromArgb("#1A1A1A"));

    /// <summary>
    /// Identifies the <see cref="IsRefreshing"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsRefreshingProperty = BindableProperty.Create(
        nameof(IsRefreshing),
        typeof(bool),
        typeof(DataGridView),
        false);

    /// <summary>
    /// Identifies the <see cref="EmptyView"/> bindable property.
    /// </summary>
    public static readonly BindableProperty EmptyViewProperty = BindableProperty.Create(
        nameof(EmptyView),
        typeof(View),
        typeof(DataGridView),
        null,
        propertyChanged: OnEmptyViewChanged);

    /// <summary>
    /// Identifies the <see cref="FrozenColumnCount"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FrozenColumnCountProperty = BindableProperty.Create(
        nameof(FrozenColumnCount),
        typeof(int),
        typeof(DataGridView),
        0,
        propertyChanged: OnFrozenColumnCountChanged);

    /// <summary>
    /// Identifies the <see cref="ShowFooter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowFooterProperty = BindableProperty.Create(
        nameof(ShowFooter),
        typeof(bool),
        typeof(DataGridView),
        false);

    /// <summary>
    /// Identifies the <see cref="FooterTemplate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FooterTemplateProperty = BindableProperty.Create(
        nameof(FooterTemplate),
        typeof(DataTemplate),
        typeof(DataGridView),
        null);

    /// <summary>
    /// Identifies the <see cref="EnablePagination"/> bindable property.
    /// </summary>
    public static readonly BindableProperty EnablePaginationProperty = BindableProperty.Create(
        nameof(EnablePagination),
        typeof(bool),
        typeof(DataGridView),
        false);

    /// <summary>
    /// Identifies the <see cref="PageSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PageSizeProperty = BindableProperty.Create(
        nameof(PageSize),
        typeof(int),
        typeof(DataGridView),
        50,
        propertyChanged: OnPaginationChanged);

    /// <summary>
    /// Identifies the <see cref="CurrentPage"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CurrentPageProperty = BindableProperty.Create(
        nameof(CurrentPage),
        typeof(int),
        typeof(DataGridView),
        1,
        BindingMode.TwoWay,
        propertyChanged: OnPaginationChanged);

    /// <summary>
    /// Identifies the <see cref="TotalItems"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TotalItemsProperty = BindableProperty.Create(
        nameof(TotalItems),
        typeof(int),
        typeof(DataGridView),
        0);

    /// <summary>
    /// Identifies the <see cref="AllowMultiColumnSort"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AllowMultiColumnSortProperty = BindableProperty.Create(
        nameof(AllowMultiColumnSort),
        typeof(bool),
        typeof(DataGridView),
        false);

    /// <summary>
    /// Identifies the <see cref="ValidateOnCellEdit"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValidateOnCellEditProperty = BindableProperty.Create(
        nameof(ValidateOnCellEdit),
        typeof(bool),
        typeof(DataGridView),
        true);

    /// <summary>
    /// Identifies the <see cref="ValidateOnRowEdit"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValidateOnRowEditProperty = BindableProperty.Create(
        nameof(ValidateOnRowEdit),
        typeof(bool),
        typeof(DataGridView),
        true);

    /// <summary>
    /// Identifies the <see cref="EditTrigger"/> bindable property.
    /// </summary>
    public static readonly BindableProperty EditTriggerProperty = BindableProperty.Create(
        nameof(EditTrigger),
        typeof(DataGridEditTrigger),
        typeof(DataGridView),
        DataGridEditTrigger.DoubleTap);

    /// <summary>
    /// Identifies the <see cref="SearchText"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SearchTextProperty = BindableProperty.Create(
        nameof(SearchText),
        typeof(string),
        typeof(DataGridView),
        null,
        propertyChanged: OnSearchTextChanged);

    /// <summary>
    /// Identifies the <see cref="HighlightSearchResults"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HighlightSearchResultsProperty = BindableProperty.Create(
        nameof(HighlightSearchResults),
        typeof(bool),
        typeof(DataGridView),
        true);

    /// <summary>
    /// Identifies the <see cref="SearchHighlightColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SearchHighlightColorProperty = BindableProperty.Create(
        nameof(SearchHighlightColor),
        typeof(Color),
        typeof(DataGridView),
        Color.FromArgb("#FFEB3B"));

    /// <summary>
    /// Identifies the <see cref="RowDetailsTemplate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowDetailsTemplateProperty = BindableProperty.Create(
        nameof(RowDetailsTemplate),
        typeof(DataTemplate),
        typeof(DataGridView),
        null);

    /// <summary>
    /// Identifies the <see cref="RowDetailsVisibility"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowDetailsVisibilityProperty = BindableProperty.Create(
        nameof(RowDetailsVisibility),
        typeof(DataGridRowDetailsVisibilityMode),
        typeof(DataGridView),
        DataGridRowDetailsVisibilityMode.Collapsed);

    /// <summary>
    /// Identifies the <see cref="CanUserReorderRows"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CanUserReorderRowsProperty = BindableProperty.Create(
        nameof(CanUserReorderRows),
        typeof(bool),
        typeof(DataGridView),
        false);

    /// <summary>
    /// Identifies the <see cref="EnableVirtualization"/> bindable property.
    /// </summary>
    public static readonly BindableProperty EnableVirtualizationProperty = BindableProperty.Create(
        nameof(EnableVirtualization),
        typeof(bool),
        typeof(DataGridView),
        false);

    /// <summary>
    /// Identifies the <see cref="VirtualizationBufferSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty VirtualizationBufferSizeProperty = BindableProperty.Create(
        nameof(VirtualizationBufferSize),
        typeof(int),
        typeof(DataGridView),
        5);

    #endregion

    #region Command Bindable Properties

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(
        nameof(SelectionChangedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellTappedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellTappedCommandProperty = BindableProperty.Create(
        nameof(CellTappedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellDoubleTappedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellDoubleTappedCommandProperty = BindableProperty.Create(
        nameof(CellDoubleTappedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="SortingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SortingCommandProperty = BindableProperty.Create(
        nameof(SortingCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="SortedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SortedCommandProperty = BindableProperty.Create(
        nameof(SortedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellEditStartedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellEditStartedCommandProperty = BindableProperty.Create(
        nameof(CellEditStartedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellEditEndedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellEditEndedCommandProperty = BindableProperty.Create(
        nameof(CellEditEndedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="RowEditEndedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowEditEndedCommandProperty = BindableProperty.Create(
        nameof(RowEditEndedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="FilteringCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FilteringCommandProperty = BindableProperty.Create(
        nameof(FilteringCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="FilteredCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FilteredCommandProperty = BindableProperty.Create(
        nameof(FilteredCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="PageChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PageChangedCommandProperty = BindableProperty.Create(
        nameof(PageChangedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ExportCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ExportCommandProperty = BindableProperty.Create(
        nameof(ExportCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="UndoCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty UndoCommandProperty = BindableProperty.Create(
        nameof(UndoCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="RedoCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RedoCommandProperty = BindableProperty.Create(
        nameof(RedoCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CopyCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CopyCommandProperty = BindableProperty.Create(
        nameof(CopyCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CutCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CutCommandProperty = BindableProperty.Create(
        nameof(CutCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="PasteCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PasteCommandProperty = BindableProperty.Create(
        nameof(PasteCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="UndoLimit"/> bindable property.
    /// </summary>
    public static readonly BindableProperty UndoLimitProperty = BindableProperty.Create(
        nameof(UndoLimit),
        typeof(int),
        typeof(DataGridView),
        100,
        propertyChanged: (b, o, n) => ((DataGridView)b)._undoLimit = (int)n);

    /// <summary>
    /// Identifies the <see cref="ShowDefaultContextMenu"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowDefaultContextMenuProperty = BindableProperty.Create(
        nameof(ShowDefaultContextMenu),
        typeof(bool),
        typeof(DataGridView),
        true);

    /// <summary>
    /// Identifies the <see cref="ContextMenuTemplate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContextMenuTemplateProperty = BindableProperty.Create(
        nameof(ContextMenuTemplate),
        typeof(DataTemplate),
        typeof(DataGridView),
        null);

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the data source.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets the columns collection.
    /// </summary>
    public ObservableCollection<DataGridColumn> Columns => _columns;

    /// <summary>
    /// Gets or sets whether to auto-generate columns from the data type.
    /// </summary>
    public bool AutoGenerateColumns
    {
        get => (bool)GetValue(AutoGenerateColumnsProperty);
        set => SetValue(AutoGenerateColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets the selected items collection.
    /// </summary>
    public IReadOnlyCollection<object> SelectedItems => _selectedItems;

    /// <summary>
    /// Gets or sets the selection mode.
    /// </summary>
    public DataGridSelectionMode SelectionMode
    {
        get => (DataGridSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can sort columns.
    /// </summary>
    public bool CanUserSort
    {
        get => (bool)GetValue(CanUserSortProperty);
        set => SetValue(CanUserSortProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can filter columns.
    /// </summary>
    public bool CanUserFilter
    {
        get => (bool)GetValue(CanUserFilterProperty);
        set => SetValue(CanUserFilterProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can edit cells.
    /// </summary>
    public bool CanUserEdit
    {
        get => (bool)GetValue(CanUserEditProperty);
        set => SetValue(CanUserEditProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can resize columns.
    /// </summary>
    public bool CanUserResize
    {
        get => (bool)GetValue(CanUserResizeProperty);
        set => SetValue(CanUserResizeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can reorder columns.
    /// </summary>
    public bool CanUserReorder
    {
        get => (bool)GetValue(CanUserReorderProperty);
        set => SetValue(CanUserReorderProperty, value);
    }

    /// <summary>
    /// Gets or sets the row height.
    /// </summary>
    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the header row height.
    /// </summary>
    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the grid lines visibility.
    /// </summary>
    public GridLinesVisibility GridLinesVisibility
    {
        get => (GridLinesVisibility)GetValue(GridLinesVisibilityProperty);
        set => SetValue(GridLinesVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the grid line color.
    /// </summary>
    public Color GridLineColor
    {
        get => (Color)GetValue(GridLineColorProperty);
        set => SetValue(GridLineColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected row color.
    /// </summary>
    public Color SelectedRowColor
    {
        get => (Color)GetValue(SelectedRowColorProperty);
        set => SetValue(SelectedRowColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the focused cell color.
    /// </summary>
    public Color FocusedCellColor
    {
        get => (Color)GetValue(FocusedCellColorProperty);
        set => SetValue(FocusedCellColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the text color for selected rows.
    /// </summary>
    public Color SelectedTextColor
    {
        get => (Color)GetValue(SelectedTextColorProperty);
        set => SetValue(SelectedTextColorProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the grid is refreshing/loading.
    /// </summary>
    public bool IsRefreshing
    {
        get => (bool)GetValue(IsRefreshingProperty);
        set => SetValue(IsRefreshingProperty, value);
    }

    /// <summary>
    /// Gets or sets the view shown when there's no data.
    /// </summary>
    public View? EmptyView
    {
        get => (View?)GetValue(EmptyViewProperty);
        set => SetValue(EmptyViewProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of frozen columns.
    /// </summary>
    public int FrozenColumnCount
    {
        get => (int)GetValue(FrozenColumnCountProperty);
        set => SetValue(FrozenColumnCountProperty, value);
    }

    /// <summary>
    /// Gets whether there are frozen columns.
    /// </summary>
    public bool HasFrozenColumns => FrozenColumnCount > 0;

    /// <summary>
    /// Gets or sets whether to show the footer row.
    /// </summary>
    public bool ShowFooter
    {
        get => (bool)GetValue(ShowFooterProperty);
        set => SetValue(ShowFooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the footer template.
    /// </summary>
    public DataTemplate? FooterTemplate
    {
        get => (DataTemplate?)GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets whether pagination is enabled.
    /// </summary>
    public bool EnablePagination
    {
        get => (bool)GetValue(EnablePaginationProperty);
        set => SetValue(EnablePaginationProperty, value);
    }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the current page (1-based).
    /// </summary>
    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    /// <summary>
    /// Gets or sets the total number of items (for server-side paging).
    /// </summary>
    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages
    {
        get
        {
            var total = TotalItems > 0 ? TotalItems : _filteredItems.Count;
            return Math.Max(1, (int)Math.Ceiling((double)total / PageSize));
        }
    }

    /// <summary>
    /// Gets or sets whether multi-column sorting is allowed.
    /// </summary>
    public bool AllowMultiColumnSort
    {
        get => (bool)GetValue(AllowMultiColumnSortProperty);
        set => SetValue(AllowMultiColumnSortProperty, value);
    }

    /// <summary>
    /// Gets the current sort descriptions.
    /// </summary>
    public IReadOnlyList<DataGridSortDescription> SortDescriptions => _sortDescriptions;

    /// <summary>
    /// Gets or sets whether to validate on cell edit.
    /// </summary>
    public bool ValidateOnCellEdit
    {
        get => (bool)GetValue(ValidateOnCellEditProperty);
        set => SetValue(ValidateOnCellEditProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to validate on row edit.
    /// </summary>
    public bool ValidateOnRowEdit
    {
        get => (bool)GetValue(ValidateOnRowEditProperty);
        set => SetValue(ValidateOnRowEditProperty, value);
    }

    /// <summary>
    /// Gets or sets the edit trigger.
    /// </summary>
    public DataGridEditTrigger EditTrigger
    {
        get => (DataGridEditTrigger)GetValue(EditTriggerProperty);
        set => SetValue(EditTriggerProperty, value);
    }

    /// <summary>
    /// Gets the currently editing item.
    /// </summary>
    public object? EditingItem => _editingItem;

    /// <summary>
    /// Gets the currently editing column.
    /// </summary>
    public DataGridColumn? EditingColumn => _editingColumn;

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    public string? SearchText
    {
        get => (string?)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to highlight search results.
    /// </summary>
    public bool HighlightSearchResults
    {
        get => (bool)GetValue(HighlightSearchResultsProperty);
        set => SetValue(HighlightSearchResultsProperty, value);
    }

    /// <summary>
    /// Gets or sets the search highlight color.
    /// </summary>
    public Color SearchHighlightColor
    {
        get => (Color)GetValue(SearchHighlightColorProperty);
        set => SetValue(SearchHighlightColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the row details template.
    /// </summary>
    public DataTemplate? RowDetailsTemplate
    {
        get => (DataTemplate?)GetValue(RowDetailsTemplateProperty);
        set => SetValue(RowDetailsTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the row details visibility mode.
    /// </summary>
    public DataGridRowDetailsVisibilityMode RowDetailsVisibility
    {
        get => (DataGridRowDetailsVisibilityMode)GetValue(RowDetailsVisibilityProperty);
        set => SetValue(RowDetailsVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can reorder rows.
    /// </summary>
    public bool CanUserReorderRows
    {
        get => (bool)GetValue(CanUserReorderRowsProperty);
        set => SetValue(CanUserReorderRowsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether virtualization is enabled.
    /// </summary>
    public bool EnableVirtualization
    {
        get => (bool)GetValue(EnableVirtualizationProperty);
        set => SetValue(EnableVirtualizationProperty, value);
    }

    /// <summary>
    /// Gets or sets the virtualization buffer size.
    /// </summary>
    public int VirtualizationBufferSize
    {
        get => (int)GetValue(VirtualizationBufferSizeProperty);
        set => SetValue(VirtualizationBufferSizeProperty, value);
    }

    /// <summary>
    /// Gets the active column filters.
    /// </summary>
    public IEnumerable<DataGridColumnFilter> ActiveFilters => _activeFilters.Values.Where(f => f.IsActive);

    /// <summary>
    /// Gets the page info text for display.
    /// </summary>
    public string PageInfoText
    {
        get
        {
            var total = TotalItems > 0 ? TotalItems : _filteredItems.Count;
            var start = ((CurrentPage - 1) * PageSize) + 1;
            var end = Math.Min(CurrentPage * PageSize, total);
            return $"Showing {start}-{end} of {total}";
        }
    }

    /// <summary>
    /// Gets the current page text for display.
    /// </summary>
    public string CurrentPageText => $"{CurrentPage} / {TotalPages}";

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
    /// Gets or sets the command to execute when a cell is tapped.
    /// </summary>
    public ICommand? CellTappedCommand
    {
        get => (ICommand?)GetValue(CellTappedCommandProperty);
        set => SetValue(CellTappedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when a cell is double-tapped.
    /// </summary>
    public ICommand? CellDoubleTappedCommand
    {
        get => (ICommand?)GetValue(CellDoubleTappedCommandProperty);
        set => SetValue(CellDoubleTappedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute before sorting.
    /// </summary>
    public ICommand? SortingCommand
    {
        get => (ICommand?)GetValue(SortingCommandProperty);
        set => SetValue(SortingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute after sorting.
    /// </summary>
    public ICommand? SortedCommand
    {
        get => (ICommand?)GetValue(SortedCommandProperty);
        set => SetValue(SortedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when cell edit starts.
    /// </summary>
    public ICommand? CellEditStartedCommand
    {
        get => (ICommand?)GetValue(CellEditStartedCommandProperty);
        set => SetValue(CellEditStartedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when cell edit ends.
    /// </summary>
    public ICommand? CellEditEndedCommand
    {
        get => (ICommand?)GetValue(CellEditEndedCommandProperty);
        set => SetValue(CellEditEndedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when row edit ends.
    /// </summary>
    public ICommand? RowEditEndedCommand
    {
        get => (ICommand?)GetValue(RowEditEndedCommandProperty);
        set => SetValue(RowEditEndedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute before filtering.
    /// </summary>
    public ICommand? FilteringCommand
    {
        get => (ICommand?)GetValue(FilteringCommandProperty);
        set => SetValue(FilteringCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute after filtering.
    /// </summary>
    public ICommand? FilteredCommand
    {
        get => (ICommand?)GetValue(FilteredCommandProperty);
        set => SetValue(FilteredCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when page changes.
    /// </summary>
    public ICommand? PageChangedCommand
    {
        get => (ICommand?)GetValue(PageChangedCommandProperty);
        set => SetValue(PageChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when exporting.
    /// </summary>
    public ICommand? ExportCommand
    {
        get => (ICommand?)GetValue(ExportCommandProperty);
        set => SetValue(ExportCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? UndoCommand
    {
        get => (ICommand?)GetValue(UndoCommandProperty);
        set => SetValue(UndoCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? RedoCommand
    {
        get => (ICommand?)GetValue(RedoCommandProperty);
        set => SetValue(RedoCommandProperty, value);
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

    /// <summary>
    /// Gets or sets whether to show the default context menu.
    /// </summary>
    public bool ShowDefaultContextMenu
    {
        get => (bool)GetValue(ShowDefaultContextMenuProperty);
        set => SetValue(ShowDefaultContextMenuProperty, value);
    }

    /// <summary>
    /// Gets or sets the context menu template.
    /// </summary>
    public DataTemplate? ContextMenuTemplate
    {
        get => (DataTemplate?)GetValue(ContextMenuTemplateProperty);
        set => SetValue(ContextMenuTemplateProperty, value);
    }

    #endregion

    #region IUndoRedo Properties

    /// <inheritdoc />
    public bool CanUndo => _undoStack.Count > 0;

    /// <inheritdoc />
    public bool CanRedo => _redoStack.Count > 0;

    /// <inheritdoc />
    public int UndoCount => _undoStack.Count;

    /// <inheritdoc />
    public int RedoCount => _redoStack.Count;

    /// <inheritdoc />
    public int UndoLimit
    {
        get => (int)GetValue(UndoLimitProperty);
        set => SetValue(UndoLimitProperty, value);
    }

    #endregion

    #region IClipboardSupport Properties

    /// <inheritdoc />
    public bool CanCopy => _selectedItems.Count > 0;

    /// <inheritdoc />
    public bool CanCut => CanCopy && CanUserEdit;

    /// <inheritdoc />
    public bool CanPaste => CanUserEdit;

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler<object?>? SelectionChanged;

    /// <summary>
    /// Occurs when a cell is tapped.
    /// </summary>
    public event EventHandler<DataGridCellEventArgs>? CellTapped;

    /// <summary>
    /// Occurs when a cell is double-tapped.
    /// </summary>
    public event EventHandler<DataGridCellEventArgs>? CellDoubleTapped;

    /// <summary>
    /// Occurs before sorting.
    /// </summary>
    public event EventHandler<DataGridSortingEventArgs>? Sorting;

    /// <summary>
    /// Occurs after sorting.
    /// </summary>
    public event EventHandler<DataGridColumn>? Sorted;

    /// <summary>
    /// Occurs before cell edit starts. Can be cancelled.
    /// </summary>
    public event EventHandler<DataGridCellEditEventArgs>? CellEditStarting;

    /// <summary>
    /// Occurs after cell edit starts.
    /// </summary>
    public event EventHandler<DataGridCellEditEventArgs>? CellEditStarted;

    /// <summary>
    /// Occurs before cell edit ends. Can be cancelled for validation.
    /// </summary>
    public event EventHandler<DataGridCellEditEventArgs>? CellEditEnding;

    /// <summary>
    /// Occurs after cell edit ends.
    /// </summary>
    public event EventHandler<DataGridCellEditEventArgs>? CellEditEnded;

    /// <summary>
    /// Occurs after row edit ends.
    /// </summary>
    public event EventHandler<DataGridRowEditEventArgs>? RowEditEnded;

    /// <summary>
    /// Occurs before filtering. Can be cancelled.
    /// </summary>
    public event EventHandler<DataGridFilterEventArgs>? Filtering;

    /// <summary>
    /// Occurs after filtering.
    /// </summary>
    public event EventHandler<DataGridFilterEventArgs>? Filtered;

    /// <summary>
    /// Occurs when column is being resized.
    /// </summary>
    public event EventHandler<DataGridColumnEventArgs>? ColumnResizing;

    /// <summary>
    /// Occurs after column is resized.
    /// </summary>
    public event EventHandler<DataGridColumnEventArgs>? ColumnResized;

    /// <summary>
    /// Occurs before column reorder. Can be cancelled.
    /// </summary>
    public event EventHandler<DataGridColumnReorderEventArgs>? ColumnReordering;

    /// <summary>
    /// Occurs after column is reordered.
    /// </summary>
    public event EventHandler<DataGridColumnReorderEventArgs>? ColumnReordered;

    /// <summary>
    /// Occurs before export. Can be cancelled.
    /// </summary>
    public event EventHandler<DataGridExportEventArgs>? Exporting;

    /// <summary>
    /// Occurs after export.
    /// </summary>
    public event EventHandler<DataGridExportEventArgs>? Exported;

    /// <summary>
    /// Occurs when page changes.
    /// </summary>
    public event EventHandler<DataGridPageChangedEventArgs>? PageChanged;

    /// <summary>
    /// Occurs before row reorder. Can be cancelled.
    /// </summary>
    public event EventHandler<DataGridRowReorderEventArgs>? RowReordering;

    /// <summary>
    /// Occurs after row is reordered.
    /// </summary>
    public event EventHandler<DataGridRowReorderEventArgs>? RowReordered;

    /// <summary>
    /// Occurs when cell validation happens.
    /// </summary>
    public event EventHandler<DataGridCellValidationEventArgs>? CellValidating;

    /// <summary>
    /// Occurs before copying. Can be cancelled.
    /// </summary>
    public event EventHandler<DataGridClipboardEventArgs>? Copying;

    /// <summary>
    /// Occurs before pasting. Can be cancelled.
    /// </summary>
    public event EventHandler<DataGridClipboardEventArgs>? Pasting;

    /// <summary>
    /// Occurs before cutting. Can be cancelled.
    /// </summary>
    public event EventHandler<DataGridClipboardEventArgs>? Cutting;

    /// <summary>
    /// Occurs before the context menu is opened. Can be cancelled.
    /// </summary>
    public event EventHandler<DataGridContextMenuEventArgs>? ContextMenuOpening;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the DataGridView control.
    /// </summary>
    public DataGridView()
    {
        InitializeComponent();
        _columns.CollectionChanged += OnColumnsCollectionChanged;

        // Set initial page size picker
        pageSizePicker.SelectedIndex = 2; // 50
    }

    #endregion

    #region Public Methods - Editing

    /// <summary>
    /// Begins editing a cell.
    /// </summary>
    public void BeginEdit(object item, DataGridColumn column)
    {
        if (!CanUserEdit || column.IsReadOnly || !column.CanUserEdit)
            return;

        var rowIndex = _sortedItems.IndexOf(item);
        if (rowIndex < 0)
            return;

        var visibleColumns = GetVisibleColumns();
        var colIndex = visibleColumns.IndexOf(column);
        if (colIndex < 0)
            return;

        BeginEditInternal(item, column, rowIndex, colIndex);
    }

    /// <summary>
    /// Commits the current edit.
    /// </summary>
    public void CommitEdit()
    {
        if (_editingItem == null || _editingColumn == null)
            return;

        CommitEditInternal();
    }

    /// <summary>
    /// Cancels the current edit.
    /// </summary>
    public void CancelEdit()
    {
        if (_editingItem == null || _editingColumn == null)
            return;

        CancelEditInternal();
    }

    /// <summary>
    /// Expands the row details for the specified item.
    /// </summary>
    public void ExpandRow(object item)
    {
        if (RowDetailsTemplate == null || _expandedItems.Contains(item))
            return;

        _expandedItems.Add(item);
        BuildDataRows();
    }

    /// <summary>
    /// Collapses the row details for the specified item.
    /// </summary>
    public void CollapseRow(object item)
    {
        if (_expandedItems.Remove(item))
        {
            BuildDataRows();
        }
    }

    /// <summary>
    /// Toggles the row details visibility for the specified item.
    /// </summary>
    public void ToggleRowDetails(object item)
    {
        if (_expandedItems.Contains(item))
            CollapseRow(item);
        else
            ExpandRow(item);
    }

    /// <summary>
    /// Gets whether the specified item's row details are expanded.
    /// </summary>
    public bool IsRowExpanded(object item) => _expandedItems.Contains(item);

    /// <summary>
    /// Expands all row details.
    /// </summary>
    public void ExpandAllRows()
    {
        if (RowDetailsTemplate == null)
            return;

        foreach (var item in _sortedItems)
        {
            _expandedItems.Add(item);
        }
        BuildDataRows();
    }

    /// <summary>
    /// Collapses all row details.
    /// </summary>
    public void CollapseAllRows()
    {
        _expandedItems.Clear();
        BuildDataRows();
    }

    #endregion

    #region Public Methods - Export

    /// <summary>
    /// Exports the grid data to CSV format.
    /// </summary>
    public string ExportToCsv(DataGridExportOptions? options = null)
    {
        options ??= new DataGridExportOptions();
        var items = GetExportItems(options);
        var columns = GetExportColumns(options);

        var args = new DataGridExportEventArgs("CSV", options);
        Exporting?.Invoke(this, args);

        if (args.Cancel)
            return string.Empty;

        var result = DataGridExporter.ExportToCsv(items, columns, options);
        args.ExportedData = result;

        Exported?.Invoke(this, args);

        if (ExportCommand?.CanExecute(args) == true)
            ExportCommand.Execute(args);

        return result;
    }

    /// <summary>
    /// Exports the grid data to CSV format asynchronously.
    /// </summary>
    public async Task ExportToCsvAsync(Stream stream, DataGridExportOptions? options = null)
    {
        options ??= new DataGridExportOptions();
        var items = GetExportItems(options);
        var columns = GetExportColumns(options);

        await DataGridExporter.ExportToCsvAsync(stream, items, columns, options);
    }

    /// <summary>
    /// Exports the grid data to JSON format.
    /// </summary>
    public string ExportToJson(DataGridExportOptions? options = null)
    {
        options ??= new DataGridExportOptions();
        var items = GetExportItems(options);
        var columns = GetExportColumns(options);

        var args = new DataGridExportEventArgs("JSON", options);
        Exporting?.Invoke(this, args);

        if (args.Cancel)
            return string.Empty;

        var result = DataGridExporter.ExportToJson(items, columns, options);
        args.ExportedData = result;

        Exported?.Invoke(this, args);

        if (ExportCommand?.CanExecute(args) == true)
            ExportCommand.Execute(args);

        return result;
    }

    /// <summary>
    /// Exports the grid data to JSON format asynchronously.
    /// </summary>
    public async Task ExportToJsonAsync(Stream stream, DataGridExportOptions? options = null)
    {
        options ??= new DataGridExportOptions();
        var items = GetExportItems(options);
        var columns = GetExportColumns(options);

        await DataGridExporter.ExportToJsonAsync(stream, items, columns, options);
    }

    /// <summary>
    /// Copies data to clipboard.
    /// </summary>
    public async Task CopyToClipboardAsync(DataGridCopyMode mode = DataGridCopyMode.SelectedRows)
    {
        var options = new DataGridExportOptions
        {
            SelectedRowsOnly = mode == DataGridCopyMode.SelectedRows || mode == DataGridCopyMode.SelectedCells
        };

        var items = mode switch
        {
            DataGridCopyMode.AllData => _sortedItems,
            DataGridCopyMode.AllVisibleRows => _filteredItems,
            _ => _selectedItems.ToList()
        };

        var columns = GetExportColumns(options);
        var text = DataGridExporter.FormatForClipboard(items, columns, options);

        await Clipboard.Default.SetTextAsync(text);
    }

    /// <summary>
    /// Prints the grid data.
    /// </summary>
    /// <param name="options">The print options.</param>
    /// <returns>True if printing was successful; otherwise, false.</returns>
    public async Task<bool> PrintAsync(DataGridPrintOptions? options = null)
    {
        options ??= new DataGridPrintOptions();

        var html = GeneratePrintHtml(options);
        var printService = GetPrintService();

        return await printService.PrintHtmlAsync(html, options.ToPrintOptions());
    }

    private Services.IPrintService GetPrintService()
    {
        // Try to get from dependency injection, fall back to default
        return new Services.DefaultPrintService();
    }

    private string GeneratePrintHtml(DataGridPrintOptions options)
    {
        var exportOptions = new DataGridExportOptions
        {
            VisibleColumnsOnly = options.VisibleColumnsOnly,
            SelectedRowsOnly = options.SelectedRowsOnly,
            IncludeHeaders = options.IncludeHeaders,
            DateFormat = options.DateFormat
        };

        var items = GetExportItems(exportOptions);
        var columns = GetExportColumns(exportOptions);

        var sb = new System.Text.StringBuilder();

        // HTML header
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine($"<title>{System.Web.HttpUtility.HtmlEncode(options.Title ?? "Data Grid")}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("h1 { font-size: 18px; margin-bottom: 10px; }");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
        if (options.IncludeGridLines)
        {
            sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        }
        else
        {
            sb.AppendLine("th, td { padding: 8px; text-align: left; }");
        }
        sb.AppendLine("th { background-color: #f5f5f5; font-weight: bold; }");
        if (options.IncludeAlternatingRows)
        {
            sb.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
        }
        sb.AppendLine("@media print { body { margin: 0; } }");
        if (!string.IsNullOrEmpty(options.CustomCss))
        {
            sb.AppendLine(options.CustomCss);
        }
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Title
        if (!string.IsNullOrEmpty(options.Title))
        {
            sb.AppendLine($"<h1>{System.Web.HttpUtility.HtmlEncode(options.Title)}</h1>");
        }

        // Table
        sb.AppendLine("<table>");

        // Header row
        if (options.IncludeHeaders)
        {
            sb.AppendLine("<thead><tr>");
            foreach (var column in columns)
            {
                sb.AppendLine($"<th>{System.Web.HttpUtility.HtmlEncode(column.Header)}</th>");
            }
            sb.AppendLine("</tr></thead>");
        }

        // Data rows
        sb.AppendLine("<tbody>");
        foreach (var item in items)
        {
            sb.AppendLine("<tr>");
            foreach (var column in columns)
            {
                var value = column.GetCellValue(item);
                var displayValue = FormatPrintValue(value, column, options);
                sb.AppendLine($"<td>{System.Web.HttpUtility.HtmlEncode(displayValue)}</td>");
            }
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody>");

        // Footer row
        if (options.IncludeFooter && ShowFooter)
        {
            sb.AppendLine("<tfoot><tr>");
            foreach (var column in columns)
            {
                var aggregate = column.CalculateAggregate(items);
                var displayValue = column.FormatAggregateValue(aggregate);
                sb.AppendLine($"<td><strong>{System.Web.HttpUtility.HtmlEncode(displayValue)}</strong></td>");
            }
            sb.AppendLine("</tr></tfoot>");
        }

        sb.AppendLine("</table>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string FormatPrintValue(object? value, DataGridColumn column, DataGridPrintOptions options)
    {
        if (value == null)
            return string.Empty;

        if (value is DateTime dt)
            return dt.ToString(options.DateFormat);

        if (value is DateTimeOffset dto)
            return dto.ToString(options.DateFormat);

        if (column is DataGridTextColumn textColumn && !string.IsNullOrEmpty(textColumn.Format) && value is IFormattable formattable)
            return formattable.ToString(textColumn.Format, null);

        return value.ToString() ?? string.Empty;
    }

    #endregion

    #region Public Methods - Filtering

    /// <summary>
    /// Clears the filter for a specific column.
    /// </summary>
    public void ClearFilter(DataGridColumn column)
    {
        if (_activeFilters.ContainsKey(column))
        {
            _activeFilters.Remove(column);
            column.FilterValues = null;
            column.FilterText = null;
            ApplyFilters();
        }
    }

    /// <summary>
    /// Clears all filters.
    /// </summary>
    public void ClearAllFilters()
    {
        foreach (var column in _activeFilters.Keys.ToList())
        {
            column.FilterValues = null;
            column.FilterText = null;
        }
        _activeFilters.Clear();
        ApplyFilters();
    }

    /// <summary>
    /// Gets distinct values for a column (for filter popup).
    /// </summary>
    public IEnumerable<object?> GetDistinctValues(DataGridColumn column)
    {
        if (ItemsSource == null)
            yield break;

        var seen = new HashSet<object?>();
        foreach (var item in ItemsSource)
        {
            var value = column.GetCellValue(item);
            if (seen.Add(value))
            {
                yield return value;
            }
        }
    }

    #endregion

    #region Public Methods - Navigation

    /// <summary>
    /// Scrolls to the specified item.
    /// </summary>
    public void ScrollTo(object item)
    {
        var index = _sortedItems.IndexOf(item);
        if (index >= 0)
        {
            var y = index * RowHeight;
            dataScrollView.ScrollToAsync(0, y, true);
        }
    }

    /// <summary>
    /// Refreshes the data display.
    /// </summary>
    public void RefreshData()
    {
        BuildGrid();
    }

    /// <summary>
    /// Clears the current sort.
    /// </summary>
    public void ClearSort()
    {
        if (_currentSortColumn != null)
        {
            _currentSortColumn.SortDirection = null;
            _currentSortColumn = null;
        }
        _sortDescriptions.Clear();
        ApplySort();
        BuildDataRows();
    }

    #endregion

    #region Public Methods - Pagination

    /// <summary>
    /// Goes to the specified page.
    /// </summary>
    public void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages)
            return;

        var oldPage = CurrentPage;
        CurrentPage = page;

        RaisePageChanged(oldPage, page);
    }

    /// <summary>
    /// Goes to the next page.
    /// </summary>
    public void NextPage()
    {
        if (CurrentPage < TotalPages)
            GoToPage(CurrentPage + 1);
    }

    /// <summary>
    /// Goes to the previous page.
    /// </summary>
    public void PreviousPage()
    {
        if (CurrentPage > 1)
            GoToPage(CurrentPage - 1);
    }

    /// <summary>
    /// Goes to the first page.
    /// </summary>
    public void FirstPage()
    {
        GoToPage(1);
    }

    /// <summary>
    /// Goes to the last page.
    /// </summary>
    public void LastPage()
    {
        GoToPage(TotalPages);
    }

    #endregion

    #region Public Methods - Column Auto-Fit

    /// <summary>
    /// Auto-fits the width of a column to its content.
    /// </summary>
    public void AutoFitColumn(DataGridColumn column)
    {
        // This would require measuring actual content width
        // For now, set to auto
        column.Width = -1;
        BuildGrid();
    }

    /// <summary>
    /// Auto-fits all columns to their content.
    /// </summary>
    public void AutoFitAllColumns()
    {
        foreach (var column in _columns)
        {
            column.Width = -1;
        }
        BuildGrid();
    }

    #endregion

    #region Public Methods - Search

    /// <summary>
    /// Finds and scrolls to the next match.
    /// </summary>
    public int FindNext(string text)
    {
        if (string.IsNullOrEmpty(text))
            return -1;

        var startIndex = _focusedRowIndex >= 0 ? _focusedRowIndex + 1 : 0;
        var visibleColumns = GetVisibleColumns();

        for (int i = startIndex; i < _sortedItems.Count; i++)
        {
            foreach (var column in visibleColumns)
            {
                var value = column.GetCellValue(_sortedItems[i])?.ToString();
                if (value != null && value.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    SelectItem(_sortedItems[i]);
                    ScrollTo(_sortedItems[i]);
                    return i;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Finds and scrolls to the previous match.
    /// </summary>
    public int FindPrevious(string text)
    {
        if (string.IsNullOrEmpty(text))
            return -1;

        var startIndex = _focusedRowIndex > 0 ? _focusedRowIndex - 1 : _sortedItems.Count - 1;
        var visibleColumns = GetVisibleColumns();

        for (int i = startIndex; i >= 0; i--)
        {
            foreach (var column in visibleColumns)
            {
                var value = column.GetCellValue(_sortedItems[i])?.ToString();
                if (value != null && value.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    SelectItem(_sortedItems[i]);
                    ScrollTo(_sortedItems[i]);
                    return i;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Clears the search.
    /// </summary>
    public void ClearSearch()
    {
        SearchText = null;
    }

    #endregion

    #region IUndoRedo Implementation

    /// <inheritdoc />
    public bool Undo()
    {
        if (_undoStack.Count == 0)
            return false;

        var operation = _undoStack.Pop();
        operation.Undo();
        _redoStack.Push(operation);

        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoCount));
        OnPropertyChanged(nameof(RedoCount));

        return true;
    }

    /// <inheritdoc />
    public bool Redo()
    {
        if (_redoStack.Count == 0)
            return false;

        var operation = _redoStack.Pop();
        operation.Redo();
        _undoStack.Push(operation);

        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoCount));
        OnPropertyChanged(nameof(RedoCount));

        return true;
    }

    /// <inheritdoc />
    public void ClearUndoHistory()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        _currentBatch = null;
        _batchNestingLevel = 0;
        _currentBatchDescription = null;

        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoCount));
        OnPropertyChanged(nameof(RedoCount));
    }

    /// <inheritdoc />
    public string? GetUndoDescription()
    {
        return _undoStack.Count > 0 ? _undoStack.Peek().Description : null;
    }

    /// <inheritdoc />
    public string? GetRedoDescription()
    {
        return _redoStack.Count > 0 ? _redoStack.Peek().Description : null;
    }

    /// <inheritdoc />
    public void BeginBatchOperation(string? description = null)
    {
        if (_batchNestingLevel == 0)
        {
            _currentBatch = new List<IUndoableOperation>();
            _currentBatchDescription = description;
        }
        _batchNestingLevel++;
    }

    /// <inheritdoc />
    public void EndBatchOperation()
    {
        if (_batchNestingLevel == 0)
            throw new InvalidOperationException("EndBatchOperation called without matching BeginBatchOperation");

        _batchNestingLevel--;

        if (_batchNestingLevel == 0 && _currentBatch != null && _currentBatch.Count > 0)
        {
            var batchOp = new BatchOperation(_currentBatchDescription, _currentBatch);
            PushUndoOperation(batchOp);
            _currentBatch = null;
            _currentBatchDescription = null;
        }
    }

    /// <inheritdoc />
    public void CancelBatchOperation()
    {
        if (_batchNestingLevel == 0)
            throw new InvalidOperationException("CancelBatchOperation called without matching BeginBatchOperation");

        // Undo all operations in the batch in reverse order
        if (_currentBatch != null)
        {
            for (int i = _currentBatch.Count - 1; i >= 0; i--)
            {
                _currentBatch[i].Undo();
            }
        }

        _batchNestingLevel = 0;
        _currentBatch = null;
        _currentBatchDescription = null;
    }

    private void PushUndoOperation(IUndoableOperation operation)
    {
        if (_batchNestingLevel > 0 && _currentBatch != null)
        {
            _currentBatch.Add(operation);
            return;
        }

        _undoStack.Push(operation);
        _redoStack.Clear();

        // Enforce undo limit
        while (_undoLimit > 0 && _undoStack.Count > _undoLimit)
        {
            // Remove oldest (bottom of stack) by recreating stack
            var items = _undoStack.Reverse().Skip(1).Reverse().ToList();
            _undoStack.Clear();
            foreach (var item in items)
            {
                _undoStack.Push(item);
            }
        }

        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoCount));
        OnPropertyChanged(nameof(RedoCount));
    }

    #endregion

    #region IClipboardSupport Implementation

    /// <inheritdoc />
    public void Copy()
    {
        if (!CanCopy)
            return;

        var args = new DataGridClipboardEventArgs(DataGridClipboardOperation.Copy, _selectedItems.ToList());
        Copying?.Invoke(this, args);

        if (args.Cancel)
            return;

        var options = new DataGridExportOptions { SelectedRowsOnly = true };
        var columns = GetExportColumns(options);
        var content = DataGridExporter.FormatForClipboard(_selectedItems, columns, options);

        args.Content = content;
        Clipboard.Default.SetTextAsync(content);

        if (CopyCommand?.CanExecute(args) == true)
            CopyCommand.Execute(args);
    }

    /// <inheritdoc />
    public void Cut()
    {
        if (!CanCut)
            return;

        var args = new DataGridClipboardEventArgs(DataGridClipboardOperation.Cut, _selectedItems.ToList());
        Cutting?.Invoke(this, args);

        if (args.Cancel)
            return;

        // Copy first
        Copy();

        // TODO: Delete selected rows if the underlying collection supports it
    }

    /// <inheritdoc />
    public void Paste()
    {
        if (!CanPaste)
            return;

        PasteAsync();
    }

    private async void PasteAsync()
    {
        var text = await Clipboard.Default.GetTextAsync();
        if (string.IsNullOrEmpty(text))
            return;

        var args = new DataGridClipboardEventArgs(DataGridClipboardOperation.Paste, _selectedItems.ToList(), text);
        Pasting?.Invoke(this, args);

        if (args.Cancel)
            return;

        // Parse TSV data and apply to cells
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            return;

        var visibleColumns = GetVisibleColumns();
        var startRowIndex = _focusedRowIndex >= 0 ? _focusedRowIndex : 0;
        var startColIndex = _focusedColumnIndex >= 0 ? _focusedColumnIndex : 0;

        var cellEdits = new List<CellEditOperation>();

        BeginBatchOperation("Paste");

        try
        {
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var rowIndex = startRowIndex + lineIndex;
                if (rowIndex >= _sortedItems.Count)
                    break;

                var item = _sortedItems[rowIndex];
                var values = lines[lineIndex].Split('\t');

                for (int colOffset = 0; colOffset < values.Length; colOffset++)
                {
                    var colIndex = startColIndex + colOffset;
                    if (colIndex >= visibleColumns.Count)
                        break;

                    var column = visibleColumns[colIndex];
                    if (column.IsReadOnly || !column.CanUserEdit)
                        continue;

                    var oldValue = column.GetCellValue(item);
                    var newValue = ConvertPastedValue(values[colOffset], column, item);

                    if (!Equals(oldValue, newValue))
                    {
                        var editOp = new CellEditOperation(this, item, column, rowIndex, colIndex, oldValue, newValue);
                        column.SetCellValue(item, newValue);
                        cellEdits.Add(editOp);
                    }
                }
            }

            EndBatchOperation();

            if (PasteCommand?.CanExecute(args) == true)
                PasteCommand.Execute(args);

            RefreshData();
        }
        catch
        {
            CancelBatchOperation();
            throw;
        }
    }

    private static object? ConvertPastedValue(string value, DataGridColumn column, object item)
    {
        var currentValue = column.GetCellValue(item);
        if (currentValue == null)
            return value;

        var targetType = currentValue.GetType();

        try
        {
            if (targetType == typeof(string))
                return value;
            if (targetType == typeof(int))
                return int.TryParse(value, out var i) ? i : currentValue;
            if (targetType == typeof(decimal))
                return decimal.TryParse(value, out var d) ? d : currentValue;
            if (targetType == typeof(double))
                return double.TryParse(value, out var dbl) ? dbl : currentValue;
            if (targetType == typeof(bool))
                return bool.TryParse(value, out var b) ? b : currentValue;
            if (targetType == typeof(DateTime))
                return DateTime.TryParse(value, out var dt) ? dt : currentValue;

            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return currentValue;
        }
    }

    /// <inheritdoc />
    public object? GetClipboardContent()
    {
        if (!CanCopy)
            return null;

        var options = new DataGridExportOptions { SelectedRowsOnly = true };
        var columns = GetExportColumns(options);
        return DataGridExporter.FormatForClipboard(_selectedItems, columns, options);
    }

    #endregion

    #region Context Menu

    /// <summary>
    /// Shows the context menu at the specified cell.
    /// </summary>
    public void ShowContextMenu(object? item, DataGridColumn? column, int rowIndex, int columnIndex)
    {
        var args = new DataGridContextMenuEventArgs(item, column, rowIndex, columnIndex);
        ContextMenuOpening?.Invoke(this, args);

        if (args.Cancel)
            return;

        // Select the item if not already selected
        if (item != null && !_selectedItems.Contains(item))
        {
            SelectItem(item);
        }

        // Build and show context menu
        if (ShowDefaultContextMenu)
        {
            BuildAndShowDefaultContextMenu(item, column, rowIndex, columnIndex, args.CustomMenuItems);
        }
    }

    private void BuildAndShowDefaultContextMenu(object? item, DataGridColumn? column, int rowIndex, int columnIndex, IList<object>? customItems)
    {
        // Build list of default menu actions for consumers to use
        var defaultActions = new List<DataGridContextMenuAction>();

        // Copy
        if (CanCopy)
        {
            defaultActions.Add(new DataGridContextMenuAction("Copy", Copy));
        }

        // Cut
        if (CanCut)
        {
            defaultActions.Add(new DataGridContextMenuAction("Cut", Cut));
        }

        // Paste
        if (CanPaste)
        {
            defaultActions.Add(new DataGridContextMenuAction("Paste", Paste));
        }

        // Undo/Redo
        if (CanUndo)
        {
            defaultActions.Add(new DataGridContextMenuAction($"Undo {GetUndoDescription()}", () => Undo()));
        }

        if (CanRedo)
        {
            defaultActions.Add(new DataGridContextMenuAction($"Redo {GetRedoDescription()}", () => Redo()));
        }

        // Expand/Collapse row details
        if (RowDetailsTemplate != null && item != null && RowDetailsVisibility == DataGridRowDetailsVisibilityMode.Collapsed)
        {
            var isExpanded = _expandedItems.Contains(item);
            defaultActions.Add(new DataGridContextMenuAction(
                isExpanded ? "Collapse Details" : "Expand Details",
                () => ToggleRowDetails(item)));
        }

        // Store the actions for consumers who handle ContextMenuOpening
        // They can build platform-specific menus using these actions
        _lastContextMenuActions = defaultActions;
    }

    private List<DataGridContextMenuAction>? _lastContextMenuActions;

    /// <summary>
    /// Gets the default context menu actions from the last context menu request.
    /// Use this in the ContextMenuOpening event handler to build custom menus.
    /// </summary>
    public IReadOnlyList<DataGridContextMenuAction>? GetDefaultContextMenuActions() => _lastContextMenuActions;

    #endregion

    #region Private Methods - Grid Building

    private void BuildGrid()
    {
        if (_isUpdating)
            return;

        _isUpdating = true;

        // Auto-generate columns if needed
        if (AutoGenerateColumns && _columns.Count == 0 && ItemsSource != null)
        {
            GenerateColumns();
        }

        // Apply filters, sort, and pagination
        ApplyFilters();
        ApplySort();

        // Build UI
        BuildHeader();
        BuildDataRows();
        BuildFooter();

        // Show/hide empty view
        UpdateEmptyView();

        // Update pagination UI
        UpdatePaginationUI();

        _isUpdating = false;
    }

    private void GenerateColumns()
    {
        _columns.Clear();

        var firstItem = ItemsSource?.Cast<object>().FirstOrDefault();
        if (firstItem == null)
            return;

        var type = firstItem.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Skip complex types
            if (!IsSimpleType(property.PropertyType))
                continue;

            DataGridColumn column;

            if (property.PropertyType == typeof(bool))
            {
                column = new DataGridCheckBoxColumn
                {
                    Header = FormatHeader(property.Name),
                    Binding = property.Name
                };
            }
            else if (property.PropertyType == typeof(ImageSource) ||
                     (property.PropertyType == typeof(string) && property.Name.Contains("Image", StringComparison.OrdinalIgnoreCase)))
            {
                column = new DataGridImageColumn
                {
                    Header = FormatHeader(property.Name),
                    Binding = property.Name
                };
            }
            else
            {
                var textColumn = new DataGridTextColumn
                {
                    Header = FormatHeader(property.Name),
                    Binding = property.Name
                };

                // Apply default formats
                if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(double) || property.PropertyType == typeof(float))
                {
                    textColumn.Format = "N2";
                    textColumn.TextAlignment = TextAlignment.End;
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    textColumn.Format = "d";
                }

                column = textColumn;
            }

            _columns.Add(column);
        }
    }

    private static bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType.IsPrimitive ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(TimeSpan) ||
               underlyingType == typeof(Guid) ||
               underlyingType.IsEnum ||
               underlyingType == typeof(ImageSource);
    }

    private static string FormatHeader(string propertyName)
    {
        // Insert spaces before capitals
        var result = string.Concat(propertyName.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));
        return result;
    }

    private List<DataGridColumn> GetVisibleColumns()
    {
        return _columns.Where(c => c.IsVisible).ToList();
    }

    private List<DataGridColumn> GetFrozenColumns()
    {
        var visible = GetVisibleColumns();
        return visible.Take(Math.Min(FrozenColumnCount, visible.Count)).ToList();
    }

    private List<DataGridColumn> GetScrollableColumns()
    {
        var visible = GetVisibleColumns();
        return visible.Skip(Math.Min(FrozenColumnCount, visible.Count)).ToList();
    }

    private void BuildHeader()
    {
        // Clear existing headers
        headerGrid.Children.Clear();
        headerGrid.ColumnDefinitions.Clear();
        frozenHeaderGrid.Children.Clear();
        frozenHeaderGrid.ColumnDefinitions.Clear();

        var frozenColumns = GetFrozenColumns();
        var scrollableColumns = GetScrollableColumns();

        // Build frozen headers
        for (int i = 0; i < frozenColumns.Count; i++)
        {
            var column = frozenColumns[i];
            var width = column.Width < 0 ? GridLength.Auto : new GridLength(column.Width);
            frozenHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

            var headerCell = CreateHeaderCell(column, i, true);
            frozenHeaderGrid.Children.Add(headerCell);
            Grid.SetColumn(headerCell, i);
        }

        // Build scrollable headers
        for (int i = 0; i < scrollableColumns.Count; i++)
        {
            var column = scrollableColumns[i];
            var width = column.Width < 0 ? GridLength.Auto : new GridLength(column.Width);
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

            var headerCell = CreateHeaderCell(column, i + frozenColumns.Count, false);
            headerGrid.Children.Add(headerCell);
            Grid.SetColumn(headerCell, i);
        }
    }

    private View CreateHeaderCell(DataGridColumn column, int columnIndex, bool isFrozen)
    {
        // Main container - use same padding as data cells for alignment
        var container = new Grid
        {
            BackgroundColor = Colors.Transparent
        };

        // Content grid with header text and indicators
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(8, 4), // Match data cell padding
            VerticalOptions = LayoutOptions.Center
        };

        // Header text
        var headerLabel = new Label
        {
            Text = column.Header,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            TextColor = EffectiveForegroundColor
        };
        contentGrid.Children.Add(headerLabel);
        Grid.SetColumn(headerLabel, 0);

        // Filter indicator
        if (CanUserFilter && column.CanUserFilter)
        {
            var filterLabel = new Label
            {
                Text = column.IsFiltered ? "🔽" : "▽",
                FontSize = 10,
                VerticalOptions = LayoutOptions.Center,
                TextColor = column.IsFiltered ? EffectiveAccentColor : Colors.Gray,
                Margin = new Thickness(4, 0, 0, 0)
            };

            var filterTap = new TapGestureRecognizer();
            filterTap.Tapped += (s, e) => ShowFilterPopup(column);
            filterLabel.GestureRecognizers.Add(filterTap);

            contentGrid.Children.Add(filterLabel);
            Grid.SetColumn(filterLabel, 1);
        }

        // Sort indicator
        var sortLabel = new Label
        {
            Text = column.SortIndicator,
            FontSize = 10,
            VerticalOptions = LayoutOptions.Center,
            TextColor = EffectiveAccentColor,
            Margin = new Thickness(4, 0, 0, 0)
        };
        sortLabel.SetBinding(Label.TextProperty, new Binding(nameof(column.SortIndicator), source: column));
        contentGrid.Children.Add(sortLabel);
        Grid.SetColumn(sortLabel, 2);

        container.Children.Add(contentGrid);

        // Add resize grip (if resizing allowed) - positioned at the right edge
        if (CanUserResize && column.CanUserResize)
        {
            // Visible resize grip with good hit area
            var resizeGrip = new BoxView
            {
                Color = Colors.Gray.WithAlpha(0.4f),
                WidthRequest = 6,
                MinimumWidthRequest = 6,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(0)
            };

            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += (s, e) => OnColumnResizePan(column, columnIndex, e);
            resizeGrip.GestureRecognizers.Add(panGesture);

            // Add hover effect for desktop
            var pointerGesture = new PointerGestureRecognizer();
            pointerGesture.PointerEntered += (s, e) => resizeGrip.Color = EffectiveAccentColor;
            pointerGesture.PointerExited += (s, e) => resizeGrip.Color = Colors.Gray.WithAlpha(0.4f);
            resizeGrip.GestureRecognizers.Add(pointerGesture);

            container.Children.Add(resizeGrip);
        }

        // Add drag gesture for column reordering
        if (CanUserReorder && column.CanUserReorder)
        {
            var dragGesture = new DragGestureRecognizer();
            dragGesture.DragStarting += (s, e) => OnColumnDragStarting(column, columnIndex, e);
            contentGrid.GestureRecognizers.Add(dragGesture);

            var dropGesture = new DropGestureRecognizer();
            dropGesture.DragOver += (s, e) => OnColumnDragOver(columnIndex, e);
            dropGesture.Drop += (s, e) => OnColumnDrop(columnIndex, e);
            contentGrid.GestureRecognizers.Add(dropGesture);
        }

        // Add tap gesture for sorting
        if (CanUserSort && column.CanUserSort)
        {
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => OnHeaderTapped(column);
            contentGrid.GestureRecognizers.Add(tapGesture);
        }

        // Add bottom border
        if (GridLinesVisibility == GridLinesVisibility.Horizontal || GridLinesVisibility == GridLinesVisibility.Both)
        {
            var border = new BoxView
            {
                Color = GridLineColor,
                HeightRequest = 1,
                VerticalOptions = LayoutOptions.End
            };
            container.Children.Add(border);
        }

        return container;
    }

    private void BuildDataRows()
    {
        // Use virtualization if enabled
        if (EnableVirtualization && !EnablePagination)
        {
            BuildVirtualizedRows();
            return;
        }

        // Clear any virtualization panels
        ClearVirtualizationPanels();

        // Clear existing rows
        dataGrid.Children.Clear();
        dataGrid.RowDefinitions.Clear();
        dataGrid.ColumnDefinitions.Clear();
        frozenDataGrid.Children.Clear();
        frozenDataGrid.RowDefinitions.Clear();
        frozenDataGrid.ColumnDefinitions.Clear();

        // Get items for current page
        var displayItems = GetDisplayItems();
        if (displayItems.Count == 0)
            return;

        var frozenColumns = GetFrozenColumns();
        var scrollableColumns = GetScrollableColumns();

        // Add column definitions for frozen grid
        for (int i = 0; i < frozenColumns.Count; i++)
        {
            var column = frozenColumns[i];
            var width = column.Width < 0 ? GridLength.Auto : new GridLength(column.Width);
            frozenDataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
        }

        // Add column definitions for scrollable grid
        for (int i = 0; i < scrollableColumns.Count; i++)
        {
            var column = scrollableColumns[i];
            var width = column.Width < 0 ? GridLength.Auto : new GridLength(column.Width);
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
        }

        // Add row definitions and cells
        var gridRowIndex = 0;
        for (int displayIndex = 0; displayIndex < displayItems.Count; displayIndex++)
        {
            frozenDataGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(RowHeight) });
            dataGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(RowHeight) });

            var item = displayItems[displayIndex];
            var actualRowIndex = _sortedItems.IndexOf(item);
            var isSelected = _selectedItems.Contains(item);
            var isAlternate = displayIndex % 2 == 1;

            // Build frozen cells
            for (int colIndex = 0; colIndex < frozenColumns.Count; colIndex++)
            {
                var column = frozenColumns[colIndex];
                var cell = CreateDataCell(item, column, actualRowIndex, colIndex, isSelected, isAlternate);
                frozenDataGrid.Children.Add(cell);
                Grid.SetRow(cell, gridRowIndex);
                Grid.SetColumn(cell, colIndex);
            }

            // Build scrollable cells
            for (int colIndex = 0; colIndex < scrollableColumns.Count; colIndex++)
            {
                var column = scrollableColumns[colIndex];
                var fullColIndex = colIndex + frozenColumns.Count;
                var cell = CreateDataCell(item, column, actualRowIndex, fullColIndex, isSelected, isAlternate);
                dataGrid.Children.Add(cell);
                Grid.SetRow(cell, gridRowIndex);
                Grid.SetColumn(cell, colIndex);
            }

            gridRowIndex++;

            // Add row details if applicable
            if (ShouldShowRowDetails(item, isSelected))
            {
                AddRowDetails(item, gridRowIndex, frozenColumns.Count, scrollableColumns.Count);
                gridRowIndex++;
            }
        }
    }

    private void BuildVirtualizedRows()
    {
        var frozenColumns = GetFrozenColumns();
        var scrollableColumns = GetScrollableColumns();

        // Clear non-virtualized grids
        dataGrid.Children.Clear();
        dataGrid.RowDefinitions.Clear();
        dataGrid.ColumnDefinitions.Clear();
        frozenDataGrid.Children.Clear();
        frozenDataGrid.RowDefinitions.Clear();
        frozenDataGrid.ColumnDefinitions.Clear();

        // Initialize or update scrollable virtualizing panel
        if (_virtualizingPanel == null)
        {
            _virtualizingPanel = new VirtualizingDataGridPanel();
            dataScrollView.Content = _virtualizingPanel;
        }

        _virtualizingPanel.RowHeight = RowHeight;
        _virtualizingPanel.BufferSize = VirtualizationBufferSize;
        _virtualizingPanel.ItemsSource = _sortedItems;
        _virtualizingPanel.RowFactory = (item, rowIndex) => CreateVirtualizedRow(item, rowIndex, scrollableColumns, frozenColumns.Count);
        _virtualizingPanel.RowUpdater = (row, item, rowIndex) => UpdateVirtualizedRow(row, item, rowIndex, scrollableColumns, frozenColumns.Count);
        _virtualizingPanel.Refresh(dataScrollView.Height > 0 ? dataScrollView.Height : 400);

        // Initialize or update frozen virtualizing panel
        if (frozenColumns.Count > 0)
        {
            if (_frozenVirtualizingPanel == null)
            {
                _frozenVirtualizingPanel = new VirtualizingDataGridPanel();
                // Note: Need to wire up frozen panel to scroll in sync
            }

            _frozenVirtualizingPanel.RowHeight = RowHeight;
            _frozenVirtualizingPanel.BufferSize = VirtualizationBufferSize;
            _frozenVirtualizingPanel.ItemsSource = _sortedItems;
            _frozenVirtualizingPanel.RowFactory = (item, rowIndex) => CreateVirtualizedRow(item, rowIndex, frozenColumns, 0);
            _frozenVirtualizingPanel.RowUpdater = (row, item, rowIndex) => UpdateVirtualizedRow(row, item, rowIndex, frozenColumns, 0);
            _frozenVirtualizingPanel.Refresh(dataScrollView.Height > 0 ? dataScrollView.Height : 400);
        }
    }

    private View CreateVirtualizedRow(object item, int rowIndex, List<DataGridColumn> columns, int columnOffset)
    {
        var isSelected = _selectedItems.Contains(item);
        var isAlternate = rowIndex % 2 == 1;

        var rowGrid = new Grid();

        // Add column definitions
        foreach (var column in columns)
        {
            var width = column.Width < 0 ? GridLength.Auto : new GridLength(column.Width);
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
        }

        // Add cells
        for (int colIndex = 0; colIndex < columns.Count; colIndex++)
        {
            var column = columns[colIndex];
            var fullColIndex = colIndex + columnOffset;
            var cell = CreateDataCell(item, column, rowIndex, fullColIndex, isSelected, isAlternate);
            rowGrid.Children.Add(cell);
            Grid.SetColumn(cell, colIndex);
        }

        return rowGrid;
    }

    private void UpdateVirtualizedRow(View row, object item, int rowIndex, List<DataGridColumn> columns, int columnOffset)
    {
        if (row is not Grid rowGrid)
            return;

        var isSelected = _selectedItems.Contains(item);
        var isAlternate = rowIndex % 2 == 1;

        // Clear and rebuild cells (simpler approach; could optimize to update in-place)
        rowGrid.Children.Clear();

        for (int colIndex = 0; colIndex < columns.Count; colIndex++)
        {
            var column = columns[colIndex];
            var fullColIndex = colIndex + columnOffset;
            var cell = CreateDataCell(item, column, rowIndex, fullColIndex, isSelected, isAlternate);
            rowGrid.Children.Add(cell);
            Grid.SetColumn(cell, colIndex);
        }
    }

    private void ClearVirtualizationPanels()
    {
        if (_virtualizingPanel != null)
        {
            _virtualizingPanel.Clear();
            _virtualizingPanel = null;
            // Restore original content
            dataScrollView.Content = dataGrid;
        }

        if (_frozenVirtualizingPanel != null)
        {
            _frozenVirtualizingPanel.Clear();
            _frozenVirtualizingPanel = null;
        }
    }

    private bool ShouldShowRowDetails(object item, bool isSelected)
    {
        if (RowDetailsTemplate == null)
            return false;

        return RowDetailsVisibility switch
        {
            DataGridRowDetailsVisibilityMode.Visible => true,
            DataGridRowDetailsVisibilityMode.VisibleWhenSelected => isSelected,
            DataGridRowDetailsVisibilityMode.Collapsed => _expandedItems.Contains(item),
            _ => false
        };
    }

    private void AddRowDetails(object item, int gridRowIndex, int frozenColumnCount, int scrollableColumnCount)
    {
        // Add row definition for details
        frozenDataGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        dataGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Create detail content
        var detailContent = RowDetailsTemplate!.CreateContent() as View;
        if (detailContent == null)
            return;

        detailContent.BindingContext = item;

        // Add detail container to scrollable grid (spanning all columns)
        var detailContainer = new Grid
        {
            BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#2A2A2A")
                : Color.FromArgb("#F8F8F8"),
            Padding = new Thickness(8)
        };
        detailContainer.Children.Add(detailContent);

        dataGrid.Children.Add(detailContainer);
        Grid.SetRow(detailContainer, gridRowIndex);
        Grid.SetColumnSpan(detailContainer, Math.Max(1, scrollableColumnCount));

        // Add empty spacer to frozen grid for alignment
        if (frozenColumnCount > 0)
        {
            var frozenSpacer = new Grid
            {
                BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#2A2A2A")
                    : Color.FromArgb("#F8F8F8")
            };
            frozenDataGrid.Children.Add(frozenSpacer);
            Grid.SetRow(frozenSpacer, gridRowIndex);
            Grid.SetColumnSpan(frozenSpacer, frozenColumnCount);
        }
    }

    private List<object> GetDisplayItems()
    {
        if (EnablePagination)
        {
            var skip = (CurrentPage - 1) * PageSize;
            return _sortedItems.Skip(skip).Take(PageSize).ToList();
        }
        return _sortedItems;
    }

    private View CreateDataCell(object item, DataGridColumn column, int rowIndex, int colIndex, bool isSelected, bool isAlternate)
    {
        var container = new Grid
        {
            Padding = new Thickness(0)
        };

        // Background color
        Color bgColor;
        var isFocused = rowIndex == _focusedRowIndex && colIndex == _focusedColumnIndex;

        if (isFocused)
        {
            bgColor = FocusedCellColor;
        }
        else if (isSelected)
        {
            bgColor = SelectedRowColor;
        }
        else if (isAlternate && AlternatingRowColor != null)
        {
            bgColor = AlternatingRowColor;
        }
        else
        {
            bgColor = Colors.Transparent;
        }
        container.BackgroundColor = bgColor;

        // Cell content
        var content = column.CreateCellContent(item);

        // Apply text color for selected/focused cells to ensure contrast
        if ((isSelected || isFocused) && content is Label contentLabel)
        {
            contentLabel.TextColor = SelectedTextColor;
        }

        // Apply search highlighting if enabled
        if (HighlightSearchResults && !string.IsNullOrEmpty(SearchText) && content is Label label && !string.IsNullOrEmpty(label.Text))
        {
            ApplySearchHighlighting(label, SearchText);
        }

        container.Children.Add(content);

        // Validation error indicator
        if (_validationErrors.TryGetValue((rowIndex, colIndex), out var validationResult) && !validationResult.IsValid)
        {
            var errorIndicator = new BoxView
            {
                Color = Colors.Red,
                HeightRequest = 2,
                VerticalOptions = LayoutOptions.End
            };
            container.Children.Add(errorIndicator);
        }

        // Grid lines
        if (GridLinesVisibility == GridLinesVisibility.Vertical || GridLinesVisibility == GridLinesVisibility.Both)
        {
            var rightBorder = new BoxView
            {
                Color = GridLineColor,
                WidthRequest = 1,
                HorizontalOptions = LayoutOptions.End
            };
            container.Children.Add(rightBorder);
        }

        if (GridLinesVisibility == GridLinesVisibility.Horizontal || GridLinesVisibility == GridLinesVisibility.Both)
        {
            var bottomBorder = new BoxView
            {
                Color = GridLineColor,
                HeightRequest = 1,
                VerticalOptions = LayoutOptions.End
            };
            container.Children.Add(bottomBorder);
        }

        // Store metadata for event handling
        container.AutomationId = $"cell_{rowIndex}_{colIndex}";

        // Add tap gestures
        var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
        tapGesture.Tapped += (s, e) => OnCellTapped(item, column, rowIndex, colIndex);
        container.GestureRecognizers.Add(tapGesture);

        var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTapGesture.Tapped += (s, e) => OnCellDoubleTapped(item, column, rowIndex, colIndex);
        container.GestureRecognizers.Add(doubleTapGesture);

        // Add row drag gesture (only on first column for drag handle)
        if (CanUserReorderRows && colIndex == 0)
        {
            var dragGesture = new DragGestureRecognizer();
            dragGesture.DragStarting += (s, e) => OnRowDragStarting(item, rowIndex, e);
            container.GestureRecognizers.Add(dragGesture);
        }

        // Add row drop gesture to all cells
        if (CanUserReorderRows)
        {
            var dropGesture = new DropGestureRecognizer();
            dropGesture.DragOver += (s, e) => OnRowDragOver(rowIndex, e);
            dropGesture.Drop += (s, e) => OnRowDrop(rowIndex, e);
            container.GestureRecognizers.Add(dropGesture);
        }

        // Add context menu gesture (right-click on desktop, long-press on mobile)
        if (ShowDefaultContextMenu || ContextMenuTemplate != null)
        {
#if WINDOWS || MACCATALYST
            var pointerGesture = new PointerGestureRecognizer();
            pointerGesture.PointerPressed += (s, e) =>
            {
                // Check for right-click (secondary button)
                // Note: Platform-specific handling may be needed
            };
            container.GestureRecognizers.Add(pointerGesture);
#endif

            // Long-press for context menu on all platforms
            var longPressHandler = new TapGestureRecognizer();
            // We'll use a custom approach since TapGestureRecognizer doesn't support long-press directly
            var panGesture = new PanGestureRecognizer();
            bool isPanStarted = false;
            System.Timers.Timer? longPressTimer = null;

            panGesture.PanUpdated += (s, e) =>
            {
                if (e.StatusType == GestureStatus.Started)
                {
                    isPanStarted = true;
                    longPressTimer = new System.Timers.Timer(500); // 500ms for long press
                    longPressTimer.AutoReset = false;
                    longPressTimer.Elapsed += (ts, te) =>
                    {
                        if (isPanStarted)
                        {
                            Dispatcher.Dispatch(() => ShowContextMenu(item, column, rowIndex, colIndex));
                        }
                    };
                    longPressTimer.Start();
                }
                else if (e.StatusType == GestureStatus.Running)
                {
                    // If moved too much, cancel the long-press
                    if (Math.Abs(e.TotalX) > 10 || Math.Abs(e.TotalY) > 10)
                    {
                        isPanStarted = false;
                        longPressTimer?.Stop();
                        longPressTimer?.Dispose();
                    }
                }
                else
                {
                    isPanStarted = false;
                    longPressTimer?.Stop();
                    longPressTimer?.Dispose();
                }
            };
            container.GestureRecognizers.Add(panGesture);
        }

        return container;
    }

    private void BuildFooter()
    {
        if (!ShowFooter)
            return;

        footerGrid.Children.Clear();
        footerGrid.ColumnDefinitions.Clear();
        frozenFooterGrid.Children.Clear();
        frozenFooterGrid.ColumnDefinitions.Clear();

        var frozenColumns = GetFrozenColumns();
        var scrollableColumns = GetScrollableColumns();

        // Build frozen footer
        for (int i = 0; i < frozenColumns.Count; i++)
        {
            var column = frozenColumns[i];
            var width = column.Width < 0 ? GridLength.Auto : new GridLength(column.Width);
            frozenFooterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

            var footerCell = CreateFooterCell(column);
            frozenFooterGrid.Children.Add(footerCell);
            Grid.SetColumn(footerCell, i);
        }

        // Build scrollable footer
        for (int i = 0; i < scrollableColumns.Count; i++)
        {
            var column = scrollableColumns[i];
            var width = column.Width < 0 ? GridLength.Auto : new GridLength(column.Width);
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

            var footerCell = CreateFooterCell(column);
            footerGrid.Children.Add(footerCell);
            Grid.SetColumn(footerCell, i);
        }
    }

    private View CreateFooterCell(DataGridColumn column)
    {
        var aggregate = column.CalculateAggregate(_sortedItems);
        var displayText = column.FormatAggregateValue(aggregate);

        return new Label
        {
            Text = displayText,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = column.TextAlignment,
            Padding = new Thickness(8, 4)
        };
    }

    #endregion

    #region Private Methods - Filtering

    private void ApplyFilters()
    {
        _filteredItems.Clear();

        if (ItemsSource == null)
            return;

        var items = ItemsSource.Cast<object>().ToList();

        // Apply search text filter
        if (!string.IsNullOrEmpty(SearchText))
        {
            var visibleColumns = GetVisibleColumns();
            items = items.Where(item =>
            {
                foreach (var column in visibleColumns)
                {
                    var value = column.GetCellValue(item)?.ToString();
                    if (value != null && value.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }).ToList();
        }

        // Apply column filters
        foreach (var filter in _activeFilters.Values.Where(f => f.IsActive))
        {
            items = items.Where(item =>
            {
                var value = filter.Column.GetCellValue(item);

                // Check selected values
                if (filter.SelectedValues.Count > 0)
                {
                    if (!filter.SelectedValues.Contains(value!))
                        return false;
                }

                // Check search text
                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    var strValue = value?.ToString() ?? string.Empty;
                    if (!strValue.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                return true;
            }).ToList();
        }

        _filteredItems.AddRange(items);
    }

    private void ShowFilterPopup(DataGridColumn column)
    {
        var distinctValues = GetDistinctValues(column);

        IEnumerable<object>? currentSelection = null;
        if (_activeFilters.TryGetValue(column, out var existingFilter))
        {
            currentSelection = existingFilter.SelectedValues;
        }

        filterPopup.Column = column;
        filterPopup.SetValues(distinctValues, currentSelection);
        filterPopupOverlay.IsVisible = true;
    }

    private void HideFilterPopup()
    {
        filterPopupOverlay.IsVisible = false;
    }

    #endregion

    #region Private Methods - Sorting

    private void ApplySort()
    {
        _sortedItems.Clear();

        if (_filteredItems.Count == 0)
            return;

        var items = _filteredItems.ToList();

        if (AllowMultiColumnSort && _sortDescriptions.Count > 0)
        {
            // Multi-column sort
            IOrderedEnumerable<object>? ordered = null;

            foreach (var desc in _sortDescriptions)
            {
                if (ordered == null)
                {
                    ordered = desc.Direction == SortDirection.Ascending
                        ? items.OrderBy(i => desc.Column.GetCellValue(i))
                        : items.OrderByDescending(i => desc.Column.GetCellValue(i));
                }
                else
                {
                    ordered = desc.Direction == SortDirection.Ascending
                        ? ordered.ThenBy(i => desc.Column.GetCellValue(i))
                        : ordered.ThenByDescending(i => desc.Column.GetCellValue(i));
                }
            }

            items = ordered?.ToList() ?? items;
        }
        else if (_currentSortColumn != null && _currentSortColumn.SortDirection.HasValue)
        {
            // Single column sort
            var propertyPath = _currentSortColumn.PropertyPath;
            if (!string.IsNullOrEmpty(propertyPath))
            {
                items = _currentSortColumn.SortDirection == SortDirection.Ascending
                    ? items.OrderBy(i => _currentSortColumn.GetCellValue(i)).ToList()
                    : items.OrderByDescending(i => _currentSortColumn.GetCellValue(i)).ToList();
            }
        }

        _sortedItems.AddRange(items);
    }

    private void ToggleSort(DataGridColumn column)
    {
        // Fire sorting event
        var sortingArgs = new DataGridSortingEventArgs(column);
        Sorting?.Invoke(this, sortingArgs);

        if (SortingCommand?.CanExecute(sortingArgs) == true)
        {
            SortingCommand.Execute(sortingArgs);
        }

        if (sortingArgs.Cancel)
            return;

        // Clear previous sort
        if (_currentSortColumn != null && _currentSortColumn != column)
        {
            _currentSortColumn.SortDirection = null;
        }

        // Toggle sort direction
        column.SortDirection = column.SortDirection switch
        {
            null => SortDirection.Ascending,
            SortDirection.Ascending => SortDirection.Descending,
            SortDirection.Descending => null,
            _ => null
        };

        _currentSortColumn = column.SortDirection.HasValue ? column : null;

        // Apply sort and rebuild
        ApplySort();
        BuildDataRows();

        // Fire sorted event
        Sorted?.Invoke(this, column);

        if (SortedCommand?.CanExecute(column) == true)
        {
            SortedCommand.Execute(column);
        }
    }

    #endregion

    #region Private Methods - Selection

    private void SelectItem(object item)
    {
        if (SelectionMode == DataGridSelectionMode.None)
            return;

        // Track previously selected items for visual update
        var previouslySelected = _selectedItems.ToList();

        if (SelectionMode == DataGridSelectionMode.Single)
        {
            _selectedItems.Clear();
            _selectedItems.Add(item);
            SelectedItem = item;
        }
        else if (SelectionMode == DataGridSelectionMode.Multiple)
        {
            if (_selectedItems.Contains(item))
            {
                _selectedItems.Remove(item);
            }
            else
            {
                _selectedItems.Add(item);
            }
            SelectedItem = _selectedItems.FirstOrDefault();
        }

        // Update focused row
        _focusedRowIndex = _sortedItems.IndexOf(item);

        // Update only the affected rows instead of rebuilding all
        UpdateSelectionVisualState(previouslySelected);
        RaiseSelectionChanged();
    }

    private void UpdateSelectionVisualState(List<object> previouslySelected)
    {
        var displayItems = GetDisplayItems();
        var frozenColumns = GetFrozenColumns();
        var scrollableColumns = GetScrollableColumns();
        var totalColumns = frozenColumns.Count + scrollableColumns.Count;

        // Find items that changed selection state
        var nowSelected = _selectedItems.ToHashSet();
        var wasSelected = previouslySelected.ToHashSet();

        // Items that need visual update: newly selected + newly deselected
        var itemsToUpdate = new HashSet<object>();
        foreach (var it in nowSelected)
        {
            if (!wasSelected.Contains(it))
                itemsToUpdate.Add(it);
        }
        foreach (var it in wasSelected)
        {
            if (!nowSelected.Contains(it))
                itemsToUpdate.Add(it);
        }

        // Update the visual state for each affected row
        foreach (var itemToUpdate in itemsToUpdate)
        {
            var rowIndex = _sortedItems.IndexOf(itemToUpdate);
            var displayIndex = displayItems.IndexOf(itemToUpdate);
            if (displayIndex < 0)
                continue;

            var isSelected = _selectedItems.Contains(itemToUpdate);
            var isAlternate = displayIndex % 2 == 1;

            // Update frozen cells
            foreach (var child in frozenDataGrid.Children)
            {
                if (child is Grid cellGrid && Grid.GetRow(cellGrid) == displayIndex)
                {
                    var colIndex = Grid.GetColumn(cellGrid);
                    UpdateCellBackground(cellGrid, rowIndex, colIndex, isSelected, isAlternate);
                }
            }

            // Update scrollable cells
            foreach (var child in dataGrid.Children)
            {
                if (child is Grid cellGrid && Grid.GetRow(cellGrid) == displayIndex)
                {
                    var colIndex = Grid.GetColumn(cellGrid) + frozenColumns.Count;
                    UpdateCellBackground(cellGrid, rowIndex, colIndex, isSelected, isAlternate);
                }
            }
        }
    }

    private void UpdateCellBackground(Grid cellGrid, int rowIndex, int colIndex, bool isSelected, bool isAlternate)
    {
        Color bgColor;
        var isFocused = rowIndex == _focusedRowIndex && colIndex == _focusedColumnIndex;

        if (isFocused)
        {
            bgColor = FocusedCellColor;
        }
        else if (isSelected)
        {
            bgColor = SelectedRowColor;
        }
        else if (isAlternate && AlternatingRowColor != null)
        {
            bgColor = AlternatingRowColor;
        }
        else
        {
            bgColor = Colors.Transparent;
        }

        cellGrid.BackgroundColor = bgColor;

        // Update text color for contrast
        foreach (var child in cellGrid.Children)
        {
            if (child is Label label)
            {
                label.TextColor = (isSelected || isFocused) ? SelectedTextColor : GetDefaultTextColor();
            }
        }
    }

    private Color GetDefaultTextColor()
    {
        // Return appropriate text color based on current theme
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Colors.White
            : Colors.Black;
    }

    private void UpdateEmptyView()
    {
        var hasData = _sortedItems.Count > 0;
        emptyViewContainer.IsVisible = !hasData && EmptyView != null;
        dataScrollView.IsVisible = hasData;
    }

    #endregion

    #region Private Methods - Editing

    private void BeginEditInternal(object item, DataGridColumn column, int rowIndex, int colIndex)
    {
        // Fire starting event
        var startingArgs = new DataGridCellEditEventArgs(item, column, rowIndex, colIndex, column.GetCellValue(item));
        CellEditStarting?.Invoke(this, startingArgs);

        if (startingArgs.Cancel)
            return;

        _editingItem = item;
        _editingColumn = column;
        _editingRowIndex = rowIndex;
        _editingColumnIndex = colIndex;
        _originalEditValue = column.GetCellValue(item);

        // Create edit control
        _currentEditControl = column.CreateEditContent(item);
        if (_currentEditControl == null)
            return;

        // Replace cell content with edit control
        var cellContainer = FindCellContainer(rowIndex, colIndex);
        if (cellContainer != null && cellContainer.Children.Count > 0)
        {
            // Store original content for restoration on cancel
            _originalCellContent = cellContainer.Children[0] as View;
            cellContainer.Children.Clear();
            cellContainer.Children.Add(_currentEditControl);

            // Focus the edit control
            if (_currentEditControl is VisualElement ve)
            {
                // Delay focus to ensure the control is fully loaded
                Dispatcher.Dispatch(() => ve.Focus());
            }

            // Wire up commit/cancel for Entry controls
            if (_currentEditControl is Entry entry)
            {
                entry.Completed += OnEditEntryCompleted;
                entry.Unfocused += OnEditControlUnfocused;
            }
            else if (_currentEditControl is CheckBox checkBox)
            {
                checkBox.CheckedChanged += OnEditCheckBoxChanged;
            }
        }

        // Track that this column was edited
        if (!_editedColumnsInRow.Contains(column))
        {
            _editedColumnsInRow.Add(column);
        }

        // Fire started event
        var startedArgs = new DataGridCellEditEventArgs(item, column, rowIndex, colIndex, _originalEditValue);
        CellEditStarted?.Invoke(this, startedArgs);

        if (CellEditStartedCommand?.CanExecute(startedArgs) == true)
        {
            CellEditStartedCommand.Execute(startedArgs);
        }
    }

    private void CommitEditInternal()
    {
        if (_editingItem == null || _editingColumn == null || _currentEditControl == null)
            return;

        // Get new value from edit control
        object? newValue = GetValueFromEditControl(_currentEditControl, _editingColumn);

        // Validate
        if (ValidateOnCellEdit)
        {
            var validationArgs = new DataGridCellValidationEventArgs(_editingItem, _editingColumn, _originalEditValue, newValue);
            CellValidating?.Invoke(this, validationArgs);

            var columnValidation = _editingColumn.Validate(_editingItem, newValue);
            if (!columnValidation.IsValid)
            {
                validationArgs.IsValid = false;
                validationArgs.ErrorMessage = columnValidation.FirstError;
            }

            if (!validationArgs.IsValid)
            {
                _validationErrors[(_editingRowIndex, _editingColumnIndex)] = ValidationResult.Failure(validationArgs.ErrorMessage ?? "Validation failed");
                BuildDataRows();
                return;
            }
            else
            {
                _validationErrors.Remove((_editingRowIndex, _editingColumnIndex));
            }
        }

        // Fire ending event
        var endingArgs = new DataGridCellEditEventArgs(_editingItem, _editingColumn, _editingRowIndex, _editingColumnIndex, _originalEditValue, newValue);
        CellEditEnding?.Invoke(this, endingArgs);

        if (endingArgs.Cancel)
            return;

        // Apply value
        _editingColumn.SetCellValue(_editingItem, newValue);

        // Push to undo stack if value changed
        if (!Equals(_originalEditValue, newValue))
        {
            var undoOp = new CellEditOperation(this, _editingItem, _editingColumn, _editingRowIndex, _editingColumnIndex, _originalEditValue, newValue);
            PushUndoOperation(undoOp);
        }

        // Fire ended event
        var endedArgs = new DataGridCellEditEventArgs(_editingItem, _editingColumn, _editingRowIndex, _editingColumnIndex, _originalEditValue, newValue);
        CellEditEnded?.Invoke(this, endedArgs);

        if (CellEditEndedCommand?.CanExecute(endedArgs) == true)
        {
            CellEditEndedCommand.Execute(endedArgs);
        }

        EndEdit();
    }

    private void CancelEditInternal()
    {
        EndEdit();
    }

    private void EndEdit()
    {
        // Unhook event handlers
        if (_currentEditControl is Entry entry)
        {
            entry.Completed -= OnEditEntryCompleted;
            entry.Unfocused -= OnEditControlUnfocused;
        }
        else if (_currentEditControl is CheckBox checkBox)
        {
            checkBox.CheckedChanged -= OnEditCheckBoxChanged;
        }

        // Fire RowEditEnded if we have edited columns in this row
        if (_editingItem != null && _editedColumnsInRow.Count > 0)
        {
            var rowArgs = new DataGridRowEditEventArgs(_editingItem, _editingRowIndex, _editedColumnsInRow.ToList());
            RowEditEnded?.Invoke(this, rowArgs);

            if (RowEditEndedCommand?.CanExecute(rowArgs) == true)
            {
                RowEditEndedCommand.Execute(rowArgs);
            }

            _editedColumnsInRow.Clear();
        }

        _editingItem = null;
        _editingColumn = null;
        _editingRowIndex = -1;
        _editingColumnIndex = -1;
        _currentEditControl = null;
        _originalEditValue = null;
        _originalCellContent = null;

        BuildDataRows();
    }

    private static object? GetValueFromEditControl(View control, DataGridColumn column)
    {
        return control switch
        {
            Entry entry => entry.Text,
            CheckBox checkBox => checkBox.IsChecked,
            Picker picker when column is DataGridComboBoxColumn comboColumn => comboColumn.GetValueFromEditControl(control),
            _ => null
        };
    }

    private Grid? FindCellContainer(int rowIndex, int colIndex)
    {
        var frozenColumns = GetFrozenColumns();
        var isFrozen = colIndex < frozenColumns.Count;
        var grid = isFrozen ? frozenDataGrid : dataGrid;
        var adjustedColIndex = isFrozen ? colIndex : colIndex - frozenColumns.Count;

        // Find the cell directly by iterating through the display rows
        var displayItems = GetDisplayItems();
        var targetItem = rowIndex >= 0 && rowIndex < _sortedItems.Count ? _sortedItems[rowIndex] : null;
        if (targetItem == null)
            return null;

        var displayIndex = displayItems.IndexOf(targetItem);
        if (displayIndex < 0)
            return null;

        foreach (var child in grid.Children)
        {
            if (child is Grid cellGrid &&
                Grid.GetRow(cellGrid) == displayIndex &&
                Grid.GetColumn(cellGrid) == adjustedColIndex)
            {
                return cellGrid;
            }
        }
        return null;
    }

    private void OnEditEntryCompleted(object? sender, EventArgs e)
    {
        CommitEdit();
        MoveFocusDown();
    }

    private void OnEditControlUnfocused(object? sender, FocusEventArgs e)
    {
        // Small delay to allow button clicks to register
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
        {
            if (_editingItem != null)
            {
                CommitEdit();
            }
        });
    }

    private void OnEditCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        // Commit immediately for checkbox changes
        CommitEdit();
    }

    /// <summary>
    /// Moves focus up by one row.
    /// </summary>
    public void MoveFocusUp() => MoveFocus(-1, 0);

    /// <summary>
    /// Moves focus down by one row.
    /// </summary>
    public void MoveFocusDown() => MoveFocus(1, 0);

    /// <summary>
    /// Moves focus left by one column.
    /// </summary>
    public void MoveFocusLeft() => MoveFocus(0, -1);

    /// <summary>
    /// Moves focus right by one column.
    /// </summary>
    public void MoveFocusRight() => MoveFocus(0, 1);

    private void MoveFocus(int rowDelta, int colDelta)
    {
        if (_sortedItems.Count == 0)
            return;

        var visibleCols = GetVisibleColumns();
        if (visibleCols.Count == 0)
            return;

        // Initialize focus if not set
        if (_focusedRowIndex < 0)
            _focusedRowIndex = 0;
        if (_focusedColumnIndex < 0)
            _focusedColumnIndex = 0;

        var newRow = Math.Clamp(_focusedRowIndex + rowDelta, 0, _sortedItems.Count - 1);
        var newCol = Math.Clamp(_focusedColumnIndex + colDelta, 0, visibleCols.Count - 1);

        if (newRow != _focusedRowIndex || newCol != _focusedColumnIndex)
        {
            var oldRow = _focusedRowIndex;
            var oldCol = _focusedColumnIndex;
            _focusedRowIndex = newRow;
            _focusedColumnIndex = newCol;

            if (rowDelta != 0)
                SelectItem(_sortedItems[newRow]);

            // Update focus visual for old and new cells
            UpdateFocusVisualState(oldRow, oldCol, newRow, newCol);
            ScrollToFocusedCell();
        }
    }

    private void UpdateFocusVisualState(int oldRow, int oldCol, int newRow, int newCol)
    {
        var displayItems = GetDisplayItems();
        var frozenColumns = GetFrozenColumns();

        // Update old focused cell
        if (oldRow >= 0 && oldCol >= 0)
        {
            var oldItem = oldRow < _sortedItems.Count ? _sortedItems[oldRow] : null;
            if (oldItem != null)
            {
                var oldDisplayIndex = displayItems.IndexOf(oldItem);
                if (oldDisplayIndex >= 0)
                {
                    var isSelected = _selectedItems.Contains(oldItem);
                    var isAlternate = oldDisplayIndex % 2 == 1;
                    var cellGrid = FindCellInGrid(oldDisplayIndex, oldCol, frozenColumns.Count);
                    if (cellGrid != null)
                        UpdateCellBackground(cellGrid, oldRow, oldCol, isSelected, isAlternate);
                }
            }
        }

        // Update new focused cell
        if (newRow >= 0 && newCol >= 0)
        {
            var newItem = newRow < _sortedItems.Count ? _sortedItems[newRow] : null;
            if (newItem != null)
            {
                var newDisplayIndex = displayItems.IndexOf(newItem);
                if (newDisplayIndex >= 0)
                {
                    var isSelected = _selectedItems.Contains(newItem);
                    var isAlternate = newDisplayIndex % 2 == 1;
                    var cellGrid = FindCellInGrid(newDisplayIndex, newCol, frozenColumns.Count);
                    if (cellGrid != null)
                        UpdateCellBackground(cellGrid, newRow, newCol, isSelected, isAlternate);
                }
            }
        }
    }

    private Grid? FindCellInGrid(int displayIndex, int colIndex, int frozenColCount)
    {
        var isFrozen = colIndex < frozenColCount;
        var grid = isFrozen ? frozenDataGrid : dataGrid;
        var adjustedColIndex = isFrozen ? colIndex : colIndex - frozenColCount;

        foreach (var child in grid.Children)
        {
            if (child is Grid cellGrid &&
                Grid.GetRow(cellGrid) == displayIndex &&
                Grid.GetColumn(cellGrid) == adjustedColIndex)
            {
                return cellGrid;
            }
        }
        return null;
    }

    /// <summary>
    /// Moves focus to the next editable cell (Tab navigation).
    /// </summary>
    public void MoveFocusToNextEditableCell()
    {
        var visibleCols = GetVisibleColumns();
        if (visibleCols.Count == 0 || _sortedItems.Count == 0)
            return;

        // Initialize if needed
        if (_focusedRowIndex < 0)
            _focusedRowIndex = 0;
        if (_focusedColumnIndex < 0)
            _focusedColumnIndex = -1;

        var oldRow = _focusedRowIndex;
        var oldCol = _focusedColumnIndex;

        // Find next editable column in current row
        for (int c = _focusedColumnIndex + 1; c < visibleCols.Count; c++)
        {
            if (visibleCols[c].CanUserEdit && !visibleCols[c].IsReadOnly)
            {
                _focusedColumnIndex = c;
                UpdateFocusVisualState(oldRow, oldCol, _focusedRowIndex, _focusedColumnIndex);
                BeginEdit(_sortedItems[_focusedRowIndex], visibleCols[c]);
                return;
            }
        }

        // Move to next row, first editable column
        if (_focusedRowIndex < _sortedItems.Count - 1)
        {
            _focusedRowIndex++;
            SelectItem(_sortedItems[_focusedRowIndex]);

            for (int c = 0; c < visibleCols.Count; c++)
            {
                if (visibleCols[c].CanUserEdit && !visibleCols[c].IsReadOnly)
                {
                    _focusedColumnIndex = c;
                    UpdateFocusVisualState(oldRow, oldCol, _focusedRowIndex, _focusedColumnIndex);
                    BeginEdit(_sortedItems[_focusedRowIndex], visibleCols[c]);
                    return;
                }
            }
        }

        // No more editable cells - just update focus visual
        UpdateFocusVisualState(oldRow, oldCol, _focusedRowIndex, _focusedColumnIndex);
    }

    /// <summary>
    /// Moves focus to the previous editable cell (Shift+Tab navigation).
    /// </summary>
    public void MoveFocusToPreviousEditableCell()
    {
        var visibleCols = GetVisibleColumns();
        if (visibleCols.Count == 0 || _sortedItems.Count == 0)
            return;

        // Initialize if needed
        if (_focusedRowIndex < 0)
            _focusedRowIndex = 0;
        if (_focusedColumnIndex < 0)
            _focusedColumnIndex = visibleCols.Count;

        var oldRow = _focusedRowIndex;
        var oldCol = _focusedColumnIndex;

        // Find previous editable column in current row
        for (int c = _focusedColumnIndex - 1; c >= 0; c--)
        {
            if (visibleCols[c].CanUserEdit && !visibleCols[c].IsReadOnly)
            {
                _focusedColumnIndex = c;
                UpdateFocusVisualState(oldRow, oldCol, _focusedRowIndex, _focusedColumnIndex);
                BeginEdit(_sortedItems[_focusedRowIndex], visibleCols[c]);
                return;
            }
        }

        // Move to previous row, last editable column
        if (_focusedRowIndex > 0)
        {
            _focusedRowIndex--;
            SelectItem(_sortedItems[_focusedRowIndex]);

            for (int c = visibleCols.Count - 1; c >= 0; c--)
            {
                if (visibleCols[c].CanUserEdit && !visibleCols[c].IsReadOnly)
                {
                    _focusedColumnIndex = c;
                    UpdateFocusVisualState(oldRow, oldCol, _focusedRowIndex, _focusedColumnIndex);
                    BeginEdit(_sortedItems[_focusedRowIndex], visibleCols[c]);
                    return;
                }
            }
        }

        // No more editable cells - just update focus visual
        UpdateFocusVisualState(oldRow, oldCol, _focusedRowIndex, _focusedColumnIndex);
    }

    private void ScrollToFocusedCell()
    {
        if (_focusedRowIndex < 0)
            return;

        // Scroll vertically to show focused row
        var targetScrollY = _focusedRowIndex * RowHeight;
        var viewportHeight = dataScrollView.Height;

        if (targetScrollY < dataScrollView.ScrollY)
        {
            dataScrollView.ScrollToAsync(dataScrollView.ScrollX, targetScrollY, true);
        }
        else if (targetScrollY + RowHeight > dataScrollView.ScrollY + viewportHeight)
        {
            dataScrollView.ScrollToAsync(dataScrollView.ScrollX, targetScrollY + RowHeight - viewportHeight, true);
        }
    }

    private void ApplySearchHighlighting(Label label, string searchText)
    {
        var text = label.Text ?? string.Empty;
        var searchLower = searchText.ToLowerInvariant();
        var textLower = text.ToLowerInvariant();

        var matchIndex = textLower.IndexOf(searchLower);
        if (matchIndex < 0)
            return;

        var formattedString = new FormattedString();

        // Text before match
        if (matchIndex > 0)
        {
            formattedString.Spans.Add(new Span { Text = text[..matchIndex] });
        }

        // Highlighted match
        formattedString.Spans.Add(new Span
        {
            Text = text.Substring(matchIndex, searchText.Length),
            BackgroundColor = SearchHighlightColor
        });

        // Text after match (continue highlighting other matches)
        var afterIndex = matchIndex + searchText.Length;
        if (afterIndex < text.Length)
        {
            var remainingText = text[afterIndex..];
            var remainingLower = remainingText.ToLowerInvariant();
            var nextMatch = remainingLower.IndexOf(searchLower);

            if (nextMatch >= 0)
            {
                // Recursively process remaining text for multiple highlights
                var remainingLabel = new Label { Text = remainingText };
                ApplySearchHighlighting(remainingLabel, searchText);

                if (remainingLabel.FormattedText != null)
                {
                    foreach (var span in remainingLabel.FormattedText.Spans)
                    {
                        formattedString.Spans.Add(new Span
                        {
                            Text = span.Text,
                            BackgroundColor = span.BackgroundColor
                        });
                    }
                }
                else
                {
                    formattedString.Spans.Add(new Span { Text = remainingText });
                }
            }
            else
            {
                formattedString.Spans.Add(new Span { Text = remainingText });
            }
        }

        label.FormattedText = formattedString;
        label.Text = null;
    }

    #endregion

    #region Private Methods - Export

    private IEnumerable<object> GetExportItems(DataGridExportOptions options)
    {
        if (options.SelectedRowsOnly)
            return _selectedItems;

        return _sortedItems;
    }

    private IEnumerable<DataGridColumn> GetExportColumns(DataGridExportOptions options)
    {
        if (options.VisibleColumnsOnly)
            return GetVisibleColumns();

        return _columns;
    }

    #endregion

    #region Private Methods - Pagination

    private void UpdatePaginationUI()
    {
        OnPropertyChanged(nameof(PageInfoText));
        OnPropertyChanged(nameof(CurrentPageText));
        OnPropertyChanged(nameof(TotalPages));

        firstPageButton.IsEnabled = CurrentPage > 1;
        prevPageButton.IsEnabled = CurrentPage > 1;
        nextPageButton.IsEnabled = CurrentPage < TotalPages;
        lastPageButton.IsEnabled = CurrentPage < TotalPages;
    }

    private void RaisePageChanged(int oldPage, int newPage)
    {
        var args = new DataGridPageChangedEventArgs(oldPage, newPage, PageSize, TotalItems > 0 ? TotalItems : _filteredItems.Count);
        PageChanged?.Invoke(this, args);

        if (PageChangedCommand?.CanExecute(args) == true)
        {
            PageChangedCommand.Execute(args);
        }
    }

    #endregion

    #region Event Handlers

    private void OnHeaderTapped(DataGridColumn column)
    {
        ToggleSort(column);
    }

    private void OnColumnResizePan(DataGridColumn column, int columnIndex, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _resizingColumnIndex = columnIndex;
                _resizeStartWidth = column.ActualWidth > 0 ? column.ActualWidth : column.Width;
                if (_resizeStartWidth < 0) _resizeStartWidth = 100; // Default for auto

                var args = new DataGridColumnEventArgs(column, columnIndex, _resizeStartWidth, _resizeStartWidth);
                ColumnResizing?.Invoke(this, args);
                break;

            case GestureStatus.Running:
                var newWidth = Math.Clamp(_resizeStartWidth + e.TotalX, column.MinWidth, column.MaxWidth);
                column.Width = newWidth;
                column.ActualWidth = newWidth;
                UpdateColumnWidth(columnIndex, newWidth);
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                var endArgs = new DataGridColumnEventArgs(column, columnIndex, _resizeStartWidth, column.Width);
                ColumnResized?.Invoke(this, endArgs);
                _resizingColumnIndex = -1;
                break;
        }
    }

    private void UpdateColumnWidth(int columnIndex, double newWidth)
    {
        var frozenCount = GetFrozenColumns().Count;
        var isFrozen = columnIndex < frozenCount;
        var adjustedIndex = isFrozen ? columnIndex : columnIndex - frozenCount;

        // Update header grid
        var headerDefs = isFrozen ? frozenHeaderGrid.ColumnDefinitions : headerGrid.ColumnDefinitions;
        if (adjustedIndex < headerDefs.Count)
            headerDefs[adjustedIndex].Width = new GridLength(newWidth);

        // Update data grid
        var dataDefs = isFrozen ? frozenDataGrid.ColumnDefinitions : dataGrid.ColumnDefinitions;
        if (adjustedIndex < dataDefs.Count)
            dataDefs[adjustedIndex].Width = new GridLength(newWidth);

        // Update footer grid if visible
        if (ShowFooter)
        {
            var footerDefs = isFrozen ? frozenFooterGrid.ColumnDefinitions : footerGrid.ColumnDefinitions;
            if (adjustedIndex < footerDefs.Count)
                footerDefs[adjustedIndex].Width = new GridLength(newWidth);
        }
    }

    private void OnColumnDragStarting(DataGridColumn column, int columnIndex, DragStartingEventArgs e)
    {
        _draggingColumnIndex = columnIndex;
        e.Data.Text = columnIndex.ToString();

        var args = new DataGridColumnReorderEventArgs(column, columnIndex, columnIndex);
        ColumnReordering?.Invoke(this, args);
        if (args.Cancel)
        {
            e.Cancel = true;
            _draggingColumnIndex = -1;
        }
    }

    private void OnColumnDragOver(int targetIndex, DragEventArgs e)
    {
        if (_draggingColumnIndex < 0 || _draggingColumnIndex == targetIndex)
        {
            e.AcceptedOperation = DataPackageOperation.None;
            return;
        }
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void OnColumnDrop(int targetIndex, DropEventArgs e)
    {
        if (_draggingColumnIndex < 0 || _draggingColumnIndex == targetIndex)
        {
            _draggingColumnIndex = -1;
            return;
        }

        var column = _columns[_draggingColumnIndex];
        var args = new DataGridColumnReorderEventArgs(column, _draggingColumnIndex, targetIndex);
        ColumnReordering?.Invoke(this, args);

        if (!args.Cancel)
        {
            _columns.Move(_draggingColumnIndex, targetIndex);
            ColumnReordered?.Invoke(this, args);
            BuildGrid();
        }

        _draggingColumnIndex = -1;
    }

    private void OnRowDragStarting(object item, int rowIndex, DragStartingEventArgs e)
    {
        _draggingRowIndex = rowIndex;
        e.Data.Properties["RowIndex"] = rowIndex;
        e.Data.Properties["Item"] = item;

        var args = new DataGridRowReorderEventArgs(item, rowIndex, rowIndex);
        RowReordering?.Invoke(this, args);
        if (args.Cancel)
        {
            e.Cancel = true;
            _draggingRowIndex = -1;
        }
    }

    private void OnRowDragOver(int targetIndex, DragEventArgs e)
    {
        if (_draggingRowIndex < 0 || _draggingRowIndex == targetIndex)
        {
            e.AcceptedOperation = DataPackageOperation.None;
            return;
        }
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void OnRowDrop(int targetIndex, DropEventArgs e)
    {
        if (_draggingRowIndex < 0 || _draggingRowIndex == targetIndex)
        {
            _draggingRowIndex = -1;
            return;
        }

        var item = _sortedItems[_draggingRowIndex];
        var args = new DataGridRowReorderEventArgs(item, _draggingRowIndex, targetIndex);
        RowReordering?.Invoke(this, args);

        if (!args.Cancel)
        {
            // Reorder in source collection if it's an IList
            if (ItemsSource is System.Collections.IList list)
            {
                var sourceItem = list[_draggingRowIndex];
                list.RemoveAt(_draggingRowIndex);
                list.Insert(targetIndex > _draggingRowIndex ? targetIndex : targetIndex, sourceItem);
            }

            RowReordered?.Invoke(this, args);
            BuildGrid();
        }

        _draggingRowIndex = -1;
    }

    private void OnCellTapped(object item, DataGridColumn column, int rowIndex, int colIndex)
    {
        SelectItem(item);
        _focusedColumnIndex = colIndex;

        var args = new DataGridCellEventArgs(item, column, rowIndex, colIndex);
        CellTapped?.Invoke(this, args);

        if (CellTappedCommand?.CanExecute(args) == true)
        {
            CellTappedCommand.Execute(args);
        }

        // Check if we should begin edit
        if (EditTrigger == DataGridEditTrigger.SingleTap && CanUserEdit && column.CanUserEdit && !column.IsReadOnly)
        {
            BeginEditInternal(item, column, rowIndex, colIndex);
        }
    }

    private void OnCellDoubleTapped(object item, DataGridColumn column, int rowIndex, int colIndex)
    {
        var args = new DataGridCellEventArgs(item, column, rowIndex, colIndex);
        CellDoubleTapped?.Invoke(this, args);

        if (CellDoubleTappedCommand?.CanExecute(args) == true)
        {
            CellDoubleTappedCommand.Execute(args);
        }

        // Check if we should begin edit
        if (EditTrigger == DataGridEditTrigger.DoubleTap && CanUserEdit && column.CanUserEdit && !column.IsReadOnly)
        {
            BeginEditInternal(item, column, rowIndex, colIndex);
        }
    }

    private void OnDataScrollViewScrolled(object? sender, ScrolledEventArgs e)
    {
        if (_isSyncingScroll)
            return;

        _isSyncingScroll = true;

        // Sync header scroll with data scroll (horizontal)
        headerScrollView.ScrollToAsync(e.ScrollX, 0, false);

        // Sync footer scroll with data scroll (horizontal)
        if (ShowFooter)
        {
            footerScrollView.ScrollToAsync(e.ScrollX, 0, false);
        }

        // Sync frozen data scroll (vertical only)
        if (HasFrozenColumns)
        {
            frozenDataScrollView.ScrollToAsync(0, e.ScrollY, false);
        }

        // Update virtualization panels if active
        if (EnableVirtualization && _virtualizingPanel != null)
        {
            _virtualizingPanel.UpdateScrollPosition(e.ScrollY, dataScrollView.Height);
            _frozenVirtualizingPanel?.UpdateScrollPosition(e.ScrollY, dataScrollView.Height);
        }

        _isSyncingScroll = false;
    }

    private void OnFrozenDataScrollViewScrolled(object? sender, ScrolledEventArgs e)
    {
        if (_isSyncingScroll)
            return;

        _isSyncingScroll = true;

        // Sync main data scroll (vertical)
        dataScrollView.ScrollToAsync(dataScrollView.ScrollX, e.ScrollY, false);

        _isSyncingScroll = false;
    }

    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BuildGrid();
    }

    private void OnFilterOverlayTapped(object? sender, EventArgs e)
    {
        HideFilterPopup();
    }

    private void OnFilterPopupApplied(object? sender, FilterAppliedEventArgs e)
    {
        if (e.Column == null)
            return;

        // Fire filtering event
        var filteringArgs = new DataGridFilterEventArgs(e.Column, e.SelectedValues, e.SearchText);
        Filtering?.Invoke(this, filteringArgs);

        if (FilteringCommand?.CanExecute(filteringArgs) == true)
        {
            FilteringCommand.Execute(filteringArgs);
        }

        if (filteringArgs.Cancel)
        {
            HideFilterPopup();
            return;
        }

        // Apply filter
        if (e.SelectedValues.Count == 0 && string.IsNullOrEmpty(e.SearchText))
        {
            // Clear filter
            _activeFilters.Remove(e.Column);
            e.Column.FilterValues = null;
            e.Column.FilterText = null;
        }
        else
        {
            // Set filter
            var filter = new DataGridColumnFilter(e.Column)
            {
                SelectedValues = new HashSet<object>(e.SelectedValues),
                SearchText = e.SearchText ?? string.Empty
            };
            _activeFilters[e.Column] = filter;
            e.Column.FilterValues = e.SelectedValues;
            e.Column.FilterText = e.SearchText;
        }

        // Rebuild grid
        ApplyFilters();
        ApplySort();
        CurrentPage = 1; // Reset to first page
        BuildDataRows();
        UpdatePaginationUI();

        HideFilterPopup();

        // Fire filtered event
        var filteredArgs = new DataGridFilterEventArgs(e.Column, e.SelectedValues, e.SearchText);
        Filtered?.Invoke(this, filteredArgs);

        if (FilteredCommand?.CanExecute(filteredArgs) == true)
        {
            FilteredCommand.Execute(filteredArgs);
        }
    }

    private void OnFilterPopupCancelled(object? sender, EventArgs e)
    {
        HideFilterPopup();
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (pageSizePicker.SelectedItem is int newSize)
        {
            PageSize = newSize;
            CurrentPage = 1;
            BuildDataRows();
            UpdatePaginationUI();
        }
    }

    private void OnFirstPageClicked(object? sender, EventArgs e) => FirstPage();
    private void OnPreviousPageClicked(object? sender, EventArgs e) => PreviousPage();
    private void OnNextPageClicked(object? sender, EventArgs e) => NextPage();
    private void OnLastPageClicked(object? sender, EventArgs e) => LastPage();

    #endregion

    #region Property Changed Handlers

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid)
        {
            // Unsubscribe from old collection
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= grid.OnItemsSourceCollectionChanged;
            }

            // Subscribe to new collection
            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += grid.OnItemsSourceCollectionChanged;
            }

            // Clear undo history when data source changes
            grid.ClearUndoHistory();

            grid.BuildGrid();
        }
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BuildGrid();
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid && !grid._isUpdating)
        {
            // Track previously selected for visual update
            var previouslySelected = grid._selectedItems.ToList();

            grid._selectedItems.Clear();
            if (newValue != null)
                grid._selectedItems.Add(newValue);

            // Use targeted visual update instead of full rebuild
            grid.UpdateSelectionVisualState(previouslySelected);
        }
    }

    private static void OnEmptyViewChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid)
        {
            grid.emptyViewContainer.Content = newValue as View;
            grid.UpdateEmptyView();
        }
    }

    private static void OnFrozenColumnCountChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid)
        {
            grid.OnPropertyChanged(nameof(HasFrozenColumns));
            grid.BuildGrid();
        }
    }

    private static void OnPaginationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid && !grid._isUpdating)
        {
            grid.BuildDataRows();
            grid.UpdatePaginationUI();
        }
    }

    private static void OnCanUserSortChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid && !grid._isUpdating)
        {
            grid.BuildHeader();
        }
    }

    private static void OnCanUserFilterChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid && !grid._isUpdating)
        {
            grid.BuildHeader();
        }
    }

    private static void OnCanUserEditChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid && !grid._isUpdating)
        {
            // Editing changes don't require header rebuild, but we should cancel any active edit
            grid.CancelEdit();
        }
    }

    private static void OnSearchTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DataGridView grid)
        {
            grid.ApplyFilters();
            grid.ApplySort();
            grid.CurrentPage = 1;
            grid.BuildDataRows();
            grid.UpdatePaginationUI();
        }
    }

    #endregion

    #region Event Raising Methods

    private void RaiseSelectionChanged()
    {
        SelectionChanged?.Invoke(this, SelectedItem);

        if (SelectionChangedCommand?.CanExecute(SelectedItem) == true)
        {
            SelectionChangedCommand.Execute(SelectedItem);
        }
    }

    #endregion

    #region IKeyboardNavigable Implementation

    private bool _isKeyboardNavigationEnabled = true;
    private static readonly List<Base.KeyboardShortcut> _keyboardShortcuts = new();

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
        typeof(DataGridView));

    /// <summary>
    /// Identifies the LostFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the KeyPressCommand bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(DataGridView));

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

        // Handle editing keys when in edit mode
        if (_editingItem != null)
        {
            return e.Key switch
            {
                "Escape" => HandleEscapeInEdit(),
                "Enter" => HandleEnterInEdit(),
                "Tab" => HandleTabInEdit(e.IsShiftPressed),
                _ => false
            };
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
            "PageUp" => HandlePageUpKey(e),
            "PageDown" => HandlePageDownKey(e),
            "Enter" => HandleEnterKey(e),
            "Space" => HandleSpaceKey(e),
            "Escape" => HandleEscapeKey(e),
            "F2" => HandleF2Key(),
            "Delete" => HandleDeleteKey(e),
            "A" when e.IsPlatformCommandPressed => HandleSelectAllKey(),
            "C" when e.IsPlatformCommandPressed => HandleCopyKey(),
            "X" when e.IsPlatformCommandPressed => HandleCutKey(),
            "V" when e.IsPlatformCommandPressed => HandlePasteKey(),
            "Z" when e.IsPlatformCommandPressed => HandleUndoKey(),
            "Y" when e.IsPlatformCommandPressed => HandleRedoKey(),
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
                new Base.KeyboardShortcut { Key = "Up", Description = "Move to previous row", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Down", Description = "Move to next row", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Left", Description = "Move to previous column", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Right", Description = "Move to next column", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Home", Description = "Move to first column", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "End", Description = "Move to last column", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Home", Modifiers = "Ctrl", Description = "Move to first cell", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "End", Modifiers = "Ctrl", Description = "Move to last cell", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "PageUp", Description = "Move up one page", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "PageDown", Description = "Move down one page", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Enter", Description = "Commit edit and move down", Category = "Editing" },
                new Base.KeyboardShortcut { Key = "Tab", Description = "Commit edit and move right", Category = "Editing" },
                new Base.KeyboardShortcut { Key = "Escape", Description = "Cancel edit", Category = "Editing" },
                new Base.KeyboardShortcut { Key = "F2", Description = "Begin editing", Category = "Editing" },
                new Base.KeyboardShortcut { Key = "Delete", Description = "Delete selected rows", Category = "Editing" },
                new Base.KeyboardShortcut { Key = "Space", Description = "Toggle row selection", Category = "Selection" },
                new Base.KeyboardShortcut { Key = "A", Modifiers = "Ctrl", Description = "Select all", Category = "Selection" },
                new Base.KeyboardShortcut { Key = "C", Modifiers = "Ctrl", Description = "Copy", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "X", Modifiers = "Ctrl", Description = "Cut", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "V", Modifiers = "Ctrl", Description = "Paste", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "Z", Modifiers = "Ctrl", Description = "Undo", Category = "Undo/Redo" },
                new Base.KeyboardShortcut { Key = "Y", Modifiers = "Ctrl", Description = "Redo", Category = "Undo/Redo" },
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

            // If no current cell, select first cell
            if (_focusedRowIndex < 0 && _sortedItems?.Count > 0)
            {
                _focusedRowIndex = 0;
                _focusedColumnIndex = 0;
                UpdateCurrentCellVisual();
            }
        }
        return result;
    }

    #region Keyboard Navigation Handlers

    private bool HandleUpKey(Base.KeyEventArgs e)
    {
        if (_sortedItems == null || _sortedItems.Count == 0) return false;

        if (e.IsShiftPressed && SelectionMode == DataGridSelectionMode.Multiple)
        {
            // Extend selection upward
            if (_focusedRowIndex > 0)
            {
                _focusedRowIndex--;
                ToggleRowSelection(_sortedItems[_focusedRowIndex]!);
                ScrollToRow(_focusedRowIndex);
                UpdateCurrentCellVisual();
            }
        }
        else
        {
            // Simple move up
            if (_focusedRowIndex > 0)
            {
                _focusedRowIndex--;
                if (SelectionMode != DataGridSelectionMode.None)
                {
                    SelectItem(_sortedItems[_focusedRowIndex]);
                }
                ScrollToRow(_focusedRowIndex);
                UpdateCurrentCellVisual();
            }
        }
        return true;
    }

    private bool HandleDownKey(Base.KeyEventArgs e)
    {
        if (_sortedItems == null || _sortedItems.Count == 0) return false;

        if (e.IsShiftPressed && SelectionMode == DataGridSelectionMode.Multiple)
        {
            // Extend selection downward
            if (_focusedRowIndex < _sortedItems.Count - 1)
            {
                _focusedRowIndex++;
                ToggleRowSelection(_sortedItems[_focusedRowIndex]!);
                ScrollToRow(_focusedRowIndex);
                UpdateCurrentCellVisual();
            }
        }
        else
        {
            // Simple move down
            if (_focusedRowIndex < _sortedItems.Count - 1)
            {
                _focusedRowIndex++;
                if (SelectionMode != DataGridSelectionMode.None)
                {
                    SelectItem(_sortedItems[_focusedRowIndex]);
                }
                ScrollToRow(_focusedRowIndex);
                UpdateCurrentCellVisual();
            }
        }
        return true;
    }

    private bool HandleLeftKey(Base.KeyEventArgs e)
    {
        if (_focusedColumnIndex > 0)
        {
            _focusedColumnIndex--;
            UpdateCurrentCellVisual();
        }
        return true;
    }

    private bool HandleRightKey(Base.KeyEventArgs e)
    {
        var visibleColumns = Columns.Where(c => c.IsVisible).ToList();
        if (_focusedColumnIndex < visibleColumns.Count - 1)
        {
            _focusedColumnIndex++;
            UpdateCurrentCellVisual();
        }
        return true;
    }

    private bool HandleHomeKey(Base.KeyEventArgs e)
    {
        if (e.IsPlatformCommandPressed)
        {
            // Go to first cell
            _focusedRowIndex = 0;
            _focusedColumnIndex = 0;
            if (SelectionMode != DataGridSelectionMode.None && _sortedItems?.Count > 0)
            {
                SelectItem(_sortedItems[0]);
            }
            ScrollToRow(0);
        }
        else
        {
            // Go to first column in current row
            _focusedColumnIndex = 0;
        }
        UpdateCurrentCellVisual();
        return true;
    }

    private bool HandleEndKey(Base.KeyEventArgs e)
    {
        var visibleColumns = Columns.Where(c => c.IsVisible).ToList();
        if (e.IsPlatformCommandPressed && _sortedItems != null)
        {
            // Go to last cell
            _focusedRowIndex = _sortedItems.Count - 1;
            _focusedColumnIndex = visibleColumns.Count - 1;
            if (SelectionMode != DataGridSelectionMode.None)
            {
                SelectItem(_sortedItems[_focusedRowIndex]);
            }
            ScrollToRow(_focusedRowIndex);
        }
        else
        {
            // Go to last column in current row
            _focusedColumnIndex = visibleColumns.Count - 1;
        }
        UpdateCurrentCellVisual();
        return true;
    }

    private bool HandlePageUpKey(Base.KeyEventArgs e)
    {
        if (_sortedItems == null || _sortedItems.Count == 0) return false;

        var pageSize = Math.Max(1, (int)(dataScrollView?.Height ?? 400) / (int)RowHeight);
        _focusedRowIndex = Math.Max(0, _focusedRowIndex - pageSize);
        if (SelectionMode != DataGridSelectionMode.None)
        {
            SelectItem(_sortedItems[_focusedRowIndex]);
        }
        ScrollToRow(_focusedRowIndex);
        UpdateCurrentCellVisual();
        return true;
    }

    private bool HandlePageDownKey(Base.KeyEventArgs e)
    {
        if (_sortedItems == null || _sortedItems.Count == 0) return false;

        var pageSize = Math.Max(1, (int)(dataScrollView?.Height ?? 400) / (int)RowHeight);
        _focusedRowIndex = Math.Min(_sortedItems.Count - 1, _focusedRowIndex + pageSize);
        if (SelectionMode != DataGridSelectionMode.None)
        {
            SelectItem(_sortedItems[_focusedRowIndex]);
        }
        ScrollToRow(_focusedRowIndex);
        UpdateCurrentCellVisual();
        return true;
    }

    private bool HandleEnterKey(Base.KeyEventArgs e)
    {
        if (CanUserEdit && _focusedRowIndex >= 0 && _focusedColumnIndex >= 0 && _sortedItems != null)
        {
            var visibleColumns = Columns.Where(c => c.IsVisible).ToList();
            if (_focusedColumnIndex < visibleColumns.Count && _focusedRowIndex < _sortedItems.Count)
            {
                var column = visibleColumns[_focusedColumnIndex];
                var item = _sortedItems[_focusedRowIndex];
                if (!column.IsReadOnly && item != null)
                {
                    BeginEdit(item, column);
                }
            }
        }
        return true;
    }

    private bool HandleSpaceKey(Base.KeyEventArgs e)
    {
        if (SelectionMode == DataGridSelectionMode.Multiple && _sortedItems != null && _focusedRowIndex >= 0)
        {
            ToggleRowSelection(_sortedItems[_focusedRowIndex]!);
        }
        return true;
    }

    private bool HandleEscapeKey(Base.KeyEventArgs e)
    {
        if (_selectedItems.Count > 0)
        {
            ClearSelection();
            return true;
        }
        return false;
    }

    private bool HandleF2Key()
    {
        if (CanUserEdit && _focusedRowIndex >= 0 && _focusedColumnIndex >= 0 && _sortedItems != null)
        {
            var visibleColumns = Columns.Where(c => c.IsVisible).ToList();
            if (_focusedColumnIndex < visibleColumns.Count && _focusedRowIndex < _sortedItems.Count)
            {
                var column = visibleColumns[_focusedColumnIndex];
                var item = _sortedItems[_focusedRowIndex];
                if (!column.IsReadOnly && item != null)
                {
                    BeginEdit(item, column);
                    return true;
                }
            }
        }
        return false;
    }

    private bool HandleDeleteKey(Base.KeyEventArgs e)
    {
        if (CanUserEdit && _selectedItems.Count > 0)
        {
            DeleteSelectedRows();
            return true;
        }
        return false;
    }

    private bool HandleSelectAllKey()
    {
        if (SelectionMode == DataGridSelectionMode.Multiple)
        {
            SelectAll();
            return true;
        }
        return false;
    }

    private bool HandleCopyKey()
    {
        if (CanCopy)
        {
            Copy();
            return true;
        }
        return false;
    }

    private bool HandleCutKey()
    {
        if (CanCut)
        {
            Cut();
            return true;
        }
        return false;
    }

    private bool HandlePasteKey()
    {
        if (CanPaste)
        {
            Paste();
            return true;
        }
        return false;
    }

    private bool HandleUndoKey()
    {
        if (CanUndo)
        {
            Undo();
            return true;
        }
        return false;
    }

    private bool HandleRedoKey()
    {
        if (CanRedo)
        {
            Redo();
            return true;
        }
        return false;
    }

    #endregion

    #region Edit Mode Key Handlers

    private bool HandleEscapeInEdit()
    {
        CancelEdit();
        return true;
    }

    private bool HandleEnterInEdit()
    {
        EndEdit();
        // Move to next row
        if (_sortedItems != null && _focusedRowIndex < _sortedItems.Count - 1)
        {
            _focusedRowIndex++;
            ScrollToRow(_focusedRowIndex);
            UpdateCurrentCellVisual();
        }
        return true;
    }

    private bool HandleTabInEdit(bool shiftPressed)
    {
        EndEdit();
        var visibleColumns = Columns.Where(c => c.IsVisible).ToList();

        if (shiftPressed)
        {
            // Move left/up
            if (_focusedColumnIndex > 0)
            {
                _focusedColumnIndex--;
            }
            else if (_focusedRowIndex > 0)
            {
                _focusedRowIndex--;
                _focusedColumnIndex = visibleColumns.Count - 1;
            }
        }
        else
        {
            // Move right/down
            if (_focusedColumnIndex < visibleColumns.Count - 1)
            {
                _focusedColumnIndex++;
            }
            else if (_sortedItems != null && _focusedRowIndex < _sortedItems.Count - 1)
            {
                _focusedRowIndex++;
                _focusedColumnIndex = 0;
            }
        }

        ScrollToRow(_focusedRowIndex);
        UpdateCurrentCellVisual();

        // Start editing next cell if editable
        if (CanUserEdit && _sortedItems != null && _focusedColumnIndex < visibleColumns.Count && _focusedRowIndex < _sortedItems.Count)
        {
            var column = visibleColumns[_focusedColumnIndex];
            var item = _sortedItems[_focusedRowIndex];
            if (!column.IsReadOnly && item != null)
            {
                BeginEdit(item, column);
            }
        }

        return true;
    }

    #endregion

    #region Visual Helpers

    private void UpdateSelectionVisuals()
    {
        // Trigger rebuild of data rows to reflect selection changes
        // For better performance, could update individual row visuals
        BuildDataRows();
    }

    private void SelectRow(int rowIndex)
    {
        if (_sortedItems == null || rowIndex < 0 || rowIndex >= _sortedItems.Count) return;

        var item = _sortedItems[rowIndex];
        if (item == null) return;

        if (SelectionMode == DataGridSelectionMode.Single)
        {
            _selectedItems.Clear();
        }

        if (!_selectedItems.Contains(item))
        {
            _selectedItems.Add(item);
        }

        SelectedItem = item;
        RaiseSelectionChanged();
        UpdateSelectionVisuals();
    }

    private void ToggleRowSelection(object item)
    {
        if (_selectedItems.Contains(item))
        {
            _selectedItems.Remove(item);
        }
        else
        {
            _selectedItems.Add(item);
        }
        RaiseSelectionChanged();
        UpdateSelectionVisuals();
    }

    private void ClearSelection()
    {
        _selectedItems.Clear();
        SelectedItem = null;
        RaiseSelectionChanged();
        UpdateSelectionVisuals();
    }

    private void SelectAll()
    {
        if (_sortedItems == null) return;

        _selectedItems.Clear();
        foreach (var item in _sortedItems)
        {
            if (item != null)
                _selectedItems.Add(item);
        }
        RaiseSelectionChanged();
        UpdateSelectionVisuals();
    }

    private void ScrollToRow(int rowIndex)
    {
        if (dataScrollView == null) return;

        var targetY = rowIndex * RowHeight;
        var viewportHeight = dataScrollView.Height;
        var currentScrollY = dataScrollView.ScrollY;

        // Check if row is above viewport
        if (targetY < currentScrollY)
        {
            dataScrollView.ScrollToAsync(0, targetY, false);
        }
        // Check if row is below viewport
        else if (targetY + RowHeight > currentScrollY + viewportHeight)
        {
            dataScrollView.ScrollToAsync(0, targetY + RowHeight - viewportHeight, false);
        }
    }

    private void UpdateCurrentCellVisual()
    {
        // This would update visual indicators for the current cell
        // Implementation depends on how cells are rendered
        // For now, we'll rely on selection visual
    }

    private void DeleteSelectedRows()
    {
        if (_selectedItems.Count == 0 || ItemsSource == null) return;

        if (ItemsSource is IList list)
        {
            BeginBatchOperation("Delete rows");
            try
            {
                var itemsToRemove = _selectedItems.ToList();
                foreach (var item in itemsToRemove)
                {
                    list.Remove(item);
                }
                _selectedItems.Clear();
                SelectedItem = null;

                // Reset current position
                if (_sortedItems != null)
                {
                    _focusedRowIndex = Math.Min(_focusedRowIndex, _sortedItems.Count - 1);
                }
                _focusedRowIndex = Math.Max(0, _focusedRowIndex);

                RaiseSelectionChanged();
                BuildDataRows();
            }
            finally
            {
                EndBatchOperation();
            }
        }
    }

    #endregion

    #endregion
}
