using DemoApp.ViewModels;
using DemoApp.Models;
namespace DemoApp.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnControlSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is not MainViewModel vm) return;
        if (e.CurrentSelection.FirstOrDefault() is not ControlInfo selectedControl) return;

        controlsCollectionView.SelectedItem = null;
        await vm.NavigateToControlCommand.ExecuteAsync(selectedControl);
    }
}
