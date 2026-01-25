using DemoApp.Models;
using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class DataGridDemoPage : ContentPage
{
    public DataGridDemoPage(DataGridDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Set the department column ItemsSource after initialization
        departmentColumn.ItemsSource = SampleData.Departments;
    }

    private void OnSelectionChanged(object? sender, object? e)
    {
        // Selection is handled via binding
    }
}
