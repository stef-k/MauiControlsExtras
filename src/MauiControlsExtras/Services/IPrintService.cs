namespace MauiControlsExtras.Services;

/// <summary>
/// Interface for print services.
/// </summary>
public interface IPrintService
{
    /// <summary>
    /// Gets a value indicating whether printing is supported on the current platform.
    /// </summary>
    bool IsPrintingSupported { get; }

    /// <summary>
    /// Prints the specified text content.
    /// </summary>
    /// <param name="content">The text content to print.</param>
    /// <param name="options">The print options.</param>
    /// <returns>True if printing was successful; otherwise, false.</returns>
    Task<bool> PrintAsync(string content, PrintOptions? options = null);

    /// <summary>
    /// Prints the specified HTML content.
    /// </summary>
    /// <param name="html">The HTML content to print.</param>
    /// <param name="options">The print options.</param>
    /// <returns>True if printing was successful; otherwise, false.</returns>
    Task<bool> PrintHtmlAsync(string html, PrintOptions? options = null);
}

/// <summary>
/// Options for printing.
/// </summary>
public class PrintOptions
{
    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the page orientation.
    /// </summary>
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;

    /// <summary>
    /// Gets or sets the number of copies.
    /// </summary>
    public int Copies { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to show the print dialog.
    /// </summary>
    public bool ShowPrintDialog { get; set; } = true;
}

/// <summary>
/// Page orientation for printing.
/// </summary>
public enum PageOrientation
{
    /// <summary>
    /// Portrait orientation (taller than wide).
    /// </summary>
    Portrait,

    /// <summary>
    /// Landscape orientation (wider than tall).
    /// </summary>
    Landscape
}
