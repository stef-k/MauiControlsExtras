using MauiControlsExtras.Controls;

namespace MauiControlsExtras.ContextMenu;

/// <summary>
/// Provides data for the ContextMenuOpening event, allowing customization and cancellation of context menus.
/// </summary>
public class ContextMenuOpeningEventArgs : EventArgs
{
    /// <summary>
    /// Gets the collection of menu items. Modify this collection to add, remove, or change menu items before display.
    /// </summary>
    public ContextMenuItemCollection Items { get; }

    /// <summary>
    /// Gets the target element that triggered the context menu (if available).
    /// </summary>
    public object? TargetElement { get; }

    /// <summary>
    /// Gets the position where the context menu was requested.
    /// </summary>
    public Point Position { get; }

    /// <summary>
    /// Gets the view that the context menu is anchored to.
    /// </summary>
    public View? AnchorView { get; }

    /// <summary>
    /// Gets or sets whether to cancel showing the context menu.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets or sets whether the event has been handled.
    /// If true, the control will not show its default context menu.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Initializes a new instance of ContextMenuOpeningEventArgs.
    /// </summary>
    /// <param name="items">The initial collection of menu items.</param>
    /// <param name="position">The position where the context menu was requested.</param>
    /// <param name="anchorView">The view that the context menu is anchored to.</param>
    /// <param name="targetElement">The target element that triggered the context menu.</param>
    public ContextMenuOpeningEventArgs(
        ContextMenuItemCollection items,
        Point position,
        View? anchorView = null,
        object? targetElement = null)
    {
        Items = items;
        Position = position;
        AnchorView = anchorView;
        TargetElement = targetElement;
    }

    /// <summary>
    /// Initializes a new instance with an empty items collection.
    /// </summary>
    /// <param name="position">The position where the context menu was requested.</param>
    /// <param name="anchorView">The view that the context menu is anchored to.</param>
    /// <param name="targetElement">The target element that triggered the context menu.</param>
    public ContextMenuOpeningEventArgs(
        Point position,
        View? anchorView = null,
        object? targetElement = null)
        : this(new ContextMenuItemCollection(), position, anchorView, targetElement)
    {
    }
}

/// <summary>
/// Provides data for DataGrid-specific context menu events with cell information.
/// </summary>
public class DataGridContextMenuOpeningEventArgs : ContextMenuOpeningEventArgs
{
    /// <summary>
    /// Gets the data item at the context menu location.
    /// </summary>
    public object? Item { get; }

    /// <summary>
    /// Gets the column at the context menu location.
    /// </summary>
    public DataGridColumn? Column { get; }

    /// <summary>
    /// Gets the row index at the context menu location. -1 if not on a row.
    /// </summary>
    public int RowIndex { get; }

    /// <summary>
    /// Gets the column index at the context menu location. -1 if not on a column.
    /// </summary>
    public int ColumnIndex { get; }

    /// <summary>
    /// Gets the cell value at the context menu location (if available).
    /// </summary>
    public object? CellValue { get; }

    /// <summary>
    /// Gets whether the context menu was triggered on a header cell.
    /// </summary>
    public bool IsHeader => RowIndex == -1 && Column != null;

    /// <summary>
    /// Gets whether the context menu was triggered on a data cell.
    /// </summary>
    public bool IsDataCell => RowIndex >= 0 && ColumnIndex >= 0;

    /// <summary>
    /// Initializes a new instance of DataGridContextMenuOpeningEventArgs.
    /// </summary>
    /// <param name="items">The initial collection of menu items.</param>
    /// <param name="position">The position where the context menu was requested.</param>
    /// <param name="item">The data item at the context menu location.</param>
    /// <param name="column">The column at the context menu location.</param>
    /// <param name="rowIndex">The row index at the context menu location.</param>
    /// <param name="columnIndex">The column index at the context menu location.</param>
    /// <param name="cellValue">The cell value at the context menu location.</param>
    /// <param name="anchorView">The view that the context menu is anchored to.</param>
    public DataGridContextMenuOpeningEventArgs(
        ContextMenuItemCollection items,
        Point position,
        object? item,
        DataGridColumn? column,
        int rowIndex,
        int columnIndex,
        object? cellValue = null,
        View? anchorView = null)
        : base(items, position, anchorView, item)
    {
        Item = item;
        Column = column;
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
        CellValue = cellValue;
    }

    /// <summary>
    /// Initializes a new instance with an empty items collection.
    /// </summary>
    public DataGridContextMenuOpeningEventArgs(
        Point position,
        object? item,
        DataGridColumn? column,
        int rowIndex,
        int columnIndex,
        object? cellValue = null,
        View? anchorView = null)
        : this(new ContextMenuItemCollection(), position, item, column, rowIndex, columnIndex, cellValue, anchorView)
    {
    }
}
