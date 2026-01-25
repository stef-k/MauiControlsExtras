namespace MauiControlsExtras.Controls;

/// <summary>
/// Event arguments for breadcrumb item click.
/// </summary>
public class BreadcrumbItemClickedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the clicked item.
    /// </summary>
    public BreadcrumbItem Item { get; }

    /// <summary>
    /// Gets the item index.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BreadcrumbItemClickedEventArgs"/> class.
    /// </summary>
    public BreadcrumbItemClickedEventArgs(BreadcrumbItem item, int index)
    {
        Item = item;
        Index = index;
    }
}

/// <summary>
/// Event arguments for breadcrumb navigation (cancelable).
/// </summary>
public class BreadcrumbNavigatingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the target item.
    /// </summary>
    public BreadcrumbItem TargetItem { get; }

    /// <summary>
    /// Gets the target index.
    /// </summary>
    public int TargetIndex { get; }

    /// <summary>
    /// Gets or sets whether the navigation should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BreadcrumbNavigatingEventArgs"/> class.
    /// </summary>
    public BreadcrumbNavigatingEventArgs(BreadcrumbItem targetItem, int targetIndex)
    {
        TargetItem = targetItem;
        TargetIndex = targetIndex;
    }
}
