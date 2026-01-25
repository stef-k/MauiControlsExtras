using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class AccordionDemoPage : ContentPage
{
    public AccordionDemoPage(AccordionDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
