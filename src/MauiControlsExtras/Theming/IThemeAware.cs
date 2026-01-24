namespace MauiControlsExtras.Theming;

/// <summary>
/// Interface for controls that need to respond to theme changes.
/// </summary>
public interface IThemeAware
{
    /// <summary>
    /// Called when the application theme changes.
    /// </summary>
    /// <param name="theme">The new application theme.</param>
    void OnThemeChanged(AppTheme theme);
}
