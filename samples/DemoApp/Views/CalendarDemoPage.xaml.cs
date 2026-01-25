using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class CalendarDemoPage : ContentPage
{
    public CalendarDemoPage(CalendarDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
