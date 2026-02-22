namespace MauiControlsExtras.Controls;

/// <summary>
/// Internal helper for anchor-based popup overlay positioning and management.
/// </summary>
internal static class PopupOverlayHelper
{
    private const double DefaultPopupHeight = 280.0;
    private const double DefaultPopupWidth = 250.0;

    /// <summary>
    /// Gets the bounds of a view relative to the page, accounting for ScrollView offsets.
    /// </summary>
    internal static Rect GetBoundsRelativeToPage(View view)
    {
        var x = 0.0;
        var y = 0.0;

        Element? current = view;
        while (current != null)
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

        var anchorBounds = GetBoundsRelativeToPage(anchor);
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

        // Dismiss on backdrop tap
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (_, _) => onDismissed?.Invoke();
        overlay.GestureRecognizers.Add(tapGesture);

        overlay.Children.Add(popup);

        // Dismiss on page size change (rotation, resize)
        void onSizeChanged(object? s, EventArgs e)
        {
            page.SizeChanged -= onSizeChanged;
            onDismissed?.Invoke();
        }
        page.SizeChanged += onSizeChanged;

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
            UnwrapRootLayoutIfNeeded(parentLayout);
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
    /// Ensures the page content is a Layout we can add overlays to.
    /// If it's not (e.g., a single ScrollView), wraps it in a Grid.
    /// </summary>
    private static Layout EnsureRootLayout(Page page)
    {
        if (page is ContentPage contentPage)
        {
            if (contentPage.Content is Layout layout)
                return layout;

            // Wrap existing content in a Grid
            var wrapper = new Grid();
            var originalContent = contentPage.Content;
            contentPage.Content = wrapper;
            if (originalContent != null)
                wrapper.Children.Add(originalContent);

            return wrapper;
        }

        throw new InvalidOperationException("Cannot find a Layout in the Page to host the popup overlay. The Page must be a ContentPage.");
    }

    /// <summary>
    /// If we previously wrapped the page content in a Grid,
    /// unwrap it when the overlay is removed and only the original content remains.
    /// </summary>
    private static void UnwrapRootLayoutIfNeeded(Layout layout)
    {
        // Only unwrap if this is a wrapper Grid with exactly one child
        // and the Grid's parent is a ContentPage (meaning we created it)
        if (layout is Grid grid
            && grid.Parent is ContentPage contentPage
            && grid.Children.Count == 1
            && grid.Children[0] is View singleChild
            && singleChild is not ComboBoxPopupContent)
        {
            contentPage.Content = singleChild;
        }
    }
}
