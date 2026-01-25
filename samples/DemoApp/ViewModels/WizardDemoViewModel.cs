using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DemoApp.ViewModels;

public partial class WizardDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private int _currentStep;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private bool _acceptTerms;

    [ObservableProperty]
    private bool _isComplete;

    public WizardDemoViewModel()
    {
        Title = "Wizard Demo";
    }

    partial void OnCurrentStepChanged(int value)
    {
        UpdateStatus($"Step {value + 1} of 4");
    }

    [RelayCommand]
    private void StepChanged(int step)
    {
        CurrentStep = step;
    }

    [RelayCommand]
    private void Complete()
    {
        IsComplete = true;
        UpdateStatus("Wizard completed successfully!");
    }

    [RelayCommand]
    private void Reset()
    {
        CurrentStep = 0;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Address = string.Empty;
        City = string.Empty;
        AcceptTerms = false;
        IsComplete = false;
        UpdateStatus("Wizard reset");
    }
}
