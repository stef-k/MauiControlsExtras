using Microsoft.UI.Xaml;

namespace DemoApp.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => DemoApp.MauiProgram.CreateMauiApp();
}
