using System.Windows.Input;

namespace MauiControlsExtras.Base;

/// <summary>
/// Interface for controls that support undo/redo operations.
/// Implement this interface on controls that track changes and allow users
/// to reverse or reapply those changes.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides both method-based and command-based undo/redo operations
/// to support both code-behind and MVVM patterns.
/// </para>
/// <para>
/// Controls implementing this interface should:
/// <list type="bullet">
///   <item>Maintain an undo stack of reversible operations</item>
///   <item>Maintain a redo stack for re-applying undone operations</item>
///   <item>Clear the redo stack when new changes are made after undo</item>
///   <item>Update <see cref="CanUndo"/> and <see cref="CanRedo"/> appropriately</item>
///   <item>Optionally limit stack size via <see cref="UndoLimit"/></item>
/// </list>
/// </para>
/// <para>
/// Operations that should typically be undoable:
/// <list type="bullet">
///   <item>Text input and deletion</item>
///   <item>Paste operations</item>
///   <item>Cut operations</item>
///   <item>Value changes in input controls</item>
///   <item>Item additions/removals in collection controls</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In XAML (MVVM):
/// &lt;local:MyControl UndoCommand="{Binding UndoCommand}"
///                  RedoCommand="{Binding RedoCommand}"
///                  UndoLimit="50" /&gt;
///
/// // In code-behind:
/// if (myControl.CanUndo)
///     myControl.Undo();
///
/// // Clear history when loading new data:
/// myControl.ClearUndoHistory();
/// </code>
/// </example>
public interface IUndoRedo
{
    #region State Properties

    /// <summary>
    /// Gets a value indicating whether there are operations that can be undone.
    /// </summary>
    /// <remarks>
    /// Returns true when the undo stack is not empty.
    /// This property should update after each operation that modifies the undo stack.
    /// </remarks>
    bool CanUndo { get; }

    /// <summary>
    /// Gets a value indicating whether there are operations that can be redone.
    /// </summary>
    /// <remarks>
    /// Returns true when the redo stack is not empty.
    /// This property should update after undo operations and clear after new changes.
    /// </remarks>
    bool CanRedo { get; }

    /// <summary>
    /// Gets the number of operations currently in the undo stack.
    /// </summary>
    int UndoCount { get; }

    /// <summary>
    /// Gets the number of operations currently in the redo stack.
    /// </summary>
    int RedoCount { get; }

    /// <summary>
    /// Gets or sets the maximum number of operations to keep in the undo history.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the undo stack exceeds this limit, the oldest operations are discarded.
    /// Set to -1 for unlimited history (use with caution for memory-intensive operations).
    /// </para>
    /// <para>
    /// Default value is typically 100, but may vary by control.
    /// Changing this value does not immediately trim existing history.
    /// </para>
    /// </remarks>
    int UndoLimit { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Undoes the most recent operation.
    /// </summary>
    /// <returns>
    /// True if an operation was undone; false if there was nothing to undo.
    /// </returns>
    /// <remarks>
    /// This method should:
    /// <list type="bullet">
    ///   <item>Return false if <see cref="CanUndo"/> is false</item>
    ///   <item>Reverse the most recent operation</item>
    ///   <item>Move the operation to the redo stack</item>
    ///   <item>Update <see cref="CanUndo"/> and <see cref="CanRedo"/></item>
    ///   <item>Raise property changed notifications</item>
    /// </list>
    /// </remarks>
    bool Undo();

    /// <summary>
    /// Redoes the most recently undone operation.
    /// </summary>
    /// <returns>
    /// True if an operation was redone; false if there was nothing to redo.
    /// </returns>
    /// <remarks>
    /// This method should:
    /// <list type="bullet">
    ///   <item>Return false if <see cref="CanRedo"/> is false</item>
    ///   <item>Reapply the most recently undone operation</item>
    ///   <item>Move the operation back to the undo stack</item>
    ///   <item>Update <see cref="CanUndo"/> and <see cref="CanRedo"/></item>
    ///   <item>Raise property changed notifications</item>
    /// </list>
    /// </remarks>
    bool Redo();

    /// <summary>
    /// Clears all undo and redo history.
    /// </summary>
    /// <remarks>
    /// This method should:
    /// <list type="bullet">
    ///   <item>Clear both undo and redo stacks</item>
    ///   <item>Set <see cref="CanUndo"/> and <see cref="CanRedo"/> to false</item>
    ///   <item>Be called when loading new data or resetting the control</item>
    /// </list>
    /// </remarks>
    void ClearUndoHistory();

    /// <summary>
    /// Gets a description of the operation that would be undone.
    /// </summary>
    /// <returns>
    /// A human-readable description of the next undo operation,
    /// or null if there is nothing to undo.
    /// </returns>
    /// <remarks>
    /// Useful for displaying "Undo [action]" in menus or tooltips.
    /// Examples: "Undo typing", "Undo paste", "Undo delete".
    /// </remarks>
    string? GetUndoDescription();

    /// <summary>
    /// Gets a description of the operation that would be redone.
    /// </summary>
    /// <returns>
    /// A human-readable description of the next redo operation,
    /// or null if there is nothing to redo.
    /// </returns>
    /// <remarks>
    /// Useful for displaying "Redo [action]" in menus or tooltips.
    /// Examples: "Redo typing", "Redo paste", "Redo delete".
    /// </remarks>
    string? GetRedoDescription();

    #endregion

    #region Batch Operations

    /// <summary>
    /// Begins a batch operation that groups multiple changes into a single undo unit.
    /// </summary>
    /// <param name="description">Optional description for the batch operation.</param>
    /// <remarks>
    /// <para>
    /// All changes made between <see cref="BeginBatchOperation"/> and <see cref="EndBatchOperation"/>
    /// will be treated as a single undoable operation.
    /// </para>
    /// <para>
    /// Batch operations can be nested. The outermost batch defines the undo unit.
    /// </para>
    /// <para>
    /// Example use cases:
    /// <list type="bullet">
    ///   <item>Search and replace all</item>
    ///   <item>Programmatic multi-step updates</item>
    ///   <item>Drag-and-drop operations</item>
    /// </list>
    /// </para>
    /// </remarks>
    void BeginBatchOperation(string? description = null);

    /// <summary>
    /// Ends the current batch operation.
    /// </summary>
    /// <remarks>
    /// Must be called after <see cref="BeginBatchOperation"/>.
    /// If nested, only the outermost EndBatchOperation commits the batch.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if called without a matching <see cref="BeginBatchOperation"/>.
    /// </exception>
    void EndBatchOperation();

    /// <summary>
    /// Cancels the current batch operation, discarding all changes made since
    /// <see cref="BeginBatchOperation"/> was called.
    /// </summary>
    /// <remarks>
    /// This method reverts all changes made during the batch and does not
    /// add anything to the undo stack.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if called without a matching <see cref="BeginBatchOperation"/>.
    /// </exception>
    void CancelBatchOperation();

    #endregion

    #region Commands (MVVM Support)

    /// <summary>
    /// Gets or sets the command to execute for undo operations.
    /// </summary>
    /// <remarks>
    /// When set, this command will be executed instead of the default <see cref="Undo"/> behavior.
    /// CanExecute should reflect <see cref="CanUndo"/>.
    /// </remarks>
    ICommand? UndoCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to execute for redo operations.
    /// </summary>
    /// <remarks>
    /// When set, this command will be executed instead of the default <see cref="Redo"/> behavior.
    /// CanExecute should reflect <see cref="CanRedo"/>.
    /// </remarks>
    ICommand? RedoCommand { get; set; }

    #endregion
}
