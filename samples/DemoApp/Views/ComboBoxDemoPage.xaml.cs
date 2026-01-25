using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class ComboBoxDemoPage : ContentPage
{
    public ComboBoxDemoPage(ComboBoxDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
