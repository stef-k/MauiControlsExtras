# Calendar

A calendar control for date selection with month, year, and decade views.

## Features

- **Date Selection** - Single, multiple, or range selection
- **Multiple Views** - Month, year, and decade views
- **Date Constraints** - Min/max date limits
- **Week Numbers** - Optional week number display
- **First Day of Week** - Configurable start day
- **Today Highlight** - Highlight current date
- **Keyboard Navigation** - Full keyboard support

## Basic Usage

```xml
<extras:Calendar
    SelectedDate="{Binding SelectedDate, Mode=TwoWay}"
    DateSelectedCommand="{Binding DateSelectedCommand}" />
```

## Selection Modes

```xml
<!-- Single date selection (default) -->
<extras:Calendar SelectionMode="Single" />

<!-- Multiple date selection -->
<extras:Calendar SelectionMode="Multiple" />

<!-- Date range selection -->
<extras:Calendar SelectionMode="Range" />

<!-- No selection (display only) -->
<extras:Calendar SelectionMode="None" />
```

## Multiple Selection

```xml
<extras:Calendar
    SelectionMode="Multiple"
    SelectedDates="{Binding SelectedDates, Mode=TwoWay}" />
```

```csharp
// In ViewModel
public ObservableCollection<DateTime> SelectedDates { get; } = new();
```

## Range Selection

```xml
<extras:Calendar
    SelectionMode="Range"
    RangeStart="{Binding StartDate, Mode=TwoWay}"
    RangeEnd="{Binding EndDate, Mode=TwoWay}" />
```

## Date Constraints

```xml
<extras:Calendar
    MinDate="2024-01-01"
    MaxDate="2024-12-31" />
```

```xml
<!-- Binding to ViewModel -->
<extras:Calendar
    MinDate="{Binding MinAllowedDate}"
    MaxDate="{Binding MaxAllowedDate}" />
```

## First Day of Week

```xml
<!-- System default -->
<extras:Calendar FirstDayOfWeek="Default" />

<!-- Start on Sunday -->
<extras:Calendar FirstDayOfWeek="Sunday" />

<!-- Start on Monday -->
<extras:Calendar FirstDayOfWeek="Monday" />
```

## Week Numbers

```xml
<extras:Calendar ShowWeekNumbers="True" />
```

## Header Styling

```xml
<extras:Calendar
    HeaderBackgroundColor="#F0F0F0"
    HeaderTextColor="#333333"
    HeaderFontSize="18"
    HeaderFontAttributes="Bold"
    HeaderPadding="8,4" />
```

## Styling

```xml
<extras:Calendar
    TodayHighlightColor="Blue"
    SelectedDateColor="Purple"
    CellSize="36" />
```

## Blackout Dates

```xml
<extras:Calendar BlackoutDates="{Binding UnavailableDates}" />
```

```csharp
// In ViewModel
public ObservableCollection<DateTime> UnavailableDates { get; } = new()
{
    new DateTime(2024, 12, 25),  // Christmas
    new DateTime(2024, 1, 1),    // New Year
};
```

## Code-Behind Operations

```csharp
// Navigate programmatically
calendar.GoToDate(new DateTime(2024, 6, 15));
calendar.GoToToday();
calendar.GoToNextMonth();
calendar.GoToPreviousMonth();

// Change view
calendar.SwitchToYearView();
calendar.SwitchToDecadeView();
calendar.SwitchToMonthView();

// Get selected dates
var selectedDates = calendar.GetSelectedDates();

// Clear selection
calendar.ClearSelection();

// Select specific date
calendar.SelectDate(new DateTime(2024, 7, 4));
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ← / → / ↑ / ↓ | Navigate dates |
| Page Up | Previous month |
| Page Down | Next month |
| Home | Go to today |
| Enter | Select date |
| Space | Select date |

## Events

| Event | Description |
|-------|-------------|
| DateSelected | Date was selected |
| DateSelecting | Date selection changing (cancelable) |
| DisplayDateChanged | Displayed month/year changed |

## Commands

| Command | Description |
|---------|-------------|
| DateSelectedCommand | Execute when date is selected |
| DisplayDateChangedCommand | Execute when display date changes |
| ClearSelectionCommand | Clear all selected dates and reset range |

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| HeaderBackgroundColor | Color | null | Month/year bar background |
| HeaderTextColor | Color | null | Month/year text color |
| HeaderFontSize | double | 16 | Header font size |
| HeaderFontAttributes | FontAttributes | Bold | Header font style |
| HeaderFontFamily | string | null | Header font family |
| HeaderPadding | Thickness | 12,8 | Header padding |
| SelectedDate | DateTime? | null | Selected date (single mode) |
| SelectedDates | IList&lt;DateTime&gt; | empty | Selected dates (multiple mode) |
| RangeStart | DateTime? | null | Range start (range mode) |
| RangeEnd | DateTime? | null | Range end (range mode) |
| SelectionMode | CalendarSelectionMode | Single | Single, Multiple, Range, None |
| DisplayDate | DateTime | Today | Currently displayed month |
| MinDate | DateTime? | null | Minimum selectable date |
| MaxDate | DateTime? | null | Maximum selectable date |
| FirstDayOfWeek | CalendarFirstDayOfWeek | Default | First day of week |
| ShowWeekNumbers | bool | false | Show week numbers |
| BlackoutDates | IList&lt;DateTime&gt; | empty | Disabled dates |
| TodayHighlightColor | Color | null | Today highlight color |
| SelectedDateColor | Color | null | Selection color |
| CellSize | double | 36 | Calendar cell size |

## Enums

### CalendarSelectionMode

| Value | Description |
|-------|-------------|
| None | No selection allowed |
| Single | Select one date |
| Multiple | Select multiple dates |
| Range | Select a date range |

### CalendarFirstDayOfWeek

| Value | Description |
|-------|-------------|
| Default | Use system setting |
| Sunday | Start on Sunday |
| Monday | Start on Monday |
| Saturday | Start on Saturday |
