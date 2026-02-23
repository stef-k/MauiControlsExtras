namespace MauiControlsExtras.Controls;

/// <summary>
/// Pure-logic helpers for DataGrid context menu decisions.
/// Extracted for unit testability without platform dependencies.
/// </summary>
internal static class DataGridContextMenuHelper
{
    /// <summary>
    /// Determines whether a cell is currently in edit mode.
    /// </summary>
    /// <param name="targetRow">Row index of the cell being checked.</param>
    /// <param name="targetCol">Column index of the cell being checked.</param>
    /// <param name="editingRow">Row index currently being edited (-1 if none).</param>
    /// <param name="editingCol">Column index currently being edited (-1 if none).</param>
    /// <param name="hasEditControl">Whether an edit control is active.</param>
    /// <returns>True if the target cell is in edit mode.</returns>
    public static bool IsCellInEditMode(int targetRow, int targetCol,
        int editingRow, int editingCol, bool hasEditControl)
        => editingRow == targetRow && editingCol == targetCol && hasEditControl;
}
