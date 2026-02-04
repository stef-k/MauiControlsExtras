using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A calendar control for date selection with month/year/decade views.
/// </summary>
public partial class Calendar : HeaderedControlBase, IKeyboardNavigable, ISelectable
{
    #region Private Fields

    private DateTime _displayDate;
    private readonly HashSet<DateTime> _selectedDates = new();
    private DateTime? _rangeStart;
    private CalendarDisplayMode _currentDisplayMode;
    private int _focusedRow;
    private int _focusedCol;
    private bool _hasKeyboardFocus;
    private bool _isInternalSelectionUpdate;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="SelectedDate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedDateProperty = BindableProperty.Create(
        nameof(SelectedDate),
        typeof(DateTime?),
        typeof(Calendar),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedDateChanged);

    /// <summary>
    /// Identifies the <see cref="SelectionMode"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(
        nameof(SelectionMode),
        typeof(CalendarSelectionMode),
        typeof(Calendar),
        CalendarSelectionMode.Single,
        propertyChanged: OnSelectionModeChanged);

    /// <summary>
    /// Identifies the <see cref="DisplayDate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisplayDateProperty = BindableProperty.Create(
        nameof(DisplayDate),
        typeof(DateTime),
        typeof(Calendar),
        DateTime.Today,
        BindingMode.TwoWay,
        propertyChanged: OnDisplayDateChanged);

    /// <summary>
    /// Identifies the <see cref="MinDate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MinDateProperty = BindableProperty.Create(
        nameof(MinDate),
        typeof(DateTime?),
        typeof(Calendar),
        null,
        propertyChanged: OnDateRangeChanged);

    /// <summary>
    /// Identifies the <see cref="MaxDate"/> bindable property.
    /// </summary>
    public static readonly BindableProperty MaxDateProperty = BindableProperty.Create(
        nameof(MaxDate),
        typeof(DateTime?),
        typeof(Calendar),
        null,
        propertyChanged: OnDateRangeChanged);

    /// <summary>
    /// Identifies the <see cref="FirstDayOfWeek"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FirstDayOfWeekProperty = BindableProperty.Create(
        nameof(FirstDayOfWeek),
        typeof(CalendarFirstDayOfWeek),
        typeof(Calendar),
        CalendarFirstDayOfWeek.Default,
        propertyChanged: OnFirstDayOfWeekChanged);

    /// <summary>
    /// Identifies the <see cref="ShowWeekNumbers"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowWeekNumbersProperty = BindableProperty.Create(
        nameof(ShowWeekNumbers),
        typeof(bool),
        typeof(Calendar),
        false,
        propertyChanged: OnShowWeekNumbersChanged);

    /// <summary>
    /// Identifies the <see cref="TodayHighlightColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TodayHighlightColorProperty = BindableProperty.Create(
        nameof(TodayHighlightColor),
        typeof(Color),
        typeof(Calendar),
        null);

    /// <summary>
    /// Identifies the <see cref="SelectedDateColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedDateColorProperty = BindableProperty.Create(
        nameof(SelectedDateColor),
        typeof(Color),
        typeof(Calendar),
        null);

    /// <summary>
    /// Identifies the <see cref="CellSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CellSizeProperty = BindableProperty.Create(
        nameof(CellSize),
        typeof(double),
        typeof(Calendar),
        36.0,
        propertyChanged: OnCellSizeChanged);

    /// <summary>
    /// Identifies the <see cref="IsKeyboardNavigationEnabled"/> bindable property.
    /// </summary>
    public static readonly BindableProperty IsKeyboardNavigationEnabledProperty = BindableProperty.Create(
        nameof(IsKeyboardNavigationEnabled),
        typeof(bool),
        typeof(Calendar),
        true);

    #endregion

    #region Command Properties

    /// <summary>
    /// Identifies the <see cref="DateSelectedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DateSelectedCommandProperty = BindableProperty.Create(
        nameof(DateSelectedCommand),
        typeof(ICommand),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="DateSelectedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DateSelectedCommandParameterProperty = BindableProperty.Create(
        nameof(DateSelectedCommandParameter),
        typeof(object),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="DisplayDateChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisplayDateChangedCommandProperty = BindableProperty.Create(
        nameof(DisplayDateChangedCommand),
        typeof(ICommand),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="DisplayDateChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisplayDateChangedCommandParameterProperty = BindableProperty.Create(
        nameof(DisplayDateChangedCommandParameter),
        typeof(object),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="GotFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandParameterProperty = BindableProperty.Create(
        nameof(GotFocusCommandParameter),
        typeof(object),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="LostFocusCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandParameterProperty = BindableProperty.Create(
        nameof(LostFocusCommandParameter),
        typeof(object),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="KeyPressCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandParameterProperty = BindableProperty.Create(
        nameof(KeyPressCommandParameter),
        typeof(object),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="SelectAllCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectAllCommandProperty = BindableProperty.Create(
        nameof(SelectAllCommand),
        typeof(ICommand),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="SelectAllCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectAllCommandParameterProperty = BindableProperty.Create(
        nameof(SelectAllCommandParameter),
        typeof(object),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="ClearSelectionCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearSelectionCommandProperty = BindableProperty.Create(
        nameof(ClearSelectionCommand),
        typeof(ICommand),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="ClearSelectionCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ClearSelectionCommandParameterProperty = BindableProperty.Create(
        nameof(ClearSelectionCommandParameter),
        typeof(object),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommand"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandProperty = BindableProperty.Create(
        nameof(SelectionChangedCommand),
        typeof(ICommand),
        typeof(Calendar));

    /// <summary>
    /// Identifies the <see cref="SelectionChangedCommandParameter"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectionChangedCommandParameterProperty = BindableProperty.Create(
        nameof(SelectionChangedCommandParameter),
        typeof(object),
        typeof(Calendar));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the selected date.
    /// </summary>
    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    /// <summary>
    /// Gets or sets the selection mode.
    /// </summary>
    public CalendarSelectionMode SelectionMode
    {
        get => (CalendarSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the displayed date (determines which month is shown).
    /// </summary>
    public DateTime DisplayDate
    {
        get => (DateTime)GetValue(DisplayDateProperty);
        set => SetValue(DisplayDateProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum selectable date.
    /// </summary>
    public DateTime? MinDate
    {
        get => (DateTime?)GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum selectable date.
    /// </summary>
    public DateTime? MaxDate
    {
        get => (DateTime?)GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }

    /// <summary>
    /// Gets or sets the first day of the week.
    /// </summary>
    public CalendarFirstDayOfWeek FirstDayOfWeek
    {
        get => (CalendarFirstDayOfWeek)GetValue(FirstDayOfWeekProperty);
        set => SetValue(FirstDayOfWeekProperty, value);
    }

    /// <summary>
    /// Gets or sets whether week numbers are shown.
    /// </summary>
    public bool ShowWeekNumbers
    {
        get => (bool)GetValue(ShowWeekNumbersProperty);
        set => SetValue(ShowWeekNumbersProperty, value);
    }

    /// <summary>
    /// Gets or sets the highlight color for today's date.
    /// </summary>
    public Color? TodayHighlightColor
    {
        get => (Color?)GetValue(TodayHighlightColorProperty);
        set => SetValue(TodayHighlightColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color for selected dates.
    /// </summary>
    public Color? SelectedDateColor
    {
        get => (Color?)GetValue(SelectedDateColorProperty);
        set => SetValue(SelectedDateColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of each calendar cell.
    /// </summary>
    public double CellSize
    {
        get => (double)GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    /// <summary>
    /// Gets the collection of selected dates (for multiple/range selection).
    /// </summary>
    public IReadOnlyCollection<DateTime> SelectedDates => _selectedDates;

    /// <summary>
    /// Gets the effective today highlight color.
    /// </summary>
    public Color EffectiveTodayHighlightColor =>
        TodayHighlightColor ?? EffectiveAccentColor;

    /// <summary>
    /// Gets the effective selected date color.
    /// </summary>
    public Color EffectiveSelectedDateColor =>
        SelectedDateColor ?? EffectiveAccentColor;

    /// <summary>
    /// Gets the collection of blackout dates.
    /// </summary>
    public ObservableCollection<DateTime> BlackoutDates { get; } = new();

    #endregion

    #region Command Properties Implementation

    /// <summary>
    /// Gets or sets the command executed when a date is selected.
    /// </summary>
    public ICommand? DateSelectedCommand
    {
        get => (ICommand?)GetValue(DateSelectedCommandProperty);
        set => SetValue(DateSelectedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="DateSelectedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? DateSelectedCommandParameter
    {
        get => GetValue(DateSelectedCommandParameterProperty);
        set => SetValue(DateSelectedCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when the display date changes.
    /// </summary>
    public ICommand? DisplayDateChangedCommand
    {
        get => (ICommand?)GetValue(DisplayDateChangedCommandProperty);
        set => SetValue(DisplayDateChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="DisplayDateChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? DisplayDateChangedCommandParameter
    {
        get => GetValue(DisplayDateChangedCommandParameterProperty);
        set => SetValue(DisplayDateChangedCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? GotFocusCommand
    {
        get => (ICommand?)GetValue(GotFocusCommandProperty);
        set => SetValue(GotFocusCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="GotFocusCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? GotFocusCommandParameter
    {
        get => GetValue(GotFocusCommandParameterProperty);
        set => SetValue(GotFocusCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="LostFocusCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? LostFocusCommandParameter
    {
        get => GetValue(LostFocusCommandParameterProperty);
        set => SetValue(LostFocusCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? KeyPressCommand
    {
        get => (ICommand?)GetValue(KeyPressCommandProperty);
        set => SetValue(KeyPressCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="KeyPressCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? KeyPressCommandParameter
    {
        get => GetValue(KeyPressCommandParameterProperty);
        set => SetValue(KeyPressCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? SelectAllCommand
    {
        get => (ICommand?)GetValue(SelectAllCommandProperty);
        set => SetValue(SelectAllCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="SelectAllCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? SelectAllCommandParameter
    {
        get => GetValue(SelectAllCommandParameterProperty);
        set => SetValue(SelectAllCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? ClearSelectionCommand
    {
        get => (ICommand?)GetValue(ClearSelectionCommandProperty);
        set => SetValue(ClearSelectionCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="ClearSelectionCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? ClearSelectionCommandParameter
    {
        get => GetValue(ClearSelectionCommandParameterProperty);
        set => SetValue(ClearSelectionCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to <see cref="SelectionChangedCommand"/>.
    /// If not set, the default event argument is used as the parameter.
    /// </summary>
    public object? SelectionChangedCommandParameter
    {
        get => GetValue(SelectionChangedCommandParameterProperty);
        set => SetValue(SelectionChangedCommandParameterProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when a date is selected.
    /// </summary>
    public event EventHandler<CalendarDateSelectedEventArgs>? DateSelected;

    /// <summary>
    /// Occurs before a date is selected (cancelable).
    /// </summary>
    public event EventHandler<CalendarDateSelectingEventArgs>? DateSelecting;

    /// <summary>
    /// Occurs when the display date changes.
    /// </summary>
    public event EventHandler<CalendarDisplayDateChangedEventArgs>? DisplayDateChanged;

    /// <inheritdoc/>
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <inheritdoc/>
#pragma warning disable CS0067
    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusLost;
#pragma warning restore CS0067

    /// <inheritdoc/>
    public event EventHandler<KeyEventArgs>? KeyPressed;

    /// <inheritdoc/>
#pragma warning disable CS0067
    public event EventHandler<KeyEventArgs>? KeyReleased;
#pragma warning restore CS0067

    /// <inheritdoc/>
    public event EventHandler<Base.SelectionChangedEventArgs>? SelectionChanged;

    #endregion

    #region IKeyboardNavigable Implementation

    /// <inheritdoc/>
    public bool CanReceiveFocus => IsEnabled && IsVisible;

    /// <inheritdoc/>
    public bool IsKeyboardNavigationEnabled
    {
        get => (bool)GetValue(IsKeyboardNavigationEnabledProperty);
        set => SetValue(IsKeyboardNavigationEnabledProperty, value);
    }

    /// <inheritdoc/>
    public bool HasKeyboardFocus => _hasKeyboardFocus;

    /// <inheritdoc/>
    public bool HandleKeyPress(KeyEventArgs e)
    {
        if (!IsKeyboardNavigationEnabled) return false;

        KeyPressed?.Invoke(this, e);
        if (e.Handled) return true;

        if (KeyPressCommand?.CanExecute(e) == true)
        {
            KeyPressCommand.Execute(KeyPressCommandParameter ?? e);
            if (e.Handled) return true;
        }

        switch (e.Key)
        {
            case "ArrowLeft":
                MoveFocus(-1, 0);
                return true;
            case "ArrowRight":
                MoveFocus(1, 0);
                return true;
            case "ArrowUp":
                MoveFocus(0, -1);
                return true;
            case "ArrowDown":
                MoveFocus(0, 1);
                return true;
            case "PageUp":
                PreviousMonth();
                return true;
            case "PageDown":
                NextMonth();
                return true;
            case "Home":
                GoToToday();
                return true;
            case "Enter":
            case " ":
                SelectFocusedDate();
                return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts()
    {
        return new List<KeyboardShortcut>
        {
            new() { Key = "Arrow Keys", Description = "Navigate dates", Category = "Navigation" },
            new() { Key = "PageUp", Description = "Previous month", Category = "Navigation" },
            new() { Key = "PageDown", Description = "Next month", Category = "Navigation" },
            new() { Key = "Home", Description = "Go to today", Category = "Navigation" },
            new() { Key = "Enter", Description = "Select date", Category = "Actions" },
            new() { Key = "Space", Description = "Select date", Category = "Actions" }
        };
    }

    /// <inheritdoc/>
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;
        _hasKeyboardFocus = true;
        OnPropertyChanged(nameof(HasKeyboardFocus));
        KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
        GotFocusCommand?.Execute(GotFocusCommandParameter ?? this);
        RebuildCalendar();
        return true;
    }

    #endregion

    #region ISelectable Implementation

    /// <inheritdoc/>
    public bool HasSelection => _selectedDates.Count > 0;

    /// <inheritdoc/>
    public bool IsAllSelected
    {
        get
        {
            if (SelectionMode == CalendarSelectionMode.Single || SelectionMode == CalendarSelectionMode.None)
                return _selectedDates.Count > 0;

            // Check if all enabled dates in current month are selected
            var enabledDates = GetEnabledDatesInCurrentMonth();
            return enabledDates.Count > 0 && enabledDates.All(d => _selectedDates.Contains(d));
        }
    }

    /// <inheritdoc/>
    public bool SupportsMultipleSelection =>
        SelectionMode == CalendarSelectionMode.Multiple || SelectionMode == CalendarSelectionMode.Range;

    /// <inheritdoc/>
    void ISelectable.SelectAll()
    {
        if (SelectionMode == CalendarSelectionMode.None) return;

        var oldSelection = _selectedDates.ToList();

        if (SelectionMode == CalendarSelectionMode.Single)
        {
            // In single mode, select today if nothing selected
            if (_selectedDates.Count == 0)
            {
                var today = DateTime.Today;
                if (IsDateEnabled(today))
                {
                    SelectDate(today);
                }
            }
        }
        else
        {
            // In multiple/range mode, select all enabled dates in current month
            var enabledDates = GetEnabledDatesInCurrentMonth();
            foreach (var date in enabledDates)
            {
                _selectedDates.Add(date);
            }
            SelectedDate = enabledDates.FirstOrDefault();
            RebuildCalendar();
        }

        RaiseSelectionChanged(oldSelection, _selectedDates.ToList());
    }

    /// <inheritdoc/>
    void ISelectable.ClearSelection()
    {
        var oldSelection = _selectedDates.ToList();
        ClearSelection();
        RaiseSelectionChanged(oldSelection, _selectedDates.ToList());
    }

    /// <inheritdoc/>
    public object? GetSelection()
    {
        return _selectedDates.Count switch
        {
            0 => null,
            1 => _selectedDates.First(),
            _ => _selectedDates.OrderBy(d => d).ToList()
        };
    }

    /// <inheritdoc/>
    public void SetSelection(object? selection)
    {
        var oldSelection = _selectedDates.ToList();

        if (selection is null)
        {
            ClearSelection();
        }
        else if (selection is DateTime date)
        {
            SelectDate(date);
        }
        else if (selection is IEnumerable<DateTime> dates)
        {
            _selectedDates.Clear();
            foreach (var d in dates.Where(IsDateEnabled))
            {
                _selectedDates.Add(d.Date);
            }
            SelectedDate = _selectedDates.FirstOrDefault();
            RebuildCalendar();
        }
        else
        {
            throw new ArgumentException(
                $"Selection type {selection.GetType().Name} is not supported. " +
                "Use DateTime or IEnumerable<DateTime>.",
                nameof(selection));
        }

        RaiseSelectionChanged(oldSelection, _selectedDates.ToList());
    }

    private List<DateTime> GetEnabledDatesInCurrentMonth()
    {
        var result = new List<DateTime>();
        var daysInMonth = DateTime.DaysInMonth(_displayDate.Year, _displayDate.Month);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_displayDate.Year, _displayDate.Month, day);
            if (IsDateEnabled(date))
            {
                result.Add(date);
            }
        }

        return result;
    }

    private void RaiseSelectionChanged(List<DateTime> oldSelection, List<DateTime> newSelection)
    {
        // Only raise event if selection actually changed
        var oldSet = new HashSet<DateTime>(oldSelection);
        var newSet = new HashSet<DateTime>(newSelection);
        if (oldSet.SetEquals(newSet)) return;

        object? oldValue = oldSelection.Count switch
        {
            0 => null,
            1 => oldSelection[0],
            _ => oldSelection
        };

        object? newValue = newSelection.Count switch
        {
            0 => null,
            1 => newSelection[0],
            _ => newSelection
        };

        var args = new Base.SelectionChangedEventArgs(oldValue, newValue);
        SelectionChanged?.Invoke(this, args);
        SelectionChangedCommand?.Execute(SelectionChangedCommandParameter ?? newValue);

        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(IsAllSelected));
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Calendar"/> class.
    /// </summary>
    public Calendar()
    {
        InitializeComponent();
        _displayDate = DateTime.Today;
        _currentDisplayMode = CalendarDisplayMode.Month;
        BlackoutDates.CollectionChanged += (s, e) => RebuildCalendar();
        RebuildCalendar();
    }

    #endregion

    #region Navigation Methods

    /// <summary>
    /// Navigates to the previous month/year/decade.
    /// </summary>
    public void Previous()
    {
        switch (_currentDisplayMode)
        {
            case CalendarDisplayMode.Month:
                PreviousMonth();
                break;
            case CalendarDisplayMode.Year:
                _displayDate = _displayDate.AddYears(-1);
                RebuildCalendar();
                break;
            case CalendarDisplayMode.Decade:
                _displayDate = _displayDate.AddYears(-10);
                RebuildCalendar();
                break;
        }
    }

    /// <summary>
    /// Navigates to the next month/year/decade.
    /// </summary>
    public void Next()
    {
        switch (_currentDisplayMode)
        {
            case CalendarDisplayMode.Month:
                NextMonth();
                break;
            case CalendarDisplayMode.Year:
                _displayDate = _displayDate.AddYears(1);
                RebuildCalendar();
                break;
            case CalendarDisplayMode.Decade:
                _displayDate = _displayDate.AddYears(10);
                RebuildCalendar();
                break;
        }
    }

    /// <summary>
    /// Navigates to the previous month.
    /// </summary>
    public void PreviousMonth()
    {
        var oldDate = _displayDate;
        _displayDate = _displayDate.AddMonths(-1);
        DisplayDate = _displayDate;
        RebuildCalendar();

        var args = new CalendarDisplayDateChangedEventArgs(oldDate, _displayDate);
        DisplayDateChanged?.Invoke(this, args);
        DisplayDateChangedCommand?.Execute(DisplayDateChangedCommandParameter ?? args);
    }

    /// <summary>
    /// Navigates to the next month.
    /// </summary>
    public void NextMonth()
    {
        var oldDate = _displayDate;
        _displayDate = _displayDate.AddMonths(1);
        DisplayDate = _displayDate;
        RebuildCalendar();

        var args = new CalendarDisplayDateChangedEventArgs(oldDate, _displayDate);
        DisplayDateChanged?.Invoke(this, args);
        DisplayDateChangedCommand?.Execute(DisplayDateChangedCommandParameter ?? args);
    }

    /// <summary>
    /// Navigates to today's date.
    /// </summary>
    public void GoToToday()
    {
        var oldDate = _displayDate;
        _displayDate = DateTime.Today;
        DisplayDate = _displayDate;
        RebuildCalendar();

        if (oldDate.Month != _displayDate.Month || oldDate.Year != _displayDate.Year)
        {
            var args = new CalendarDisplayDateChangedEventArgs(oldDate, _displayDate);
            DisplayDateChanged?.Invoke(this, args);
            DisplayDateChangedCommand?.Execute(DisplayDateChangedCommandParameter ?? args);
        }
    }

    private void MoveFocus(int deltaX, int deltaY)
    {
        _focusedCol = Math.Clamp(_focusedCol + deltaX, 0, 6);
        _focusedRow = Math.Clamp(_focusedRow + deltaY, 0, 5);
        RebuildCalendar();
    }

    private void SelectFocusedDate()
    {
        // Calculate focused date from row/col
        var firstOfMonth = new DateTime(_displayDate.Year, _displayDate.Month, 1);
        var startDay = GetFirstDayOfWeekValue();
        var firstDayOffset = ((int)firstOfMonth.DayOfWeek - startDay + 7) % 7;
        var dayIndex = _focusedRow * 7 + _focusedCol - firstDayOffset;

        if (dayIndex >= 0 && dayIndex < DateTime.DaysInMonth(_displayDate.Year, _displayDate.Month))
        {
            var date = new DateTime(_displayDate.Year, _displayDate.Month, dayIndex + 1);
            SelectDate(date);
        }
    }

    #endregion

    #region Selection Methods

    /// <summary>
    /// Selects a date.
    /// </summary>
    public void SelectDate(DateTime date)
    {
        if (SelectionMode == CalendarSelectionMode.None) return;
        if (!IsDateEnabled(date)) return;

        // Raise selecting event
        var selectingArgs = new CalendarDateSelectingEventArgs(date, true);
        DateSelecting?.Invoke(this, selectingArgs);
        if (selectingArgs.Cancel) return;

        var oldSelection = _selectedDates.ToList();
        var normalizedDate = date.Date;

        _isInternalSelectionUpdate = true;
        try
        {
            switch (SelectionMode)
            {
                case CalendarSelectionMode.Single:
                    _selectedDates.Clear();
                    _selectedDates.Add(normalizedDate);
                    SelectedDate = normalizedDate;
                    break;

                case CalendarSelectionMode.Multiple:
                    if (_selectedDates.Contains(normalizedDate))
                    {
                        _selectedDates.Remove(normalizedDate);
                    }
                    else
                    {
                        _selectedDates.Add(normalizedDate);
                    }
                    SelectedDate = _selectedDates.FirstOrDefault();
                    break;

                case CalendarSelectionMode.Range:
                    if (_rangeStart == null)
                    {
                        _rangeStart = normalizedDate;
                        _selectedDates.Clear();
                        _selectedDates.Add(normalizedDate);
                    }
                    else
                    {
                        _selectedDates.Clear();
                        var start = _rangeStart.Value < normalizedDate ? _rangeStart.Value : normalizedDate;
                        var end = _rangeStart.Value < normalizedDate ? normalizedDate : _rangeStart.Value;
                        for (var d = start; d <= end; d = d.AddDays(1))
                        {
                            if (IsDateEnabled(d))
                            {
                                _selectedDates.Add(d);
                            }
                        }
                        _rangeStart = null;
                    }
                    SelectedDate = normalizedDate;
                    break;
            }
        }
        finally
        {
            _isInternalSelectionUpdate = false;
        }

        RebuildCalendar();

        var selectedArgs = new CalendarDateSelectedEventArgs(normalizedDate, true);
        DateSelected?.Invoke(this, selectedArgs);
        DateSelectedCommand?.Execute(DateSelectedCommandParameter ?? selectedArgs);

        // Raise ISelectable events
        RaiseSelectionChanged(oldSelection, _selectedDates.ToList());
    }

    /// <summary>
    /// Clears all selected dates.
    /// </summary>
    public void ClearSelection()
    {
        _selectedDates.Clear();
        _rangeStart = null;
        SelectedDate = null;
        RebuildCalendar();

        // Update ISelectable state
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(IsAllSelected));
    }

    private bool IsDateEnabled(DateTime date)
    {
        if (MinDate.HasValue && date.Date < MinDate.Value.Date) return false;
        if (MaxDate.HasValue && date.Date > MaxDate.Value.Date) return false;
        if (BlackoutDates.Any(d => d.Date == date.Date)) return false;
        return true;
    }

    private bool IsDateSelected(DateTime date)
    {
        return _selectedDates.Contains(date.Date);
    }

    #endregion

    #region UI Building

    private void RebuildCalendar()
    {
        switch (_currentDisplayMode)
        {
            case CalendarDisplayMode.Month:
                BuildMonthView();
                break;
            case CalendarDisplayMode.Year:
                BuildYearView();
                break;
            case CalendarDisplayMode.Decade:
                BuildDecadeView();
                break;
        }
    }

    private void BuildMonthView()
    {
        // Update header
        headerLabel.Text = _displayDate.ToString("MMMM yyyy");

        // Build day of week header
        dayOfWeekHeader.ColumnDefinitions.Clear();
        dayOfWeekHeader.Children.Clear();

        var startDay = GetFirstDayOfWeekValue();
        var culture = CultureInfo.CurrentCulture;

        if (ShowWeekNumbers)
        {
            dayOfWeekHeader.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(CellSize)));
            dayOfWeekHeader.Children.Add(new Label
            {
                Text = "Wk",
                FontSize = 11,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = MauiControlsExtrasTheme.GetForegroundColor().WithAlpha(0.6f)
            });
        }

        for (int i = 0; i < 7; i++)
        {
            dayOfWeekHeader.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(CellSize)));
            var dayIndex = (startDay + i) % 7;
            var dayName = culture.DateTimeFormat.AbbreviatedDayNames[dayIndex];

            var label = new Label
            {
                Text = dayName,
                FontSize = 11,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = MauiControlsExtrasTheme.GetForegroundColor().WithAlpha(0.6f)
            };
            dayOfWeekHeader.Add(label, ShowWeekNumbers ? i + 1 : i, 0);
        }

        // Build calendar grid
        calendarGrid.RowDefinitions.Clear();
        calendarGrid.ColumnDefinitions.Clear();
        calendarGrid.Children.Clear();

        for (int r = 0; r < 6; r++)
        {
            calendarGrid.RowDefinitions.Add(new RowDefinition(new GridLength(CellSize)));
        }

        var colOffset = 0;
        if (ShowWeekNumbers)
        {
            calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(CellSize)));
            colOffset = 1;
        }

        for (int c = 0; c < 7; c++)
        {
            calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(CellSize)));
        }

        var firstOfMonth = new DateTime(_displayDate.Year, _displayDate.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(_displayDate.Year, _displayDate.Month);
        var firstDayOffset = ((int)firstOfMonth.DayOfWeek - startDay + 7) % 7;

        var today = DateTime.Today;

        for (int row = 0; row < 6; row++)
        {
            // Week number
            if (ShowWeekNumbers)
            {
                var weekDate = firstOfMonth.AddDays(row * 7 - firstDayOffset);
                var weekNum = culture.Calendar.GetWeekOfYear(
                    weekDate,
                    culture.DateTimeFormat.CalendarWeekRule,
                    culture.DateTimeFormat.FirstDayOfWeek);

                calendarGrid.Add(new Label
                {
                    Text = weekNum.ToString(),
                    FontSize = 10,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = MauiControlsExtrasTheme.GetForegroundColor().WithAlpha(0.4f)
                }, 0, row);
            }

            for (int col = 0; col < 7; col++)
            {
                var dayIndex = row * 7 + col - firstDayOffset;

                if (dayIndex < 0 || dayIndex >= daysInMonth)
                {
                    // Empty cell for days outside the month
                    continue;
                }

                var date = new DateTime(_displayDate.Year, _displayDate.Month, dayIndex + 1);
                var cell = CreateDayCell(date, today, row, col);
                calendarGrid.Add(cell, col + colOffset, row);
            }
        }
    }

    private View CreateDayCell(DateTime date, DateTime today, int row, int col)
    {
        var isToday = date.Date == today;
        var isSelected = IsDateSelected(date);
        var isEnabled = IsDateEnabled(date);
        var isFocused = _hasKeyboardFocus && row == _focusedRow && col == _focusedCol;

        var container = new Border
        {
            WidthRequest = CellSize - 4,
            HeightRequest = CellSize - 4,
            StrokeThickness = isFocused ? 2 : (isToday ? 1 : 0),
            Stroke = isFocused ? new SolidColorBrush(EffectiveAccentColor) :
                     (isToday ? new SolidColorBrush(EffectiveTodayHighlightColor) : null),
            BackgroundColor = isSelected ? EffectiveSelectedDateColor : Colors.Transparent,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = (CellSize - 4) / 2 }
        };

        var label = new Label
        {
            Text = date.Day.ToString(),
            FontSize = 13,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            TextColor = isEnabled
                ? (isSelected ? Colors.White : EffectiveForegroundColor)
                : EffectiveDisabledColor
        };

        container.Content = label;

        if (isEnabled && SelectionMode != CalendarSelectionMode.None)
        {
            var tapGesture = new TapGestureRecognizer();
            var capturedDate = date;
            tapGesture.Tapped += (s, e) =>
            {
                _focusedRow = row;
                _focusedCol = col;
                SelectDate(capturedDate);
            };
            container.GestureRecognizers.Add(tapGesture);
        }

        return container;
    }

    private void BuildYearView()
    {
        headerLabel.Text = _displayDate.Year.ToString();
        dayOfWeekHeader.IsVisible = false;

        calendarGrid.RowDefinitions.Clear();
        calendarGrid.ColumnDefinitions.Clear();
        calendarGrid.Children.Clear();

        for (int r = 0; r < 4; r++)
        {
            calendarGrid.RowDefinitions.Add(new RowDefinition(new GridLength(CellSize * 1.5)));
        }
        for (int c = 0; c < 3; c++)
        {
            calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        var culture = CultureInfo.CurrentCulture;
        for (int month = 1; month <= 12; month++)
        {
            var row = (month - 1) / 3;
            var col = (month - 1) % 3;

            var monthName = culture.DateTimeFormat.AbbreviatedMonthNames[month - 1];
            var label = new Label
            {
                Text = monthName,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = EffectiveForegroundColor
            };

            var container = new Border
            {
                Content = label,
                BackgroundColor = Colors.Transparent,
                Padding = 8
            };

            var capturedMonth = month;
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                _displayDate = new DateTime(_displayDate.Year, capturedMonth, 1);
                _currentDisplayMode = CalendarDisplayMode.Month;
                dayOfWeekHeader.IsVisible = true;
                RebuildCalendar();
            };
            container.GestureRecognizers.Add(tapGesture);

            calendarGrid.Add(container, col, row);
        }
    }

    private void BuildDecadeView()
    {
        var startYear = (_displayDate.Year / 10) * 10;
        headerLabel.Text = $"{startYear} - {startYear + 9}";
        dayOfWeekHeader.IsVisible = false;

        calendarGrid.RowDefinitions.Clear();
        calendarGrid.ColumnDefinitions.Clear();
        calendarGrid.Children.Clear();

        for (int r = 0; r < 4; r++)
        {
            calendarGrid.RowDefinitions.Add(new RowDefinition(new GridLength(CellSize * 1.5)));
        }
        for (int c = 0; c < 3; c++)
        {
            calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        for (int i = 0; i < 12; i++)
        {
            var row = i / 3;
            var col = i % 3;
            var year = startYear - 1 + i;

            var label = new Label
            {
                Text = year.ToString(),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = (i == 0 || i == 11) ? EffectiveDisabledColor : EffectiveForegroundColor
            };

            var container = new Border
            {
                Content = label,
                BackgroundColor = Colors.Transparent,
                Padding = 8
            };

            if (i >= 1 && i <= 10)
            {
                var capturedYear = year;
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) =>
                {
                    _displayDate = new DateTime(capturedYear, 1, 1);
                    _currentDisplayMode = CalendarDisplayMode.Year;
                    RebuildCalendar();
                };
                container.GestureRecognizers.Add(tapGesture);
            }

            calendarGrid.Add(container, col, row);
        }
    }

    private int GetFirstDayOfWeekValue()
    {
        return FirstDayOfWeek switch
        {
            CalendarFirstDayOfWeek.Sunday => 0,
            CalendarFirstDayOfWeek.Monday => 1,
            _ => (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
        };
    }

    #endregion

    #region Event Handlers

    private void OnPreviousClicked(object? sender, EventArgs e) => Previous();
    private void OnNextClicked(object? sender, EventArgs e) => Next();

    private void OnHeaderTapped(object? sender, EventArgs e)
    {
        // Cycle through display modes
        _currentDisplayMode = _currentDisplayMode switch
        {
            CalendarDisplayMode.Month => CalendarDisplayMode.Year,
            CalendarDisplayMode.Year => CalendarDisplayMode.Decade,
            _ => CalendarDisplayMode.Month
        };
        dayOfWeekHeader.IsVisible = _currentDisplayMode == CalendarDisplayMode.Month;
        RebuildCalendar();
    }

    #endregion

    #region Property Changed Handlers

    private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Calendar calendar && newValue is DateTime date)
        {
            // Skip if this is an internal update from SelectDate()
            if (calendar._isInternalSelectionUpdate)
                return;

            calendar._displayDate = date;
            calendar._selectedDates.Clear();
            calendar._selectedDates.Add(date.Date);
            calendar.RebuildCalendar();
        }
    }

    private static void OnSelectionModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Calendar calendar)
        {
            calendar.ClearSelection();
        }
    }

    private static void OnDisplayDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Calendar calendar && newValue is DateTime date)
        {
            calendar._displayDate = date;
            calendar.RebuildCalendar();
        }
    }

    private static void OnDateRangeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Calendar calendar)
        {
            calendar.RebuildCalendar();
        }
    }

    private static void OnFirstDayOfWeekChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Calendar calendar)
        {
            calendar.RebuildCalendar();
        }
    }

    private static void OnShowWeekNumbersChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Calendar calendar)
        {
            calendar.RebuildCalendar();
        }
    }

    private static void OnCellSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Calendar calendar)
        {
            calendar.RebuildCalendar();
        }
    }

    #endregion

    #region HeaderedControlBase Overrides

    /// <inheritdoc/>
    protected override void OnHeaderBackgroundColorChanged(Color? oldValue, Color? newValue)
        => OnPropertyChanged(nameof(EffectiveHeaderBackgroundColor));

    /// <inheritdoc/>
    protected override void OnHeaderTextColorChanged(Color? oldValue, Color? newValue)
        => OnPropertyChanged(nameof(EffectiveHeaderTextColor));

    /// <inheritdoc/>
    protected override void OnHeaderFontSizeChanged(double oldValue, double newValue)
        => RebuildCalendar();

    /// <inheritdoc/>
    protected override void OnHeaderFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue)
        => RebuildCalendar();

    /// <inheritdoc/>
    protected override void OnHeaderFontFamilyChanged(string? oldValue, string? newValue)
        => RebuildCalendar();

    /// <inheritdoc/>
    protected override void OnHeaderPaddingChanged(Thickness oldValue, Thickness newValue)
    {
        // Handled by XAML binding
    }

    #endregion
}
