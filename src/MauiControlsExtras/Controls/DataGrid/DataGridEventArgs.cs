using MauiControlsExtras.Base.Validation;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Event arguments for cell edit events.
/// </summary>
public class DataGridCellEditEventArgs : EventArgs
{
    /// <summary>
    /// Gets the item being edited.
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the column being edited.
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
    /// Gets the old value before editing.
    /// </summary>
    public object? OldValue { get; }

    /// <summary>
    /// Gets or sets the new value after editing.
    /// </summary>
    public object? NewValue { get; set; }

    /// <summary>
    /// Gets or sets whether to cancel the operation.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridCellEditEventArgs.
    /// </summary>
    public DataGridCellEditEventArgs(object item, DataGridColumn column, int rowIndex, int columnIndex, object? oldValue = null, object? newValue = null)
    {
        Item = item;
        Column = column;
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Event arguments for row edit events.
/// </summary>
public class DataGridRowEditEventArgs : EventArgs
{
    /// <summary>
    /// Gets the item that was edited.
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the row index.
    /// </summary>
    public int RowIndex { get; }

    /// <summary>
    /// Gets the list of edited columns.
    /// </summary>
    public IReadOnlyList<DataGridColumn> EditedColumns { get; }

    /// <summary>
    /// Initializes a new instance of DataGridRowEditEventArgs.
    /// </summary>
    public DataGridRowEditEventArgs(object item, int rowIndex, IReadOnlyList<DataGridColumn> editedColumns)
    {
        Item = item;
        RowIndex = rowIndex;
        EditedColumns = editedColumns;
    }
}

/// <summary>
/// Event arguments for column events.
/// </summary>
public class DataGridColumnEventArgs : EventArgs
{
    /// <summary>
    /// Gets the column.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Gets the column index.
    /// </summary>
    public int ColumnIndex { get; }

    /// <summary>
    /// Gets the old width (for resize events).
    /// </summary>
    public double OldWidth { get; }

    /// <summary>
    /// Gets the new width (for resize events).
    /// </summary>
    public double NewWidth { get; }

    /// <summary>
    /// Gets or sets whether to cancel the operation.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridColumnEventArgs.
    /// </summary>
    public DataGridColumnEventArgs(DataGridColumn column, int columnIndex, double oldWidth = 0, double newWidth = 0)
    {
        Column = column;
        ColumnIndex = columnIndex;
        OldWidth = oldWidth;
        NewWidth = newWidth;
    }
}

/// <summary>
/// Event arguments for column reorder events.
/// </summary>
public class DataGridColumnReorderEventArgs : EventArgs
{
    /// <summary>
    /// Gets the column being reordered.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Gets the old index.
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    /// Gets the new index.
    /// </summary>
    public int NewIndex { get; }

    /// <summary>
    /// Gets or sets whether to cancel the operation.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridColumnReorderEventArgs.
    /// </summary>
    public DataGridColumnReorderEventArgs(DataGridColumn column, int oldIndex, int newIndex)
    {
        Column = column;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }
}

/// <summary>
/// Event arguments for filter events.
/// </summary>
public class DataGridFilterEventArgs : EventArgs
{
    /// <summary>
    /// Gets the column being filtered.
    /// </summary>
    public DataGridColumn? Column { get; }

    /// <summary>
    /// Gets the filter values.
    /// </summary>
    public IReadOnlyCollection<object>? FilterValues { get; }

    /// <summary>
    /// Gets the filter text.
    /// </summary>
    public string? FilterText { get; }

    /// <summary>
    /// Gets or sets whether to cancel the operation.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets or sets whether this is a server-side filter (grid should not filter locally).
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridFilterEventArgs.
    /// </summary>
    public DataGridFilterEventArgs(DataGridColumn? column = null, IReadOnlyCollection<object>? filterValues = null, string? filterText = null)
    {
        Column = column;
        FilterValues = filterValues;
        FilterText = filterText;
    }
}

/// <summary>
/// Event arguments for export events.
/// </summary>
public class DataGridExportEventArgs : EventArgs
{
    /// <summary>
    /// Gets the export format.
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// Gets the export options.
    /// </summary>
    public DataGridExportOptions Options { get; }

    /// <summary>
    /// Gets or sets the exported data (output).
    /// </summary>
    public string? ExportedData { get; set; }

    /// <summary>
    /// Gets or sets whether to cancel the operation.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridExportEventArgs.
    /// </summary>
    public DataGridExportEventArgs(string format, DataGridExportOptions options)
    {
        Format = format;
        Options = options;
    }
}

/// <summary>
/// Event arguments for cell validation events.
/// </summary>
public class DataGridCellValidationEventArgs : EventArgs
{
    /// <summary>
    /// Gets the item being validated.
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the column being validated.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Gets the old value.
    /// </summary>
    public object? OldValue { get; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public object? NewValue { get; }

    /// <summary>
    /// Gets or sets whether the value is valid.
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets or sets the error message if invalid.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridCellValidationEventArgs.
    /// </summary>
    public DataGridCellValidationEventArgs(object item, DataGridColumn column, object? oldValue, object? newValue)
    {
        Item = item;
        Column = column;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Event arguments for page changed events.
/// </summary>
public class DataGridPageChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the old page number.
    /// </summary>
    public int OldPage { get; }

    /// <summary>
    /// Gets the new page number.
    /// </summary>
    public int NewPage { get; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total number of items.
    /// </summary>
    public int TotalItems { get; }

    /// <summary>
    /// Initializes a new instance of DataGridPageChangedEventArgs.
    /// </summary>
    public DataGridPageChangedEventArgs(int oldPage, int newPage, int pageSize, int totalItems)
    {
        OldPage = oldPage;
        NewPage = newPage;
        PageSize = pageSize;
        TotalItems = totalItems;
    }
}

/// <summary>
/// Event arguments for row reorder events.
/// </summary>
public class DataGridRowReorderEventArgs : EventArgs
{
    /// <summary>
    /// Gets the item being reordered.
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the old index.
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    /// Gets the new index.
    /// </summary>
    public int NewIndex { get; }

    /// <summary>
    /// Gets or sets whether to cancel the operation.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridRowReorderEventArgs.
    /// </summary>
    public DataGridRowReorderEventArgs(object item, int oldIndex, int newIndex)
    {
        Item = item;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }
}

/// <summary>
/// Represents a column filter configuration.
/// </summary>
public class DataGridColumnFilter
{
    /// <summary>
    /// Gets the column this filter applies to.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Gets or sets the selected filter values.
    /// </summary>
    public HashSet<object> SelectedValues { get; set; } = new();

    /// <summary>
    /// Gets or sets the search text within the filter.
    /// </summary>
    public string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Gets whether this filter is active.
    /// </summary>
    public bool IsActive => SelectedValues.Count > 0 || !string.IsNullOrEmpty(SearchText);

    /// <summary>
    /// Initializes a new instance of DataGridColumnFilter.
    /// </summary>
    public DataGridColumnFilter(DataGridColumn column)
    {
        Column = column;
    }
}

/// <summary>
/// Options for exporting data from the grid.
/// </summary>
public class DataGridExportOptions
{
    /// <summary>
    /// Gets or sets whether to include headers in the export.
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to export only visible columns.
    /// </summary>
    public bool VisibleColumnsOnly { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to export only selected rows.
    /// </summary>
    public bool SelectedRowsOnly { get; set; }

    /// <summary>
    /// Gets or sets whether to apply formatting.
    /// </summary>
    public bool ApplyFormatting { get; set; } = true;

    /// <summary>
    /// Gets or sets the date format for export.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Gets or sets the delimiter for CSV export.
    /// </summary>
    public string Delimiter { get; set; } = ",";

    /// <summary>
    /// Gets or sets whether to pretty-print JSON output.
    /// </summary>
    public bool PrettyPrint { get; set; }
}

/// <summary>
/// Represents a sort description for multi-column sorting.
/// </summary>
public class DataGridSortDescription
{
    /// <summary>
    /// Gets the column being sorted.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Gets the sort direction.
    /// </summary>
    public SortDirection Direction { get; }

    /// <summary>
    /// Gets the sort order (1, 2, 3...).
    /// </summary>
    public int SortOrder { get; }

    /// <summary>
    /// Initializes a new instance of DataGridSortDescription.
    /// </summary>
    public DataGridSortDescription(DataGridColumn column, SortDirection direction, int sortOrder)
    {
        Column = column;
        Direction = direction;
        SortOrder = sortOrder;
    }
}

/// <summary>
/// Event arguments for clipboard operations.
/// </summary>
public class DataGridClipboardEventArgs : EventArgs
{
    /// <summary>
    /// Gets the clipboard operation type.
    /// </summary>
    public DataGridClipboardOperation Operation { get; }

    /// <summary>
    /// Gets or sets the content being copied/pasted.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Gets the items involved in the operation.
    /// </summary>
    public IReadOnlyCollection<object> Items { get; }

    /// <summary>
    /// Gets or sets whether to cancel the operation.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridClipboardEventArgs.
    /// </summary>
    public DataGridClipboardEventArgs(DataGridClipboardOperation operation, IReadOnlyCollection<object> items, string? content = null)
    {
        Operation = operation;
        Items = items;
        Content = content;
    }
}

/// <summary>
/// Specifies the clipboard operation type.
/// </summary>
public enum DataGridClipboardOperation
{
    /// <summary>
    /// Copy operation.
    /// </summary>
    Copy,

    /// <summary>
    /// Cut operation.
    /// </summary>
    Cut,

    /// <summary>
    /// Paste operation.
    /// </summary>
    Paste
}

/// <summary>
/// Event arguments for context menu events.
/// </summary>
public class DataGridContextMenuEventArgs : EventArgs
{
    /// <summary>
    /// Gets the item at the context menu location.
    /// </summary>
    public object? Item { get; }

    /// <summary>
    /// Gets the column at the context menu location.
    /// </summary>
    public DataGridColumn? Column { get; }

    /// <summary>
    /// Gets the row index at the context menu location.
    /// </summary>
    public int RowIndex { get; }

    /// <summary>
    /// Gets the column index at the context menu location.
    /// </summary>
    public int ColumnIndex { get; }

    /// <summary>
    /// Gets or sets whether to cancel showing the context menu.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets or sets the custom menu items to show.
    /// </summary>
    public IList<object>? CustomMenuItems { get; set; }

    /// <summary>
    /// Initializes a new instance of DataGridContextMenuEventArgs.
    /// </summary>
    public DataGridContextMenuEventArgs(object? item, DataGridColumn? column, int rowIndex, int columnIndex)
    {
        Item = item;
        Column = column;
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
    }
}

/// <summary>
/// Represents a context menu action.
/// </summary>
public class DataGridContextMenuAction
{
    /// <summary>
    /// Gets the display text for the action.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the action to execute.
    /// </summary>
    public Action Action { get; }

    /// <summary>
    /// Initializes a new instance of DataGridContextMenuAction.
    /// </summary>
    public DataGridContextMenuAction(string text, Action action)
    {
        Text = text;
        Action = action;
    }
}

/// <summary>
/// Event arguments for row selection events.
/// </summary>
public class DataGridRowSelectionEventArgs : EventArgs
{
    /// <summary>
    /// Gets the data item of the row.
    /// </summary>
    public object Item { get; }

    /// <summary>
    /// Gets the index of the row.
    /// </summary>
    public int RowIndex { get; }

    /// <summary>
    /// Initializes a new instance of DataGridRowSelectionEventArgs.
    /// </summary>
    public DataGridRowSelectionEventArgs(object item, int rowIndex)
    {
        Item = item;
        RowIndex = rowIndex;
    }
}

