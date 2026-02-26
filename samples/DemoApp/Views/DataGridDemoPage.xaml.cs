using DemoApp.Models;
using DemoApp.ViewModels;
using MauiControlsExtras.Controls;

// NOTE: This demo uses direct event handlers for simplicity.
// For production applications, we recommend:
// - Using ViewModels with ICommand implementations
// - Binding commands in XAML instead of event handlers
// - Using CommunityToolkit.Mvvm for RelayCommand/ObservableObject

namespace DemoApp.Views;

public partial class DataGridDemoPage : ContentPage
{
    public DataGridDemoPage(DataGridDemoViewModel viewModel)
    {
        InitializeComponent();

        // Add columns programmatically (XAML column definitions have issues with ContentProperty attribute)
        // ComboBox column now uses the library's ComboBox control with search/filtering support
        var departmentColumn = new DataGridComboBoxColumn
        {
            Header = "Department",
            Binding = "Department",
            SizeMode = DataGridColumnSizeMode.FitHeader,
            ItemsSource = SampleData.Departments,
            Placeholder = "Search departments..."
        };

        dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = "Id", IsReadOnly = true, SizeMode = DataGridColumnSizeMode.FitHeader });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = "Name", Width = 2, SizeMode = DataGridColumnSizeMode.Fill });
        dataGrid.Columns.Add(departmentColumn);
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Salary", Binding = "Salary", Format = "C0", SizeMode = DataGridColumnSizeMode.FitHeader });
        dataGrid.Columns.Add(new DataGridDatePickerColumn { Header = "Hire Date", Binding = "HireDate", Format = "d", SizeMode = DataGridColumnSizeMode.FitHeader });
        dataGrid.Columns.Add(new DataGridCheckBoxColumn { Header = "Active", Binding = "IsActive", SizeMode = DataGridColumnSizeMode.FitHeader });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = "Email", SizeMode = DataGridColumnSizeMode.Fill });

        BindingContext = viewModel;
    }

    private void OnSelectionChanged(object? sender, object? e)
    {
        // Selection is handled via binding
    }
}
