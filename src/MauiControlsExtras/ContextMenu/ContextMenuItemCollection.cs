using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MauiControlsExtras.ContextMenu;

/// <summary>
/// A collection of context menu items with helper methods for common operations.
/// </summary>
public class ContextMenuItemCollection : ObservableCollection<ContextMenuItem>
{
    /// <summary>
    /// Adds a separator to the collection.
    /// </summary>
    public void AddSeparator()
    {
        Add(ContextMenuItem.Separator());
    }

    /// <summary>
    /// Creates and adds a menu item with text and an action.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="action">The action to execute when selected.</param>
    /// <param name="iconGlyph">Optional icon glyph.</param>
    /// <param name="keyboardShortcut">Optional keyboard shortcut hint.</param>
    /// <returns>The created menu item.</returns>
    public ContextMenuItem Add(string text, Action action, string? iconGlyph = null, string? keyboardShortcut = null)
    {
        var item = ContextMenuItem.Create(text, action, iconGlyph, keyboardShortcut);
        Add(item);
        return item;
    }

    /// <summary>
    /// Creates and adds a menu item with text and a command.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="command">The command to execute when selected.</param>
    /// <param name="parameter">Optional command parameter.</param>
    /// <param name="iconGlyph">Optional icon glyph.</param>
    /// <param name="keyboardShortcut">Optional keyboard shortcut hint.</param>
    /// <returns>The created menu item.</returns>
    public ContextMenuItem Add(string text, ICommand command, object? parameter = null, string? iconGlyph = null, string? keyboardShortcut = null)
    {
        var item = ContextMenuItem.Create(text, command, parameter, iconGlyph, keyboardShortcut);
        Add(item);
        return item;
    }

    /// <summary>
    /// Creates and adds a submenu item with child items.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="subItems">The child menu items.</param>
    /// <param name="iconGlyph">Optional icon glyph.</param>
    /// <returns>The created menu item.</returns>
    public ContextMenuItem AddSubMenu(string text, IEnumerable<ContextMenuItem> subItems, string? iconGlyph = null)
    {
        var item = ContextMenuItem.CreateSubMenu(text, subItems, iconGlyph);
        Add(item);
        return item;
    }

    /// <summary>
    /// Creates and adds a submenu item with child items built via a builder action.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="buildSubMenu">Action to populate the submenu items.</param>
    /// <param name="iconGlyph">Optional icon glyph.</param>
    /// <returns>The created menu item.</returns>
    public ContextMenuItem AddSubMenu(string text, Action<ContextMenuItemCollection> buildSubMenu, string? iconGlyph = null)
    {
        var item = new ContextMenuItem
        {
            Text = text,
            IconGlyph = iconGlyph
        };

        buildSubMenu(item.SubItems);
        Add(item);
        return item;
    }

    /// <summary>
    /// Adds a range of items from an existing collection.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<ContextMenuItem> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Gets the first visible and enabled item with the specified text.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>The matching item, or null if not found.</returns>
    public ContextMenuItem? FindByText(string text)
    {
        return this.FirstOrDefault(i => i.Text == text && i.IsVisible);
    }

    /// <summary>
    /// Inserts a separator at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert the separator.</param>
    public void InsertSeparator(int index)
    {
        Insert(index, ContextMenuItem.Separator());
    }

    /// <summary>
    /// Gets all visible items (excludes items where IsVisible is false).
    /// </summary>
    public IEnumerable<ContextMenuItem> GetVisibleItems()
    {
        return this.Where(i => i.IsVisible);
    }
}
