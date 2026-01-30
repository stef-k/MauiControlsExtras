using DemoApp.Models;
using DemoApp.ViewModels;
using MauiControlsExtras.Controls;

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
            Width = 150,
            ItemsSource = SampleData.Departments,
            Placeholder = "Search departments..."
        };

        dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = "Id", Width = 60, IsReadOnly = true });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = "Name", Width = 150 });
        dataGrid.Columns.Add(departmentColumn);
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Salary", Binding = "Salary", Width = 100, Format = "C0" });
        dataGrid.Columns.Add(new DataGridDatePickerColumn { Header = "Hire Date", Binding = "HireDate", Width = 120, Format = "d" });
        dataGrid.Columns.Add(new DataGridCheckBoxColumn { Header = "Active", Binding = "IsActive", Width = 70 });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = "Email", Width = 200 });

        BindingContext = viewModel;
    }

    private void OnSelectionChanged(object? sender, object? e)
    {
        // Selection is handled via binding
    }
}
