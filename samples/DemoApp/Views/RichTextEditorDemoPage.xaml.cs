using DemoApp.ViewModels;
using MauiControlsExtras.Controls;

// NOTE: This demo uses direct event handlers for simplicity.
// For production applications, we recommend:
// - Using ViewModels with ICommand implementations
// - Binding commands in XAML instead of event handlers
// - Using CommunityToolkit.Mvvm for RelayCommand/ObservableObject

namespace DemoApp.Views;

public partial class RichTextEditorDemoPage : ContentPage
{
    private readonly RichTextEditorDemoViewModel _viewModel;

    public RichTextEditorDemoPage(RichTextEditorDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnContentChanged(object? sender, RichTextContentChangedEventArgs e)
    {
        _viewModel.GetHtmlCommand.Execute(null);
    }
}
