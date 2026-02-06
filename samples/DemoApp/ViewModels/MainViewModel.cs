using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DemoApp.Models;

namespace DemoApp.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<ControlInfo> _controls = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    private readonly List<ControlInfo> _allControls;

    public MainViewModel()
    {
        Title = "Gallery";
        _allControls = SampleData.AllControls;
        Controls = new ObservableCollection<ControlInfo>(_allControls);
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterControls();
    }

    private void FilterControls()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Controls = new ObservableCollection<ControlInfo>(_allControls);
        }
        else
        {
            var filtered = _allControls
                .Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                           c.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                           c.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Controls = new ObservableCollection<ControlInfo>(filtered);
        }
    }

    [RelayCommand]
    private async Task NavigateToControl(ControlInfo? control)
    {
        if (control is null) return;

        try
        {
            await Shell.Current.GoToAsync($"//{control.Route}");
        }
        catch
        {
            try
            {
                await Shell.Current.GoToAsync($"///{control.Route}");
            }
            catch
            {
                await Shell.Current.GoToAsync(control.Route);
            }
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        if (Application.Current is App app)
        {
            app.ToggleTheme();
        }
    }
}
