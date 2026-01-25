using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class MaskedEntryDemoPage : ContentPage
{
    public MaskedEntryDemoPage(MaskedEntryDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
