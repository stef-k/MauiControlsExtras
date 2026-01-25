using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DemoApp.Models;

namespace DemoApp.ViewModels;

public partial class PropertyGridDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private Product _selectedObject;

    [ObservableProperty]
    private bool _showCategories = true;

    [ObservableProperty]
    private bool _isReadOnly;

    public PropertyGridDemoViewModel()
    {
        Title = "PropertyGrid Demo";
        _selectedObject = SampleData.SampleProduct;
    }

    partial void OnShowCategoriesChanged(bool value)
    {
        UpdateStatus($"Categories: {(value ? "Visible" : "Hidden")}");
    }

    partial void OnIsReadOnlyChanged(bool value)
    {
        UpdateStatus($"Read-only mode: {(value ? "On" : "Off")}");
    }

    [RelayCommand]
    private void HandlePropertyChanged(string propertyName)
    {
        UpdateStatus($"Property changed: {propertyName}");
    }

    [RelayCommand]
    private void ResetProduct()
    {
        SelectedObject = new Product
        {
            Name = "Premium Widget",
            Description = "A high-quality widget for all your needs",
            Price = 99.99m,
            Quantity = 150,
            Category = "Electronics",
            IsAvailable = true,
            ReleaseDate = new DateTime(2024, 6, 15),
            ThemeColor = Colors.Blue
        };
        UpdateStatus("Product reset to defaults");
    }
}
