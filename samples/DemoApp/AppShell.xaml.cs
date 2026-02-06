namespace DemoApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    private async void OnGoToGalleryClicked(object? sender, EventArgs e)
    {
        await GoToGalleryAsync();
    }

    private async void OnToggleThemeClicked(object? sender, EventArgs e)
    {
        if (Application.Current is App app)
        {
            app.ToggleTheme();
        }

        await GoToGalleryAsync();
    }

    private static async Task GoToGalleryAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch
        {
            await Shell.Current.GoToAsync("///MainPage");
        }
    }
}
