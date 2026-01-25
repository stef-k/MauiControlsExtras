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
