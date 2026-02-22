using MauiControlsExtras.Controls;

namespace MauiControlsExtras.Tests.Controls;

public class PopupPositionCalculationTests
{
    private const double PageWidth = 400;
    private const double PageHeight = 800;
    private const double PopupWidth = 250;
    private const double PopupHeight = 280;

    [Fact]
    public void CalculatePosition_PlacesBelow_WhenAutoAndSpaceAvailable()
    {
        var anchor = new Rect(50, 100, 200, 40);

        var (left, top) = PopupPositionCalculator.CalculatePosition(
            anchor, PopupWidth, PopupHeight, PageWidth, PageHeight, PopupPlacement.Auto);

        // Should be 2px below anchor bottom
        Assert.Equal(142, top); // 100 + 40 + 2
        Assert.Equal(50, left);
    }

    [Fact]
    public void CalculatePosition_FlipsAbove_WhenAutoAndNoSpaceBelow()
    {
        // Anchor near the bottom: page is 800, anchor at y=600, height=40
        // Available below: 800 - 640 - 10 = 150 (< 280)
        // Available above: 600 - 10 = 590 (>= 280)
        var anchor = new Rect(50, 600, 200, 40);

        var (left, top) = PopupPositionCalculator.CalculatePosition(
            anchor, PopupWidth, PopupHeight, PageWidth, PageHeight, PopupPlacement.Auto);

        // Should flip above: 600 - 280 - 2 = 318
        Assert.Equal(318, top);
    }

    [Fact]
    public void CalculatePosition_PlacesBelow_WhenBottomAndSpaceAvailable()
    {
        var anchor = new Rect(50, 100, 200, 40);

        var (left, top) = PopupPositionCalculator.CalculatePosition(
            anchor, PopupWidth, PopupHeight, PageWidth, PageHeight, PopupPlacement.Bottom);

        Assert.Equal(142, top);
    }

    [Fact]
    public void CalculatePosition_PlacesAbove_WhenTopAndSpaceAvailable()
    {
        // Anchor at y=400, enough space above: 400 - 10 = 390 >= 280
        var anchor = new Rect(50, 400, 200, 40);

        var (left, top) = PopupPositionCalculator.CalculatePosition(
            anchor, PopupWidth, PopupHeight, PageWidth, PageHeight, PopupPlacement.Top);

        // Should be above: 400 - 280 - 2 = 118
        Assert.Equal(118, top);
    }

    [Fact]
    public void CalculatePosition_FlipsBelow_WhenTopButNoSpaceAbove()
    {
        // Anchor at y=50, not enough space above: 50 - 10 = 40 (< 280)
        // Available below: 800 - 90 - 10 = 700 (>= 280)
        var anchor = new Rect(50, 50, 200, 40);

        var (left, top) = PopupPositionCalculator.CalculatePosition(
            anchor, PopupWidth, PopupHeight, PageWidth, PageHeight, PopupPlacement.Top);

        // Should flip below: 50 + 40 + 2 = 92
        Assert.Equal(92, top);
    }

    [Fact]
    public void CalculatePosition_ClampsLeft_WhenAnchorNearRightEdge()
    {
        var anchor = new Rect(300, 100, 100, 40);

        var (left, _) = PopupPositionCalculator.CalculatePosition(
            anchor, PopupWidth, PopupHeight, PageWidth, PageHeight, PopupPlacement.Auto);

        // pageWidth - popupWidth - margin = 400 - 250 - 10 = 140
        Assert.Equal(140, left);
    }

    [Fact]
    public void CalculatePosition_ClampsLeft_NeverNegative()
    {
        var anchor = new Rect(-20, 100, 100, 40);

        var (left, _) = PopupPositionCalculator.CalculatePosition(
            anchor, PopupWidth, PopupHeight, PageWidth, PageHeight, PopupPlacement.Auto);

        Assert.True(left >= 0);
    }

    [Fact]
    public void CalculatePosition_ClampsTop_NeverLessThanMargin()
    {
        // Anchor at very top, placement = Top, should clamp to margin
        var anchor = new Rect(50, 5, 200, 40);

        var (_, top) = PopupPositionCalculator.CalculatePosition(
            anchor, PopupWidth, PopupHeight, PageWidth, PageHeight, PopupPlacement.Top);

        Assert.True(top >= 10); // Margin is 10
    }

    [Fact]
    public void CalculatePosition_ClampsTop_WhenPopupExceedsPageHeight()
    {
        // Anchor near bottom, popup would go off-screen
        var anchor = new Rect(50, 750, 200, 40);

        var (_, top) = PopupPositionCalculator.CalculatePosition(
            anchor, PopupWidth, PopupHeight, PageWidth, PageHeight, PopupPlacement.Bottom);

        // Should be clamped to: pageHeight - popupHeight - margin = 800 - 280 - 10 = 510
        Assert.True(top <= 510);
    }

    [Fact]
    public void CalculatePosition_PrefersBelowEvenIfTight_WhenAutoAndBelowEqualOrGreater()
    {
        // Anchor exactly in the middle — availableBelow should equal availableAbove
        // pageHeight = 800, anchorTop = 380, anchorHeight = 40
        // availableBelow = 800 - 420 - 10 = 370
        // availableAbove = 380 - 10 = 370
        // Both equal, so Auto prefers below
        var anchor = new Rect(50, 380, 200, 40);

        var (_, top) = PopupPositionCalculator.CalculatePosition(
            anchor, PopupWidth, PopupHeight, PageWidth, PageHeight, PopupPlacement.Auto);

        // Below: 380 + 40 + 2 = 422
        Assert.Equal(422, top);
    }
}
