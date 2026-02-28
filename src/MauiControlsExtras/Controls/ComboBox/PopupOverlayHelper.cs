namespace MauiControlsExtras.Controls;

/// <summary>
/// Internal helper for anchor-based popup overlay positioning and management.
/// </summary>
internal static class PopupOverlayHelper
{
    private const double DefaultPopupHeight = 280.0;
    private const double DefaultPopupWidth = 250.0;
    private const string OverlayWrapperStyleId = "__ComboBoxOverlayWrapper__";

    /// <summary>
    /// Gets the bounds of a view relative to the specified container,
    /// accounting for ScrollView offsets.  Stops accumulating at <paramref name="relativeTo"/>
    /// so the result is in that container's coordinate space.
    /// </summary>
    internal static Rect GetBoundsRelativeTo(View view, Element? relativeTo)
    {
        var x = 0.0;
        var y = 0.0;

        Element? current = view;
        while (current != null && current != relativeTo)
        {
            if (current is VisualElement ve)
            {
                x += ve.X;
                y += ve.Y;

                if (current.Parent is ScrollView scrollView)
                {
                    x -= scrollView.ScrollX;
                    y -= scrollView.ScrollY;
                }
            }
            current = current.Parent;
        }

        return new Rect(x, y, view.Width, view.Height);
    }

    /// <summary>
    /// Gets the bounds of a view relative to the page, accounting for ScrollView offsets.
    /// </summary>
    internal static Rect GetBoundsRelativeToPage(View view) => GetBoundsRelativeTo(view, relativeTo: null);

    /// <summary>
    /// Creates a full-page overlay with the popup anchored to the specified view.
    /// Returns the overlay Grid for later removal.
    /// </summary>
    internal static Grid ShowAnchored(
        View anchor,
        ComboBoxPopupContent popup,
        PopupPlacement placement,
        Action? onDismissed)
    {
        var page = FindPage(anchor);
        if (page == null)
            throw new InvalidOperationException("Cannot find a Page in the visual tree to host the popup overlay.");

        var rootLayout = EnsureRootLayout(page);

        var anchorBounds = GetBoundsRelativeTo(anchor, rootLayout);
        var popupWidth = Math.Max(anchorBounds.Width, DefaultPopupWidth);
        var popupHeight = DefaultPopupHeight;
        var pageWidth = page.Width > 0 ? page.Width : rootLayout.Width;
        var pageHeight = page.Height > 0 ? page.Height : rootLayout.Height;

        var (left, top) = PopupPositionCalculator.CalculatePosition(anchorBounds, popupWidth, popupHeight, pageWidth, pageHeight, placement);

        // Configure popup positioning
        popup.HorizontalOptions = LayoutOptions.Start;
        popup.VerticalOptions = LayoutOptions.Start;
        popup.WidthRequest = popupWidth;
        popup.Margin = new Thickness(left, top, 0, 0);

        // Create overlay
        var overlay = new Grid
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            ZIndex = 9999
        };

        // Guard against multiple dismiss calls
        var dismissed = false;
        void safeDismiss()
        {
            if (dismissed) return;
            dismissed = true;
            page.SizeChanged -= onPageSizeChanged;
            onDismissed?.Invoke();
        }

        // Dismiss on backdrop tap
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (_, _) => safeDismiss();
        overlay.GestureRecognizers.Add(tapGesture);

        overlay.Children.Add(popup);

        // Dismiss on page size change (rotation, resize)
        void onPageSizeChanged(object? s, EventArgs e) => safeDismiss();
        page.SizeChanged += onPageSizeChanged;

        rootLayout.Children.Add(overlay);

        return overlay;
    }

    /// <summary>
    /// Removes the overlay from the page layout.
    /// </summary>
    internal static void Dismiss(Grid overlay)
    {
        if (overlay.Parent is Layout parentLayout)
        {
            parentLayout.Children.Remove(overlay);
        }
    }

    /// <summary>
    /// Finds the Page ancestor of the given element.
    /// </summary>
    private static Page? FindPage(Element element)
    {
        Element? current = element;
        while (current != null)
        {
            if (current is Page page)
                return page;
            current = current.Parent;
        }
        return null;
    }

    /// <summary>
    /// Ensures the page content is wrapped in a dedicated overlay-hosting Grid.
    /// This Grid has no RowDefinitions/ColumnDefinitions, so children can freely
    /// overlap — which is required for the full-page popup overlay to work
    /// regardless of the original content's layout type (StackLayout, Grid with
    /// rows, etc.).
    /// </summary>
    private static Layout EnsureRootLayout(Page page)
    {
        if (page is not ContentPage contentPage)
            throw new InvalidOperationException("Cannot find a Layout in the Page to host the popup overlay. The Page must be a ContentPage.");

        // Already wrapped by a previous call — reuse.
        if (contentPage.Content is Grid grid && grid.StyleId == OverlayWrapperStyleId)
            return grid;

        // Wrap existing content in a plain Grid (no RowDefinitions/ColumnDefinitions)
        // so that the overlay can cover the full page area.
        var wrapper = new Grid { StyleId = OverlayWrapperStyleId };
        var originalContent = contentPage.Content;
        contentPage.Content = wrapper;
        if (originalContent != null)
            wrapper.Children.Add(originalContent);

        return wrapper;
    }

}
