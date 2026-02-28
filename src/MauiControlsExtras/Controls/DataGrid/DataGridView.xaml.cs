using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;
using MauiControlsExtras.Behaviors;
using MauiControlsExtras.ContextMenu;
using MauiControlsExtras.Helpers;

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
public partial class DataGridView : Base.ListStyledControlBase, Base.IUndoRedo, Base.IClipboardSupport, Base.IKeyboardNavigable, Base.ISelectable, Base.IContextMenuSupport, Base.Validation.IValidatable
{
    #region Private Fields

    private readonly ObservableCollection<DataGridColumn> _columns = new();
    private readonly List<object> _sortedItems = new();
    private readonly List<object> _filteredItems = new();
    private readonly HashSet<object> _selectedItems = new();
    private readonly Dictionary<DataGridColumn, DataGridColumnFilter> _activeFilters = new();
    private readonly List<DataGridSortDescription> _sortDescriptions = new();
    private readonly List<(DataGridColumn column, PropertyChangedEventHandler handler)> _sortHandlers = new();
    private readonly List<DataGridColumn> _editedColumnsInRow = new();
    private readonly Dictionary<(int, int), ValidationResult> _cellValidationErrors = new();
    private readonly List<string> _gridValidationErrors = new();

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
    private bool _isDistributingFill;
    private bool _pendingFillSync;
    private double _lastDistributionWidth;
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

    // ComboBox popup state
    private ComboBox? _activeComboBox;
    private DataGridComboBoxColumn? _activeComboBoxColumn;
    private object? _activeComboBoxItem;

    // Context menu long-press duration for iOS/macOS UILongPressGestureRecognizer.
    // Android and Windows use their platform-default long-press/holding thresholds.
    private const double LongPressDurationSeconds = 0.5;

    // Grid-level context menu handler state (one per ScrollView, replaces per-cell handlers).
    private readonly GridContextMenuHandlerState _dataScrollViewContextMenu = new();
    private readonly GridContextMenuHandlerState _frozenScrollViewContextMenu = new();
#if ANDROID
    // Stores the last touch-down position (in DIPs) for the Android long-click handler,
    // keyed per ScrollView platform view.
    private Point _androidDataScrollLastTouch;
    private Point _androidFrozenScrollLastTouch;
#endif

    /// <summary>
    /// Stores native event handler references for grid-level context menus on a ScrollView.
    /// Two instances exist: one for <c>dataScrollView</c> and one for <c>frozenDataScrollView</c>.
    /// </summary>
    private sealed class GridContextMenuHandlerState
    {
        public bool IsAttached;
#if WINDOWS
        public Microsoft.UI.Xaml.Input.RightTappedEventHandler? RightTappedHandler;
        public Microsoft.UI.Xaml.Input.HoldingEventHandler? HoldingHandler;
#elif MACCATALYST || IOS
#if MACCATALYST
        public UIKit.UITapGestureRecognizer? SecondaryClickRecognizer;
#endif
        public UIKit.UILongPressGestureRecognizer? LongPressRecognizer;
#elif ANDROID
        public EventHandler<Android.Views.View.LongClickEventArgs>? LongClickHandler;
        public EventHandler<Android.Views.View.TouchEventArgs>? TouchHandler;
#endif
    }

    /// <summary>
    /// Mutable metadata stored per cell container. Gesture handlers capture the metadata
    /// object (not field values) so that in-place cell updates automatically propagate
    /// to tap/double-tap/drag/drop handlers without re-attaching recognizers.
    /// </summary>
    private sealed class CellMetadata
    {
        public object Item = null!;
        public DataGridColumn Column = null!;
        public int RowIndex;
        public int ColIndex;
    }

    private readonly ConditionalWeakTable<Grid, CellMetadata> _cellMetadata = new();

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
    /// Identifies the <see cref="IsRequired"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(DataGridView),
        false);

    /// <summary>
    /// Identifies the <see cref="RequiredErrorMessage"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RequiredErrorMessageProperty = BindableProperty.Create(
        nameof(RequiredErrorMessage),
        typeof(string),
        typeof(DataGridView),
        "At least one row is required.");

    /// <summary>
    /// Identifies the <see cref="IsValid"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsValidProperty = BindableProperty.Create(
        nameof(IsValid),
        typeof(bool),
        typeof(DataGridView),
        true);

    /// <summary>
    /// Identifies the <see cref="ValidateCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ValidateCommandProperty = BindableProperty.Create(
        nameof(ValidateCommand),
        typeof(ICommand),
        typeof(DataGridView),
        default(ICommand));

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
    /// Identifies the <see cref="SelectionChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandParameterProperty = BindableProperty.Create(
        nameof(SelectionChangedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellTappedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellTappedCommandProperty = BindableProperty.Create(
        nameof(CellTappedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellTappedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellTappedCommandParameterProperty = BindableProperty.Create(
        nameof(CellTappedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellDoubleTappedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellDoubleTappedCommandProperty = BindableProperty.Create(
        nameof(CellDoubleTappedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellDoubleTappedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellDoubleTappedCommandParameterProperty = BindableProperty.Create(
        nameof(CellDoubleTappedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="SortingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SortingCommandProperty = BindableProperty.Create(
        nameof(SortingCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="SortingCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SortingCommandParameterProperty = BindableProperty.Create(
        nameof(SortingCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="SortedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SortedCommandProperty = BindableProperty.Create(
        nameof(SortedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="SortedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SortedCommandParameterProperty = BindableProperty.Create(
        nameof(SortedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellEditStartedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellEditStartedCommandProperty = BindableProperty.Create(
        nameof(CellEditStartedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellEditStartedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellEditStartedCommandParameterProperty = BindableProperty.Create(
        nameof(CellEditStartedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellEditEndedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellEditEndedCommandProperty = BindableProperty.Create(
        nameof(CellEditEndedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellEditEndedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellEditEndedCommandParameterProperty = BindableProperty.Create(
        nameof(CellEditEndedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellEditCancelledCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellEditCancelledCommandProperty = BindableProperty.Create(
        nameof(CellEditCancelledCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CellEditCancelledCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellEditCancelledCommandParameterProperty = BindableProperty.Create(
        nameof(CellEditCancelledCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="RowEditEndedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowEditEndedCommandProperty = BindableProperty.Create(
        nameof(RowEditEndedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="RowEditEndedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowEditEndedCommandParameterProperty = BindableProperty.Create(
        nameof(RowEditEndedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="FilteringCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FilteringCommandProperty = BindableProperty.Create(
        nameof(FilteringCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="FilteringCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FilteringCommandParameterProperty = BindableProperty.Create(
        nameof(FilteringCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="FilteredCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FilteredCommandProperty = BindableProperty.Create(
        nameof(FilteredCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="FilteredCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FilteredCommandParameterProperty = BindableProperty.Create(
        nameof(FilteredCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="PageChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PageChangedCommandProperty = BindableProperty.Create(
        nameof(PageChangedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="PageChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PageChangedCommandParameterProperty = BindableProperty.Create(
        nameof(PageChangedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ExportCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ExportCommandProperty = BindableProperty.Create(
        nameof(ExportCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ExportCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ExportCommandParameterProperty = BindableProperty.Create(
        nameof(ExportCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ExportingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ExportingCommandProperty = BindableProperty.Create(
        nameof(ExportingCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ExportingCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ExportingCommandParameterProperty = BindableProperty.Create(
        nameof(ExportingCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="UndoCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty UndoCommandProperty = BindableProperty.Create(
        nameof(UndoCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="UndoCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty UndoCommandParameterProperty = BindableProperty.Create(
        nameof(UndoCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="RedoCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RedoCommandProperty = BindableProperty.Create(
        nameof(RedoCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="RedoCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RedoCommandParameterProperty = BindableProperty.Create(
        nameof(RedoCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CopyCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CopyCommandProperty = BindableProperty.Create(
        nameof(CopyCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CopyCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CopyCommandParameterProperty = BindableProperty.Create(
        nameof(CopyCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CutCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CutCommandProperty = BindableProperty.Create(
        nameof(CutCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="CutCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CutCommandParameterProperty = BindableProperty.Create(
        nameof(CutCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="PasteCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PasteCommandProperty = BindableProperty.Create(
        nameof(PasteCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="PasteCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PasteCommandParameterProperty = BindableProperty.Create(
        nameof(PasteCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ColumnReorderedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ColumnReorderedCommandProperty = BindableProperty.Create(
        nameof(ColumnReorderedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ColumnReorderedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ColumnReorderedCommandParameterProperty = BindableProperty.Create(
        nameof(ColumnReorderedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ColumnResizingCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ColumnResizingCommandProperty = BindableProperty.Create(
        nameof(ColumnResizingCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ColumnResizingCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ColumnResizingCommandParameterProperty = BindableProperty.Create(
        nameof(ColumnResizingCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="RowSelectedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowSelectedCommandProperty = BindableProperty.Create(
        nameof(RowSelectedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="RowSelectedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowSelectedCommandParameterProperty = BindableProperty.Create(
        nameof(RowSelectedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="RowDeselectedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowDeselectedCommandProperty = BindableProperty.Create(
        nameof(RowDeselectedCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="RowDeselectedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty RowDeselectedCommandParameterProperty = BindableProperty.Create(
        nameof(RowDeselectedCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="SelectAllCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectAllCommandProperty = BindableProperty.Create(
        nameof(SelectAllCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="SelectAllCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectAllCommandParameterProperty = BindableProperty.Create(
        nameof(SelectAllCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ClearSelectionCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearSelectionCommandProperty = BindableProperty.Create(
        nameof(ClearSelectionCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ClearSelectionCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearSelectionCommandParameterProperty = BindableProperty.Create(
        nameof(ClearSelectionCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ContextMenuItemsOpeningCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContextMenuItemsOpeningCommandProperty = BindableProperty.Create(
        nameof(ContextMenuItemsOpeningCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="ContextMenuItemsOpeningCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContextMenuItemsOpeningCommandParameterProperty = BindableProperty.Create(
        nameof(ContextMenuItemsOpeningCommandParameter),
        typeof(object),
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
        true,
        propertyChanged: OnContextMenuPropertyChanged);

    /// <summary>
    /// Identifies the <see cref="ContextMenuTemplate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContextMenuTemplateProperty = BindableProperty.Create(
        nameof(ContextMenuTemplate),
        typeof(DataTemplate),
        typeof(DataGridView),
        null,
        propertyChanged: OnContextMenuPropertyChanged);

    /// <summary>
    /// Identifies the <see cref="ContextMenuItems"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ContextMenuItemsProperty = BindableProperty.Create(
        nameof(ContextMenuItems),
        typeof(ContextMenuItemCollection),
        typeof(DataGridView),
        null,
        propertyChanged: OnContextMenuPropertyChanged);

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
    /// Gets or sets whether the grid requires at least one data row to be valid.
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets the error message displayed when <see cref="IsRequired"/> is true and the grid has no rows.
    /// </summary>
    public string RequiredErrorMessage
    {
        get => (string)GetValue(RequiredErrorMessageProperty);
        set => SetValue(RequiredErrorMessageProperty, value);
    }

    /// <summary>
    /// Gets whether the grid is currently valid (no cell errors and required-row check passes).
    /// </summary>
    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        private set => SetValue(IsValidProperty, value);
    }

    /// <summary>
    /// Gets the current list of grid-level validation errors.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors => _gridValidationErrors.AsReadOnly();

    /// <summary>
    /// Gets or sets the command to execute when validation is triggered.
    /// The command parameter will be the <see cref="ValidationResult"/>.
    /// </summary>
    public ICommand? ValidateCommand
    {
        get => (ICommand?)GetValue(ValidateCommandProperty);
        set => SetValue(ValidateCommandProperty, value);
    }

    /// <summary>
    /// Occurs when the <see cref="IsValid"/> state changes.
    /// </summary>
    public event EventHandler<bool>? ValidationChanged;

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
    /// Gets or sets the parameter to pass to <see cref="SelectionChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? SelectionChangedCommandParameter
    {
        get => GetValue(SelectionChangedCommandParameterProperty);
        set => SetValue(SelectionChangedCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="CellTappedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CellTappedCommandParameter
    {
        get => GetValue(CellTappedCommandParameterProperty);
        set => SetValue(CellTappedCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="CellDoubleTappedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CellDoubleTappedCommandParameter
    {
        get => GetValue(CellDoubleTappedCommandParameterProperty);
        set => SetValue(CellDoubleTappedCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="SortingCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? SortingCommandParameter
    {
        get => GetValue(SortingCommandParameterProperty);
        set => SetValue(SortingCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="SortedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? SortedCommandParameter
    {
        get => GetValue(SortedCommandParameterProperty);
        set => SetValue(SortedCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="CellEditStartedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CellEditStartedCommandParameter
    {
        get => GetValue(CellEditStartedCommandParameterProperty);
        set => SetValue(CellEditStartedCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="CellEditEndedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CellEditEndedCommandParameter
    {
        get => GetValue(CellEditEndedCommandParameterProperty);
        set => SetValue(CellEditEndedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when cell edit is cancelled (e.g., user presses Escape).
    /// </summary>
    public ICommand? CellEditCancelledCommand
    {
        get => (ICommand?)GetValue(CellEditCancelledCommandProperty);
        set => SetValue(CellEditCancelledCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="CellEditCancelledCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CellEditCancelledCommandParameter
    {
        get => GetValue(CellEditCancelledCommandParameterProperty);
        set => SetValue(CellEditCancelledCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="RowEditEndedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? RowEditEndedCommandParameter
    {
        get => GetValue(RowEditEndedCommandParameterProperty);
        set => SetValue(RowEditEndedCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="FilteringCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? FilteringCommandParameter
    {
        get => GetValue(FilteringCommandParameterProperty);
        set => SetValue(FilteringCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="FilteredCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? FilteredCommandParameter
    {
        get => GetValue(FilteredCommandParameterProperty);
        set => SetValue(FilteredCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="PageChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? PageChangedCommandParameter
    {
        get => GetValue(PageChangedCommandParameterProperty);
        set => SetValue(PageChangedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when exporting.
    /// </summary>
    public ICommand? ExportCommand
    {
        get => (ICommand?)GetValue(ExportCommandProperty);
        set => SetValue(ExportCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ExportCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ExportCommandParameter
    {
        get => GetValue(ExportCommandParameterProperty);
        set => SetValue(ExportCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed before exporting.
    /// The command parameter is <see cref="DataGridExportEventArgs"/>.
    /// </summary>
    public ICommand? ExportingCommand
    {
        get => (ICommand?)GetValue(ExportingCommandProperty);
        set => SetValue(ExportingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ExportingCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ExportingCommandParameter
    {
        get => GetValue(ExportingCommandParameterProperty);
        set => SetValue(ExportingCommandParameterProperty, value);
    }

    /// <inheritdoc />
    public ICommand? UndoCommand
    {
        get => (ICommand?)GetValue(UndoCommandProperty);
        set => SetValue(UndoCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="UndoCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? UndoCommandParameter
    {
        get => GetValue(UndoCommandParameterProperty);
        set => SetValue(UndoCommandParameterProperty, value);
    }

    /// <inheritdoc />
    public ICommand? RedoCommand
    {
        get => (ICommand?)GetValue(RedoCommandProperty);
        set => SetValue(RedoCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="RedoCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? RedoCommandParameter
    {
        get => GetValue(RedoCommandParameterProperty);
        set => SetValue(RedoCommandParameterProperty, value);
    }

    /// <inheritdoc />
    public ICommand? CopyCommand
    {
        get => (ICommand?)GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="CopyCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CopyCommandParameter
    {
        get => GetValue(CopyCommandParameterProperty);
        set => SetValue(CopyCommandParameterProperty, value);
    }

    /// <inheritdoc />
    public ICommand? CutCommand
    {
        get => (ICommand?)GetValue(CutCommandProperty);
        set => SetValue(CutCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="CutCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? CutCommandParameter
    {
        get => GetValue(CutCommandParameterProperty);
        set => SetValue(CutCommandParameterProperty, value);
    }

    /// <inheritdoc />
    public ICommand? PasteCommand
    {
        get => (ICommand?)GetValue(PasteCommandProperty);
        set => SetValue(PasteCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="PasteCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? PasteCommandParameter
    {
        get => GetValue(PasteCommandParameterProperty);
        set => SetValue(PasteCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when a column is reordered.
    /// The command parameter is <see cref="DataGridColumnReorderEventArgs"/>.
    /// </summary>
    public ICommand? ColumnReorderedCommand
    {
        get => (ICommand?)GetValue(ColumnReorderedCommandProperty);
        set => SetValue(ColumnReorderedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ColumnReorderedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ColumnReorderedCommandParameter
    {
        get => GetValue(ColumnReorderedCommandParameterProperty);
        set => SetValue(ColumnReorderedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when a column resize starts.
    /// The command parameter is <see cref="DataGridColumnEventArgs"/>.
    /// </summary>
    public ICommand? ColumnResizingCommand
    {
        get => (ICommand?)GetValue(ColumnResizingCommandProperty);
        set => SetValue(ColumnResizingCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ColumnResizingCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ColumnResizingCommandParameter
    {
        get => GetValue(ColumnResizingCommandParameterProperty);
        set => SetValue(ColumnResizingCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when a row is selected.
    /// The command parameter is <see cref="DataGridRowSelectionEventArgs"/>.
    /// </summary>
    public ICommand? RowSelectedCommand
    {
        get => (ICommand?)GetValue(RowSelectedCommandProperty);
        set => SetValue(RowSelectedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="RowSelectedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? RowSelectedCommandParameter
    {
        get => GetValue(RowSelectedCommandParameterProperty);
        set => SetValue(RowSelectedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when a row is deselected.
    /// The command parameter is <see cref="DataGridRowSelectionEventArgs"/>.
    /// </summary>
    public ICommand? RowDeselectedCommand
    {
        get => (ICommand?)GetValue(RowDeselectedCommandProperty);
        set => SetValue(RowDeselectedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="RowDeselectedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? RowDeselectedCommandParameter
    {
        get => GetValue(RowDeselectedCommandParameterProperty);
        set => SetValue(RowDeselectedCommandParameterProperty, value);
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
    /// Gets or sets the parameter to pass to <see cref="SelectAllCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? SelectAllCommandParameter
    {
        get => GetValue(SelectAllCommandParameterProperty);
        set => SetValue(SelectAllCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute for clear selection operations.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the command executed before context menu items are shown.
    /// The command parameter is <see cref="DataGridContextMenuOpeningEventArgs"/>.
    /// </summary>
    public ICommand? ContextMenuItemsOpeningCommand
    {
        get => (ICommand?)GetValue(ContextMenuItemsOpeningCommandProperty);
        set => SetValue(ContextMenuItemsOpeningCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ContextMenuItemsOpeningCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ContextMenuItemsOpeningCommandParameter
    {
        get => GetValue(ContextMenuItemsOpeningCommandParameterProperty);
        set => SetValue(ContextMenuItemsOpeningCommandParameterProperty, value);
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

    /// <summary>
    /// Gets the collection of context menu items to display.
    /// Use XAML to define static items, or handle ContextMenuOpening to add dynamic items.
    /// </summary>
    public ContextMenuItemCollection ContextMenuItems
    {
        get
        {
            var items = (ContextMenuItemCollection?)GetValue(ContextMenuItemsProperty);
            if (items == null)
            {
                items = new ContextMenuItemCollection();
                SetValue(ContextMenuItemsProperty, items);
            }
            return items;
        }
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

    #region ISelectable Implementation

    /// <inheritdoc />
    public bool HasSelection => _selectedItems.Count > 0;

    /// <inheritdoc />
    public bool IsAllSelected
    {
        get
        {
            if (_sortedItems == null || _sortedItems.Count == 0)
                return false;

            return _selectedItems.Count == _sortedItems.Count;
        }
    }

    /// <inheritdoc />
    public bool SupportsMultipleSelection => SelectionMode == DataGridSelectionMode.Multiple;

    /// <inheritdoc />
    public object? GetSelection()
    {
        if (SelectionMode == DataGridSelectionMode.Single)
        {
            return SelectedItem;
        }
        else if (SelectionMode == DataGridSelectionMode.Multiple)
        {
            return _selectedItems.Count > 0 ? _selectedItems.ToList() : null;
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

        if (SelectionMode == DataGridSelectionMode.Single)
        {
            // Clear and select single item
            var itemsToDeselect = _selectedItems.ToList();
            _selectedItems.Clear();

            foreach (var item in itemsToDeselect)
            {
                var rowIndex = _sortedItems?.IndexOf(item) ?? -1;
                RaiseRowDeselected(item, rowIndex);
            }

            if (_sortedItems?.Contains(selection) == true)
            {
                _selectedItems.Add(selection);
                SelectedItem = selection;
                var rowIndex = _sortedItems.IndexOf(selection);
                RaiseRowSelected(selection, rowIndex);
            }
        }
        else if (SelectionMode == DataGridSelectionMode.Multiple)
        {
            // Clear and select items from collection
            var itemsToDeselect = _selectedItems.ToList();
            _selectedItems.Clear();

            foreach (var item in itemsToDeselect)
            {
                var rowIndex = _sortedItems?.IndexOf(item) ?? -1;
                RaiseRowDeselected(item, rowIndex);
            }

            if (selection is System.Collections.IEnumerable items && selection is not string)
            {
                foreach (var item in items)
                {
                    if (_sortedItems?.Contains(item) == true)
                    {
                        _selectedItems.Add(item);
                        var rowIndex = _sortedItems.IndexOf(item);
                        RaiseRowSelected(item, rowIndex);
                    }
                }
            }
            else if (_sortedItems?.Contains(selection) == true)
            {
                _selectedItems.Add(selection);
                var rowIndex = _sortedItems.IndexOf(selection);
                RaiseRowSelected(selection, rowIndex);
            }
        }

        RaiseSelectionChanged(oldSelection);
        UpdateSelectionVisuals();
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler<Base.SelectionChangedEventArgs>? SelectionChanged;

    /// <summary>
    /// Occurs when a row is selected.
    /// </summary>
    public event EventHandler<DataGridRowSelectionEventArgs>? RowSelected;

    /// <summary>
    /// Occurs when a row is deselected.
    /// </summary>
    public event EventHandler<DataGridRowSelectionEventArgs>? RowDeselected;

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
    /// Occurs when cell edit is cancelled (e.g., user presses Escape or clicks away without saving).
    /// </summary>
    public event EventHandler<DataGridCellEditEventArgs>? CellEditCancelled;

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
    /// Use this event to dynamically add, remove, or modify context menu items.
    /// </summary>
    public event EventHandler<DataGridContextMenuEventArgs>? ContextMenuOpening;

    /// <summary>
    /// Occurs before the context menu is opened with the new context menu framework.
    /// Provides access to the <see cref="ContextMenuItemCollection"/> for dynamic customization.
    /// </summary>
    public event EventHandler<DataGridContextMenuOpeningEventArgs>? ContextMenuItemsOpening;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the DataGridView control.
    /// </summary>
    [DynamicDependency(nameof(HasFrozenColumns), typeof(DataGridView))]
    [DynamicDependency(nameof(PageInfoText), typeof(DataGridView))]
    [DynamicDependency(nameof(CurrentPageText), typeof(DataGridView))]
    public DataGridView()
    {
        InitializeComponent();
        _columns.CollectionChanged += OnColumnsCollectionChanged;

        // Set initial page size picker
        pageSizePicker.SelectedIndex = 2; // 50

        // DataGrid handles Tab internally for cell navigation.
        var keyboardBehavior = Behaviors.OfType<KeyboardBehavior>().FirstOrDefault();
        if (keyboardBehavior != null)
        {
            keyboardBehavior.HandleTabKey = true;
        }

        Loaded += OnDataGridViewLoaded;
        Unloaded += OnDataGridViewUnloaded;
        SizeChanged += OnDataGridSizeChanged;
        dataContainer.SizeChanged += OnDataContainerSizeChanged;
        dataScrollView.HandlerChanged += OnDataScrollViewHandlerChanged;
        frozenDataScrollView.HandlerChanged += OnFrozenScrollViewHandlerChanged;
    }

    #endregion

    #region Theme Support

    /// <inheritdoc />
    public override void OnThemeChanged(AppTheme theme)
    {
        base.OnThemeChanged(theme);

        if (_columns.Count > 0)
        {
            BuildHeader();
        }

        RefreshDataCellTextColors();
    }

    /// <inheritdoc />
    protected override void OnForegroundColorChanged(Color? oldValue, Color? newValue)
    {
        base.OnForegroundColorChanged(oldValue, newValue);

        if (_columns.Count > 0)
        {
            BuildHeader();
        }

        RefreshDataCellTextColors();
    }

    /// <summary>
    /// Re-applies text colors to all visible data cells after a theme change,
    /// so that non-selected cells pick up the correct default text color.
    /// </summary>
    private void RefreshDataCellTextColors()
    {
        var defaultColor = GetDefaultTextColor();

        RefreshGridTextColors(frozenDataGrid, defaultColor);
        RefreshGridTextColors(dataGrid, defaultColor);

        if (_virtualizingPanel != null)
        {
            foreach (var child in _virtualizingPanel.Children)
            {
                if (child is Grid rowGrid)
                {
                    RefreshGridTextColors(rowGrid, defaultColor);
                }
            }
        }
    }

    private void RefreshGridTextColors(Layout grid, Color defaultColor)
    {
        foreach (var child in grid.Children)
        {
            if (child is Grid cellGrid)
            {
                foreach (var cellChild in cellGrid.Children)
                {
                    if (cellChild is Label label)
                    {
                        // Preserve SelectedTextColor for selected/focused cells
                        if (label.TextColor == SelectedTextColor)
                            continue;

                        label.TextColor = defaultColor;
                    }
                }
            }
        }
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
    /// Performs validation and returns the result.
    /// Checks the required-row constraint and aggregates all per-cell validation errors.
    /// Fires <see cref="ValidateCommand"/> with the result.
    /// </summary>
    public ValidationResult Validate()
    {
        var previousIsValid = IsValid;

        ComputeGridValidationErrors();

        var result = _gridValidationErrors.Count == 0
            ? ValidationResult.Success
            : ValidationResult.Failure(_gridValidationErrors.ToList());

        IsValid = result.IsValid;
        OnPropertyChanged(nameof(ValidationErrors));

        if (previousIsValid != IsValid)
        {
            ValidationChanged?.Invoke(this, IsValid);
        }

        if (ValidateCommand?.CanExecute(result) == true)
        {
            ValidateCommand.Execute(result);
        }

        return result;
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
        if (ExportingCommand?.CanExecute(args) == true)
        {
            ExportingCommand.Execute(ExportingCommandParameter ?? args);
        }

        if (args.Cancel)
            return string.Empty;

        var result = DataGridExporter.ExportToCsv(items, columns, options);
        args.ExportedData = result;

        Exported?.Invoke(this, args);

        if (ExportCommand?.CanExecute(args) == true)
            ExportCommand.Execute(ExportCommandParameter ?? args);

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
        if (ExportingCommand?.CanExecute(args) == true)
        {
            ExportingCommand.Execute(ExportingCommandParameter ?? args);
        }

        if (args.Cancel)
            return string.Empty;

        var result = DataGridExporter.ExportToJson(items, columns, options);
        args.ExportedData = result;

        Exported?.Invoke(this, args);

        if (ExportCommand?.CanExecute(args) == true)
            ExportCommand.Execute(ExportCommandParameter ?? args);

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
        // Fill columns use proportional distribution — skip to avoid destroying star weights
        if (column.SizeMode == DataGridColumnSizeMode.Fill)
            return;

        var headerWidth = MeasureHeaderWidth(column);
        var clamped = Math.Clamp(headerWidth, column.MinWidth, column.MaxWidth);
        column.Width = clamped;
        column.ActualWidth = clamped;
        BuildGrid();
    }

    /// <summary>
    /// Auto-fits all columns to their content.
    /// </summary>
    public void AutoFitAllColumns()
    {
        foreach (var column in _columns)
        {
            // Fill columns use proportional distribution — skip to avoid destroying star weights
            if (column.SizeMode == DataGridColumnSizeMode.Fill)
                continue;

            var headerWidth = MeasureHeaderWidth(column);
            var clamped = Math.Clamp(headerWidth, column.MinWidth, column.MaxWidth);
            column.Width = clamped;
            column.ActualWidth = clamped;
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

        var copyParam = CopyCommandParameter ?? args;
        if (CopyCommand?.CanExecute(copyParam) == true)
            CopyCommand.Execute(copyParam);
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
                PasteCommand.Execute(PasteCommandParameter ?? args);

            RefreshData();
        }
        catch
        {
            CancelBatchOperation();
            throw;
        }
    }

    [UnconditionalSuppressMessage("AOT", "IL2026:RequiresUnreferencedCode",
        Justification = "Fallback for unknown types; common types use TryParse branches above.")]
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

            return PropertyAccessor.ConvertToType(value, targetType);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException
            or OverflowException or ArgumentException or InvalidOperationException)
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

    // Backing field for the explicit IContextMenuSupport.ContextMenuOpening event
    private EventHandler<ContextMenuOpeningEventArgs>? _contextMenuOpeningHandler;

    /// <summary>
    /// Explicit implementation of IContextMenuSupport.ContextMenuOpening.
    /// The legacy ContextMenuOpening event uses DataGridContextMenuEventArgs, so this
    /// uses explicit interface implementation to avoid a naming conflict.
    /// </summary>
    event EventHandler<ContextMenuOpeningEventArgs>? IContextMenuSupport.ContextMenuOpening
    {
        add => _contextMenuOpeningHandler += value;
        remove => _contextMenuOpeningHandler -= value;
    }

    /// <inheritdoc />
    public void ShowContextMenu(Point? position = null)
    {
        object? item = null;
        DataGridColumn? column = null;
        var rowIndex = _focusedRowIndex;
        var columnIndex = _focusedColumnIndex;

        if (rowIndex >= 0 && rowIndex < _sortedItems.Count)
            item = _sortedItems[rowIndex];

        if (columnIndex >= 0)
        {
            var visibleCols = _columns.Where(c => c.IsVisible).ToList();
            if (columnIndex < visibleCols.Count)
                column = visibleCols[columnIndex];
        }

        DispatchShowContextMenuSafely(item, column, rowIndex, columnIndex, position, null);
    }

    /// <summary>
    /// Shows the context menu at the specified cell.
    /// </summary>
    public void ShowContextMenu(object? item, DataGridColumn? column, int rowIndex, int columnIndex)
    {
        DispatchShowContextMenuSafely(item, column, rowIndex, columnIndex, null, null);
    }

    /// <summary>
    /// Shows the context menu at the specified cell and position.
    /// </summary>
    /// <param name="item">The data item at the context menu location.</param>
    /// <param name="column">The column at the context menu location.</param>
    /// <param name="rowIndex">The row index.</param>
    /// <param name="columnIndex">The column index.</param>
    /// <param name="position">The position to show the menu at (relative to anchorView).</param>
    /// <param name="anchorView">The view to anchor the menu to. If null, uses the DataGrid.</param>
    public async Task ShowContextMenuAsync(object? item, DataGridColumn? column, int rowIndex, int columnIndex, Point? position, View? anchorView = null)
    {
        // Fire legacy event for backward compatibility
        var legacyArgs = new DataGridContextMenuEventArgs(item, column, rowIndex, columnIndex);
        ContextMenuOpening?.Invoke(this, legacyArgs);

        if (legacyArgs.Cancel)
            return;

        // Select the item if not already selected
        if (item != null && !_selectedItems.Contains(item))
        {
            SelectItem(item);
        }

        // Build the menu items collection
        var menuItems = new ContextMenuItemCollection();

        // Add XAML-defined items first
        foreach (var xamlItem in ContextMenuItems)
        {
            menuItems.Add(xamlItem);
        }

        // Add default items if enabled
        if (ShowDefaultContextMenu)
        {
            if (menuItems.Count > 0)
            {
                menuItems.AddSeparator();
            }
            AddDefaultContextMenuItems(menuItems, item);
        }

        // Get cell value if available
        object? cellValue = null;
        if (item != null && column != null)
        {
            cellValue = column.GetCellValue(item);
        }

        // Fire new event for dynamic customization
        var newArgs = new DataGridContextMenuOpeningEventArgs(
            menuItems,
            position ?? Point.Zero,
            item,
            column,
            rowIndex,
            columnIndex,
            cellValue,
            this);

        ContextMenuItemsOpening?.Invoke(this, newArgs);
        if (ContextMenuItemsOpeningCommand?.CanExecute(newArgs) == true)
        {
            ContextMenuItemsOpeningCommand.Execute(ContextMenuItemsOpeningCommandParameter ?? newArgs);
        }

        // Fire IContextMenuSupport.ContextMenuOpening event (explicit interface event)
        _contextMenuOpeningHandler?.Invoke(this, newArgs);

        if (newArgs.Cancel)
            return;

        // If handled by the new event, don't show default menu
        if (newArgs.Handled)
            return;

        // Store legacy actions for backward compatibility
        BuildLegacyContextMenuActions(item, column, rowIndex, columnIndex, legacyArgs.CustomMenuItems);

        // Show the context menu using the service
        var visibleItems = menuItems.GetVisibleItems().ToList();
        if (visibleItems.Count > 0)
        {
            await ContextMenuService.Current.ShowAsync(anchorView ?? this, visibleItems, position);
        }
    }

    private void AddDefaultContextMenuItems(ContextMenuItemCollection items, object? item)
    {
        // Copy
        if (CanCopy)
        {
            items.Add("Copy", Copy, "\uE8C8", "Ctrl+C");
        }

        // Cut
        if (CanCut)
        {
            items.Add("Cut", Cut, "\uE8C6", "Ctrl+X");
        }

        // Paste
        if (CanPaste)
        {
            items.Add("Paste", Paste, "\uE77F", "Ctrl+V");
        }

        // Undo/Redo
        if (CanUndo || CanRedo)
        {
            items.AddSeparator();

            if (CanUndo)
            {
                items.Add($"Undo {GetUndoDescription()}", () => Undo(), "\uE7A7", "Ctrl+Z");
            }

            if (CanRedo)
            {
                items.Add($"Redo {GetRedoDescription()}", () => Redo(), "\uE7A6", "Ctrl+Y");
            }
        }

        // Expand/Collapse row details
        if (RowDetailsTemplate != null && item != null && RowDetailsVisibility == DataGridRowDetailsVisibilityMode.Collapsed)
        {
            items.AddSeparator();

            var isExpanded = _expandedItems.Contains(item);
            items.Add(
                isExpanded ? "Collapse Details" : "Expand Details",
                () => ToggleRowDetails(item),
                isExpanded ? "\uE70D" : "\uE70E");
        }
    }

    private void BuildLegacyContextMenuActions(object? item, DataGridColumn? column, int rowIndex, int columnIndex, IList<object>? customItems)
    {
        // Build list of default menu actions for consumers who still use the legacy API
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
        _lastContextMenuActions = defaultActions;
    }

    private List<DataGridContextMenuAction>? _lastContextMenuActions;

    /// <summary>
    /// Gets the default context menu actions from the last context menu request.
    /// Use this in the ContextMenuOpening event handler to build custom menus.
    /// </summary>
    public IReadOnlyList<DataGridContextMenuAction>? GetDefaultContextMenuActions() => _lastContextMenuActions;

    // ── Grid-level context menu handler attachment ──
    // Instead of per-cell native handlers (which created ~14,000 event subscriptions for
    // 3,500 cells), we attach ONE right-click/long-press handler per ScrollView (2 total).
    // At event time, we hit-test the content Grid to find which cell was targeted.

    private static void OnContextMenuPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not DataGridView grid)
            return;

        // Use GetValue to avoid ContextMenuItems getter's lazy auto-creation (which would recurse)
        var wantsContextMenu = grid.ShowDefaultContextMenu
            || grid.ContextMenuTemplate != null
            || (grid.GetValue(ContextMenuItemsProperty) is ContextMenuItemCollection c && c.Count > 0);

        if (wantsContextMenu)
        {
            grid.AttachGridLevelContextMenuHandlers(grid.dataScrollView, grid._dataScrollViewContextMenu, isFrozen: false);
            grid.AttachGridLevelContextMenuHandlers(grid.frozenDataScrollView, grid._frozenScrollViewContextMenu, isFrozen: true);
        }
        else
        {
            grid.DetachGridLevelContextMenuHandlers(grid.dataScrollView, grid._dataScrollViewContextMenu);
            grid.DetachGridLevelContextMenuHandlers(grid.frozenDataScrollView, grid._frozenScrollViewContextMenu);
        }
    }

    private void OnDataScrollViewHandlerChanged(object? sender, EventArgs e)
    {
        AttachGridLevelContextMenuHandlers(dataScrollView, _dataScrollViewContextMenu, isFrozen: false);
    }

    private void OnFrozenScrollViewHandlerChanged(object? sender, EventArgs e)
    {
        AttachGridLevelContextMenuHandlers(frozenDataScrollView, _frozenScrollViewContextMenu, isFrozen: true);
    }

    private void AttachGridLevelContextMenuHandlers(ScrollView scrollView, GridContextMenuHandlerState state, bool isFrozen)
    {
        if (state.IsAttached || scrollView.Handler?.PlatformView == null)
            return;

        if (!(ShowDefaultContextMenu || ContextMenuTemplate != null
            || (GetValue(ContextMenuItemsProperty) is ContextMenuItemCollection c && c.Count > 0)))
            return;

        var platformView = scrollView.Handler.PlatformView;

#if WINDOWS
        if (platformView is Microsoft.UI.Xaml.FrameworkElement element)
        {
            state.RightTappedHandler = (sender, args) =>
            {
                try
                {
                    var winPos = args.GetPosition(element);
                    var contentX = winPos.X + scrollView.ScrollX;
                    var contentY = winPos.Y + scrollView.ScrollY;
                    args.Handled = HandleGridLevelContextMenu(contentX, contentY, isFrozen);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning($"[DataGridView] RightTapped handler error: {ex}");
                }
            };
            element.RightTapped += state.RightTappedHandler;

            state.HoldingHandler = (sender, args) =>
            {
                try
                {
                    if (args.HoldingState != Microsoft.UI.Input.HoldingState.Started)
                        return;

                    var winPos = args.GetPosition(element);
                    var contentX = winPos.X + scrollView.ScrollX;
                    var contentY = winPos.Y + scrollView.ScrollY;
                    args.Handled = HandleGridLevelContextMenu(contentX, contentY, isFrozen);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning($"[DataGridView] Holding handler error: {ex}");
                }
            };
            element.Holding += state.HoldingHandler;
        }
#elif MACCATALYST || IOS
        if (platformView is UIKit.UIView uiView)
        {
#if MACCATALYST
            state.SecondaryClickRecognizer = new UIKit.UITapGestureRecognizer((gesture) =>
            {
                try
                {
                    var location = gesture.LocationInView(uiView);
                    var contentX = location.X + scrollView.ScrollX;
                    var contentY = location.Y + scrollView.ScrollY;
                    HandleGridLevelContextMenu(contentX, contentY, isFrozen);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning($"[DataGridView] SecondaryClick handler error: {ex}");
                }
            });
            state.SecondaryClickRecognizer.ButtonMaskRequired = UIKit.UIEventButtonMask.Secondary;
            uiView.AddGestureRecognizer(state.SecondaryClickRecognizer);
#endif

            state.LongPressRecognizer = new UIKit.UILongPressGestureRecognizer((gesture) =>
            {
                try
                {
                    if (gesture.State != UIKit.UIGestureRecognizerState.Began)
                        return;

                    var location = gesture.LocationInView(uiView);
                    var contentX = location.X + scrollView.ScrollX;
                    var contentY = location.Y + scrollView.ScrollY;
                    HandleGridLevelContextMenu(contentX, contentY, isFrozen);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning($"[DataGridView] LongPress handler error: {ex}");
                }
            });
            state.LongPressRecognizer.MinimumPressDuration = LongPressDurationSeconds;
            uiView.AddGestureRecognizer(state.LongPressRecognizer);
        }
#elif ANDROID
        if (platformView is Android.Views.View androidView)
        {
            state.TouchHandler = (sender, args) =>
            {
                try
                {
                    if (args.Event?.Action == Android.Views.MotionEventActions.Down)
                    {
                        var density = androidView.Context?.Resources?.DisplayMetrics?.Density ?? 1f;
                        var rawX = args.Event.GetX();
                        var rawY = args.Event.GetY();
                        if (isFrozen)
                            _androidFrozenScrollLastTouch = new Point(rawX / density, rawY / density);
                        else
                            _androidDataScrollLastTouch = new Point(rawX / density, rawY / density);
                    }
                    args.Handled = false;
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning($"[DataGridView] Touch handler error: {ex}");
                }
            };
            androidView.LongClickable = true;
            androidView.Touch += state.TouchHandler;

            state.LongClickHandler = (sender, args) =>
            {
                try
                {
                    var viewportPos = isFrozen ? _androidFrozenScrollLastTouch : _androidDataScrollLastTouch;
                    var contentX = viewportPos.X + scrollView.ScrollX;
                    var contentY = viewportPos.Y + scrollView.ScrollY;
                    args.Handled = HandleGridLevelContextMenu(contentX, contentY, isFrozen);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning($"[DataGridView] LongClick handler error: {ex}");
                }
            };
            androidView.LongClick += state.LongClickHandler;
        }
#endif
        state.IsAttached = true;
    }

    private void DetachGridLevelContextMenuHandlers(ScrollView scrollView, GridContextMenuHandlerState state)
    {
        if (!state.IsAttached)
            return;

        var platformView = scrollView.Handler?.PlatformView;
        if (platformView == null)
        {
            state.IsAttached = false;
            return;
        }

#if WINDOWS
        if (platformView is Microsoft.UI.Xaml.FrameworkElement element)
        {
            if (state.RightTappedHandler != null)
            {
                element.RightTapped -= state.RightTappedHandler;
                state.RightTappedHandler = null;
            }
            if (state.HoldingHandler != null)
            {
                element.Holding -= state.HoldingHandler;
                state.HoldingHandler = null;
            }
        }
#elif MACCATALYST || IOS
        if (platformView is UIKit.UIView uiView)
        {
#if MACCATALYST
            if (state.SecondaryClickRecognizer != null)
            {
                uiView.RemoveGestureRecognizer(state.SecondaryClickRecognizer);
                state.SecondaryClickRecognizer.Dispose();
                state.SecondaryClickRecognizer = null;
            }
#endif
            if (state.LongPressRecognizer != null)
            {
                uiView.RemoveGestureRecognizer(state.LongPressRecognizer);
                state.LongPressRecognizer.Dispose();
                state.LongPressRecognizer = null;
            }
        }
#elif ANDROID
        if (platformView is Android.Views.View androidView)
        {
            if (state.LongClickHandler != null)
            {
                androidView.LongClick -= state.LongClickHandler;
                state.LongClickHandler = null;
            }
            if (state.TouchHandler != null)
            {
                androidView.Touch -= state.TouchHandler;
                state.TouchHandler = null;
            }
        }
#endif
        state.IsAttached = false;
    }

    /// <summary>
    /// Hit-tests content coordinates to find the cell container, then dispatches the context menu.
    /// Works for both non-virtualized (dataGrid) and virtualized (_virtualizingPanel) modes.
    /// </summary>
    /// <returns><c>true</c> if a cell was found and the context menu dispatched; <c>false</c> if
    /// the hit-test missed (empty area, suppressed cell, etc.).</returns>
    private bool HandleGridLevelContextMenu(double contentX, double contentY, bool isFrozen)
    {
        // Determine the content grid to search
        Layout contentGrid;
        bool isVirtualized;
        if (isFrozen)
        {
            isVirtualized = _frozenVirtualizingPanel != null;
            contentGrid = _frozenVirtualizingPanel as Layout ?? frozenDataGrid;
        }
        else
        {
            isVirtualized = _virtualizingPanel != null;
            contentGrid = _virtualizingPanel as Layout ?? dataGrid;
        }

        Grid? cell;
        double cellAbsoluteX, cellAbsoluteY;

        if (isVirtualized)
        {
            // Two-level: virtualizing panel children are row Grids, cells are nested inside
            var row = FindChildAtPosition(contentGrid, contentX, contentY);
            if (row is not Grid rowGrid)
                return false;

            // Convert content-space coordinates to row-relative coordinates
            cell = FindChildAtPosition(rowGrid, contentX - rowGrid.Frame.X, contentY - rowGrid.Frame.Y) as Grid;
            if (cell == null)
                return false;

            cellAbsoluteX = rowGrid.Frame.X + cell.Frame.X;
            cellAbsoluteY = rowGrid.Frame.Y + cell.Frame.Y;
        }
        else
        {
            // Flat grid: cells are direct children with content-space Frames
            cell = FindChildAtPosition(contentGrid, contentX, contentY) as Grid;
            if (cell == null)
                return false;

            cellAbsoluteX = cell.Frame.X;
            cellAbsoluteY = cell.Frame.Y;
        }

        if (!_cellMetadata.TryGetValue(cell, out var meta))
            return false;

        if (ShouldSuppressContextMenu(meta.RowIndex, meta.ColIndex))
            return false;

        var cellRelativePosition = new Point(contentX - cellAbsoluteX, contentY - cellAbsoluteY);
        DispatchShowContextMenuSafely(meta.Item, meta.Column, meta.RowIndex, meta.ColIndex, cellRelativePosition, cell);
        return true;
    }

    /// <summary>
    /// Finds the first child view at the given coordinates within a layout.
    /// Coordinates are relative to the layout's content space.
    /// </summary>
    private static View? FindChildAtPosition(Layout grid, double x, double y)
    {
        foreach (var child in grid.Children)
        {
            if (child is View view && view.Frame.Width > 0)
            {
                var f = view.Frame;
                if (x >= f.X && x < f.X + f.Width &&
                    y >= f.Y && y < f.Y + f.Height)
                    return view;
            }
        }
        return null;
    }

    private void DispatchShowContextMenuSafely(object? item, DataGridColumn? column,
        int rowIndex, int colIndex, Point? position, View? anchorView)
    {
        Dispatcher.Dispatch(async () =>
        {
            try
            {
                await ShowContextMenuAsync(item, column, rowIndex, colIndex, position, anchorView);
            }
            catch (Exception ex)
            {
                // Log but do not re-throw — this runs inside Dispatcher.Dispatch(Action)
                // which is async void; re-throwing would surface as an unhandled WinUI exception.
                Trace.TraceWarning($"[DataGridView] Context menu error at [{rowIndex},{colIndex}]: {ex}");
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[DataGridView] Context menu error at [{rowIndex},{colIndex}]: {ex}");
#endif
            }
        });
    }

    /// <summary>
    /// Suppresses the DataGrid context menu when the target cell is currently being edited.
    /// All edit control types (Entry, CheckBox, DatePicker, TimePicker, Picker, ComboBox) are
    /// suppressed uniformly rather than per-type—simpler and safer against new control types.
    /// Non-editing cells are not affected and will show the context menu normally.
    /// </summary>
    private bool ShouldSuppressContextMenu(int rowIndex, int colIndex)
        => DataGridContextMenuHelper.IsCellInEditMode(rowIndex, colIndex, _editingRowIndex, _editingColumnIndex, _currentEditControl != null);

    private void OnDataGridViewLoaded(object? sender, EventArgs e)
    {
        // Re-subscribe handlers that were detached in OnDataGridViewUnloaded.
        // -= before += prevents double subscription on first Loaded (constructor already subscribes).
        SizeChanged -= OnDataGridSizeChanged;
        SizeChanged += OnDataGridSizeChanged;
        dataContainer.SizeChanged -= OnDataContainerSizeChanged;
        dataContainer.SizeChanged += OnDataContainerSizeChanged;
        dataScrollView.HandlerChanged -= OnDataScrollViewHandlerChanged;
        dataScrollView.HandlerChanged += OnDataScrollViewHandlerChanged;
        frozenDataScrollView.HandlerChanged -= OnFrozenScrollViewHandlerChanged;
        frozenDataScrollView.HandlerChanged += OnFrozenScrollViewHandlerChanged;
    }

    private void OnDataGridViewUnloaded(object? sender, EventArgs e)
    {
        SizeChanged -= OnDataGridSizeChanged;
        dataContainer.SizeChanged -= OnDataContainerSizeChanged;
        dataScrollView.HandlerChanged -= OnDataScrollViewHandlerChanged;
        frozenDataScrollView.HandlerChanged -= OnFrozenScrollViewHandlerChanged;
        DetachGridLevelContextMenuHandlers(dataScrollView, _dataScrollViewContextMenu);
        DetachGridLevelContextMenuHandlers(frozenDataScrollView, _frozenScrollViewContextMenu);
        ClearVirtualizationPanels();
    }

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

        // Build UI — PreMeasure before BuildHeader so FitHeader columns
        // have ActualWidth set when ResolveColumnWidth is called for headers.
        PreMeasureFitHeaderColumns();
        BuildHeader();
        BuildDataRows();
        BuildFooter();

        // Defer Fill distribution off the current call stack to avoid LayoutCycleException
        // on WinUI — modifying ColumnDefinition.Width during a layout/binding pass causes
        // a layout cycle. ScheduleColumnWidthSync handles the final sync after Auto columns
        // have accurate widths.
        ScheduleColumnWidthSync();

        // Show/hide empty view
        UpdateEmptyView();

        // Update pagination UI
        UpdatePaginationUI();

        _isUpdating = false;
    }

    [UnconditionalSuppressMessage("AOT", "IL2070:UnrecognizedReflectionPattern",
        Justification = "Reflection fallback for auto-generating columns. Define columns explicitly for AOT compatibility.")]
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
        // Unsubscribe stale sort-indicator handlers before clearing header cells
        foreach (var (col, handler) in _sortHandlers)
            col.PropertyChanged -= handler;
        _sortHandlers.Clear();

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
            var width = ResolveColumnWidth(column);
            frozenHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

            var headerCell = CreateHeaderCell(column, i, true);
            frozenHeaderGrid.Children.Add(headerCell);
            Grid.SetColumn(headerCell, i);
        }

        // Build scrollable headers
        for (int i = 0; i < scrollableColumns.Count; i++)
        {
            var column = scrollableColumns[i];
            var width = ResolveColumnWidth(column);
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

        // Header text — prevent wrapping so FitHeader measurement matches rendering
        var headerLabel = new Label
        {
            Text = column.Header,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            MaxLines = 1,
            LineBreakMode = LineBreakMode.NoWrap,
            VerticalOptions = LayoutOptions.Center,
            TextColor = EffectiveForegroundColor
        };
        contentGrid.Children.Add(headerLabel);
        Grid.SetColumn(headerLabel, 0);

        // Filter indicator (using funnel/filter icon distinct from sort arrows)
        if (CanUserFilter && column.CanUserFilter)
        {
            var filterLabel = new Label
            {
                Text = column.IsFiltered ? "⫧" : "⫶",
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = column.IsFiltered ? EffectiveAccentColor : Colors.Gray
            };

            // Wrap in Border for 44×44pt minimum touch target (Apple HIG / Material Design)
            var filterContainer = new Border
            {
                BackgroundColor = Colors.Transparent,
                StrokeThickness = 0,
                MinimumWidthRequest = 44,
                MinimumHeightRequest = 44,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(4, 0, 0, 0),
                Content = filterLabel
            };

            var filterTap = new TapGestureRecognizer();
            filterTap.Tapped += (s, e) => ShowFilterPopup(column);
            filterContainer.GestureRecognizers.Add(filterTap);

            contentGrid.Children.Add(filterContainer);
            Grid.SetColumn(filterContainer, 1);
        }

        // Sort indicator (only show if sorting is enabled)
        if (CanUserSort && column.CanUserSort)
        {
            var sortLabel = new Label
            {
                Text = GetSortIndicatorText(column),
                FontSize = 10,
                VerticalOptions = LayoutOptions.Center,
                TextColor = column.SortDirection != null ? EffectiveAccentColor : Colors.Gray,
                Margin = new Thickness(4, 0, 0, 0)
            };
            PropertyChangedEventHandler sortHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(DataGridColumn.SortDirection))
                {
                    sortLabel.Text = GetSortIndicatorText(column);
                    sortLabel.TextColor = column.SortDirection != null ? EffectiveAccentColor : Colors.Gray;
                }
            };
            column.PropertyChanged += sortHandler;
            _sortHandlers.Add((column, sortHandler));
            contentGrid.Children.Add(sortLabel);
            Grid.SetColumn(sortLabel, 2);
        }

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

    /// <summary>
    /// Builds an O(1) lookup from item to its index in <see cref="_sortedItems"/>.
    /// Replaces O(n) <c>_sortedItems.IndexOf(item)</c> calls in row-building loops.
    /// </summary>
    private Dictionary<object, int> BuildSortedItemsIndex()
    {
        var index = new Dictionary<object, int>(_sortedItems.Count, ReferenceEqualityComparer.Instance);
        for (int i = 0; i < _sortedItems.Count; i++)
        {
            // First occurrence wins (duplicates are rare but possible with bad data)
            index.TryAdd(_sortedItems[i], i);
        }
        return index;
    }

    /// <summary>
    /// Fast path for page-only changes (non-virtualized mode). Updates existing cell
    /// containers in-place instead of tearing down and rebuilding the entire grid.
    /// Returns false when structural changes require a full rebuild.
    /// </summary>
    private bool TryUpdateDataRowsInPlace()
    {
        // Virtualized mode has its own fast path (UpdateVirtualizedDataForPageChange)
        if (_virtualizingPanel != null)
            return false;

        // Row details make grid row indices unpredictable — fall back to full rebuild
        if (RowDetailsTemplate != null)
            return false;

        var displayItems = GetDisplayItems();
        var frozenColumns = GetFrozenColumns();
        var scrollableColumns = GetScrollableColumns();

        // Structural mismatch: column count changed since last build
        if (frozenDataGrid.ColumnDefinitions.Count != frozenColumns.Count ||
            dataGrid.ColumnDefinitions.Count != scrollableColumns.Count)
            return false;

        // Item count changed (last page, page size change) — row definitions don't match
        if (dataGrid.RowDefinitions.Count != displayItems.Count ||
            frozenDataGrid.RowDefinitions.Count != displayItems.Count)
            return false;

        // Verify expected cell counts: rows * columns for each grid
        if (dataGrid.Children.Count != displayItems.Count * scrollableColumns.Count ||
            frozenDataGrid.Children.Count != displayItems.Count * frozenColumns.Count)
            return false;

        var sortedIndex = BuildSortedItemsIndex();

        // Cells are laid out in row-major order (row 0 col 0, row 0 col 1, ..., row 1 col 0, ...).
        // Direct index calculation: childIndex = displayIndex * columnCount + colIndex.
        for (int displayIndex = 0; displayIndex < displayItems.Count; displayIndex++)
        {
            var item = displayItems[displayIndex];
            var actualRowIndex = sortedIndex.TryGetValue(item, out var idx) ? idx : -1;
            var isSelected = _selectedItems.Contains(item);
            var isAlternate = displayIndex % 2 == 1;

            // Update frozen cells via direct index
            for (int colIndex = 0; colIndex < frozenColumns.Count; colIndex++)
            {
                var childIndex = displayIndex * frozenColumns.Count + colIndex;
                if (frozenDataGrid.Children[childIndex] is Grid cell)
                {
                    UpdateDataCellContent(cell, item, frozenColumns[colIndex], actualRowIndex, colIndex, isSelected, isAlternate);
                }
            }

            // Update scrollable cells via direct index
            for (int colIndex = 0; colIndex < scrollableColumns.Count; colIndex++)
            {
                var childIndex = displayIndex * scrollableColumns.Count + colIndex;
                if (dataGrid.Children[childIndex] is Grid cell)
                {
                    var fullColIndex = colIndex + frozenColumns.Count;
                    UpdateDataCellContent(cell, item, scrollableColumns[colIndex], actualRowIndex, fullColIndex, isSelected, isAlternate);
                }
            }
        }

        dataGrid.InvalidateMeasure();
        frozenDataGrid.InvalidateMeasure();
        return true;
    }

    /// <summary>
    /// Returns the number of data rows that fit in the current viewport.
    /// Falls back to 400px when the viewport height is not yet available.
    /// </summary>
    private int GetVisibleRowCapacity()
    {
        var viewportHeight = dataScrollView.Height;
        if (viewportHeight <= 0 || double.IsNaN(viewportHeight))
            viewportHeight = 400; // sensible fallback before layout
        var effectiveRowHeight = RowHeight > 0 ? RowHeight : 44.0;
        return Math.Max(1, (int)(viewportHeight / effectiveRowHeight));
    }

    /// <summary>
    /// Determines whether virtualization should be used for the current dataset.
    /// Returns true if the user explicitly enabled it, or if the item count
    /// exceeds twice the viewport capacity (auto-virtualization).
    /// Uses hysteresis: once auto-virtualization is active, only disengages when
    /// items drop below 1 screenful (instead of 2) to prevent flip-flop on resize.
    /// </summary>
    private bool ShouldUseVirtualization()
    {
        if (EnableVirtualization)
            return true;
        var capacity = GetVisibleRowCapacity();
        // Hysteresis: if already auto-virtualized, use lower threshold to disengage
        var threshold = _virtualizingPanel != null ? capacity : capacity * 2;
        return _sortedItems.Count > threshold;
    }

    private void BuildDataRows()
    {
        // Use virtualization if enabled or if the dataset exceeds 2 screenfuls
        if (ShouldUseVirtualization())
        {
            try
            {
                BuildVirtualizedRows();
                return;
            }
            catch (Exception ex)
            {
                // Fall back to non-virtualized rendering if virtualization setup fails
                // (e.g., WinUI exception from visual tree modification during layout)
                Trace.TraceWarning($"[DataGridView] Virtualization setup failed, falling back to non-virtualized: {ex}");
                ClearVirtualizationPanels();
            }
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
        {
            dataGrid.InvalidateMeasure();
            frozenDataGrid.InvalidateMeasure();
            return;
        }

        var frozenColumns = GetFrozenColumns();
        var scrollableColumns = GetScrollableColumns();

        // Add column definitions for frozen grid
        for (int i = 0; i < frozenColumns.Count; i++)
        {
            var column = frozenColumns[i];
            var width = ResolveColumnWidth(column);
            frozenDataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
        }

        // Add column definitions for scrollable grid
        for (int i = 0; i < scrollableColumns.Count; i++)
        {
            var column = scrollableColumns[i];
            var width = ResolveColumnWidth(column);
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
        }

        // Pre-build O(1) index lookup (replaces O(n) _sortedItems.IndexOf per row)
        var sortedIndex = BuildSortedItemsIndex();

        // Suppress per-child layout passes during bulk cell creation
        dataGrid.BatchBegin();
        frozenDataGrid.BatchBegin();
        try
        {
            // Add row definitions and cells
            var gridRowIndex = 0;
            for (int displayIndex = 0; displayIndex < displayItems.Count; displayIndex++)
            {
                frozenDataGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(RowHeight) });
                dataGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(RowHeight) });

                var item = displayItems[displayIndex];
                var actualRowIndex = sortedIndex.TryGetValue(item, out var idx) ? idx : -1;
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
        finally
        {
            frozenDataGrid.BatchCommit();
            dataGrid.BatchCommit();
        }

        // Force layout invalidation so ScrollView containers re-measure
        dataGrid.InvalidateMeasure();
        frozenDataGrid.InvalidateMeasure();
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

        // Feed page slice when paginated, all items otherwise
        var displayItems = GetDisplayItems();

        var dynamicBuffer = GetVisibleRowCapacity();
        _virtualizingPanel.RowHeight = RowHeight;
        _virtualizingPanel.BufferSize = EnableVirtualization ? VirtualizationBufferSize : dynamicBuffer;
        _virtualizingPanel.ItemsSource = displayItems;
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
            _frozenVirtualizingPanel.BufferSize = EnableVirtualization ? VirtualizationBufferSize : dynamicBuffer;
            _frozenVirtualizingPanel.ItemsSource = displayItems;
            _frozenVirtualizingPanel.RowFactory = (item, rowIndex) => CreateVirtualizedRow(item, rowIndex, frozenColumns, 0);
            _frozenVirtualizingPanel.RowUpdater = (row, item, rowIndex) => UpdateVirtualizedRow(row, item, rowIndex, frozenColumns, 0);
            _frozenVirtualizingPanel.Refresh(dataScrollView.Height > 0 ? dataScrollView.Height : 400);
        }
    }

    /// <summary>
    /// Fast path for page changes when virtualization panels are already initialized.
    /// Updates ItemsSource with the new page slice and triggers Refresh(), which moves
    /// existing rows to the recycle pool and then re-populates via RowUpdater.
    /// </summary>
    private void UpdateVirtualizedDataForPageChange()
    {
        if (_virtualizingPanel == null)
        {
            // Panels not initialized yet — full build
            BuildDataRows();
            return;
        }

        var displayItems = GetDisplayItems();
        _virtualizingPanel.ItemsSource = displayItems;

        if (_frozenVirtualizingPanel != null)
            _frozenVirtualizingPanel.ItemsSource = displayItems;

        double viewportHeight = dataScrollView.Height > 0 ? dataScrollView.Height : 400;
        _virtualizingPanel.Refresh(viewportHeight);
        _frozenVirtualizingPanel?.Refresh(viewportHeight);
    }

    private View CreateVirtualizedRow(object item, int rowIndex, List<DataGridColumn> columns, int columnOffset)
    {
        var isSelected = _selectedItems.Contains(item);
        var isAlternate = rowIndex % 2 == 1;

        var rowGrid = new Grid();

        // Add column definitions
        foreach (var column in columns)
        {
            var width = ResolveColumnWidth(column);
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

        // Sync ColumnDefinitions to latest column.ActualWidth (Fill/FitHeader may have
        // changed since this row was created or last recycled).
        SyncRowGridColumnWidths(rowGrid, columns);

        var isSelected = _selectedItems.Contains(item);
        var isAlternate = rowIndex % 2 == 1;

        // In-place update of existing cells when column count matches (common case).
        // This avoids the expensive DetachContextMenuHandlers + Children.Clear + CreateDataCell loop.
        if (rowGrid.Children.Count == columns.Count)
        {
            for (int colIndex = 0; colIndex < columns.Count; colIndex++)
            {
                if (rowGrid.Children[colIndex] is Grid cell)
                {
                    var column = columns[colIndex];
                    var fullColIndex = colIndex + columnOffset;
                    UpdateDataCellContent(cell, item, column, rowIndex, fullColIndex, isSelected, isAlternate);
                }
            }
            return;
        }

        // Column count mismatch (structural change) — fall back to full rebuild
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
        container.BindingContext = item;

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
        if (_cellValidationErrors.TryGetValue((rowIndex, colIndex), out var validationResult) && !validationResult.IsValid)
        {
            var errorIndicator = new BoxView
            {
                Color = EffectiveErrorBorderColor,
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

        // Store mutable metadata for event handling. Gesture handlers capture the metadata
        // *object* and read fields at tap time, so in-place cell updates automatically
        // propagate without re-attaching recognizers.
        var meta = new CellMetadata { Item = item, Column = column, RowIndex = rowIndex, ColIndex = colIndex };
        _cellMetadata.AddOrUpdate(container, meta);
        container.AutomationId = $"cell_{rowIndex}_{colIndex}";

        // Add tap gestures (read from meta at tap time, not captured locals)
        var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
        tapGesture.Tapped += (s, e) => OnCellTapped(meta.Item, meta.Column, meta.RowIndex, meta.ColIndex);
        container.GestureRecognizers.Add(tapGesture);

        var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTapGesture.Tapped += (s, e) => OnCellDoubleTapped(meta.Item, meta.Column, meta.RowIndex, meta.ColIndex);
        container.GestureRecognizers.Add(doubleTapGesture);

        // Add row drag gesture (only on first column for drag handle)
        if (CanUserReorderRows && colIndex == 0)
        {
            var dragGesture = new DragGestureRecognizer();
            dragGesture.DragStarting += (s, e) => OnRowDragStarting(meta.Item, meta.RowIndex, e);
            container.GestureRecognizers.Add(dragGesture);
        }

        // Add row drop gesture to all cells
        if (CanUserReorderRows)
        {
            var dropGesture = new DropGestureRecognizer();
            dropGesture.DragOver += (s, e) => OnRowDragOver(meta.RowIndex, e);
            dropGesture.Drop += (s, e) => OnRowDrop(meta.RowIndex, e);
            container.GestureRecognizers.Add(dropGesture);
        }

        return container;
    }

    /// <summary>
    /// Updates an existing cell container in-place for pagination fast path.
    /// Preserves the Grid shell, gesture recognizers, and native context menu handlers;
    /// replaces only data-dependent state (content, colors, metadata).
    /// </summary>
    private void UpdateDataCellContent(Grid container, object item, DataGridColumn column, int rowIndex, int colIndex, bool isSelected, bool isAlternate)
    {
        // Update binding context (AutomationId is set once in CreateDataCell; it is
        // immutable in MAUI and would throw on reassignment)
        container.BindingContext = item;

        // Update background
        UpdateCellBackground(container, rowIndex, colIndex, isSelected, isAlternate);

        // Replace visual children (content + error indicator + grid lines).
        // Grid lines are cheap BoxViews; recreating them is simpler and safer than
        // trying to distinguish them from error indicators.
        container.Children.Clear();

        // Cell content
        var content = column.CreateCellContent(item);

        var isFocused = rowIndex == _focusedRowIndex && colIndex == _focusedColumnIndex;
        if ((isSelected || isFocused) && content is Label contentLabel)
        {
            contentLabel.TextColor = SelectedTextColor;
        }

        if (HighlightSearchResults && !string.IsNullOrEmpty(SearchText) && content is Label label && !string.IsNullOrEmpty(label.Text))
        {
            ApplySearchHighlighting(label, SearchText);
        }

        container.Children.Add(content);

        // Validation error indicator
        if (_cellValidationErrors.TryGetValue((rowIndex, colIndex), out var validationResult) && !validationResult.IsValid)
        {
            container.Children.Add(new BoxView
            {
                Color = EffectiveErrorBorderColor,
                HeightRequest = 2,
                VerticalOptions = LayoutOptions.End
            });
        }

        // Grid lines (same as CreateDataCell)
        if (GridLinesVisibility == GridLinesVisibility.Vertical || GridLinesVisibility == GridLinesVisibility.Both)
        {
            container.Children.Add(new BoxView
            {
                Color = GridLineColor,
                WidthRequest = 1,
                HorizontalOptions = LayoutOptions.End
            });
        }

        if (GridLinesVisibility == GridLinesVisibility.Horizontal || GridLinesVisibility == GridLinesVisibility.Both)
        {
            container.Children.Add(new BoxView
            {
                Color = GridLineColor,
                HeightRequest = 1,
                VerticalOptions = LayoutOptions.End
            });
        }

        // Update CellMetadata so gesture handlers see current values
        if (_cellMetadata.TryGetValue(container, out var meta))
        {
            meta.Item = item;
            meta.Column = column;
            meta.RowIndex = rowIndex;
            meta.ColIndex = colIndex;
        }
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
            var width = ResolveColumnWidth(column);
            frozenFooterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });

            var footerCell = CreateFooterCell(column);
            frozenFooterGrid.Children.Add(footerCell);
            Grid.SetColumn(footerCell, i);
        }

        // Build scrollable footer
        for (int i = 0; i < scrollableColumns.Count; i++)
        {
            var column = scrollableColumns[i];
            var width = ResolveColumnWidth(column);
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
        _filteredItems.AddRange(ApplyFiltersCore());
    }

    private List<object> GetItemsFilteredExcluding(DataGridColumn excludeColumn)
        => ApplyFiltersCore(excludeColumn);

    private List<object> ApplyFiltersCore(DataGridColumn? excludeColumn = null)
    {
        if (ItemsSource == null)
            return new List<object>();

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

        // Apply column filters (skip excludeColumn when provided for cascading popup values)
        foreach (var filter in _activeFilters.Values.Where(f => f.IsActive && (excludeColumn == null || f.Column != excludeColumn)))
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

        return items;
    }

    private void ShowFilterPopup(DataGridColumn column)
    {
        // Cascading filter: show only values from rows filtered by all OTHER columns
        var baseItems = GetItemsFilteredExcluding(column);

        var seen = new HashSet<object?>();
        var distinctValues = new List<object?>();
        foreach (var item in baseItems)
        {
            var value = column.GetCellValue(item);
            if (seen.Add(value))
                distinctValues.Add(value);
        }

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
            SortingCommand.Execute(SortingCommandParameter ?? sortingArgs);
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
            SortedCommand.Execute(SortedCommandParameter ?? column);
        }
    }

    #endregion

    #region Private Methods - Selection

    private void SelectItem(object item)
    {
        if (SelectionMode == DataGridSelectionMode.None)
            return;

        // Track old selection for event args
        var oldSelection = GetSelection();

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
        RaiseSelectionChanged(oldSelection);
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

            if (_virtualizingPanel != null)
            {
                // Virtualized: cells are inside row Grids in the virtualizing panel
                var rowView = _virtualizingPanel.GetVisibleRow(displayIndex);
                if (rowView is Grid rowGrid)
                {
                    foreach (var cell in rowGrid.Children)
                    {
                        if (cell is Grid cellGrid)
                        {
                            var colIndex = Grid.GetColumn(cellGrid) + frozenColumns.Count;
                            UpdateCellBackground(cellGrid, rowIndex, colIndex, isSelected, isAlternate);
                        }
                    }
                }

                // Also update frozen virtualizing panel if present
                if (_frozenVirtualizingPanel != null)
                {
                    var frozenRowView = _frozenVirtualizingPanel.GetVisibleRow(displayIndex);
                    if (frozenRowView is Grid frozenRowGrid)
                    {
                        foreach (var cell in frozenRowGrid.Children)
                        {
                            if (cell is Grid cellGrid)
                            {
                                var colIndex = Grid.GetColumn(cellGrid);
                                UpdateCellBackground(cellGrid, rowIndex, colIndex, isSelected, isAlternate);
                            }
                        }
                    }
                }
            }
            else
            {
                // Non-virtualized: cells are direct children of dataGrid/frozenDataGrid
                foreach (var child in frozenDataGrid.Children)
                {
                    if (child is Grid cellGrid && Grid.GetRow(cellGrid) == displayIndex)
                    {
                        var colIndex = Grid.GetColumn(cellGrid);
                        UpdateCellBackground(cellGrid, rowIndex, colIndex, isSelected, isAlternate);
                    }
                }

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

    private static string GetSortIndicatorText(DataGridColumn column)
    {
        return column.SortDirection switch
        {
            SortDirection.Ascending => "▲",
            SortDirection.Descending => "▼",
            _ => "⇅" // Neutral indicator showing column is sortable
        };
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

        // Create edit control
        _currentEditControl = column.CreateEditContent(item);
        if (_currentEditControl == null)
            return;

        // Find the cell container before setting editing state to avoid half-initialized state
        var cellContainer = FindCellContainer(rowIndex, colIndex);
        if (cellContainer == null || cellContainer.Children.Count == 0)
        {
            _currentEditControl = null;
            return;
        }

        _editingItem = item;
        _editingColumn = column;
        _editingRowIndex = rowIndex;
        _editingColumnIndex = colIndex;
        _originalEditValue = column.GetCellValue(item);

        // Replace cell content with edit control (Children.Count > 0 guaranteed by early return above)
        {
            // Store original content for restoration on cancel
            _originalCellContent = cellContainer.Children[0] as View;
            cellContainer.Children.Clear();
            cellContainer.Children.Add(_currentEditControl);

            // Set appropriate background for edit mode (ensure contrast)
            cellContainer.BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#2D2D2D")
                : Color.FromArgb("#FFFFFF");

            // Apply theme-appropriate styling to edit controls
            ApplyEditControlStyling(_currentEditControl);

            // Focus the edit control
            if (_currentEditControl is VisualElement ve)
            {
                // Delay focus to ensure the control is fully loaded
                Dispatcher.Dispatch(() => ve.Focus());
            }

            // Wire up commit/cancel for edit controls
            // Note: Picker-based controls (Picker, DatePicker, TimePicker) should NOT use Unfocused
            // because opening the dropdown causes focus to transfer to the popup, triggering Unfocused
            if (_currentEditControl is Entry entry)
            {
                entry.Completed += OnEditEntryCompleted;
                // Note: We intentionally don't use Unfocused for Entry because:
                // 1. Native context menus (Cut/Copy/Paste) steal focus, causing premature commits
                // 2. This matches the approach for Picker/DatePicker/TimePicker
                // Instead, we commit when: Enter pressed, another cell clicked, or Tab pressed
                WireUpEntryEscapeKey(entry);
            }
            else if (_currentEditControl is CheckBox checkBox)
            {
                checkBox.CheckedChanged += OnEditCheckBoxChanged;
            }
            else if (_currentEditControl is DatePicker datePicker)
            {
                // Use DateSelected instead of Unfocused - Unfocused fires when dropdown opens
                datePicker.DateSelected += OnDatePickerDateSelected;
                WireUpPickerEscapeKey(datePicker);
            }
            else if (_currentEditControl is TimePicker timePicker)
            {
                // TimePicker doesn't have a selection event, so we use PropertyChanged
                timePicker.PropertyChanged += OnTimePickerPropertyChanged;
                WireUpPickerEscapeKey(timePicker);
            }
            else if (_currentEditControl is Picker picker)
            {
                // Use SelectedIndexChanged instead of Unfocused (fallback for standard Picker)
                picker.SelectedIndexChanged += OnPickerSelectedIndexChanged;
                WireUpPickerEscapeKey(picker);
            }
            else if (_currentEditControl is ComboBox comboBox)
            {
                // ComboBox in popup mode - wire up popup request and selection events
                comboBox.PopupRequested += OnComboBoxPopupRequested;
                comboBox.SelectionChanged += OnComboBoxSelectionChanged;

                // Store references for popup handling
                _activeComboBox = comboBox;
                _activeComboBoxColumn = column as DataGridComboBoxColumn;
                _activeComboBoxItem = item;

                // Automatically trigger popup when ComboBox is in PopupMode
                if (comboBox.PopupMode)
                {
                    Dispatcher.Dispatch(() => comboBox.Open());
                }
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
            CellEditStartedCommand.Execute(CellEditStartedCommandParameter ?? startedArgs);
        }
    }

    private void ComputeGridValidationErrors()
    {
        _gridValidationErrors.Clear();

        if (IsRequired && _sortedItems.Count == 0)
        {
            _gridValidationErrors.Add(RequiredErrorMessage);
        }

        foreach (var ((rowIndex, colIndex), cellResult) in _cellValidationErrors)
        {
            if (rowIndex >= _sortedItems.Count)
                continue;

            var colHeader = colIndex < _columns.Count ? _columns[colIndex].Header : $"Column {colIndex}";
            foreach (var error in cellResult.Errors)
            {
                _gridValidationErrors.Add($"Row {rowIndex + 1}, {colHeader}: {error}");
            }
        }
    }

    private void UpdateGridValidationState()
    {
        var previousIsValid = IsValid;

        ComputeGridValidationErrors();

        IsValid = _gridValidationErrors.Count == 0;
        OnPropertyChanged(nameof(ValidationErrors));

        if (previousIsValid != IsValid)
        {
            ValidationChanged?.Invoke(this, IsValid);
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
                _cellValidationErrors[(_editingRowIndex, _editingColumnIndex)] = ValidationResult.Failure(validationArgs.ErrorMessage ?? "Validation failed");
                UpdateGridValidationState();

                // Use targeted refresh when virtualization is active to avoid full panel rebuild
                if (_virtualizingPanel != null && _editingRowIndex >= 0 && _editingColumnIndex >= 0)
                    RefreshVirtualizedCell(_editingRowIndex, _editingColumnIndex);
                else
                    BuildDataRows();

                return;
            }
            else
            {
                _cellValidationErrors.Remove((_editingRowIndex, _editingColumnIndex));
                UpdateGridValidationState();
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
            CellEditEndedCommand.Execute(CellEditEndedCommandParameter ?? endedArgs);
        }

        EndEdit();
    }

    private void CancelEditInternal()
    {
        // Fire CellEditCancelled event and command before cleaning up
        if (_editingItem != null && _editingColumn != null)
        {
            var cancelledArgs = new DataGridCellEditEventArgs(
                _editingItem,
                _editingColumn,
                _editingRowIndex,
                _editingColumnIndex,
                _originalEditValue);

            CellEditCancelled?.Invoke(this, cancelledArgs);

            if (CellEditCancelledCommand?.CanExecute(cancelledArgs) == true)
            {
                CellEditCancelledCommand.Execute(CellEditCancelledCommandParameter ?? cancelledArgs);
            }
        }

        EndEdit();
    }

    private void EndEdit()
    {
        // Unhook event handlers
        if (_currentEditControl is Entry entry)
        {
            entry.Completed -= OnEditEntryCompleted;
        }
        else if (_currentEditControl is CheckBox checkBox)
        {
            checkBox.CheckedChanged -= OnEditCheckBoxChanged;
        }
        else if (_currentEditControl is DatePicker datePicker)
        {
            datePicker.DateSelected -= OnDatePickerDateSelected;
        }
        else if (_currentEditControl is TimePicker timePicker)
        {
            timePicker.PropertyChanged -= OnTimePickerPropertyChanged;
        }
        else if (_currentEditControl is Picker picker)
        {
            picker.SelectedIndexChanged -= OnPickerSelectedIndexChanged;
        }
        else if (_currentEditControl is ComboBox comboBox)
        {
            comboBox.PopupRequested -= OnComboBoxPopupRequested;
            comboBox.SelectionChanged -= OnComboBoxSelectionChanged;
        }

        // Hide ComboBox popup if visible
        HideComboBoxPopup();

        // Fire RowEditEnded if we have edited columns in this row
        if (_editingItem != null && _editedColumnsInRow.Count > 0)
        {
            var rowArgs = new DataGridRowEditEventArgs(_editingItem, _editingRowIndex, _editedColumnsInRow.ToList());
            RowEditEnded?.Invoke(this, rowArgs);

            if (RowEditEndedCommand?.CanExecute(rowArgs) == true)
            {
                RowEditEndedCommand.Execute(RowEditEndedCommandParameter ?? rowArgs);
            }

            _editedColumnsInRow.Clear();
        }

        var editedRowIndex = _editingRowIndex;
        var editedColIndex = _editingColumnIndex;

        _editingItem = null;
        _editingColumn = null;
        _editingRowIndex = -1;
        _editingColumnIndex = -1;
        _currentEditControl = null;
        _originalEditValue = null;
        _originalCellContent = null;

        // Use targeted refresh when virtualization is active to avoid full panel rebuild
        if (_virtualizingPanel != null && editedRowIndex >= 0 && editedColIndex >= 0)
        {
            RefreshVirtualizedCell(editedRowIndex, editedColIndex);
            return;
        }

        BuildDataRows();
    }

    private void RefreshVirtualizedCell(int rowIndex, int colIndex)
    {
        var frozenColumns = GetFrozenColumns();
        var scrollableColumns = GetScrollableColumns();
        var isFrozen = colIndex < frozenColumns.Count;
        var panel = isFrozen ? _frozenVirtualizingPanel : _virtualizingPanel;
        var columns = isFrozen ? frozenColumns : scrollableColumns;
        var adjustedColIndex = isFrozen ? colIndex : colIndex - frozenColumns.Count;

        var rowView = panel?.GetVisibleRow(rowIndex);
        if (rowView is not Grid rowGrid)
            return;

        if (rowIndex < 0 || rowIndex >= _sortedItems.Count)
            return;

        var item = _sortedItems[rowIndex];
        if (item == null || adjustedColIndex < 0 || adjustedColIndex >= columns.Count)
            return;

        var column = columns[adjustedColIndex];
        var isSelected = _selectedItems.Contains(item);
        var isAlternate = rowIndex % 2 == 1;
        var newCell = CreateDataCell(item, column, rowIndex, colIndex, isSelected, isAlternate);

        // Find and replace the existing cell at this column.
        // If not found the row was recycled and will render correctly when it returns to view.
        for (int i = 0; i < rowGrid.Children.Count; i++)
        {
            if (rowGrid.Children[i] is Grid cellGrid && Grid.GetColumn(cellGrid) == adjustedColIndex)
            {
                rowGrid.Children.RemoveAt(i);
                rowGrid.Children.Insert(i, newCell);
                Grid.SetColumn(newCell, adjustedColIndex);
                return;
            }
        }
    }

    private static object? GetValueFromEditControl(View control, DataGridColumn column)
    {
        return control switch
        {
            Entry entry => entry.Text,
            CheckBox checkBox => checkBox.IsChecked,
            ComboBox comboBox when column is DataGridComboBoxColumn comboColumn => comboColumn.GetValueFromEditControl(control),
            Picker picker when column is DataGridComboBoxColumn comboColumn => comboColumn.GetValueFromEditControl(control),
            DatePicker datePicker when column is DataGridDatePickerColumn dateColumn => dateColumn.GetValueFromEditControl(control),
            TimePicker timePicker when column is DataGridTimePickerColumn timeColumn => timeColumn.GetValueFromEditControl(control),
            _ => null
        };
    }

    private Grid? FindCellContainer(int rowIndex, int colIndex)
    {
        // Virtualized path: cells live inside the virtualizing panels, not the static grids
        if (_virtualizingPanel != null)
            return FindCellContainerVirtualized(rowIndex, colIndex);

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

    private Grid? FindCellContainerVirtualized(int rowIndex, int colIndex)
    {
        var frozenColumns = GetFrozenColumns();
        var isFrozen = colIndex < frozenColumns.Count;
        var panel = isFrozen ? _frozenVirtualizingPanel : _virtualizingPanel;
        var adjustedColIndex = isFrozen ? colIndex : colIndex - frozenColumns.Count;

        var rowView = panel?.GetVisibleRow(rowIndex);
        if (rowView is not Grid rowGrid)
            return null;

        foreach (var child in rowGrid.Children)
        {
            if (child is Grid cellGrid && Grid.GetColumn(cellGrid) == adjustedColIndex)
                return cellGrid;
        }

        return null;
    }

    private void OnEditEntryCompleted(object? sender, EventArgs e)
    {
        CommitEdit();
        MoveFocusDown();
    }

    private void OnEditCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        // Commit immediately for checkbox changes
        CommitEdit();
    }

    private void OnDatePickerDateSelected(object? sender, DateChangedEventArgs e)
    {
        // Commit when a date is selected
        CommitEdit();
    }

    private void OnTimePickerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Commit when the Time property changes (user selected a time)
        if (e.PropertyName == nameof(TimePicker.Time) && _editingItem != null)
        {
            // Small delay to allow the picker to close
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
            {
                if (_editingItem != null)
                {
                    CommitEdit();
                }
            });
        }
    }

    private void OnPickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        // Commit when a selection is made in the picker
        if (_editingItem != null)
        {
            CommitEdit();
        }
    }

    private void WireUpEntryEscapeKey(Entry entry)
    {
#if WINDOWS
        void WireUpKeyHandler()
        {
            if (entry.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
            {
                textBox.KeyDown += (sender, args) =>
                {
                    if (args.Key == Windows.System.VirtualKey.Escape)
                    {
                        Dispatcher.Dispatch(() => CancelEdit());
                        args.Handled = true;
                    }
                };
            }
        }

        // Handler might already be set, check immediately
        if (entry.Handler != null)
        {
            WireUpKeyHandler();
        }
        else
        {
            // Wait for handler to be set
            entry.HandlerChanged += (s, e) => WireUpKeyHandler();
        }
#elif MACCATALYST || IOS
        // On Mac/iOS, ESC key handling is typically done through keyboard commands
        // The default behavior should work through the keyboard handler
#endif
    }

    private void ApplyEditControlStyling(View control)
    {
        var isDarkTheme = Application.Current?.RequestedTheme == AppTheme.Dark;
        var textColor = isDarkTheme ? Colors.White : Colors.Black;
        var bgColor = isDarkTheme ? Color.FromArgb("#2D2D2D") : Colors.White;

        switch (control)
        {
            case Entry entry:
                entry.TextColor = textColor;
                entry.BackgroundColor = bgColor;
                break;
            case DatePicker datePicker:
                datePicker.TextColor = textColor;
                datePicker.BackgroundColor = bgColor;
                break;
            case TimePicker timePicker:
                timePicker.TextColor = textColor;
                timePicker.BackgroundColor = bgColor;
                break;
            case Picker picker:
                picker.TextColor = textColor;
                picker.BackgroundColor = bgColor;
                break;
        }
    }

    private void WireUpPickerEscapeKey(View picker)
    {
#if WINDOWS
        void WireUpKeyHandler()
        {
            if (picker.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement element)
            {
                element.KeyDown += (sender, args) =>
                {
                    if (args.Key == Windows.System.VirtualKey.Escape)
                    {
                        Dispatcher.Dispatch(() => CancelEdit());
                        args.Handled = true;
                    }
                };
            }
        }

        if (picker.Handler != null)
        {
            WireUpKeyHandler();
        }
        else
        {
            picker.HandlerChanged += (s, e) => WireUpKeyHandler();
        }
#endif
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
            PageChangedCommand.Execute(PageChangedCommandParameter ?? args);
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
                if (ColumnResizingCommand?.CanExecute(args) == true)
                {
                    ColumnResizingCommand.Execute(ColumnResizingCommandParameter ?? args);
                }
                break;

            case GestureStatus.Running:
                // Manual resize converts Fill/FitHeader → Fixed
                if (column.SizeMode is DataGridColumnSizeMode.Fill or DataGridColumnSizeMode.FitHeader)
                    column.SizeMode = DataGridColumnSizeMode.Fixed;

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

    private GridLength ResolveColumnWidth(DataGridColumn column)
    {
        return column.SizeMode switch
        {
            DataGridColumnSizeMode.Fixed => new GridLength(Math.Clamp(column.Width, column.MinWidth, column.MaxWidth)),
            DataGridColumnSizeMode.FitHeader => column.ActualWidth > 0
                ? new GridLength(Math.Clamp(column.ActualWidth, column.MinWidth, column.MaxWidth))
                : GridLength.Auto, // Auto until SyncColumnWidths locks to actual rendered width
            DataGridColumnSizeMode.Fill => column.ActualWidth > 0
                ? new GridLength(column.ActualWidth)
                : new GridLength(column.MinWidth), // Use MinWidth until first SizeChanged distributes
            _ => column.Width < 0 ? GridLength.Auto : new GridLength(column.Width), // Auto (backward compat)
        };
    }

    private double MeasureHeaderWidth(DataGridColumn column)
    {
        var headerText = column.Header ?? string.Empty;
        double fontSize = 14;

        // Try Label.Measure first — works when a handler is available
        var label = new Label
        {
            Text = headerText,
            FontSize = fontSize,
            FontAttributes = FontAttributes.Bold,
            MaxLines = 1,
            LineBreakMode = LineBreakMode.NoWrap
        };

        var measured = label.Measure(double.PositiveInfinity, double.PositiveInfinity);
        var textWidth = measured.Width;

        // Fallback: Label.Measure returns 0 for disconnected elements (no handler).
        // Estimate using average bold character width for proportional fonts (Segoe UI / SF Pro).
        if (textWidth <= 0 && headerText.Length > 0)
            textWidth = Math.Ceiling(headerText.Length * fontSize * 0.62);

        // Add padding for contentGrid (8 left + 8 right)
        double padding = 16;

        // Add space for filter icon container if filterable
        if (column.CanUserFilter)
            padding += 48; // 44px min width + 4px margin

        // Add space for sort indicator if sortable
        if (column.CanUserSort)
            padding += 18; // ~14px text + 4px margin

        // Add space for resize grip if resizable
        if (column.CanUserResize)
            padding += 6;

        return textWidth + padding;
    }

    private void PreMeasureFitHeaderColumns()
    {
        var visibleColumns = GetVisibleColumns();
        foreach (var column in visibleColumns)
        {
            if (column.SizeMode != DataGridColumnSizeMode.FitHeader)
                continue;

            var measured = MeasureHeaderWidth(column);
            var clamped = Math.Clamp(measured, column.MinWidth, column.MaxWidth);
            column.ActualWidth = clamped;
            column.Width = clamped;
        }
    }

    /// <summary>
    /// Calls <see cref="DistributeFillColumnWidths"/> with the <see cref="_isDistributingFill"/>
    /// re-entrancy guard set. Use this from event handlers that may be re-triggered synchronously
    /// by layout invalidation (e.g. SizeChanged on WinUI).
    /// </summary>
    private void DistributeFillColumnWidthsGuarded()
    {
        if (_isDistributingFill)
            return;

        _isDistributingFill = true;
        try
        {
            DistributeFillColumnWidths();
        }
        finally
        {
            _isDistributingFill = false;
        }
    }

    private void DistributeFillColumnWidths()
    {
        var visibleColumns = GetVisibleColumns();
        var frozenColumns = GetFrozenColumns();
        var scrollableColumns = GetScrollableColumns();

        // Fill only applies to scrollable columns; frozen columns auto-size
        var fillColumns = scrollableColumns.Where(c => c.SizeMode == DataGridColumnSizeMode.Fill).ToList();
        if (fillColumns.Count == 0)
            return;

        // Use the inner content area width (dataContainer), not the outer Width which
        // includes Border StrokeThickness/padding. Using this.Width causes a feedback loop
        // in content-sized layouts: the border overhead is mistaken for "extra space",
        // Fill columns grow, content grows, Width grows, ad infinitum.
        var totalWidth = dataContainer.Width;
        if (totalWidth <= 0 || double.IsNaN(totalWidth) || double.IsInfinity(totalWidth))
            return;

        _lastDistributionWidth = totalWidth;

        // Calculate width consumed by frozen columns
        double frozenConsumedWidth = 0;
        foreach (var column in frozenColumns)
        {
            if (column.ActualWidth > 0)
                frozenConsumedWidth += column.ActualWidth;
            else if (column.Width >= 0)
                frozenConsumedWidth += column.Width;
            else
                frozenConsumedWidth += column.MinWidth;
        }

        // Calculate width consumed by non-fill scrollable columns
        double scrollableConsumedWidth = 0;
        foreach (var column in scrollableColumns)
        {
            if (column.SizeMode == DataGridColumnSizeMode.Fill)
                continue;

            if (column.ActualWidth > 0)
                scrollableConsumedWidth += column.ActualWidth;
            else if (column.Width >= 0)
                scrollableConsumedWidth += column.Width;
            else
                scrollableConsumedWidth += column.MinWidth;
        }

        var remainingWidth = Math.Max(0, totalWidth - frozenConsumedWidth - scrollableConsumedWidth);
        if (remainingWidth <= 0)
            return;

        // Two-pass distribution to respect MinWidth/MaxWidth without overflow
        // Pass 1: assign MinWidth to all Fill columns, compute surplus
        double totalMinWidth = fillColumns.Sum(c => c.MinWidth);
        double surplus = remainingWidth - totalMinWidth;

        if (surplus <= 0)
        {
            // Not enough space — everyone gets MinWidth
            foreach (var column in fillColumns)
            {
                column.ActualWidth = column.MinWidth;
                var columnIndex = visibleColumns.IndexOf(column);
                if (columnIndex >= 0)
                    UpdateColumnWidth(columnIndex, column.MinWidth);
            }
            return;
        }

        // Pass 2: distribute surplus proportionally, capping at MaxWidth
        var remaining = new List<DataGridColumn>(fillColumns);
        var assigned = new Dictionary<DataGridColumn, double>();
        foreach (var column in fillColumns)
            assigned[column] = column.MinWidth;

        double surplusToDistribute = surplus;

        while (remaining.Count > 0 && surplusToDistribute > 0.5)
        {
            double totalWeight = 0;
            foreach (var column in remaining)
            {
                var weight = column.Width > 0 ? column.Width : 1;
                totalWeight += weight;
            }

            if (totalWeight <= 0)
                break;

            var capped = new List<DataGridColumn>();
            double usedSurplus = 0;

            foreach (var column in remaining)
            {
                var weight = column.Width > 0 ? column.Width : 1;
                var share = surplusToDistribute * (weight / totalWeight);
                var proposed = assigned[column] + share;

                if (proposed >= column.MaxWidth)
                {
                    usedSurplus += column.MaxWidth - assigned[column];
                    assigned[column] = column.MaxWidth;
                    capped.Add(column);
                }
                else
                {
                    usedSurplus += share;
                    assigned[column] = proposed;
                }
            }

            if (capped.Count == 0)
                break; // No columns hit MaxWidth — distribution is complete

            surplusToDistribute -= usedSurplus;
            foreach (var c in capped)
                remaining.Remove(c);
        }

        // Apply computed widths
        foreach (var column in fillColumns)
        {
            column.ActualWidth = assigned[column];
            var columnIndex = visibleColumns.IndexOf(column);
            if (columnIndex >= 0)
                UpdateColumnWidth(columnIndex, assigned[column]);
        }
    }

    private void OnDataContainerSizeChanged(object? sender, EventArgs e)
    {
        if (_isUpdating || _isDistributingFill)
            return;

        var width = dataContainer.Width;
        if (width <= 0 || double.IsNaN(width) || double.IsInfinity(width))
            return;

        // First-time sync after BuildGrid — run full SynchronizeColumnWidths
        if (_pendingFillSync)
        {
            _pendingFillSync = false;
            Dispatcher.Dispatch(() =>
            {
                if (!_isUpdating && !_isDistributingFill)
                    SynchronizeColumnWidths();
            });
            return;
        }

        // Subsequent resize — redistribute Fill columns when container width changes
        if (Math.Abs(width - _lastDistributionWidth) < 1.0)
            return;

        var visibleColumns = GetVisibleColumns();
        if (!visibleColumns.Any(c => c.SizeMode == DataGridColumnSizeMode.Fill))
            return;

        Dispatcher.Dispatch(() =>
        {
            if (!_isUpdating && !_isDistributingFill)
            {
                DistributeFillColumnWidthsGuarded();
                SyncVirtualizedRowColumnWidths();
            }
        });
    }

    private void OnDataGridSizeChanged(object? sender, EventArgs e)
    {
        if (_isUpdating || _isDistributingFill)
            return;

        var currentWidth = dataContainer.Width;
        if (currentWidth <= 0 || double.IsNaN(currentWidth) || double.IsInfinity(currentWidth))
            return;

        // Skip if width hasn't meaningfully changed (avoids redundant dispatches)
        if (Math.Abs(currentWidth - _lastDistributionWidth) < 1.0)
            return;

        var visibleColumns = GetVisibleColumns();
        if (!visibleColumns.Any(c => c.SizeMode == DataGridColumnSizeMode.Fill))
            return;

        // CRITICAL: defer column width changes off the layout pass.
        // Modifying ColumnDefinition.Width during SizeChanged causes
        // LayoutCycleException on WinUI.
        Dispatcher.Dispatch(() =>
        {
            if (!_isUpdating && !_isDistributingFill)
            {
                DistributeFillColumnWidthsGuarded();
                SyncVirtualizedRowColumnWidths();
            }
        });
    }

    private void UpdateColumnWidth(int columnIndex, double newWidth)
    {
        var frozenCount = GetFrozenColumns().Count;
        var isFrozen = columnIndex < frozenCount;
        var adjustedIndex = isFrozen ? columnIndex : columnIndex - frozenCount;

        // Update header grid (skip if unchanged to avoid layout invalidation)
        var headerDefs = isFrozen ? frozenHeaderGrid.ColumnDefinitions : headerGrid.ColumnDefinitions;
        if (adjustedIndex < headerDefs.Count && !IsGridLengthEqual(headerDefs[adjustedIndex].Width, newWidth))
            headerDefs[adjustedIndex].Width = new GridLength(newWidth);

        // Update data grid
        var dataDefs = isFrozen ? frozenDataGrid.ColumnDefinitions : dataGrid.ColumnDefinitions;
        if (adjustedIndex < dataDefs.Count && !IsGridLengthEqual(dataDefs[adjustedIndex].Width, newWidth))
            dataDefs[adjustedIndex].Width = new GridLength(newWidth);

        // Update footer grid if visible
        if (ShowFooter)
        {
            var footerDefs = isFrozen ? frozenFooterGrid.ColumnDefinitions : footerGrid.ColumnDefinitions;
            if (adjustedIndex < footerDefs.Count && !IsGridLengthEqual(footerDefs[adjustedIndex].Width, newWidth))
                footerDefs[adjustedIndex].Width = new GridLength(newWidth);
        }
    }

    private static bool IsGridLengthEqual(GridLength current, double newValue)
        => current.IsAbsolute && Math.Abs(current.Value - newValue) < 0.5;

    private void ScheduleColumnWidthSync()
    {
        var width = dataContainer.Width;
        if (width > 0 && !double.IsNaN(width) && !double.IsInfinity(width))
        {
            // Width already valid (subsequent rebuilds) — sync on next dispatch
            Dispatcher.Dispatch(SynchronizeColumnWidths);
            return;
        }

        // Width not yet available — let dataContainer.SizeChanged trigger sync.
        // Fallback timer in case SizeChanged never fires.
        _pendingFillSync = true;
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
        {
            // Re-check all guards inside the callback — OnDataContainerSizeChanged
            // may have already cleared _pendingFillSync and dispatched its own sync.
            if (_pendingFillSync && !_isUpdating && !_isDistributingFill)
            {
                _pendingFillSync = false;
                SynchronizeColumnWidths();
            }
        });
    }

    private void SynchronizeColumnWidths()
    {
        if (_isUpdating || _isDistributingFill)
            return;

        var frozenColumns = GetFrozenColumns();
        var scrollableColumns = GetScrollableColumns();

        // Sync frozen columns
        SyncColumnWidthsBetweenGrids(frozenHeaderGrid, frozenDataGrid, frozenFooterGrid, frozenColumns);

        // Sync scrollable columns
        SyncColumnWidthsBetweenGrids(headerGrid, dataGrid, footerGrid, scrollableColumns);

        // Re-distribute Fill columns now that Auto columns have accurate ActualWidth
        var visibleColumns = GetVisibleColumns();
        if (visibleColumns.Any(c => c.SizeMode == DataGridColumnSizeMode.Fill))
            DistributeFillColumnWidthsGuarded();

        // Propagate final column widths to all visible virtualized row Grids.
        // Virtualized rows have independent ColumnDefinitions that don't auto-sync
        // with the header/footer grids — they must be updated explicitly.
        SyncVirtualizedRowColumnWidths();
    }

    private void SyncColumnWidthsBetweenGrids(Grid headerGridRef, Grid dataGridRef, Grid footerGridRef, List<DataGridColumn> columns)
    {
        if (headerGridRef.ColumnDefinitions.Count == 0)
            return;

        // When virtualized, dataGrid is empty — use header-only widths for FitHeader columns.
        bool hasDataGrid = dataGridRef.ColumnDefinitions.Count > 0;

        for (int i = 0; i < columns.Count && i < headerGridRef.ColumnDefinitions.Count; i++)
        {
            var column = columns[i];

            // Skip columns with explicit widths (not Auto) and Fill (proportionally distributed).
            // FitHeader columns use Auto initially and get locked here after first layout.
            if (column.SizeMode == DataGridColumnSizeMode.Fill)
                continue;
            if (column.SizeMode != DataGridColumnSizeMode.FitHeader && column.Width >= 0)
                continue;

            // Get the actual rendered widths
            var headerWidth = GetColumnActualWidth(headerGridRef, i);
            var dataWidth = hasDataGrid && i < dataGridRef.ColumnDefinitions.Count
                ? GetColumnActualWidth(dataGridRef, i)
                : 0;

            // Use the maximum width to ensure alignment
            var maxWidth = Math.Max(headerWidth, dataWidth);

            // Only update if we have a valid width
            if (maxWidth > 0)
            {
                var newGridLength = new GridLength(maxWidth);
                headerGridRef.ColumnDefinitions[i].Width = newGridLength;

                if (hasDataGrid && i < dataGridRef.ColumnDefinitions.Count)
                    dataGridRef.ColumnDefinitions[i].Width = newGridLength;

                // Update footer if visible
                if (ShowFooter && i < footerGridRef.ColumnDefinitions.Count)
                    footerGridRef.ColumnDefinitions[i].Width = newGridLength;

                // Update the column's actual width
                column.ActualWidth = maxWidth;
            }
        }
    }

    private void SyncVirtualizedRowColumnWidths()
    {
        var scrollableColumns = GetScrollableColumns();
        var frozenColumns = GetFrozenColumns();

        if (_virtualizingPanel != null)
            SyncPanelRowColumnWidths(_virtualizingPanel, scrollableColumns);
        if (_frozenVirtualizingPanel != null)
            SyncPanelRowColumnWidths(_frozenVirtualizingPanel, frozenColumns);
    }

    private void SyncPanelRowColumnWidths(VirtualizingDataGridPanel panel, List<DataGridColumn> columns)
    {
        // Snapshot children — layout invalidation from width changes could trigger
        // UpdateVisibleRows which modifies the live Children collection.
        var snapshot = panel.Children.ToList();
        foreach (var child in snapshot)
        {
            if (child is Grid rowGrid)
                SyncRowGridColumnWidths(rowGrid, columns);
        }
    }

    private void SyncRowGridColumnWidths(Grid rowGrid, List<DataGridColumn> columns)
    {
        for (int i = 0; i < columns.Count && i < rowGrid.ColumnDefinitions.Count; i++)
        {
            var newWidth = ResolveColumnWidth(columns[i]);
            var current = rowGrid.ColumnDefinitions[i].Width;

            // Skip if unchanged to avoid unnecessary layout invalidation
            if (newWidth.IsAbsolute && current.IsAbsolute && Math.Abs(newWidth.Value - current.Value) < 0.5)
                continue;
            if (newWidth.GridUnitType == current.GridUnitType && Math.Abs(newWidth.Value - current.Value) < 0.5)
                continue;

            rowGrid.ColumnDefinitions[i].Width = newWidth;
        }
    }

    private double GetColumnActualWidth(Grid grid, int columnIndex)
    {
        // Find the widest child in this column
        double maxWidth = 0;

        foreach (var child in grid.Children)
        {
            if (child is View view && Grid.GetColumn(view) == columnIndex)
            {
                var width = view.Width;
                if (width > 0 && width > maxWidth)
                    maxWidth = width;
            }
        }

        return maxWidth;
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

            if (ColumnReorderedCommand?.CanExecute(args) == true)
            {
                ColumnReorderedCommand.Execute(ColumnReorderedCommandParameter ?? args);
            }

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
        // If we're editing a different cell, commit the edit first
        if (_editingItem != null && (_editingRowIndex != rowIndex || _editingColumnIndex != colIndex))
        {
            CommitEdit();
        }

        SelectItem(item);
        _focusedColumnIndex = colIndex;

        // Focus the grid to enable keyboard shortcuts (F2, ESC, arrow keys, etc.)
        this.Focus();

        var args = new DataGridCellEventArgs(item, column, rowIndex, colIndex);
        CellTapped?.Invoke(this, args);

        if (CellTappedCommand?.CanExecute(args) == true)
        {
            CellTappedCommand.Execute(CellTappedCommandParameter ?? args);
        }

        // Check if we should begin edit
        if (EditTrigger == DataGridEditTrigger.SingleTap && CanUserEdit && column.CanUserEdit && !column.IsReadOnly)
        {
            BeginEditInternal(item, column, rowIndex, colIndex);
        }
    }

    private void OnCellDoubleTapped(object item, DataGridColumn column, int rowIndex, int colIndex)
    {
        // If we're editing a different cell, commit the edit first
        if (_editingItem != null && (_editingRowIndex != rowIndex || _editingColumnIndex != colIndex))
        {
            CommitEdit();
        }

        var args = new DataGridCellEventArgs(item, column, rowIndex, colIndex);
        CellDoubleTapped?.Invoke(this, args);

        if (CellDoubleTappedCommand?.CanExecute(args) == true)
        {
            CellDoubleTappedCommand.Execute(CellDoubleTappedCommandParameter ?? args);
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
        if (_virtualizingPanel != null)
        {
            // Auto-commit any active edit before recycling rows to prevent data loss
            if (_editingItem != null && _editingRowIndex >= 0)
            {
                var newFirstVisible = Math.Max(0, (int)(e.ScrollY / _virtualizingPanel.RowHeight) - _virtualizingPanel.BufferSize);
                var newLastVisible = Math.Min((_sortedItems?.Count ?? 0) - 1,
                    (int)((e.ScrollY + dataScrollView.Height) / _virtualizingPanel.RowHeight) + _virtualizingPanel.BufferSize);

                if (_editingRowIndex < newFirstVisible || _editingRowIndex > newLastVisible)
                {
                    CommitEdit();
                }
            }

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
            FilteringCommand.Execute(FilteringCommandParameter ?? filteringArgs);
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
            FilteredCommand.Execute(FilteredCommandParameter ?? filteredArgs);
        }
    }

    private void OnFilterPopupCancelled(object? sender, EventArgs e)
    {
        HideFilterPopup();
    }

    private void OnComboBoxPopupRequested(object? sender, ComboBoxPopupRequestEventArgs e)
    {
        ShowComboBoxPopup(e);
    }

    private void OnComboBoxSelectionChanged(object? sender, object? selectedItem)
    {
        // Selection made through inline dropdown (non-popup mode)
        // Commit the edit
        if (_editingItem != null && sender is ComboBox comboBox && !comboBox.PopupMode)
        {
            CommitEdit();
        }
    }

    private void OnComboBoxOverlayTapped(object? sender, EventArgs e)
    {
        HideComboBoxPopup();
        CancelEdit();
    }

    private void OnComboBoxPopupItemSelected(object? sender, object? selectedItem)
    {
        if (_activeComboBox != null && selectedItem != null)
        {
            _activeComboBox.SetSelectedItemFromPopup(selectedItem);
        }
        HideComboBoxPopup();
        CommitEdit();
    }

    private void OnComboBoxPopupCancelled(object? sender, EventArgs e)
    {
        HideComboBoxPopup();
        CancelEdit();
    }

    private void ShowComboBoxPopup(ComboBoxPopupRequestEventArgs e)
    {
        // Reset search text first
        comboBoxPopup.Reset();

        // Configure the popup content
        comboBoxPopup.DisplayMemberPath = e.DisplayMemberPath;
        comboBoxPopup.DisplayMemberFunc = e.DisplayMemberFunc;
        comboBoxPopup.ItemsSource = e.ItemsSource;  // This populates filtered items
        comboBoxPopup.SelectedItem = e.SelectedItem;

        // Calculate position relative to the data grid
        PositionComboBoxPopup(e.AnchorBounds);

        // Show the overlay
        comboBoxPopupOverlay.IsVisible = true;

        // Focus the search entry after a brief delay to ensure rendering is complete
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), () =>
        {
            comboBoxPopup.Focus();
        });
    }

    private void HideComboBoxPopup()
    {
        comboBoxPopupOverlay.IsVisible = false;
        _activeComboBox = null;
        _activeComboBoxColumn = null;
        _activeComboBoxItem = null;
    }

    private void PositionComboBoxPopup(Rect anchorBounds)
    {
        var popupHeight = 280.0;
        var popupWidth = 250.0;

        // Get the DataGridView's actual dimensions
        var gridHeight = Height;
        var gridWidth = Width;

        // Calculate available space below and above the anchor
        var availableBelow = gridHeight - anchorBounds.Bottom - 10;
        var availableAbove = anchorBounds.Top - 10;

        // Determine vertical position - prefer below if there's enough space
        double top;
        if (availableBelow >= popupHeight || availableBelow >= availableAbove)
        {
            // Position below the anchor
            top = anchorBounds.Bottom + 2;
        }
        else
        {
            // Position above the anchor
            top = anchorBounds.Top - popupHeight - 2;
        }

        // Determine horizontal position - align with anchor, but keep within bounds
        var left = Math.Max(0, Math.Min(anchorBounds.Left, gridWidth - popupWidth - 10));

        // Ensure top is within bounds
        top = Math.Max(10, Math.Min(top, gridHeight - popupHeight - 10));

        comboBoxPopup.SetPosition(left, top);
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (pageSizePicker.SelectedItem is int newSize)
        {
            PageSize = newSize;
            CurrentPage = 1;
            // Use same fast-path routing as OnPaginationChanged
            if (_virtualizingPanel != null)
                UpdateVirtualizedDataForPageChange();
            else
                BuildDataRows(); // Row count changes, so TryUpdateDataRowsInPlace would return false
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

            grid._cellValidationErrors.Clear();
            grid.BuildGrid();
            grid.UpdateGridValidationState();
        }
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BuildGrid();
        UpdateGridValidationState();
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
            if (grid._virtualizingPanel != null)
                grid.UpdateVirtualizedDataForPageChange();
            else if (!grid.TryUpdateDataRowsInPlace())
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

    private void RaiseSelectionChanged(object? oldSelection = null)
    {
        var newSelection = GetSelection();
        var args = new Base.SelectionChangedEventArgs(oldSelection, newSelection);
        SelectionChanged?.Invoke(this, args);

        if (SelectionChangedCommand?.CanExecute(newSelection) == true)
        {
            SelectionChangedCommand.Execute(SelectionChangedCommandParameter ?? newSelection);
        }
    }

    private void RaiseRowSelected(object item, int rowIndex)
    {
        var args = new DataGridRowSelectionEventArgs(item, rowIndex);
        RowSelected?.Invoke(this, args);

        if (RowSelectedCommand?.CanExecute(args) == true)
        {
            RowSelectedCommand.Execute(RowSelectedCommandParameter ?? args);
        }
    }

    private void RaiseRowDeselected(object item, int rowIndex)
    {
        var args = new DataGridRowSelectionEventArgs(item, rowIndex);
        RowDeselected?.Invoke(this, args);

        if (RowDeselectedCommand?.CanExecute(args) == true)
        {
            RowDeselectedCommand.Execute(RowDeselectedCommandParameter ?? args);
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
    /// Identifies the <see cref="GotFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandParameterProperty = BindableProperty.Create(
        nameof(GotFocusCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the LostFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandParameterProperty = BindableProperty.Create(
        nameof(LostFocusCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the KeyPressCommand bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(DataGridView));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandParameterProperty = BindableProperty.Create(
        nameof(KeyPressCommandParameter),
        typeof(object),
        typeof(DataGridView));

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
            KeyPressCommand.Execute(KeyPressCommandParameter ?? e);
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
            "ArrowUp" => HandleUpKey(e),
            "ArrowDown" => HandleDownKey(e),
            "ArrowLeft" => HandleLeftKey(e),
            "ArrowRight" => HandleRightKey(e),
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
                new Base.KeyboardShortcut { Key = "ArrowUp", Description = "Move to previous row", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "ArrowDown", Description = "Move to next row", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "ArrowLeft", Description = "Move to previous column", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "ArrowRight", Description = "Move to next column", Category = "Navigation" },
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
            GotFocusCommand?.Execute(GotFocusCommandParameter ?? this);

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

        var oldSelection = GetSelection();

        // Track items to deselect in single mode
        var itemsToDeselect = new List<object>();
        if (SelectionMode == DataGridSelectionMode.Single)
        {
            itemsToDeselect.AddRange(_selectedItems.Where(i => i != item));
            _selectedItems.Clear();
        }

        var wasAlreadySelected = _selectedItems.Contains(item);
        if (!wasAlreadySelected)
        {
            _selectedItems.Add(item);
        }

        SelectedItem = item;
        RaiseSelectionChanged(oldSelection);
        UpdateSelectionVisuals();

        // Raise deselected events for items cleared in single mode
        foreach (var deselectedItem in itemsToDeselect)
        {
            var deselectedIndex = _sortedItems.IndexOf(deselectedItem);
            RaiseRowDeselected(deselectedItem, deselectedIndex);
        }

        // Raise selected event for newly selected item
        if (!wasAlreadySelected)
        {
            RaiseRowSelected(item, rowIndex);
        }
    }

    private void ToggleRowSelection(object item)
    {
        var oldSelection = GetSelection();
        var rowIndex = _sortedItems?.IndexOf(item) ?? -1;

        if (_selectedItems.Contains(item))
        {
            _selectedItems.Remove(item);
            RaiseSelectionChanged(oldSelection);
            UpdateSelectionVisuals();
            RaiseRowDeselected(item, rowIndex);
        }
        else
        {
            _selectedItems.Add(item);
            RaiseSelectionChanged(oldSelection);
            UpdateSelectionVisuals();
            RaiseRowSelected(item, rowIndex);
        }
    }

    /// <inheritdoc />
    public void ClearSelection()
    {
        if (ClearSelectionCommand?.CanExecute(null) == true)
        {
            ClearSelectionCommand.Execute(ClearSelectionCommandParameter);
            return;
        }

        var oldSelection = GetSelection();
        var itemsToDeselect = _selectedItems.ToList();
        _selectedItems.Clear();
        SelectedItem = null;
        RaiseSelectionChanged(oldSelection);
        UpdateSelectionVisuals();

        // Raise deselected events for all cleared items
        foreach (var item in itemsToDeselect)
        {
            var rowIndex = _sortedItems?.IndexOf(item) ?? -1;
            RaiseRowDeselected(item, rowIndex);
        }
    }

    /// <inheritdoc />
    public void SelectAll()
    {
        if (SelectionMode == DataGridSelectionMode.None)
            return;

        if (SelectAllCommand?.CanExecute(null) == true)
        {
            SelectAllCommand.Execute(SelectAllCommandParameter);
            return;
        }

        if (_sortedItems == null) return;

        var oldSelection = GetSelection();
        var previouslySelected = new HashSet<object>(_selectedItems);
        _selectedItems.Clear();

        if (SelectionMode == DataGridSelectionMode.Single)
        {
            // In single selection mode, select the first item
            if (_sortedItems.Count > 0)
            {
                var item = _sortedItems[0];
                if (item != null)
                {
                    _selectedItems.Add(item);
                    SelectedItem = item;
                    if (!previouslySelected.Contains(item))
                    {
                        RaiseRowSelected(item, 0);
                    }
                }
            }
        }
        else
        {
            // Multiple selection mode - select all items
            for (int i = 0; i < _sortedItems.Count; i++)
            {
                var item = _sortedItems[i];
                if (item != null)
                {
                    _selectedItems.Add(item);

                    // Raise selected event for newly selected items
                    if (!previouslySelected.Contains(item))
                    {
                        RaiseRowSelected(item, i);
                    }
                }
            }
        }

        RaiseSelectionChanged(oldSelection);
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
            var oldSelection = GetSelection();

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

                RaiseSelectionChanged(oldSelection);
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
