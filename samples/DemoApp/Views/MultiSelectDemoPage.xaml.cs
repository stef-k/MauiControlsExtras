using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class MultiSelectDemoPage : ContentPage
{
    public MultiSelectDemoPage(MultiSelectDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
