namespace MauiControlsExtras.Controls;

/// <summary>
/// Specifies the text format type for the RichTextEditor.
/// </summary>
public enum FormatType
{
    /// <summary>
    /// Bold text formatting.
    /// </summary>
    Bold,

    /// <summary>
    /// Italic text formatting.
    /// </summary>
    Italic,

    /// <summary>
    /// Underline text formatting.
    /// </summary>
    Underline,

    /// <summary>
    /// Strikethrough text formatting.
    /// </summary>
    Strikethrough,

    /// <summary>
    /// Heading level 1.
    /// </summary>
    Heading1,

    /// <summary>
    /// Heading level 2.
    /// </summary>
    Heading2,

    /// <summary>
    /// Heading level 3.
    /// </summary>
    Heading3,

    /// <summary>
    /// Bullet list.
    /// </summary>
    BulletList,

    /// <summary>
    /// Numbered list.
    /// </summary>
    NumberedList,

    /// <summary>
    /// Block quote.
    /// </summary>
    Quote,

    /// <summary>
    /// Code block.
    /// </summary>
    CodeBlock,

    /// <summary>
    /// Inline code.
    /// </summary>
    Code
}

/// <summary>
/// Specifies the content format for the RichTextEditor.
/// </summary>
public enum ContentFormat
{
    /// <summary>
    /// HTML format.
    /// </summary>
    Html,

    /// <summary>
    /// Markdown format.
    /// </summary>
    Markdown
}

/// <summary>
/// Specifies the position of the toolbar in the RichTextEditor.
/// </summary>
public enum ToolbarPosition
{
    /// <summary>
    /// Toolbar at the top of the editor.
    /// </summary>
    Top,

    /// <summary>
    /// Toolbar at the bottom of the editor.
    /// </summary>
    Bottom,

    /// <summary>
    /// No toolbar displayed.
    /// </summary>
    None
}
