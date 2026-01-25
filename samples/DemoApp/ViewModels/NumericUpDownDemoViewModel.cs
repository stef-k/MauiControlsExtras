using CommunityToolkit.Mvvm.ComponentModel;

namespace DemoApp.ViewModels;

public partial class NumericUpDownDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private double _basicValue = 10;

    [ObservableProperty]
    private double _quantityValue = 1;

    [ObservableProperty]
    private double _temperatureValue = 20;

    [ObservableProperty]
    private double _percentageValue = 50;

    [ObservableProperty]
    private double _priceValue = 99.99;

    public NumericUpDownDemoViewModel()
    {
        Title = "NumericUpDown Demo";
    }

    partial void OnBasicValueChanged(double value) => UpdateStatus($"Basic value: {value}");
    partial void OnQuantityValueChanged(double value) => UpdateStatus($"Quantity: {value}");
    partial void OnTemperatureValueChanged(double value) => UpdateStatus($"Temperature: {value}°C");
    partial void OnPercentageValueChanged(double value) => UpdateStatus($"Percentage: {value}%");
    partial void OnPriceValueChanged(double value) => UpdateStatus($"Price: ${value:F2}");
}
