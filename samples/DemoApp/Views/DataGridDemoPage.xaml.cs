using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class DataGridDemoPage : ContentPage
{
    public DataGridDemoPage(DataGridDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnSelectionChanged(object? sender, object? e)
    {
        // Selection is handled via binding
    }
}
