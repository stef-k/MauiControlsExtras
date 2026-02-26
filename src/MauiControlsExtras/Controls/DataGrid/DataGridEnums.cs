namespace MauiControlsExtras.Controls;

/// <summary>
/// Specifies how data is copied to the clipboard.
/// </summary>
public enum DataGridCopyMode
{
    /// <summary>
    /// Copy only selected rows.
    /// </summary>
    SelectedRows,

    /// <summary>
    /// Copy only selected cells.
    /// </summary>
    SelectedCells,

    /// <summary>
    /// Copy all visible rows.
    /// </summary>
    AllVisibleRows,

    /// <summary>
    /// Copy all data including filtered items.
    /// </summary>
    AllData
}

/// <summary>
/// Specifies the aggregate function for a column footer.
/// </summary>
public enum DataGridAggregateType
{
    /// <summary>
    /// No aggregate.
    /// </summary>
    None,

    /// <summary>
    /// Sum of values.
    /// </summary>
    Sum,

    /// <summary>
    /// Average of values.
    /// </summary>
    Average,

    /// <summary>
    /// Count of items.
    /// </summary>
    Count,

    /// <summary>
    /// Minimum value.
    /// </summary>
    Min,

    /// <summary>
    /// Maximum value.
    /// </summary>
    Max
}

/// <summary>
/// Specifies when row details are displayed.
/// </summary>
public enum DataGridRowDetailsVisibilityMode
{
    /// <summary>
    /// Row details are collapsed.
    /// </summary>
    Collapsed,

    /// <summary>
    /// Row details are always visible.
    /// </summary>
    Visible,

    /// <summary>
    /// Row details are visible only when the row is selected.
    /// </summary>
    VisibleWhenSelected
}

/// <summary>
/// Specifies the edit trigger for cells.
/// </summary>
public enum DataGridEditTrigger
{
    /// <summary>
    /// Edit on single tap.
    /// </summary>
    SingleTap,

    /// <summary>
    /// Edit on double tap.
    /// </summary>
    DoubleTap,

    /// <summary>
    /// Edit only programmatically or via F2 key.
    /// </summary>
    Manual
}

/// <summary>
/// Specifies how a data grid column determines its width.
/// </summary>
public enum DataGridColumnSizeMode
{
    /// <summary>
    /// Backward-compatible default: Width &lt; 0 uses auto sizing, Width &gt;= 0 uses the explicit pixel value.
    /// </summary>
    Auto,

    /// <summary>
    /// Explicit pixel width from <see cref="DataGridColumn.Width"/>.
    /// </summary>
    Fixed,

    /// <summary>
    /// Measures the header text so it never wraps. The computed width is stored in <see cref="DataGridColumn.ActualWidth"/>.
    /// </summary>
    FitHeader,

    /// <summary>
    /// Fills remaining space proportionally. <see cref="DataGridColumn.Width"/> acts as a star weight (default 1).
    /// </summary>
    Fill
}
