using CommunityToolkit.Mvvm.ComponentModel;

namespace DemoApp.ViewModels;

public partial class MaskedEntryDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _dateValue = string.Empty;

    [ObservableProperty]
    private string _creditCardValue = string.Empty;

    public MaskedEntryDemoViewModel()
    {
        Title = "MaskedEntry Demo";
    }

    partial void OnDateValueChanged(string value) => UpdateStatus($"Date: {value}");
    partial void OnCreditCardValueChanged(string value) => UpdateStatus($"Card: {value}");
}
