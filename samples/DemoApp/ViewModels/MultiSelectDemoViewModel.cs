using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DemoApp.Models;

namespace DemoApp.ViewModels;

public partial class MultiSelectDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Country> _countries = [];

    [ObservableProperty]
    private ObservableCollection<Country> _selectedCountries = [];

    [ObservableProperty]
    private ObservableCollection<string> _departments = [];

    [ObservableProperty]
    private ObservableCollection<string> _selectedDepartments = [];

    [ObservableProperty]
    private ObservableCollection<string> _skills = [];

    [ObservableProperty]
    private ObservableCollection<string> _selectedSkills = [];

    public MultiSelectDemoViewModel()
    {
        Title = "MultiSelectComboBox Demo";
        Countries = new ObservableCollection<Country>(SampleData.Countries);
        Departments = new ObservableCollection<string>(SampleData.Departments);
        Skills = new ObservableCollection<string>([
            "C#", "JavaScript", "Python", "Java", "TypeScript",
            "React", "Angular", "Vue.js", ".NET", "Node.js",
            "SQL", "MongoDB", "Docker", "Kubernetes", "AWS"
        ]);
    }

    partial void OnSelectedCountriesChanged(ObservableCollection<Country> value)
    {
        var names = string.Join(", ", value.Select(c => c.Name));
        UpdateStatus($"Selected countries: {(string.IsNullOrEmpty(names) ? "None" : names)}");
    }

    partial void OnSelectedDepartmentsChanged(ObservableCollection<string> value)
    {
        var names = string.Join(", ", value);
        UpdateStatus($"Selected departments: {(string.IsNullOrEmpty(names) ? "None" : names)}");
    }

    partial void OnSelectedSkillsChanged(ObservableCollection<string> value)
    {
        UpdateStatus($"Selected {value.Count} skills");
    }
}
