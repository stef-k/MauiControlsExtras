using DemoApp.ViewModels;

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
