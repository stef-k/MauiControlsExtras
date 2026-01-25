namespace MauiControlsExtras.Controls;

/// <summary>
/// Event arguments for date selection changes.
/// </summary>
public class CalendarDateSelectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the selected date.
    /// </summary>
    public DateTime Date { get; }

    /// <summary>
    /// Gets whether the date was added (true) or removed (false) from selection.
    /// </summary>
    public bool IsSelected { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarDateSelectedEventArgs"/> class.
    /// </summary>
    public CalendarDateSelectedEventArgs(DateTime date, bool isSelected)
    {
        Date = date;
        IsSelected = isSelected;
    }
}

/// <summary>
/// Event arguments for date selection changing (cancelable).
/// </summary>
public class CalendarDateSelectingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the date being selected/deselected.
    /// </summary>
    public DateTime Date { get; }

    /// <summary>
    /// Gets whether the date is being selected (true) or deselected (false).
    /// </summary>
    public bool IsSelecting { get; }

    /// <summary>
    /// Gets or sets whether the selection should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarDateSelectingEventArgs"/> class.
    /// </summary>
    public CalendarDateSelectingEventArgs(DateTime date, bool isSelecting)
    {
        Date = date;
        IsSelecting = isSelecting;
    }
}

/// <summary>
/// Event arguments for displayed month changes.
/// </summary>
public class CalendarDisplayDateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the old displayed date.
    /// </summary>
    public DateTime OldDate { get; }

    /// <summary>
    /// Gets the new displayed date.
    /// </summary>
    public DateTime NewDate { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarDisplayDateChangedEventArgs"/> class.
    /// </summary>
    public CalendarDisplayDateChangedEventArgs(DateTime oldDate, DateTime newDate)
    {
        OldDate = oldDate;
        NewDate = newDate;
    }
}
