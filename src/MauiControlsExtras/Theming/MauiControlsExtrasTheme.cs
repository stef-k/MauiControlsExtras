namespace MauiControlsExtras.Theming;

/// <summary>
/// Static theme manager for MauiControlsExtras controls.
/// Provides global theme defaults and theme change notifications.
/// </summary>
public static class MauiControlsExtrasTheme
{
    private static readonly WeakEventManager _themeChangedEventManager = new();
    private static ControlsTheme _current = ControlsTheme.Default;

    /// <summary>
    /// Gets or sets the current theme configuration.
    /// Setting this property raises the ThemeChanged event.
    /// </summary>
    public static ControlsTheme Current
    {
        get => _current;
        set
        {
            _current = value ?? ControlsTheme.Default;
            RaiseThemeChanged();
        }
    }

    /// <summary>
    /// Gets the default accent color.
    /// </summary>
    public static Color DefaultAccentColor => Current.AccentColor;

    /// <summary>
    /// Gets the default corner radius.
    /// </summary>
    public static double DefaultCornerRadius => Current.CornerRadius;

    /// <summary>
    /// Gets the default border thickness.
    /// </summary>
    public static double DefaultBorderThickness => Current.BorderThickness;

    /// <summary>
    /// Gets whether controls should have shadows by default.
    /// </summary>
    public static bool DefaultHasShadow => Current.HasShadow;

    /// <summary>
    /// Gets the border color for the current app theme.
    /// </summary>
    /// <returns>The appropriate border color based on the current app theme.</returns>
    public static Color GetBorderColor()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Current.BorderColorDark
            : Current.BorderColorLight;
    }

    /// <summary>
    /// Occurs when the theme configuration changes.
    /// Uses WeakEventManager to prevent memory leaks.
    /// </summary>
    public static event EventHandler ThemeChanged
    {
        add => _themeChangedEventManager.AddEventHandler(value);
        remove => _themeChangedEventManager.RemoveEventHandler(value);
    }

    /// <summary>
    /// Applies a predefined theme.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    public static void ApplyTheme(ControlsTheme theme)
    {
        Current = theme;
    }

    /// <summary>
    /// Resets to the default theme.
    /// </summary>
    public static void ResetToDefault()
    {
        Current = ControlsTheme.Default;
    }

    /// <summary>
    /// Notifies all subscribed controls that the theme has changed.
    /// Call this after modifying theme properties without replacing the entire theme.
    /// </summary>
    public static void RaiseThemeChanged()
    {
        _themeChangedEventManager.HandleEvent(null, EventArgs.Empty, nameof(ThemeChanged));
    }
}
