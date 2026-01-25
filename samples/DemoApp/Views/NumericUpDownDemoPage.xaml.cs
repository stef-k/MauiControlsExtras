using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class NumericUpDownDemoPage : ContentPage
{
    public NumericUpDownDemoPage(NumericUpDownDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
