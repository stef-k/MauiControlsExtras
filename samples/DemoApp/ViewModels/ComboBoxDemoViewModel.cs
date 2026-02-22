using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DemoApp.Models;

namespace DemoApp.ViewModels;

public partial class ComboBoxDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Country> _countries = [];

    [ObservableProperty]
    private Country? _selectedCountry;

    [ObservableProperty]
    private Country? _searchableCountry;

    [ObservableProperty]
    private string? _selectedDepartment;

    [ObservableProperty]
    private ObservableCollection<string> _departments = [];

    [ObservableProperty]
    private Employee? _selectedEmployee;

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = [];

    [ObservableProperty]
    private bool _isSearchVisible = true;

    [ObservableProperty]
    private string? _selectedPriority;

    [ObservableProperty]
    private ObservableCollection<string> _priorities = [];

    [ObservableProperty]
    private Country? _popupCountry;

    public ComboBoxDemoViewModel()
    {
        Title = "ComboBox Demo";
        Countries = new ObservableCollection<Country>(SampleData.Countries);
        Departments = new ObservableCollection<string>(SampleData.Departments);
        Employees = new ObservableCollection<Employee>(SampleData.Employees.Take(10));
        Priorities = new ObservableCollection<string>(["Low", "Normal", "High", "Critical"]);
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

    partial void OnSelectedPriorityChanged(string? value)
    {
        if (value is not null)
            UpdateStatus($"Selected priority: {value}");
    }

    partial void OnIsSearchVisibleChanged(bool value)
    {
        UpdateStatus($"Search visibility: {(value ? "Visible" : "Hidden")}");
    }

    partial void OnPopupCountryChanged(Country? value)
    {
        if (value is not null)
            UpdateStatus($"Popup selected: {value.Name} ({value.Code})");
    }
}
