using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DemoApp.ViewModels;

public partial class CalendarDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private DateTime? _selectedDate;

    [ObservableProperty]
    private DateTime? _rangeStartDate;

    [ObservableProperty]
    private DateTime? _rangeEndDate;

    [ObservableProperty]
    private ObservableCollection<DateTime> _multipleDates = [];

    [ObservableProperty]
    private DateTime _displayMonth;

    [ObservableProperty]
    private DateTime _minimumDate;

    [ObservableProperty]
    private DateTime _maximumDate;

    public CalendarDemoViewModel()
    {
        Title = "Calendar Demo";
        SelectedDate = DateTime.Today;
        DisplayMonth = DateTime.Today;
        MinimumDate = DateTime.Today.AddYears(-1);
        MaximumDate = DateTime.Today.AddYears(1);
    }

    partial void OnSelectedDateChanged(DateTime? value)
    {
        if (value.HasValue)
            UpdateStatus($"Selected date: {value:d}");
    }

    partial void OnRangeStartDateChanged(DateTime? value)
    {
        if (value.HasValue && RangeEndDate.HasValue)
            UpdateStatus($"Range: {value:d} - {RangeEndDate:d}");
    }

    partial void OnRangeEndDateChanged(DateTime? value)
    {
        if (value.HasValue && RangeStartDate.HasValue)
            UpdateStatus($"Range: {RangeStartDate:d} - {value:d}");
    }

    partial void OnMultipleDatesChanged(ObservableCollection<DateTime> value)
    {
        UpdateStatus($"Selected {value.Count} dates");
    }
}
