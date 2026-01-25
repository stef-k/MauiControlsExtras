using DemoApp.ViewModels;
using MauiControlsExtras.Controls;

namespace DemoApp.Views;

public partial class WizardDemoPage : ContentPage
{
    private readonly WizardDemoViewModel _viewModel;

    public WizardDemoPage(WizardDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnStepChanged(object? sender, WizardStepChangedEventArgs e)
    {
        _viewModel.StepChangedCommand.Execute(e.NewIndex);
    }

    private void OnWizardFinished(object? sender, WizardFinishedEventArgs e)
    {
        _viewModel.CompleteCommand.Execute(null);
    }
}
