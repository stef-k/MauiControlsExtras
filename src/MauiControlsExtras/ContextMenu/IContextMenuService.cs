namespace MauiControlsExtras.ContextMenu;

/// <summary>
/// Service interface for displaying platform-native context menus.
/// </summary>
public interface IContextMenuService
{
    /// <summary>
    /// Shows a context menu anchored to the specified view.
    /// </summary>
    /// <param name="anchor">The view to anchor the menu to.</param>
    /// <param name="items">The menu items to display.</param>
    /// <param name="position">Optional position relative to the anchor. If null, the menu is positioned at the anchor's location.</param>
    /// <returns>A task that completes when the menu is dismissed.</returns>
    Task ShowAsync(View anchor, IList<ContextMenuItem> items, Point? position = null);

    /// <summary>
    /// Shows a context menu at the specified screen position.
    /// </summary>
    /// <param name="items">The menu items to display.</param>
    /// <param name="screenPosition">The screen position to show the menu at.</param>
    /// <returns>A task that completes when the menu is dismissed.</returns>
    Task ShowAtAsync(IList<ContextMenuItem> items, Point screenPosition);

    /// <summary>
    /// Dismisses any currently visible context menu.
    /// </summary>
    void Dismiss();
}
