using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests.Helpers;

/// <summary>
/// Base class for tests that modify the global theme.
/// Resets <see cref="MauiControlsExtrasTheme.Current"/> to <see cref="ControlsTheme.Default"/>
/// in both constructor and <see cref="Dispose"/> to isolate test state.
/// </summary>
public abstract class ThemeTestBase : IDisposable
{
    protected ThemeTestBase()
    {
        MauiControlsExtrasTheme.ResetToDefault();
    }

    public void Dispose()
    {
        MauiControlsExtrasTheme.ResetToDefault();
        GC.SuppressFinalize(this);
    }
}
