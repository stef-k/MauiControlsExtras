using DemoApp.ViewModels;
using MauiControlsExtras.Controls;

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
