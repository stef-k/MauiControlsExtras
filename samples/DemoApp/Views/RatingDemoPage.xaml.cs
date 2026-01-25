using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class RatingDemoPage : ContentPage
{
    public RatingDemoPage(RatingDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
