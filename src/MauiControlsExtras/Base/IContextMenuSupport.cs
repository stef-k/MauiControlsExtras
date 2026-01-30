using MauiControlsExtras.ContextMenu;

namespace MauiControlsExtras.Base;

/// <summary>
/// Interface for controls that support context menus.
/// </summary>
public interface IContextMenuSupport
{
    /// <summary>
    /// Gets the collection of context menu items to display.
    /// </summary>
    ContextMenuItemCollection ContextMenuItems { get; }

    /// <summary>
    /// Gets or sets whether to show the default context menu items (e.g., Copy, Paste).
    /// </summary>
    bool ShowDefaultContextMenu { get; set; }

    /// <summary>
    /// Occurs before the context menu is opened. Allows customization of menu items and cancellation.
    /// </summary>
    event EventHandler<ContextMenuOpeningEventArgs>? ContextMenuOpening;

    /// <summary>
    /// Programmatically shows the context menu at the specified position.
    /// </summary>
    /// <param name="position">The position to show the menu at. If null, uses a default position (e.g., center of control).</param>
    void ShowContextMenu(Point? position = null);
}
