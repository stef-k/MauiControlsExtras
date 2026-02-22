namespace MauiControlsExtras.Controls;

/// <summary>
/// Pure math helper for calculating popup position relative to an anchor.
/// Separated from <see cref="PopupOverlayHelper"/> for testability without MAUI control dependencies.
/// </summary>
internal static class PopupPositionCalculator
{
    internal const double Margin = 10.0;
    internal const double AnchorGap = 2.0;

    /// <summary>
    /// Calculates popup position given anchor bounds, popup size, page size, and placement.
    /// Returns (left, top).
    /// </summary>
    internal static (double Left, double Top) CalculatePosition(
        Rect anchorBounds,
        double popupWidth,
        double popupHeight,
        double pageWidth,
        double pageHeight,
        PopupPlacement placement)
    {
        var availableBelow = pageHeight - anchorBounds.Bottom - Margin;
        var availableAbove = anchorBounds.Top - Margin;

        double top;

        bool preferBelow = placement switch
        {
            PopupPlacement.Top => false,
            PopupPlacement.Bottom => true,
            // Auto: prefer below
            _ => true
        };

        if (preferBelow)
        {
            if (availableBelow >= popupHeight || availableBelow >= availableAbove)
            {
                top = anchorBounds.Bottom + AnchorGap;
            }
            else
            {
                top = anchorBounds.Top - popupHeight - AnchorGap;
            }
        }
        else
        {
            if (availableAbove >= popupHeight || availableAbove >= availableBelow)
            {
                top = anchorBounds.Top - popupHeight - AnchorGap;
            }
            else
            {
                top = anchorBounds.Bottom + AnchorGap;
            }
        }

        var left = Math.Max(0, Math.Min(anchorBounds.Left, pageWidth - popupWidth - Margin));
        top = Math.Max(Margin, Math.Min(top, pageHeight - popupHeight - Margin));

        return (left, top);
    }
}
