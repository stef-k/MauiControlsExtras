using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DemoApp.Models;

namespace DemoApp.ViewModels;

public partial class DataGridDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Employee> _employees = [];

    [ObservableProperty]
    private Employee? _selectedEmployee;

    [ObservableProperty]
    private bool _canUserSort = true;

    [ObservableProperty]
    private bool _canUserFilter = true;

    [ObservableProperty]
    private bool _canUserEdit = true;

    [ObservableProperty]
    private bool _enablePagination;

    public List<string> Departments => SampleData.Departments;

    public DataGridDemoViewModel()
    {
        Title = "DataGridView Demo";
        Employees = new ObservableCollection<Employee>(SampleData.Employees);
    }

    partial void OnSelectedEmployeeChanged(Employee? value)
    {
        if (value is not null)
            UpdateStatus($"Selected: {value.Name} ({value.Department})");
    }

    [RelayCommand]
    private void AddEmployee()
    {
        var newId = Employees.Count > 0 ? Employees.Max(e => e.Id) + 1 : 1;
        var employee = new Employee
        {
            Id = newId,
            Name = "New Employee",
            Department = "Engineering",
            Salary = 60000,
            HireDate = DateTime.Today,
            IsActive = true,
            Email = $"employee{newId}@company.com"
        };
        Employees.Add(employee);
        SelectedEmployee = employee;
        UpdateStatus($"Added new employee: {employee.Name}");
    }

    [RelayCommand]
    private void DeleteEmployee()
    {
        if (SelectedEmployee is not null)
        {
            var name = SelectedEmployee.Name;
            Employees.Remove(SelectedEmployee);
            SelectedEmployee = null;
            UpdateStatus($"Deleted employee: {name}");
        }
    }

    [RelayCommand]
    private void RefreshData()
    {
        Employees = new ObservableCollection<Employee>(SampleData.Employees);
        UpdateStatus("Data refreshed");
    }
}
