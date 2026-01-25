namespace MauiControlsExtras.Controls;

/// <summary>
/// Event arguments for RichTextEditor content changes.
/// </summary>
public class RichTextContentChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the old content before the change.
    /// </summary>
    public string? OldContent { get; }

    /// <summary>
    /// Gets the new content after the change.
    /// </summary>
    public string? NewContent { get; }

    /// <summary>
    /// Gets the content format (HTML or Markdown).
    /// </summary>
    public ContentFormat Format { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextContentChangedEventArgs"/> class.
    /// </summary>
    public RichTextContentChangedEventArgs(string? oldContent, string? newContent, ContentFormat format)
    {
        OldContent = oldContent;
        NewContent = newContent;
        Format = format;
    }
}

/// <summary>
/// Event arguments for RichTextEditor selection changes.
/// </summary>
public class RichTextSelectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the selection start index.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// Gets the selection length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets the selected text.
    /// </summary>
    public string? SelectedText { get; }

    /// <summary>
    /// Gets whether there is a selection (length > 0).
    /// </summary>
    public bool HasSelection => Length > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextSelectionChangedEventArgs"/> class.
    /// </summary>
    public RichTextSelectionChangedEventArgs(int start, int length, string? selectedText)
    {
        Start = start;
        Length = length;
        SelectedText = selectedText;
    }
}

/// <summary>
/// Event arguments for when a link is tapped in the RichTextEditor.
/// </summary>
public class RichTextLinkTappedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the URL of the tapped link.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Gets the link text.
    /// </summary>
    public string? Text { get; }

    /// <summary>
    /// Gets or sets whether the default navigation should be cancelled.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextLinkTappedEventArgs"/> class.
    /// </summary>
    public RichTextLinkTappedEventArgs(string url, string? text)
    {
        Url = url;
        Text = text;
    }
}

/// <summary>
/// Event arguments for when an image is requested to be inserted.
/// </summary>
public class RichTextImageRequestedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the image URL to insert. Set this in the event handler.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the image alt text.
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Gets or sets whether the request was handled.
    /// </summary>
    public bool Handled { get; set; }
}

/// <summary>
/// Event arguments for RichTextEditor focus changes.
/// </summary>
public class RichTextFocusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets whether the editor has focus.
    /// </summary>
    public bool IsFocused { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextFocusChangedEventArgs"/> class.
    /// </summary>
    public RichTextFocusChangedEventArgs(bool isFocused)
    {
        IsFocused = isFocused;
    }
}
