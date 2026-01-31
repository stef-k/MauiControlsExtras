using DemoApp.ViewModels;

// NOTE: This demo uses direct event handlers for simplicity.
// For production applications, we recommend:
// - Using ViewModels with ICommand implementations
// - Binding commands in XAML instead of event handlers
// - Using CommunityToolkit.Mvvm for RelayCommand/ObservableObject

namespace DemoApp.Views;

public partial class TokenEntryDemoPage : ContentPage
{
    private readonly TokenEntryDemoViewModel _viewModel;

    public TokenEntryDemoPage(TokenEntryDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnTokenAdded(object? sender, string e)
    {
        _viewModel.TokenAddedCommand.Execute(e);
    }

    private void OnTokenRemoved(object? sender, string e)
    {
        _viewModel.TokenRemovedCommand.Execute(e);
    }
}
