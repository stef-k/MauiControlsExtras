using MauiControlsExtras.Controls;

namespace MauiControlsExtras.Tests.Controls;

public class DataGridContextMenuTests
{
    [Theory]
    [InlineData(2, 3, 2, 3, true, true)]   // Same cell, edit control active → in edit mode
    [InlineData(2, 3, 2, 3, false, false)]  // Same cell, no edit control → not in edit mode
    [InlineData(0, 3, 2, 3, true, false)]   // Different row → not in edit mode
    [InlineData(2, 0, 2, 3, true, false)]   // Different column → not in edit mode
    [InlineData(2, 3, -1, -1, true, false)]  // Not editing at all → not in edit mode
    [InlineData(2, 3, -1, -1, false, false)] // Not editing, no control → not in edit mode
    [InlineData(0, 0, 0, 0, true, true)]    // Zero-index cell in edit mode
    [InlineData(0, 0, 0, 0, false, false)]  // Zero-index cell, no edit control
    [InlineData(0, 0, 0, 1, true, false)]   // Zero-index row, different column
    [InlineData(0, 0, 1, 0, true, false)]   // Zero-index column, different row
    public void IsCellInEditMode_ReturnsExpected(
        int targetRow, int targetCol,
        int editingRow, int editingCol,
        bool hasEditControl, bool expected)
    {
        var result = DataGridContextMenuHelper.IsCellInEditMode(
            targetRow, targetCol, editingRow, editingCol, hasEditControl);

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Validates that IsCellInEditMode correctly transitions from edit to non-edit
    /// when hasEditControl changes. This is a pure-logic helper so no lifecycle
    /// (attach/detach) concerns apply—just parameter combinations.
    /// </summary>
    [Fact]
    public void IsCellInEditMode_TransitionFromEditToNonEdit()
    {
        // Cell is in edit mode
        Assert.True(DataGridContextMenuHelper.IsCellInEditMode(1, 2, 1, 2, true));

        // Same cell, edit control removed → no longer in edit mode
        Assert.False(DataGridContextMenuHelper.IsCellInEditMode(1, 2, 1, 2, false));

        // Different cell queried while editing is active
        Assert.False(DataGridContextMenuHelper.IsCellInEditMode(3, 4, 1, 2, true));
    }
}
