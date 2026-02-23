using MauiControlsExtras.Controls;

namespace MauiControlsExtras.Tests.Controls;

public class VirtualizingDataGridPanelTests
{
    private VirtualizingDataGridPanel CreatePanel(int itemCount = 100, double rowHeight = 44)
    {
        var panel = new VirtualizingDataGridPanel
        {
            RowHeight = rowHeight,
            BufferSize = 5,
            ItemsSource = Enumerable.Range(0, itemCount).Cast<object>().ToList(),
            RowFactory = (item, index) => new Grid(),
            RowUpdater = (row, item, index) => { }
        };
        return panel;
    }

    [Fact]
    public void GetVisibleRow_ReturnsNull_WhenIndexNotVisible()
    {
        var panel = CreatePanel();

        // No rows have been materialized yet
        var row = panel.GetVisibleRow(50);

        Assert.Null(row);
    }

    [Fact]
    public void GetVisibleRow_ReturnsRow_WhenIndexIsVisible()
    {
        var panel = CreatePanel();

        // Refresh with a viewport that shows some rows
        panel.Refresh(200);

        // Row 0 should be visible (viewport 200 / rowHeight 44 ≈ 4.5 rows + 5 buffer)
        var row = panel.GetVisibleRow(0);

        Assert.NotNull(row);
    }

    [Fact]
    public void VisibleRange_CalculatedCorrectly()
    {
        var panel = CreatePanel(itemCount: 100, rowHeight: 44);

        // Refresh at scroll=0 with viewport=200
        panel.Refresh(200);

        var range = panel.VisibleRange;

        // First visible = max(0, 0/44 - 5) = 0
        // Last visible = min(99, (0+200)/44 + 5) = min(99, 9) = 9
        Assert.Equal(0, range.First);
        Assert.Equal(9, range.Last);
    }

    [Fact]
    public void UpdateScrollPosition_RecyclesOutOfRangeRows()
    {
        var panel = CreatePanel(itemCount: 100, rowHeight: 44);

        // Initial position
        panel.Refresh(200);

        // Row 0 should be visible initially
        Assert.NotNull(panel.GetVisibleRow(0));

        // Scroll far enough that row 0 goes out of range
        // New first = max(0, 2000/44 - 5) = max(0, 40) = 40
        panel.UpdateScrollPosition(2000, 200);

        // Row 0 should no longer be visible
        Assert.Null(panel.GetVisibleRow(0));

        // Row 45 (middle of new range) should be visible
        Assert.NotNull(panel.GetVisibleRow(45));
    }
}
