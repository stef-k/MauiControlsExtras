using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiControlsExtras.Controls;

namespace DemoApp.ViewModels;

public partial class BreadcrumbDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _currentPath = string.Empty;

    public BreadcrumbDemoViewModel()
    {
        Title = "Breadcrumb Demo";
        UpdateCurrentPath(["Home", "Products", "Electronics", "Phones"]);
    }

    [RelayCommand]
    private void NavigateTo(string item)
    {
        UpdateStatus($"Navigated to: {item}");
    }

    [RelayCommand]
    private void GoToHome()
    {
        UpdateCurrentPath(["Home"]);
        UpdateStatus("Navigated to Home");
    }

    [RelayCommand]
    private void GoBack()
    {
        UpdateStatus("Back navigation clicked");
    }

    [RelayCommand]
    private void Reset()
    {
        UpdateCurrentPath(["Home", "Products", "Electronics", "Phones"]);
        UpdateStatus("Breadcrumb reset");
    }

    private void UpdateCurrentPath(string[] items)
    {
        CurrentPath = string.Join(" / ", items);
    }
}
