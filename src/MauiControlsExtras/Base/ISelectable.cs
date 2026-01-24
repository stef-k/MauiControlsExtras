using System.Windows.Input;

namespace MauiControlsExtras.Base;

/// <summary>
/// Interface for controls that support selection operations.
/// Implement this interface on controls that allow users to select content,
/// items, or ranges of data.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides both method-based and command-based selection operations
/// to support both code-behind and MVVM patterns.
/// </para>
/// <para>
/// Controls implementing this interface should:
/// <list type="bullet">
///   <item>Track selection state and expose it via <see cref="HasSelection"/></item>
///   <item>Provide selection information through <see cref="GetSelection"/></item>
///   <item>Support programmatic selection via <see cref="SelectAll"/> and <see cref="ClearSelection"/></item>
///   <item>Raise <see cref="SelectionChanged"/> when selection changes</item>
///   <item>Raise property changed notifications for <see cref="HasSelection"/> changes</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In XAML (MVVM):
/// &lt;local:MyControl SelectAllCommand="{Binding SelectAllCommand}"
///                  SelectionChanged="OnSelectionChanged" /&gt;
///
/// // In code-behind:
/// myControl.SelectAll();
/// var selection = myControl.GetSelection();
/// </code>
/// </example>
public interface ISelectable
{
    #region State Properties

    /// <summary>
    /// Gets a value indicating whether the control currently has any selection.
    /// </summary>
    /// <remarks>
    /// Returns true if any content, item, or range is selected.
    /// This property should update dynamically as selection changes.
    /// </remarks>
    bool HasSelection { get; }

    /// <summary>
    /// Gets a value indicating whether all selectable content is currently selected.
    /// </summary>
    /// <remarks>
    /// Returns true only when everything that can be selected is selected.
    /// Useful for implementing "Select All" toggle behavior or checkbox state.
    /// </remarks>
    bool IsAllSelected { get; }

    /// <summary>
    /// Gets a value indicating whether the control supports multiple selection.
    /// </summary>
    /// <remarks>
    /// When true, users can select multiple items/ranges simultaneously.
    /// When false, selecting new content clears previous selection.
    /// </remarks>
    bool SupportsMultipleSelection { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Selects all selectable content in the control.
    /// </summary>
    /// <remarks>
    /// This method should:
    /// <list type="bullet">
    ///   <item>Select all text in text-based controls</item>
    ///   <item>Select all items in list-based controls</item>
    ///   <item>Select all cells/rows in grid-based controls</item>
    ///   <item>Raise <see cref="SelectionChanged"/> after selection completes</item>
    ///   <item>Update <see cref="HasSelection"/> and <see cref="IsAllSelected"/></item>
    /// </list>
    /// </remarks>
    void SelectAll();

    /// <summary>
    /// Clears the current selection without removing any content.
    /// </summary>
    /// <remarks>
    /// This method should:
    /// <list type="bullet">
    ///   <item>Deselect all selected content/items</item>
    ///   <item>Not modify or delete any actual content</item>
    ///   <item>Raise <see cref="SelectionChanged"/> after clearing</item>
    ///   <item>Update <see cref="HasSelection"/> to false</item>
    /// </list>
    /// </remarks>
    void ClearSelection();

    /// <summary>
    /// Gets the current selection.
    /// </summary>
    /// <returns>
    /// The current selection. The type depends on the control:
    /// <list type="bullet">
    ///   <item>Text controls: Selected string or (start, length) tuple</item>
    ///   <item>List controls: Selected item(s) or indices</item>
    ///   <item>Grid controls: Selected cells, rows, or ranges</item>
    /// </list>
    /// Returns null if nothing is selected.
    /// </returns>
    object? GetSelection();

    /// <summary>
    /// Sets the selection programmatically.
    /// </summary>
    /// <param name="selection">
    /// The selection to apply. The expected type depends on the control.
    /// Pass null to clear selection.
    /// </param>
    /// <remarks>
    /// This method should:
    /// <list type="bullet">
    ///   <item>Validate the selection parameter type and range</item>
    ///   <item>Apply the selection if valid</item>
    ///   <item>Raise <see cref="SelectionChanged"/> after applying</item>
    ///   <item>Throw <see cref="ArgumentException"/> if selection type is incompatible</item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the selection parameter type is not compatible with this control.
    /// </exception>
    void SetSelection(object? selection);

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    /// <remarks>
    /// This event should be raised:
    /// <list type="bullet">
    ///   <item>When user interactively changes selection</item>
    ///   <item>When <see cref="SelectAll"/> is called</item>
    ///   <item>When <see cref="ClearSelection"/> is called</item>
    ///   <item>When <see cref="SetSelection"/> is called</item>
    /// </list>
    /// The event args contain both old and new selection information.
    /// </remarks>
    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    #endregion

    #region Commands (MVVM Support)

    /// <summary>
    /// Gets or sets the command to execute for select all operations.
    /// </summary>
    /// <remarks>
    /// When set, this command will be executed instead of the default <see cref="SelectAll"/> behavior.
    /// CanExecute should typically return true when there is selectable content.
    /// </remarks>
    ICommand? SelectAllCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to execute for clear selection operations.
    /// </summary>
    /// <remarks>
    /// When set, this command will be executed instead of the default <see cref="ClearSelection"/> behavior.
    /// CanExecute should reflect <see cref="HasSelection"/>.
    /// </remarks>
    ICommand? ClearSelectionCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to execute when selection changes.
    /// </summary>
    /// <remarks>
    /// This command is executed after any selection change.
    /// The command parameter is the new selection (result of <see cref="GetSelection"/>).
    /// </remarks>
    ICommand? SelectionChangedCommand { get; set; }

    #endregion
}

/// <summary>
/// Provides data for the <see cref="ISelectable.SelectionChanged"/> event.
/// </summary>
public class SelectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldSelection">The previous selection.</param>
    /// <param name="newSelection">The new selection.</param>
    public SelectionChangedEventArgs(object? oldSelection, object? newSelection)
    {
        OldSelection = oldSelection;
        NewSelection = newSelection;
    }

    /// <summary>
    /// Gets the previous selection before the change.
    /// </summary>
    public object? OldSelection { get; }

    /// <summary>
    /// Gets the new selection after the change.
    /// </summary>
    public object? NewSelection { get; }

    /// <summary>
    /// Gets a value indicating whether the selection was cleared.
    /// </summary>
    public bool SelectionCleared => NewSelection is null && OldSelection is not null;

    /// <summary>
    /// Gets a value indicating whether this is the first selection (from no selection).
    /// </summary>
    public bool IsFirstSelection => OldSelection is null && NewSelection is not null;
}
