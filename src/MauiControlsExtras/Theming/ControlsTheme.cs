namespace MauiControlsExtras.Theming;

/// <summary>
/// Theme configuration for MauiControlsExtras controls.
/// Defines colors, typography, and shape properties for consistent styling.
/// </summary>
public class ControlsTheme
{
    #region Semantic Colors

    /// <summary>
    /// Gets or sets the accent color used for focus and selection states.
    /// </summary>
    public Color AccentColor { get; set; } = Color.FromArgb("#0078D4");

    /// <summary>
    /// Gets or sets the color used for error states.
    /// </summary>
    public Color ErrorColor { get; set; } = Color.FromArgb("#D32F2F");

    /// <summary>
    /// Gets or sets the color used for success states.
    /// </summary>
    public Color SuccessColor { get; set; } = Color.FromArgb("#388E3C");

    /// <summary>
    /// Gets or sets the color used for warning states.
    /// </summary>
    public Color WarningColor { get; set; } = Color.FromArgb("#F57C00");

    /// <summary>
    /// Gets or sets the color used for disabled elements.
    /// </summary>
    public Color DisabledColor { get; set; } = Colors.Gray;

    #endregion

    #region Surface Colors (Light Theme)

    /// <summary>
    /// Gets or sets the background color for light theme.
    /// </summary>
    public Color BackgroundColorLight { get; set; } = Colors.White;

    /// <summary>
    /// Gets or sets the surface/card background color for light theme.
    /// </summary>
    public Color SurfaceColorLight { get; set; } = Colors.White;

    /// <summary>
    /// Gets or sets the foreground (text) color for light theme.
    /// </summary>
    public Color ForegroundColorLight { get; set; } = Color.FromArgb("#212121");

    /// <summary>
    /// Gets or sets the border color for light theme.
    /// </summary>
    public Color BorderColorLight { get; set; } = Color.FromArgb("#E0E0E0");

    /// <summary>
    /// Gets or sets the disabled border color for light theme.
    /// </summary>
    public Color DisabledBorderColorLight { get; set; } = Color.FromArgb("#BDBDBD");

    /// <summary>
    /// Gets or sets the shadow color for light theme.
    /// </summary>
    public Color ShadowColorLight { get; set; } = Color.FromArgb("#000000");

    #endregion

    #region Surface Colors (Dark Theme)

    /// <summary>
    /// Gets or sets the background color for dark theme.
    /// </summary>
    public Color BackgroundColorDark { get; set; } = Color.FromArgb("#1E1E1E");

    /// <summary>
    /// Gets or sets the surface/card background color for dark theme.
    /// </summary>
    public Color SurfaceColorDark { get; set; } = Color.FromArgb("#2D2D2D");

    /// <summary>
    /// Gets or sets the foreground (text) color for dark theme.
    /// </summary>
    public Color ForegroundColorDark { get; set; } = Color.FromArgb("#FFFFFF");

    /// <summary>
    /// Gets or sets the border color for dark theme.
    /// </summary>
    public Color BorderColorDark { get; set; } = Color.FromArgb("#3C3C3C");

    /// <summary>
    /// Gets or sets the disabled border color for dark theme.
    /// </summary>
    public Color DisabledBorderColorDark { get; set; } = Color.FromArgb("#616161");

    /// <summary>
    /// Gets or sets the shadow color for dark theme.
    /// </summary>
    public Color ShadowColorDark { get; set; } = Color.FromArgb("#000000");

    #endregion

    #region Selection Colors

    /// <summary>
    /// Gets or sets the selected item background color.
    /// </summary>
    public Color SelectedBackgroundColor { get; set; } = Color.FromArgb("#0078D4").WithAlpha(0.2f);

    /// <summary>
    /// Gets or sets the selected item text color.
    /// </summary>
    public Color SelectedForegroundColor { get; set; } = Color.FromArgb("#0078D4");

    /// <summary>
    /// Gets or sets the hover/pointer-over color.
    /// </summary>
    public Color HoverColor { get; set; } = Color.FromArgb("#0078D4").WithAlpha(0.1f);

    #endregion

    #region Typography

    /// <summary>
    /// Gets or sets the default font family. Null uses system default.
    /// </summary>
    public string? FontFamily { get; set; } = null;

    /// <summary>
    /// Gets or sets the default font size.
    /// </summary>
    public double FontSize { get; set; } = 14;

    /// <summary>
    /// Gets or sets the default placeholder text color for light theme.
    /// </summary>
    public Color PlaceholderColorLight { get; set; } = Color.FromArgb("#9E9E9E");

    /// <summary>
    /// Gets or sets the default placeholder text color for dark theme.
    /// </summary>
    public Color PlaceholderColorDark { get; set; } = Color.FromArgb("#757575");

    #endregion

    #region Shape

    /// <summary>
    /// Gets or sets the default corner radius for controls.
    /// </summary>
    public double CornerRadius { get; set; } = 4;

    /// <summary>
    /// Gets or sets the default border thickness for controls.
    /// </summary>
    public double BorderThickness { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether controls should have shadows by default.
    /// </summary>
    public bool HasShadow { get; set; } = false;

    #endregion

    #region Animation

    /// <summary>
    /// Gets or sets the default animation duration in milliseconds.
    /// </summary>
    public int AnimationDuration { get; set; } = 250;

    /// <summary>
    /// Gets or sets the default animation easing function.
    /// </summary>
    public Easing AnimationEasing { get; set; } = Easing.CubicInOut;

    /// <summary>
    /// Gets or sets whether animations are enabled by default.
    /// </summary>
    public bool EnableAnimations { get; set; } = true;

    #endregion

    #region Convenience Properties

    /// <summary>
    /// Gets the disabled border color based on the current app theme.
    /// </summary>
    public Color DisabledBorderColor =>
        Application.Current?.RequestedTheme == AppTheme.Dark
            ? DisabledBorderColorDark
            : DisabledBorderColorLight;

    #endregion

    #region Predefined Themes

    /// <summary>
    /// Gets the default theme configuration.
    /// </summary>
    public static ControlsTheme Default => new();

    /// <summary>
    /// Gets a modern theme configuration with rounded corners and subtle shadows.
    /// </summary>
    public static ControlsTheme Modern => new()
    {
        CornerRadius = 12,
        HasShadow = true,
        BorderThickness = 1,
        AccentColor = Color.FromArgb("#6200EE"),
        SelectedBackgroundColor = Color.FromArgb("#6200EE").WithAlpha(0.15f),
        SelectedForegroundColor = Color.FromArgb("#6200EE"),
        HoverColor = Color.FromArgb("#6200EE").WithAlpha(0.08f)
    };

    /// <summary>
    /// Gets a compact theme configuration with smaller corner radius and tighter spacing.
    /// </summary>
    public static ControlsTheme Compact => new()
    {
        CornerRadius = 2,
        BorderThickness = 1,
        HasShadow = false,
        FontSize = 13
    };

    /// <summary>
    /// Gets a Fluent Design inspired theme.
    /// </summary>
    public static ControlsTheme Fluent => new()
    {
        CornerRadius = 4,
        BorderThickness = 1,
        HasShadow = false,
        AccentColor = Color.FromArgb("#0078D4"),
        ErrorColor = Color.FromArgb("#C42B1C"),
        SuccessColor = Color.FromArgb("#0F7B0F"),
        WarningColor = Color.FromArgb("#9D5D00"),
        BorderColorLight = Color.FromArgb("#8A8886"),
        BorderColorDark = Color.FromArgb("#605E5C")
    };

    /// <summary>
    /// Gets a Material Design 3 inspired theme.
    /// </summary>
    public static ControlsTheme Material3 => new()
    {
        CornerRadius = 12,
        BorderThickness = 1,
        HasShadow = false,
        AccentColor = Color.FromArgb("#6750A4"),
        ErrorColor = Color.FromArgb("#B3261E"),
        SuccessColor = Color.FromArgb("#386A20"),
        WarningColor = Color.FromArgb("#7D5700"),
        SelectedBackgroundColor = Color.FromArgb("#6750A4").WithAlpha(0.12f),
        SelectedForegroundColor = Color.FromArgb("#6750A4"),
        HoverColor = Color.FromArgb("#6750A4").WithAlpha(0.08f),
        SurfaceColorLight = Color.FromArgb("#FFFBFE"),
        SurfaceColorDark = Color.FromArgb("#1C1B1F")
    };

    /// <summary>
    /// Gets a high contrast theme for accessibility.
    /// </summary>
    public static ControlsTheme HighContrast => new()
    {
        CornerRadius = 0,
        BorderThickness = 2,
        HasShadow = false,
        AccentColor = Color.FromArgb("#1AEBFF"),
        ErrorColor = Color.FromArgb("#FF6060"),
        SuccessColor = Color.FromArgb("#3FF23F"),
        WarningColor = Color.FromArgb("#FFFF00"),
        BackgroundColorLight = Colors.White,
        BackgroundColorDark = Colors.Black,
        ForegroundColorLight = Colors.Black,
        ForegroundColorDark = Colors.White,
        BorderColorLight = Colors.Black,
        BorderColorDark = Colors.White
    };

    #endregion

    #region Clone

    /// <summary>
    /// Creates a copy of this theme that can be modified independently.
    /// </summary>
    public ControlsTheme Clone()
    {
        return (ControlsTheme)MemberwiseClone();
    }

    #endregion
}
