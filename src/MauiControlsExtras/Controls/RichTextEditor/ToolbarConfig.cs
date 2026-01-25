namespace MauiControlsExtras.Controls;

/// <summary>
/// Configuration for the RichTextEditor toolbar buttons.
/// </summary>
public class ToolbarConfig
{
    /// <summary>
    /// Gets or sets whether the Bold button is shown.
    /// </summary>
    public bool Bold { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Italic button is shown.
    /// </summary>
    public bool Italic { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Underline button is shown.
    /// </summary>
    public bool Underline { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Strikethrough button is shown.
    /// </summary>
    public bool Strikethrough { get; set; }

    /// <summary>
    /// Gets or sets whether the Bullet List button is shown.
    /// </summary>
    public bool BulletList { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Numbered List button is shown.
    /// </summary>
    public bool NumberedList { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Heading buttons (H1, H2, H3) are shown.
    /// </summary>
    public bool Heading { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Quote/Blockquote button is shown.
    /// </summary>
    public bool Quote { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Code Block button is shown.
    /// </summary>
    public bool CodeBlock { get; set; }

    /// <summary>
    /// Gets or sets whether the Link button is shown.
    /// </summary>
    public bool Link { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Image button is shown.
    /// </summary>
    public bool Image { get; set; }

    /// <summary>
    /// Gets or sets whether the Undo button is shown.
    /// </summary>
    public bool Undo { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Redo button is shown.
    /// </summary>
    public bool Redo { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Clear Formatting button is shown.
    /// </summary>
    public bool ClearFormatting { get; set; } = true;

    /// <summary>
    /// Gets a minimal toolbar with only bold, italic, and link buttons.
    /// </summary>
    public static ToolbarConfig Minimal => new()
    {
        Bold = true,
        Italic = true,
        Underline = false,
        BulletList = false,
        NumberedList = false,
        Heading = false,
        Quote = false,
        Link = true,
        Undo = false,
        Redo = false,
        ClearFormatting = false
    };

    /// <summary>
    /// Gets the standard/default toolbar configuration.
    /// </summary>
    public static ToolbarConfig Standard => new();

    /// <summary>
    /// Gets a full toolbar with all options enabled.
    /// </summary>
    public static ToolbarConfig Full => new()
    {
        Bold = true,
        Italic = true,
        Underline = true,
        Strikethrough = true,
        BulletList = true,
        NumberedList = true,
        Heading = true,
        Quote = true,
        CodeBlock = true,
        Link = true,
        Image = true,
        Undo = true,
        Redo = true,
        ClearFormatting = true
    };

    /// <summary>
    /// Gets a toolbar configuration with no buttons (for read-only display).
    /// </summary>
    public static ToolbarConfig None => new()
    {
        Bold = false,
        Italic = false,
        Underline = false,
        Strikethrough = false,
        BulletList = false,
        NumberedList = false,
        Heading = false,
        Quote = false,
        CodeBlock = false,
        Link = false,
        Image = false,
        Undo = false,
        Redo = false,
        ClearFormatting = false
    };
}
