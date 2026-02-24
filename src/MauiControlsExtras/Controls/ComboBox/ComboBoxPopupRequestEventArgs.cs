using System.Collections;

namespace MauiControlsExtras.Controls;

/// <summary>
/// Event arguments for ComboBox popup requests in popup mode.
/// </summary>
public class ComboBoxPopupRequestEventArgs : EventArgs
{
    /// <summary>
    /// Gets the bounds of the anchor element relative to its container.
    /// </summary>
    public Rect AnchorBounds { get; }

    /// <summary>
    /// Gets the items source for the popup.
    /// </summary>
    public IEnumerable? ItemsSource { get; }

    /// <summary>
    /// Gets the display member path for item display.
    /// </summary>
    public string? DisplayMemberPath { get; }

    /// <summary>
    /// Gets the currently selected item.
    /// </summary>
    public object? SelectedItem { get; }

    /// <summary>
    /// Gets the placeholder text for the search entry.
    /// </summary>
    public string? Placeholder { get; }

    /// <summary>
    /// Gets whether the search input should be visible in the popup.
    /// </summary>
    public bool IsSearchVisible { get; }

    /// <summary>
    /// Gets the source ComboBox that raised the event.
    /// </summary>
    public ComboBox Source { get; }

    /// <summary>
    /// Gets the preferred popup placement relative to the anchor.
    /// </summary>
    public PopupPlacement PreferredPlacement { get; }

    /// <summary>
    /// Gets the AOT-safe display member function, if set on the source ComboBox.
    /// </summary>
    public Func<object, string?>? DisplayMemberFunc { get; }

    /// <summary>
    /// Initializes a new instance of ComboBoxPopupRequestEventArgs.
    /// </summary>
    public ComboBoxPopupRequestEventArgs(
        ComboBox source,
        Rect anchorBounds,
        IEnumerable? itemsSource,
        string? displayMemberPath,
        object? selectedItem,
        string? placeholder,
        bool isSearchVisible = true,
        PopupPlacement preferredPlacement = PopupPlacement.Auto,
        Func<object, string?>? displayMemberFunc = null)
    {
        Source = source;
        AnchorBounds = anchorBounds;
        ItemsSource = itemsSource;
        DisplayMemberPath = displayMemberPath;
        SelectedItem = selectedItem;
        Placeholder = placeholder;
        IsSearchVisible = isSearchVisible;
        PreferredPlacement = preferredPlacement;
        DisplayMemberFunc = displayMemberFunc;
    }
}
