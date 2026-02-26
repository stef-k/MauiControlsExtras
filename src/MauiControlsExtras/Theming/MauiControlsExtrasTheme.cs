namespace MauiControlsExtras.Theming;

/// <summary>
/// Static theme manager for MauiControlsExtras controls.
/// Provides global theme defaults and theme change notifications.
/// </summary>
public static class MauiControlsExtrasTheme
{
    private static readonly WeakEventManager _themeChangedEventManager = new();
    private static ControlsTheme _current = ControlsTheme.Default;
    private static int _isMauiThemeBridged; // 0 = false, 1 = true; accessed via Interlocked
    private static AppTheme _lastNotifiedTheme;

    #region Current Theme

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

    #endregion

    #region Theme-Aware Color Helpers

    /// <summary>
    /// Gets the border color for the current app theme.
    /// </summary>
    public static Color GetBorderColor()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Current.BorderColorDark
            : Current.BorderColorLight;
    }

    /// <summary>
    /// Gets the foreground color for the current app theme.
    /// </summary>
    public static Color GetForegroundColor()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Current.ForegroundColorDark
            : Current.ForegroundColorLight;
    }

    /// <summary>
    /// Gets the background color for the current app theme.
    /// </summary>
    public static Color GetBackgroundColor()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Current.BackgroundColorDark
            : Current.BackgroundColorLight;
    }

    /// <summary>
    /// Gets the surface color for the current app theme.
    /// </summary>
    public static Color GetSurfaceColor()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Current.SurfaceColorDark
            : Current.SurfaceColorLight;
    }

    /// <summary>
    /// Gets the placeholder color for the current app theme.
    /// </summary>
    public static Color GetPlaceholderColor()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Current.PlaceholderColorDark
            : Current.PlaceholderColorLight;
    }

    /// <summary>
    /// Gets the shadow color for the current app theme.
    /// </summary>
    public static Color GetShadowColor()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Current.ShadowColorDark
            : Current.ShadowColorLight;
    }

    /// <summary>
    /// Gets the disabled border color for the current app theme.
    /// </summary>
    public static Color GetDisabledBorderColor()
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark
            ? Current.DisabledBorderColorDark
            : Current.DisabledBorderColorLight;
    }

    #endregion

    #region Quick Access Properties

    /// <summary>
    /// Gets the default accent color.
    /// </summary>
    public static Color DefaultAccentColor => Current.AccentColor;

    /// <summary>
    /// Gets the default error color.
    /// </summary>
    public static Color DefaultErrorColor => Current.ErrorColor;

    /// <summary>
    /// Gets the default success color.
    /// </summary>
    public static Color DefaultSuccessColor => Current.SuccessColor;

    /// <summary>
    /// Gets the default warning color.
    /// </summary>
    public static Color DefaultWarningColor => Current.WarningColor;

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
    /// Gets the default font size.
    /// </summary>
    public static double DefaultFontSize => Current.FontSize;

    /// <summary>
    /// Gets the default font family.
    /// </summary>
    public static string? DefaultFontFamily => Current.FontFamily;

    /// <summary>
    /// Gets the default animation duration in milliseconds.
    /// </summary>
    public static int DefaultAnimationDuration => Current.AnimationDuration;

    /// <summary>
    /// Gets the default animation easing function.
    /// </summary>
    public static Easing DefaultAnimationEasing => Current.AnimationEasing;

    /// <summary>
    /// Gets whether animations are enabled by default.
    /// </summary>
    public static bool DefaultEnableAnimations => Current.EnableAnimations;

    #endregion

    #region Theme Changed Event

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
    /// Notifies all subscribed controls that the theme has changed.
    /// Call this after modifying theme properties without replacing the entire theme.
    /// </summary>
    public static void RaiseThemeChanged()
    {
        _lastNotifiedTheme = Application.Current?.RequestedTheme ?? AppTheme.Unspecified;
        _themeChangedEventManager.HandleEvent(null, EventArgs.Empty, nameof(ThemeChanged));
    }

    #endregion

    #region MAUI Theme Bridge

    /// <summary>
    /// Subscribes to MAUI's <see cref="Application.RequestedThemeChanged"/> event and forwards it
    /// as a library <see cref="ThemeChanged"/> event. Safe to call multiple times — only the first
    /// call has effect. Does nothing when <see cref="Application.Current"/> is null (e.g. in unit tests).
    /// </summary>
    public static void EnableMauiThemeBridge()
    {
        if (Interlocked.CompareExchange(ref _isMauiThemeBridged, 1, 0) != 0)
            return;

        if (Application.Current is not { } app)
        {
            Volatile.Write(ref _isMauiThemeBridged, 0); // reset so next call can retry
            return;
        }

        // Static delegate → singleton Application: both live for the process lifetime,
        // so no unsubscription is needed and no leak is possible.
        app.RequestedThemeChanged += OnMauiRequestedThemeChanged;
        _lastNotifiedTheme = app.RequestedTheme;
    }

    private static void OnMauiRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        if (e.RequestedTheme == _lastNotifiedTheme)
            return;

        RaiseThemeChanged();
    }

    #endregion

    #region Theme Management

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
    /// Applies the Modern theme.
    /// </summary>
    public static void ApplyModernTheme() => ApplyTheme(ControlsTheme.Modern);

    /// <summary>
    /// Applies the Compact theme.
    /// </summary>
    public static void ApplyCompactTheme() => ApplyTheme(ControlsTheme.Compact);

    /// <summary>
    /// Applies the Fluent Design theme.
    /// </summary>
    public static void ApplyFluentTheme() => ApplyTheme(ControlsTheme.Fluent);

    /// <summary>
    /// Applies the Material Design 3 theme.
    /// </summary>
    public static void ApplyMaterial3Theme() => ApplyTheme(ControlsTheme.Material3);

    /// <summary>
    /// Applies the High Contrast theme for accessibility.
    /// </summary>
    public static void ApplyHighContrastTheme() => ApplyTheme(ControlsTheme.HighContrast);

    #endregion

    #region Customization Helpers

    /// <summary>
    /// Creates a customized theme based on the current theme.
    /// </summary>
    /// <param name="configure">Action to configure the theme.</param>
    /// <returns>A new theme instance with the customizations applied.</returns>
    public static ControlsTheme CreateCustomTheme(Action<ControlsTheme> configure)
    {
        var theme = Current.Clone();
        configure(theme);
        return theme;
    }

    /// <summary>
    /// Modifies the current theme and raises the ThemeChanged event.
    /// </summary>
    /// <param name="configure">Action to modify the current theme.</param>
    public static void ModifyCurrentTheme(Action<ControlsTheme> configure)
    {
        configure(Current);
        RaiseThemeChanged();
    }

    #endregion
}
