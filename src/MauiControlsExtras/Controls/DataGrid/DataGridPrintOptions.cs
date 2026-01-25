using MauiControlsExtras.Services;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Options for printing a data grid.
/// </summary>
public class DataGridPrintOptions
{
    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets whether to print only visible columns.
    /// </summary>
    public bool VisibleColumnsOnly { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to print only selected rows.
    /// </summary>
    public bool SelectedRowsOnly { get; set; }

    /// <summary>
    /// Gets or sets the page orientation.
    /// </summary>
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;

    /// <summary>
    /// Gets or sets whether to include headers in the printout.
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include footer/aggregates in the printout.
    /// </summary>
    public bool IncludeFooter { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include grid lines in the printout.
    /// </summary>
    public bool IncludeGridLines { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include alternating row colors.
    /// </summary>
    public bool IncludeAlternatingRows { get; set; } = true;

    /// <summary>
    /// Gets or sets the date format for the printout.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Gets or sets custom CSS styles to apply to the printout.
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// Gets or sets whether to show the print dialog.
    /// </summary>
    public bool ShowPrintDialog { get; set; } = true;

    /// <summary>
    /// Converts these options to generic print options.
    /// </summary>
    internal PrintOptions ToPrintOptions()
    {
        return new PrintOptions
        {
            Title = Title,
            Orientation = Orientation,
            ShowPrintDialog = ShowPrintDialog
        };
    }
}
