using CommunityToolkit.Mvvm.ComponentModel;

namespace DemoApp.ViewModels;

public partial class MaskedEntryDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _dateValue = string.Empty;

    [ObservableProperty]
    private string _ssnValue = string.Empty;

    [ObservableProperty]
    private string _creditCardValue = string.Empty;

    [ObservableProperty]
    private string _zipCodeValue = string.Empty;

    [ObservableProperty]
    private string _ipAddressValue = string.Empty;

    public MaskedEntryDemoViewModel()
    {
        Title = "MaskedEntry Demo";
    }

    partial void OnPhoneNumberChanged(string value) => UpdateStatus($"Phone: {value}");
    partial void OnDateValueChanged(string value) => UpdateStatus($"Date: {value}");
    partial void OnSsnValueChanged(string value) => UpdateStatus($"SSN: {value}");
    partial void OnCreditCardValueChanged(string value) => UpdateStatus($"Card: {value}");
    partial void OnZipCodeValueChanged(string value) => UpdateStatus($"ZIP: {value}");
    partial void OnIpAddressValueChanged(string value) => UpdateStatus($"IP: {value}");
}
