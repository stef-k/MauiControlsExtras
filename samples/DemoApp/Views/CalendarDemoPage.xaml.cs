using DemoApp.ViewModels;
using MauiControlsExtras.Controls;

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

    private void OnMultipleDateSelected(object? sender, CalendarDateSelectedEventArgs e)
    {
        if (sender is Calendar calendar)
        {
            multipleDatesLabel.Text = $"{calendar.SelectedDates.Count} dates selected";
        }
    }
}
