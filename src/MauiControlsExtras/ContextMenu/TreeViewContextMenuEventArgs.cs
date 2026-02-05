using MauiControlsExtras.Controls;

namespace MauiControlsExtras.ContextMenu;

/// <summary>
/// Provides data for TreeView-specific context menu events with node information.
/// </summary>
public class TreeViewContextMenuOpeningEventArgs : ContextMenuOpeningEventArgs
{
    /// <summary>
    /// Gets the TreeViewNode at the context menu location.
    /// </summary>
    public Controls.TreeViewNode? Node { get; }

    /// <summary>
    /// Gets the underlying data item at the context menu location.
    /// </summary>
    public object? DataItem { get; }

    /// <summary>
    /// Gets the depth level of the node in the tree (0 for root nodes).
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Gets whether the node has no children (is a leaf node).
    /// </summary>
    public bool IsLeafNode { get; }

    /// <summary>
    /// Gets the parent node of the context menu location, if any.
    /// </summary>
    public Controls.TreeViewNode? ParentNode { get; }

    /// <summary>
    /// Initializes a new instance of TreeViewContextMenuOpeningEventArgs.
    /// </summary>
    /// <param name="items">The initial collection of menu items.</param>
    /// <param name="position">The position where the context menu was requested.</param>
    /// <param name="node">The TreeViewNode at the context menu location.</param>
    /// <param name="anchorView">The view that the context menu is anchored to.</param>
    public TreeViewContextMenuOpeningEventArgs(
        ContextMenuItemCollection items,
        Point position,
        Controls.TreeViewNode? node,
        View? anchorView = null)
        : base(items, position, anchorView, node?.DataItem)
    {
        Node = node;
        DataItem = node?.DataItem;
        Level = node?.Level ?? 0;
        IsLeafNode = node != null && !node.HasChildren;
        ParentNode = node?.Parent;
    }

    /// <summary>
    /// Initializes a new instance with an empty items collection.
    /// </summary>
    /// <param name="position">The position where the context menu was requested.</param>
    /// <param name="node">The TreeViewNode at the context menu location.</param>
    /// <param name="anchorView">The view that the context menu is anchored to.</param>
    public TreeViewContextMenuOpeningEventArgs(
        Point position,
        Controls.TreeViewNode? node,
        View? anchorView = null)
        : this(new ContextMenuItemCollection(), position, node, anchorView)
    {
    }
}
