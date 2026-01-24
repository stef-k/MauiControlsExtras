namespace MauiControlsExtras.Theming;

/// <summary>
/// Predefined theme configurations for MauiControlsExtras controls.
/// </summary>
public class ControlsTheme
{
    /// <summary>
    /// Gets or sets the accent color used for focus and selection states.
    /// </summary>
    public Color AccentColor { get; set; } = Color.FromArgb("#0078D4");

    /// <summary>
    /// Gets or sets the default corner radius for controls.
    /// </summary>
    public double CornerRadius { get; set; } = 8;

    /// <summary>
    /// Gets or sets the default border thickness for controls.
    /// </summary>
    public double BorderThickness { get; set; } = 1.5;

    /// <summary>
    /// Gets or sets the border color for light theme.
    /// </summary>
    public Color BorderColorLight { get; set; } = Color.FromArgb("#BDBDBD");

    /// <summary>
    /// Gets or sets the border color for dark theme.
    /// </summary>
    public Color BorderColorDark { get; set; } = Color.FromArgb("#9E9E9E");

    /// <summary>
    /// Gets or sets whether controls should have shadows by default.
    /// </summary>
    public bool HasShadow { get; set; } = false;

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
        BorderThickness = 1
    };

    /// <summary>
    /// Gets a compact theme configuration with smaller corner radius.
    /// </summary>
    public static ControlsTheme Compact => new()
    {
        CornerRadius = 4,
        BorderThickness = 1,
        HasShadow = false
    };
}
