using DemoApp.ViewModels;
using MauiControlsExtras.Controls;

// NOTE: This demo uses direct event handlers for simplicity.
// For production applications, we recommend:
// - Using ViewModels with ICommand implementations
// - Binding commands in XAML instead of event handlers
// - Using CommunityToolkit.Mvvm for RelayCommand/ObservableObject

namespace DemoApp.Views;

public partial class CalendarDemoPage : ContentPage
{
    private readonly CalendarDemoViewModel _viewModel;

    public CalendarDemoPage(CalendarDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnRangeDateSelected(object? sender, CalendarDateSelectedEventArgs e)
    {
        if (sender is Calendar calendar)
        {
            var dates = calendar.SelectedDates.OrderBy(d => d).ToList();
            if (dates.Count >= 1)
            {
                _viewModel.RangeStartDate = dates.First();
                _viewModel.RangeEndDate = dates.Count > 1 ? dates.Last() : null;
            }
        }
    }

    private void OnClearRangeClicked(object? sender, EventArgs e)
    {
        rangeCalendar.ClearSelection();
        _viewModel.RangeStartDate = null;
        _viewModel.RangeEndDate = null;
    }

    private void OnMultipleDateSelected(object? sender, CalendarDateSelectedEventArgs e)
    {
        if (sender is Calendar calendar)
        {
            multipleDatesLabel.Text = $"{calendar.SelectedDates.Count} dates selected";
        }
    }

    private void OnClearMultipleClicked(object? sender, EventArgs e)
    {
        multipleCalendar.ClearSelection();
        multipleDatesLabel.Text = "0 dates selected";
    }
}
