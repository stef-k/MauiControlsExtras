using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class RangeSliderDemoPage : ContentPage
{
    public RangeSliderDemoPage(RangeSliderDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
