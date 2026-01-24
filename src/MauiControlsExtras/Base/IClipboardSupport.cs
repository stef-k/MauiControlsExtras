using System.Windows.Input;

namespace MauiControlsExtras.Base;

/// <summary>
/// Interface for controls that support clipboard operations (copy, cut, paste).
/// Implement this interface on controls that handle text or data that users may want
/// to transfer via the clipboard.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides both method-based and command-based clipboard operations
/// to support both code-behind and MVVM patterns.
/// </para>
/// <para>
/// Controls implementing this interface should:
/// <list type="bullet">
///   <item>Return appropriate content from <see cref="GetClipboardContent"/> based on selection</item>
///   <item>Handle paste operations gracefully, validating content before applying</item>
///   <item>Update <see cref="CanCopy"/>, <see cref="CanCut"/>, and <see cref="CanPaste"/> appropriately</item>
///   <item>Raise property changed notifications when can-execute states change</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In XAML (MVVM):
/// &lt;local:MyControl CopyCommand="{Binding CopyCommand}" /&gt;
///
/// // In code-behind:
/// if (myControl.CanCopy)
///     myControl.Copy();
/// </code>
/// </example>
public interface IClipboardSupport
{
    #region State Properties

    /// <summary>
    /// Gets a value indicating whether the control currently has content that can be copied.
    /// </summary>
    /// <remarks>
    /// Typically returns true when there is a selection or content available for copying.
    /// This property should update dynamically as selection changes.
    /// </remarks>
    bool CanCopy { get; }

    /// <summary>
    /// Gets a value indicating whether the control currently has content that can be cut.
    /// </summary>
    /// <remarks>
    /// Typically returns true when there is a selection and the control is not read-only.
    /// This property should update dynamically as selection and read-only state changes.
    /// </remarks>
    bool CanCut { get; }

    /// <summary>
    /// Gets a value indicating whether the control can currently accept pasted content.
    /// </summary>
    /// <remarks>
    /// Typically returns true when the control is not read-only and the clipboard contains
    /// compatible content. This property should update when clipboard content changes
    /// or when the control's read-only state changes.
    /// </remarks>
    bool CanPaste { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Copies the current selection or content to the clipboard.
    /// </summary>
    /// <remarks>
    /// This method should:
    /// <list type="bullet">
    ///   <item>Do nothing if <see cref="CanCopy"/> is false</item>
    ///   <item>Copy selected content if there is a selection</item>
    ///   <item>Optionally copy all content if nothing is selected (control-specific behavior)</item>
    /// </list>
    /// </remarks>
    void Copy();

    /// <summary>
    /// Cuts the current selection to the clipboard.
    /// </summary>
    /// <remarks>
    /// This method should:
    /// <list type="bullet">
    ///   <item>Do nothing if <see cref="CanCut"/> is false</item>
    ///   <item>Copy selected content to clipboard</item>
    ///   <item>Remove the selected content from the control</item>
    ///   <item>This operation should be undoable if the control implements <see cref="IUndoRedo"/></item>
    /// </list>
    /// </remarks>
    void Cut();

    /// <summary>
    /// Pastes content from the clipboard into the control.
    /// </summary>
    /// <remarks>
    /// This method should:
    /// <list type="bullet">
    ///   <item>Do nothing if <see cref="CanPaste"/> is false</item>
    ///   <item>Replace selected content if there is a selection</item>
    ///   <item>Insert at cursor/caret position if no selection</item>
    ///   <item>Validate and transform clipboard content as needed</item>
    ///   <item>This operation should be undoable if the control implements <see cref="IUndoRedo"/></item>
    /// </list>
    /// </remarks>
    void Paste();

    /// <summary>
    /// Gets the current content that would be copied to the clipboard.
    /// </summary>
    /// <returns>
    /// The content to be copied, or null if nothing can be copied.
    /// The type depends on the control (string for text controls, object for data controls).
    /// </returns>
    /// <remarks>
    /// This method allows inspection of what would be copied without actually
    /// performing the copy operation. Useful for preview scenarios or custom
    /// clipboard handling.
    /// </remarks>
    object? GetClipboardContent();

    #endregion

    #region Commands (MVVM Support)

    /// <summary>
    /// Gets or sets the command to execute for copy operations.
    /// </summary>
    /// <remarks>
    /// When set, this command will be executed instead of the default <see cref="Copy"/> behavior.
    /// The command parameter will be the result of <see cref="GetClipboardContent"/>.
    /// CanExecute should reflect <see cref="CanCopy"/>.
    /// </remarks>
    ICommand? CopyCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to execute for cut operations.
    /// </summary>
    /// <remarks>
    /// When set, this command will be executed instead of the default <see cref="Cut"/> behavior.
    /// The command parameter will be the result of <see cref="GetClipboardContent"/>.
    /// CanExecute should reflect <see cref="CanCut"/>.
    /// </remarks>
    ICommand? CutCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to execute for paste operations.
    /// </summary>
    /// <remarks>
    /// When set, this command will be executed instead of the default <see cref="Paste"/> behavior.
    /// The command parameter will be the clipboard content (if retrievable).
    /// CanExecute should reflect <see cref="CanPaste"/>.
    /// </remarks>
    ICommand? PasteCommand { get; set; }

    #endregion
}
