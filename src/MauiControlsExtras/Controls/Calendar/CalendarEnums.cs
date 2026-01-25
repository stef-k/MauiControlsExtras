namespace MauiControlsExtras.Controls;

/// <summary>
/// Specifies the selection mode for the calendar.
/// </summary>
public enum CalendarSelectionMode
{
    /// <summary>
    /// Only one date can be selected.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple individual dates can be selected.
    /// </summary>
    Multiple,

    /// <summary>
    /// A range of dates can be selected.
    /// </summary>
    Range,

    /// <summary>
    /// No dates can be selected (display only).
    /// </summary>
    None
}

/// <summary>
/// Specifies the display mode for the calendar.
/// </summary>
public enum CalendarDisplayMode
{
    /// <summary>
    /// Displays days of a month.
    /// </summary>
    Month,

    /// <summary>
    /// Displays months of a year.
    /// </summary>
    Year,

    /// <summary>
    /// Displays years of a decade.
    /// </summary>
    Decade
}

/// <summary>
/// Specifies the day of week to start the calendar.
/// </summary>
public enum CalendarFirstDayOfWeek
{
    /// <summary>
    /// Use system default.
    /// </summary>
    Default,

    /// <summary>
    /// Sunday as first day.
    /// </summary>
    Sunday,

    /// <summary>
    /// Monday as first day.
    /// </summary>
    Monday
}
