using CommunityToolkit.Mvvm.ComponentModel;

namespace DemoApp.ViewModels;

public partial class RangeSliderDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private double _priceMin = 100;

    [ObservableProperty]
    private double _priceMax = 500;

    [ObservableProperty]
    private double _ageMin = 18;

    [ObservableProperty]
    private double _ageMax = 65;

    [ObservableProperty]
    private double _temperatureMin = 15;

    [ObservableProperty]
    private double _temperatureMax = 25;

    [ObservableProperty]
    private double _timeMin = 9;

    [ObservableProperty]
    private double _timeMax = 17;

    public RangeSliderDemoViewModel()
    {
        Title = "RangeSlider Demo";
    }

    partial void OnPriceMinChanged(double value) => UpdateStatus($"Price range: ${value:F0} - ${PriceMax:F0}");
    partial void OnPriceMaxChanged(double value) => UpdateStatus($"Price range: ${PriceMin:F0} - ${value:F0}");
    partial void OnAgeMinChanged(double value) => UpdateStatus($"Age range: {value:F0} - {AgeMax:F0} years");
    partial void OnAgeMaxChanged(double value) => UpdateStatus($"Age range: {AgeMin:F0} - {value:F0} years");
    partial void OnTemperatureMinChanged(double value) => UpdateStatus($"Temperature: {value:F1}°C - {TemperatureMax:F1}°C");
    partial void OnTemperatureMaxChanged(double value) => UpdateStatus($"Temperature: {TemperatureMin:F1}°C - {value:F1}°C");
    partial void OnTimeMinChanged(double value) => UpdateStatus($"Time: {value:F0}:00 - {TimeMax:F0}:00");
    partial void OnTimeMaxChanged(double value) => UpdateStatus($"Time: {TimeMin:F0}:00 - {value:F0}:00");
}
