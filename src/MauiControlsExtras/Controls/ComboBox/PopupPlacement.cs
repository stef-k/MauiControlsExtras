namespace MauiControlsExtras.Controls;

/// <summary>
/// Specifies the preferred placement of a ComboBox popup relative to its anchor.
/// </summary>
public enum PopupPlacement
{
    /// <summary>
    /// Automatically choose placement: prefer below, flip above if insufficient space.
    /// </summary>
    Auto,

    /// <summary>
    /// Prefer below the anchor, flip above if insufficient space.
    /// </summary>
    Bottom,

    /// <summary>
    /// Prefer above the anchor, flip below if insufficient space.
    /// </summary>
    Top
}
