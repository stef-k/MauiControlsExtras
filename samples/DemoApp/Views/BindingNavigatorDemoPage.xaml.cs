using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class BindingNavigatorDemoPage : ContentPage
{
    public BindingNavigatorDemoPage(BindingNavigatorDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
