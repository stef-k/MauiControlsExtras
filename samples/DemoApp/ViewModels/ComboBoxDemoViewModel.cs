using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DemoApp.Models;

namespace DemoApp.ViewModels;

public partial class ComboBoxDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Country> _countries;

    [ObservableProperty]
    private Country? _selectedCountry;

    [ObservableProperty]
    private Country? _searchableCountry;

    [ObservableProperty]
    private string? _selectedDepartment;

    [ObservableProperty]
    private ObservableCollection<string> _departments;

    [ObservableProperty]
    private Employee? _selectedEmployee;

    [ObservableProperty]
    private ObservableCollection<Employee> _employees;

    public ComboBoxDemoViewModel()
    {
        Title = "ComboBox Demo";
        _countries = new ObservableCollection<Country>(SampleData.Countries);
        _departments = new ObservableCollection<string>(SampleData.Departments);
        _employees = new ObservableCollection<Employee>(SampleData.Employees.Take(10));
    }

    partial void OnSelectedCountryChanged(Country? value)
    {
        if (value is not null)
            UpdateStatus($"Selected country: {value.Name} ({value.Code})");
    }

    partial void OnSearchableCountryChanged(Country? value)
    {
        if (value is not null)
            UpdateStatus($"Searched and selected: {value.Name}");
    }

    partial void OnSelectedDepartmentChanged(string? value)
    {
        if (value is not null)
            UpdateStatus($"Selected department: {value}");
    }

    partial void OnSelectedEmployeeChanged(Employee? value)
    {
        if (value is not null)
            UpdateStatus($"Selected employee: {value.Name}");
    }
}
