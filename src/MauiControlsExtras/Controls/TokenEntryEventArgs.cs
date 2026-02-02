namespace MauiControlsExtras.Controls;

/// <summary>
/// Specifies the type of clipboard operation.
/// </summary>
public enum TokenClipboardOperation
{
    /// <summary>
    /// Copy operation - token text is copied to clipboard.
    /// </summary>
    Copy,

    /// <summary>
    /// Cut operation - token text is copied to clipboard and removed from the control.
    /// </summary>
    Cut,

    /// <summary>
    /// Paste operation - tokens are added from clipboard text.
    /// </summary>
    Paste
}

/// <summary>
/// Provides data for clipboard-related events in TokenEntry.
/// </summary>
public class TokenClipboardEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of clipboard operation.
    /// </summary>
    public TokenClipboardOperation Operation { get; }

    /// <summary>
    /// Gets the tokens involved in the operation.
    /// For Copy/Cut: the token being copied.
    /// For Paste: the tokens that will be/were added.
    /// </summary>
    public IReadOnlyList<string> Tokens { get; }

    /// <summary>
    /// Gets the raw clipboard text content.
    /// For Copy/Cut: the text being copied.
    /// For Paste: the text from clipboard before splitting.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets or sets whether to cancel the operation.
    /// Only applicable for events raised before the operation (Copying, Cutting, Pasting).
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets the list of tokens that were skipped during paste operation.
    /// Only populated for Pasted event.
    /// </summary>
    public IReadOnlyList<string> SkippedTokens { get; internal set; } = Array.Empty<string>();

    /// <summary>
    /// Gets the reasons why tokens were skipped during paste operation.
    /// Keys are skipped tokens, values are skip reasons.
    /// Only populated for Pasted event.
    /// </summary>
    public IReadOnlyDictionary<string, string> SkipReasons { get; internal set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the number of tokens successfully added during paste operation.
    /// Only populated for Pasted event.
    /// </summary>
    public int SuccessCount { get; internal set; }

    /// <summary>
    /// Initializes a new instance of TokenClipboardEventArgs.
    /// </summary>
    /// <param name="operation">The type of clipboard operation.</param>
    /// <param name="tokens">The tokens involved in the operation.</param>
    /// <param name="content">The raw clipboard text content.</param>
    public TokenClipboardEventArgs(TokenClipboardOperation operation, IReadOnlyList<string> tokens, string content)
    {
        Operation = operation;
        Tokens = tokens;
        Content = content;
    }

    /// <summary>
    /// Initializes a new instance of TokenClipboardEventArgs for a single token.
    /// </summary>
    /// <param name="operation">The type of clipboard operation.</param>
    /// <param name="token">The token involved in the operation.</param>
    public TokenClipboardEventArgs(TokenClipboardOperation operation, string token)
        : this(operation, new[] { token }, token)
    {
    }
}
