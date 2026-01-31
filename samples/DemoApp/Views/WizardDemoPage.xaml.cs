using DemoApp.ViewModels;
using MauiControlsExtras.Controls;

// NOTE: This demo uses direct event handlers for simplicity.
// For production applications, we recommend:
// - Using ViewModels with ICommand implementations
// - Binding commands in XAML instead of event handlers
// - Using CommunityToolkit.Mvvm for RelayCommand/ObservableObject

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

    private void OnResetClicked(object? sender, EventArgs e)
    {
        wizard.Reset();
        _viewModel.ResetCommand.Execute(null);
    }
}
