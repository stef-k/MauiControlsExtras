namespace DemoApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell())
        {
            Title = "MauiControlsExtras Demo"
        };
    }

    public void ToggleTheme()
    {
        UserAppTheme = UserAppTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
    }
}
