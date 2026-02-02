using CommunityToolkit.Mvvm.ComponentModel;

namespace DemoApp.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public void UpdateStatus(string message)
    {
        StatusMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
    }
}
