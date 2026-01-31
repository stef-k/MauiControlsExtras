# Calendar API Reference

Full API documentation for the `MauiControlsExtras.Controls.Calendar` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class Calendar : StyledControlBase, IKeyboardNavigable
```

## Inheritance

Inherits from [StyledControlBase](base-classes.md#styledcontrolbase). See base class documentation for inherited styling properties.

## Interfaces

- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### SelectedDate

Gets or sets the currently selected date.

```csharp
public DateTime? SelectedDate { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `DateTime?` | `null` | Yes | TwoWay |

---

#### SelectedDates

Gets or sets the collection of selected dates (for multi-select mode).

```csharp
public IList<DateTime>? SelectedDates { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `IList<DateTime>?` | `null` | Yes | TwoWay |

---

#### DisplayDate

Gets or sets the date displayed in the calendar (month/year view).

```csharp
public DateTime DisplayDate { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `DateTime` | `DateTime.Today` | Yes | TwoWay |

---

#### SelectionMode

Gets or sets the selection mode.

```csharp
public CalendarSelectionMode SelectionMode { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `CalendarSelectionMode` | `Single` | Yes |

---

#### MinDate

Gets or sets the minimum selectable date.

```csharp
public DateTime? MinDate { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `DateTime?` | `null` | Yes |

---

#### MaxDate

Gets or sets the maximum selectable date.

```csharp
public DateTime? MaxDate { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `DateTime?` | `null` | Yes |

---

#### FirstDayOfWeek

Gets or sets the first day of the week.

```csharp
public CalendarFirstDayOfWeek FirstDayOfWeek { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `CalendarFirstDayOfWeek` | `Sunday` | Yes |

---

#### DisplayMode

Gets or sets the current display mode (Month, Year, Decade).

```csharp
public CalendarDisplayMode DisplayMode { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `CalendarDisplayMode` | `Month` | Yes |

---

### Appearance Properties

#### ShowWeekNumbers

Gets or sets whether week numbers are displayed.

```csharp
public bool ShowWeekNumbers { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### ShowNavigationButtons

Gets or sets whether navigation buttons are visible.

```csharp
public bool ShowNavigationButtons { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### ShowTodayButton

Gets or sets whether the "Today" button is shown.

```csharp
public bool ShowTodayButton { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### TodayColor

Gets or sets the highlight color for today's date.

```csharp
public Color? TodayColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses accent) | Yes |

---

#### SelectedColor

Gets or sets the background color for selected dates.

```csharp
public Color? SelectedColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses accent) | Yes |

---

#### WeekendColor

Gets or sets the text color for weekend days.

```csharp
public Color? WeekendColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` | Yes |

---

#### DisabledDateColor

Gets or sets the color for disabled dates.

```csharp
public Color? DisabledDateColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses disabled color) | Yes |

---

#### OtherMonthDayColor

Gets or sets the color for days from adjacent months.

```csharp
public Color? OtherMonthDayColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (muted foreground) | Yes |

---

#### DayNameFormat

Gets or sets the format for day name headers.

```csharp
public CalendarDayNameFormat DayNameFormat { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `CalendarDayNameFormat` | `Short` | Yes |

---

### Blackout Dates

#### BlackoutDates

Gets or sets dates that cannot be selected.

```csharp
public IList<DateTime>? BlackoutDates { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `IList<DateTime>?` | `null` | Yes |

---

#### BlackoutDateRanges

Gets or sets date ranges that cannot be selected.

```csharp
public IList<DateRange>? BlackoutDateRanges { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `IList<DateRange>?` | `null` | Yes |

---

## Events

### DateSelected

Occurs when a date is selected.

```csharp
public event EventHandler<CalendarDateSelectedEventArgs>? DateSelected;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| SelectedDate | `DateTime` | The selected date |
| PreviousDate | `DateTime?` | Previously selected date |

---

### SelectionChanged

Occurs when the selection changes (single or multi-select).

```csharp
public event EventHandler<CalendarSelectionChangedEventArgs>? SelectionChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| AddedDates | `IReadOnlyList<DateTime>` | Newly selected dates |
| RemovedDates | `IReadOnlyList<DateTime>` | Deselected dates |

---

### DisplayDateChanged

Occurs when the display date changes.

```csharp
public event EventHandler<DateTime>? DisplayDateChanged;
```

---

### DisplayModeChanged

Occurs when the display mode changes.

```csharp
public event EventHandler<CalendarDisplayMode>? DisplayModeChanged;
```

---

## Commands

### DateSelectedCommand

Executed when a date is selected.

```csharp
public ICommand? DateSelectedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Date | `DateTime` |

---

### SelectionChangedCommand

Executed when selection changes.

```csharp
public ICommand? SelectionChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `CalendarSelectionChangedEventArgs` |

---

## Methods

### GoToDate(DateTime date)

Navigates to display the specified date's month.

```csharp
public void GoToDate(DateTime date)
```

---

### GoToToday()

Navigates to the current month and optionally selects today.

```csharp
public void GoToToday()
```

---

### GoToNextMonth()

Navigates to the next month.

```csharp
public void GoToNextMonth()
```

---

### GoToPreviousMonth()

Navigates to the previous month.

```csharp
public void GoToPreviousMonth()
```

---

### GoToNextYear()

Navigates to the next year.

```csharp
public void GoToNextYear()
```

---

### GoToPreviousYear()

Navigates to the previous year.

```csharp
public void GoToPreviousYear()
```

---

### ClearSelection()

Clears all selected dates.

```csharp
public void ClearSelection()
```

---

### IsDateSelectable(DateTime date)

Returns whether the specified date can be selected.

```csharp
public bool IsDateSelectable(DateTime date)
```

---

## Enumerations

### CalendarSelectionMode

```csharp
public enum CalendarSelectionMode
{
    None,       // No selection allowed
    Single,     // Single date selection
    Multiple,   // Multiple date selection
    Range       // Date range selection
}
```

### CalendarDisplayMode

```csharp
public enum CalendarDisplayMode
{
    Month,   // Shows days of the month
    Year,    // Shows months of the year
    Decade   // Shows years of the decade
}
```

### CalendarFirstDayOfWeek

```csharp
public enum CalendarFirstDayOfWeek
{
    Sunday,
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday
}
```

### CalendarDayNameFormat

```csharp
public enum CalendarDayNameFormat
{
    Full,       // "Sunday"
    Short,      // "Sun"
    SingleChar  // "S"
}
```

---

## Supporting Types

### DateRange

```csharp
public class DateRange
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public bool Contains(DateTime date);
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Arrow Keys | Move selection between days |
| Page Up | Previous month |
| Page Down | Next month |
| Ctrl+Page Up | Previous year |
| Ctrl+Page Down | Next year |
| Home | First day of month |
| End | Last day of month |
| Enter | Select focused date |
| Space | Toggle selection (multi-select mode) |
| T | Go to today |

---

## Usage Examples

### Basic Date Picker

```xml
<extras:Calendar SelectedDate="{Binding Birthday, Mode=TwoWay}"
                 DateSelectedCommand="{Binding OnDateSelectedCommand}" />
```

### With Date Constraints

```xml
<extras:Calendar SelectedDate="{Binding AppointmentDate}"
                 MinDate="{Binding Today}"
                 MaxDate="{Binding MaxBookingDate}"
                 SelectionMode="Single" />
```

### Multi-Select

```xml
<extras:Calendar SelectionMode="Multiple"
                 SelectedDates="{Binding VacationDays}"
                 SelectionChangedCommand="{Binding OnDatesChangedCommand}" />
```

### Range Selection

```xml
<extras:Calendar SelectionMode="Range"
                 SelectedDates="{Binding DateRange}"
                 SelectionChanged="OnRangeSelected" />
```

### Custom Appearance

```xml
<extras:Calendar FirstDayOfWeek="Monday"
                 ShowWeekNumbers="True"
                 ShowTodayButton="True"
                 DayNameFormat="SingleChar"
                 TodayColor="#4CAF50"
                 SelectedColor="#2196F3"
                 WeekendColor="#9E9E9E"
                 AccentColor="#1976D2" />
```

### With Blackout Dates

```xml
<extras:Calendar SelectedDate="{Binding SelectedDate}"
                 BlackoutDates="{Binding Holidays}"
                 BlackoutDateRanges="{Binding OfficeClosures}" />
```

### Code-Behind

```csharp
// Create calendar
var calendar = new Calendar
{
    SelectionMode = CalendarSelectionMode.Single,
    FirstDayOfWeek = CalendarFirstDayOfWeek.Monday,
    MinDate = DateTime.Today,
    MaxDate = DateTime.Today.AddYears(1)
};

// Add blackout dates
calendar.BlackoutDates = new List<DateTime>
{
    new DateTime(2024, 12, 25),
    new DateTime(2024, 12, 26),
    new DateTime(2025, 1, 1)
};

// Handle selection
calendar.DateSelected += (sender, args) =>
{
    Console.WriteLine($"Selected: {args.SelectedDate:d}");
};

// Navigate programmatically
calendar.GoToDate(new DateTime(2024, 6, 1));
calendar.GoToToday();

// Check if date is available
if (calendar.IsDateSelectable(requestedDate))
{
    calendar.SelectedDate = requestedDate;
}
```
