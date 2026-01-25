using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DemoApp.Models;

namespace DemoApp.ViewModels;

public partial class BindingNavigatorDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Employee> _employees;

    [ObservableProperty]
    private int _currentIndex;

    [ObservableProperty]
    private Employee? _currentEmployee;

    [ObservableProperty]
    private bool _hasChanges;

    public BindingNavigatorDemoViewModel()
    {
        Title = "BindingNavigator Demo";
        _employees = new ObservableCollection<Employee>(SampleData.Employees.Take(10));

        if (_employees.Count > 0)
        {
            CurrentIndex = 0;
            CurrentEmployee = _employees[0];
        }
    }

    partial void OnCurrentIndexChanged(int value)
    {
        if (value >= 0 && value < Employees.Count)
        {
            CurrentEmployee = Employees[value];
            UpdateStatus($"Record {value + 1} of {Employees.Count}");
        }
    }

    [RelayCommand]
    private void MoveFirst()
    {
        CurrentIndex = 0;
    }

    [RelayCommand]
    private void MovePrevious()
    {
        if (CurrentIndex > 0)
            CurrentIndex--;
    }

    [RelayCommand]
    private void MoveNext()
    {
        if (CurrentIndex < Employees.Count - 1)
            CurrentIndex++;
    }

    [RelayCommand]
    private void MoveLast()
    {
        CurrentIndex = Employees.Count - 1;
    }

    [RelayCommand]
    private void AddNew()
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
            Email = $"new{newId}@company.com"
        };
        Employees.Add(employee);
        CurrentIndex = Employees.Count - 1;
        HasChanges = true;
        UpdateStatus("New record added");
    }

    [RelayCommand]
    private void Delete()
    {
        if (CurrentEmployee is not null && Employees.Count > 0)
        {
            var name = CurrentEmployee.Name;
            var index = CurrentIndex;
            Employees.Remove(CurrentEmployee);

            if (Employees.Count > 0)
            {
                CurrentIndex = Math.Min(index, Employees.Count - 1);
            }
            else
            {
                CurrentEmployee = null;
            }

            HasChanges = true;
            UpdateStatus($"Deleted: {name}");
        }
    }

    [RelayCommand]
    private void Save()
    {
        HasChanges = false;
        UpdateStatus("Changes saved");
    }

    [RelayCommand]
    private void Refresh()
    {
        Employees = new ObservableCollection<Employee>(SampleData.Employees.Take(10));
        CurrentIndex = 0;
        HasChanges = false;
        UpdateStatus("Data refreshed");
    }
}
