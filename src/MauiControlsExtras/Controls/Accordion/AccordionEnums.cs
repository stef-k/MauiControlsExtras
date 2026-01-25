namespace MauiControlsExtras.Controls;

/// <summary>
/// Specifies the expansion mode for the Accordion.
/// </summary>
public enum AccordionExpandMode
{
    /// <summary>
    /// Only one item can be expanded at a time.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple items can be expanded simultaneously.
    /// </summary>
    Multiple,

    /// <summary>
    /// At least one item must always be expanded.
    /// </summary>
    AtLeastOne
}

/// <summary>
/// Specifies the position of the expand icon.
/// </summary>
public enum ExpandIconPosition
{
    /// <summary>
    /// Icon is on the left side of the header.
    /// </summary>
    Left,

    /// <summary>
    /// Icon is on the right side of the header.
    /// </summary>
    Right
}
