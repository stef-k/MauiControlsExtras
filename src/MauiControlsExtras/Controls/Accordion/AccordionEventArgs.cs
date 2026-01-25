namespace MauiControlsExtras.Controls;

/// <summary>
/// Event arguments for accordion item expansion changes.
/// </summary>
public class AccordionItemExpandedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the accordion item.
    /// </summary>
    public AccordionItem Item { get; }

    /// <summary>
    /// Gets the item index.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets whether the item is now expanded.
    /// </summary>
    public bool IsExpanded { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccordionItemExpandedEventArgs"/> class.
    /// </summary>
    public AccordionItemExpandedEventArgs(AccordionItem item, int index, bool isExpanded)
    {
        Item = item;
        Index = index;
        IsExpanded = isExpanded;
    }
}

/// <summary>
/// Event arguments for accordion item expanding/collapsing (cancelable).
/// </summary>
public class AccordionItemExpandingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the accordion item.
    /// </summary>
    public AccordionItem Item { get; }

    /// <summary>
    /// Gets the item index.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets whether the item is expanding (true) or collapsing (false).
    /// </summary>
    public bool IsExpanding { get; }

    /// <summary>
    /// Gets or sets whether the expansion/collapse should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccordionItemExpandingEventArgs"/> class.
    /// </summary>
    public AccordionItemExpandingEventArgs(AccordionItem item, int index, bool isExpanding)
    {
        Item = item;
        Index = index;
        IsExpanding = isExpanding;
    }
}
