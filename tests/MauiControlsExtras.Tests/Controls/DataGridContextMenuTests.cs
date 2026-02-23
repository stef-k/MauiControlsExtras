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
    public void IsCellInEditMode_ReturnsExpected(
        int targetRow, int targetCol,
        int editingRow, int editingCol,
        bool hasEditControl, bool expected)
    {
        var result = DataGridContextMenuHelper.IsCellInEditMode(
            targetRow, targetCol, editingRow, editingCol, hasEditControl);

        Assert.Equal(expected, result);
    }
}
