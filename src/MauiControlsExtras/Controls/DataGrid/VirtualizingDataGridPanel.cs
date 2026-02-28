using System.Collections;
using Microsoft.Maui.Layouts;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A virtualizing panel for DataGridView that only renders visible rows.
/// </summary>
public class VirtualizingDataGridPanel : Layout, ILayoutManager
{
    private readonly Dictionary<int, View> _visibleRows = new();
    private readonly Queue<View> _recycledRows = new();
    private double _scrollOffset;
    private double _lastMeasuredWidth;
    private int _firstVisibleIndex;
    private int _lastVisibleIndex;

    /// <summary>
    /// Gets or sets the row height.
    /// </summary>
    public double RowHeight { get; set; } = 44;

    /// <summary>
    /// Gets or sets the buffer size (extra rows above/below visible area).
    /// </summary>
    public int BufferSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets the items source.
    /// </summary>
    public IList? ItemsSource { get; set; }

    /// <summary>
    /// Gets or sets the function to create a row view.
    /// </summary>
    public Func<object, int, View>? RowFactory { get; set; }

    /// <summary>
    /// Gets or sets the function to update an existing row view with new data.
    /// </summary>
    public Action<View, object, int>? RowUpdater { get; set; }

    /// <summary>
    /// Gets or sets a callback invoked for each row when it enters the recycle pool or
    /// is discarded during <see cref="Clear"/>. Use this to detach native handlers that
    /// would otherwise leak. The callback must be idempotent—<see cref="Clear"/> may call
    /// it on rows that were already cleaned up when recycled.
    /// </summary>
    public Action<View>? RowCleanup { get; set; }

    /// <summary>
    /// Gets the total content height.
    /// </summary>
    public double TotalHeight => (ItemsSource?.Count ?? 0) * RowHeight;

    /// <summary>
    /// Gets the currently visible row indices.
    /// </summary>
    public (int First, int Last) VisibleRange => (_firstVisibleIndex, _lastVisibleIndex);

    /// <summary>
    /// Initializes a new instance of VirtualizingDataGridPanel.
    /// </summary>
    public VirtualizingDataGridPanel()
    {
    }

    /// <summary>
    /// Updates the scroll position and refreshes visible rows.
    /// </summary>
    public void UpdateScrollPosition(double scrollY, double viewportHeight)
    {
        _scrollOffset = scrollY;
        UpdateVisibleRows(viewportHeight);
    }

    /// <summary>
    /// Refreshes all visible rows (e.g., after data changes).
    /// </summary>
    public void Refresh(double viewportHeight)
    {
        // Clear all rows and rebuild
        foreach (var row in _visibleRows.Values)
        {
            Children.Remove(row);
            RecycleRow(row);
        }
        _visibleRows.Clear();

        UpdateVisibleRows(viewportHeight);
    }

    /// <summary>
    /// Clears all rows and recycled containers.
    /// </summary>
    public new void Clear()
    {
        // Rows in the recycle pool already had RowCleanup called via RecycleRow,
        // but we call again here because the callback is idempotent and this
        // provides a safety net for any code path that enqueued without cleanup.
        if (RowCleanup != null)
        {
            foreach (var row in _visibleRows.Values)
                RowCleanup(row);
            foreach (var row in _recycledRows)
                RowCleanup(row);
        }

        _visibleRows.Clear();
        _recycledRows.Clear();
        Children.Clear();
    }

    private void RecycleRow(View row)
    {
        RowCleanup?.Invoke(row);
        _recycledRows.Enqueue(row);
    }

    private void UpdateVisibleRows(double viewportHeight)
    {
        if (ItemsSource == null || ItemsSource.Count == 0 || RowFactory == null)
        {
            // Remove all visible rows
            foreach (var row in _visibleRows.Values)
            {
                Children.Remove(row);
                RecycleRow(row);
            }
            _visibleRows.Clear();
            return;
        }

        // Calculate visible range
        var firstVisible = Math.Max(0, (int)(_scrollOffset / RowHeight) - BufferSize);
        var lastVisible = Math.Min(ItemsSource.Count - 1,
            (int)((_scrollOffset + viewportHeight) / RowHeight) + BufferSize);

        _firstVisibleIndex = firstVisible;
        _lastVisibleIndex = lastVisible;

        // Remove rows that are no longer visible
        var indicesToRemove = _visibleRows.Keys
            .Where(i => i < firstVisible || i > lastVisible)
            .ToList();

        foreach (var index in indicesToRemove)
        {
            var row = _visibleRows[index];
            _visibleRows.Remove(index);
            Children.Remove(row);
            RecycleRow(row);
        }

        // Add/update rows in visible range
        for (int i = firstVisible; i <= lastVisible; i++)
        {
            if (!_visibleRows.ContainsKey(i))
            {
                var item = ItemsSource[i];
                if (item == null) continue;

                View row;
                if (_recycledRows.Count > 0 && RowUpdater != null)
                {
                    // Recycle existing row
                    row = _recycledRows.Dequeue();
                    RowUpdater(row, item, i);
                }
                else
                {
                    // Create new row
                    row = RowFactory(item, i);
                }

                _visibleRows[i] = row;
                Children.Add(row);
            }
        }

        // Trigger layout
        InvalidateMeasure();
    }

    /// <inheritdoc />
    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }

    /// <inheritdoc />
    public new Size Measure(double widthConstraint, double heightConstraint)
    {
        // Measure all visible children and track actual content width
        double maxChildWidth = 0;
        foreach (var child in Children)
        {
            child.Measure(widthConstraint, RowHeight);
            if (child.DesiredSize.Width > maxChildWidth)
                maxChildWidth = child.DesiredSize.Width;
        }

        // Never return infinite desired size — WinUI throws COMException 0x80004005.
        // When inside a ScrollView (Orientation=Both), widthConstraint is PositiveInfinity;
        // use the actual measured content width instead. If no children are rendered yet
        // (transient state during initial load), use _lastMeasuredWidth to prevent collapse.
        if (maxChildWidth > 0)
            _lastMeasuredWidth = maxChildWidth;
        var desiredWidth = double.IsInfinity(widthConstraint)
            ? (maxChildWidth > 0 ? maxChildWidth : _lastMeasuredWidth)
            : widthConstraint;
        return new Size(desiredWidth, TotalHeight);
    }

    /// <inheritdoc />
    public Size ArrangeChildren(Rect bounds)
    {
        // Position each visible row at its correct Y offset
        foreach (var kvp in _visibleRows)
        {
            var index = kvp.Key;
            var row = kvp.Value;
            var y = index * RowHeight;
            row.Arrange(new Rect(0, y, bounds.Width, RowHeight));
        }

        return new Size(bounds.Width, TotalHeight);
    }

    /// <summary>
    /// Gets a visible row view by index, if available.
    /// </summary>
    public View? GetVisibleRow(int index)
    {
        return _visibleRows.TryGetValue(index, out var row) ? row : null;
    }

    /// <summary>
    /// Scrolls to bring the specified item index into view.
    /// </summary>
    public double GetScrollPositionForIndex(int index, double viewportHeight)
    {
        var itemTop = index * RowHeight;
        var itemBottom = itemTop + RowHeight;

        // If item is above viewport, scroll to show it at top
        if (itemTop < _scrollOffset)
        {
            return itemTop;
        }

        // If item is below viewport, scroll to show it at bottom
        if (itemBottom > _scrollOffset + viewportHeight)
        {
            return itemBottom - viewportHeight;
        }

        // Item is already visible
        return _scrollOffset;
    }
}
