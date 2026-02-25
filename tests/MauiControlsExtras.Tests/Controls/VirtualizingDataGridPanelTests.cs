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

    [Fact]
    public void Clear_CallsRowCleanup_ForVisibleAndRecycledRows()
    {
        var cleanedRows = new List<View>();
        var panel = CreatePanel();
        panel.RowCleanup = row => cleanedRows.Add(row);

        // Materialize rows, then scroll so some go to recycle pool
        panel.Refresh(200);
        panel.UpdateScrollPosition(2000, 200);

        cleanedRows.Clear();
        panel.Clear();

        // Should have cleaned up both visible rows and recycled rows
        Assert.NotEmpty(cleanedRows);
    }

    [Fact]
    public void UpdateScrollPosition_CallsRowCleanup_WhenRowsScrollOut()
    {
        var cleanedRows = new List<View>();
        var panel = CreatePanel();
        panel.RowCleanup = row => cleanedRows.Add(row);

        panel.Refresh(200);

        cleanedRows.Clear();

        // Scroll far enough that all initially visible rows go out of range
        panel.UpdateScrollPosition(2000, 200);

        // RowCleanup should have been called for the rows that scrolled out
        Assert.NotEmpty(cleanedRows);
    }

    [Fact]
    public void Refresh_CallsRowCleanup_BeforeRebuilding()
    {
        var cleanedRows = new List<View>();
        var panel = CreatePanel();
        panel.RowCleanup = row => cleanedRows.Add(row);

        panel.Refresh(200);
        var initialRowCount = cleanedRows.Count;

        cleanedRows.Clear();

        // Refresh rebuilds all rows — existing rows should get cleanup before recycling
        panel.Refresh(200);

        Assert.NotEmpty(cleanedRows);
    }

    [Fact]
    public void RowRecycling_InvokesCleanupBeforeUpdater()
    {
        var events = new List<(View Row, string Event)>();

        var panel = new VirtualizingDataGridPanel
        {
            RowHeight = 44,
            BufferSize = 5,
            ItemsSource = Enumerable.Range(0, 100).Cast<object>().ToList(),
            RowFactory = (item, index) => new Grid(),
            RowUpdater = (row, item, index) => events.Add((row, "update")),
            RowCleanup = row => events.Add((row, "cleanup"))
        };

        panel.Refresh(200);
        events.Clear();

        // Scroll far enough that all initial rows recycle and get reused
        panel.UpdateScrollPosition(2000, 200);

        var recycledRows = events.Select(e => e.Row).Distinct()
            .Where(r => events.Any(e => e.Row == r && e.Event == "cleanup")
                      && events.Any(e => e.Row == r && e.Event == "update"))
            .ToList();

        Assert.NotEmpty(recycledRows);

        foreach (var row in recycledRows)
        {
            var rowEvents = events.Where(e => e.Row == row).ToList();
            var lastCleanup = rowEvents.FindLastIndex(e => e.Event == "cleanup");
            var firstUpdate = rowEvents.FindIndex(e => e.Event == "update");

            Assert.True(lastCleanup < firstUpdate,
                $"RowCleanup (index {lastCleanup}) must precede RowUpdater (index {firstUpdate}) on recycled rows.");
        }
    }

    [Fact]
    public void RowCleanup_IsIdempotent_CalledTwiceOnSameRowWithoutError()
    {
        var cleanupCounts = new Dictionary<View, int>();
        var panel = CreatePanel();
        panel.RowCleanup = row =>
        {
            cleanupCounts.TryGetValue(row, out var count);
            cleanupCounts[row] = count + 1;
        };

        // Materialize rows then scroll so some enter the recycle pool (cleanup called once via RecycleRow)
        panel.Refresh(200);
        panel.UpdateScrollPosition(2000, 200);

        // Clear calls cleanup again on recycled rows (second call on the same row)
        panel.Clear();

        // At least one row should have been cleaned up more than once
        Assert.Contains(cleanupCounts, kvp => kvp.Value >= 2);
    }
}
