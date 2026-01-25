using DemoApp.ViewModels;
using MauiControlsExtras.Controls;

namespace DemoApp.Views;

public partial class PropertyGridDemoPage : ContentPage
{
    private readonly PropertyGridDemoViewModel _viewModel;

    public PropertyGridDemoPage(PropertyGridDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnPropertyValueChanged(object? sender, PropertyValueChangedEventArgs e)
    {
        _viewModel.HandlePropertyChangedCommand.Execute(e.Property.DisplayName);
    }
}
