using System.Windows.Input;

namespace MauiControlsExtras.Base;

/// <summary>
/// Interface for controls that support keyboard navigation and input.
/// Implement this interface on interactive controls that need to respond to
/// keyboard events on desktop platforms (Windows, macOS).
/// </summary>
/// <remarks>
/// <para>
/// This interface provides a standardized contract for keyboard handling across
/// all controls in the library, ensuring consistent keyboard shortcuts and
/// navigation patterns.
/// </para>
/// <para>
/// Controls implementing this interface should:
/// <list type="bullet">
///   <item>Handle standard keyboard shortcuts (Tab, Enter, Escape, arrow keys, etc.)</item>
///   <item>Support keyboard-only operation for accessibility</item>
///   <item>Provide visual focus indicators</item>
///   <item>Document all supported keyboard shortcuts via <see cref="GetKeyboardShortcuts"/></item>
///   <item>Integrate with platform focus systems</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Implementing keyboard navigation:
/// public bool HandleKeyPress(KeyEventArgs e)
/// {
///     if (e.Key == Keys.Enter)
///     {
///         ActivateCurrentItem();
///         return true; // Handled
///     }
///     return false; // Not handled, allow propagation
/// }
/// </code>
/// </example>
public interface IKeyboardNavigable
{
    #region State Properties

    /// <summary>
    /// Gets a value indicating whether this control can receive keyboard focus.
    /// </summary>
    /// <remarks>
    /// Controls that are disabled, hidden, or otherwise non-interactive should return false.
    /// This property affects Tab navigation order and focus management.
    /// </remarks>
    bool CanReceiveFocus { get; }

    /// <summary>
    /// Gets or sets a value indicating whether keyboard navigation is enabled for this control.
    /// </summary>
    /// <remarks>
    /// When false, the control will not process keyboard events (except Tab for focus movement).
    /// Defaults to true. Set to false to temporarily disable keyboard handling, for example
    /// during animations or when the control is in a read-only preview mode.
    /// </remarks>
    bool IsKeyboardNavigationEnabled { get; set; }

    /// <summary>
    /// Gets a value indicating whether this control currently has keyboard focus.
    /// </summary>
    /// <remarks>
    /// This property should reflect the actual focus state and update when focus changes.
    /// Controls should provide visual feedback when focused (e.g., focus ring, highlight).
    /// </remarks>
    bool HasKeyboardFocus { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Handles a key press event.
    /// </summary>
    /// <param name="e">The key event arguments containing information about the pressed key.</param>
    /// <returns>
    /// <c>true</c> if the key press was handled and should not propagate further;
    /// <c>false</c> if the key press was not handled and should continue to propagate.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is the primary entry point for keyboard input handling.
    /// Implementations should check <see cref="IsKeyboardNavigationEnabled"/> before processing.
    /// </para>
    /// <para>
    /// Standard keys that should typically be handled:
    /// <list type="bullet">
    ///   <item><b>Tab/Shift+Tab</b>: Move focus between focusable elements</item>
    ///   <item><b>Enter/Space</b>: Activate/select the current item</item>
    ///   <item><b>Escape</b>: Cancel current operation or close popup</item>
    ///   <item><b>Arrow keys</b>: Navigate within the control</item>
    ///   <item><b>Home/End</b>: Navigate to first/last item</item>
    ///   <item><b>Page Up/Down</b>: Navigate by page in scrollable controls</item>
    /// </list>
    /// </para>
    /// </remarks>
    bool HandleKeyPress(KeyEventArgs e);

    /// <summary>
    /// Gets a list of all keyboard shortcuts supported by this control.
    /// </summary>
    /// <returns>
    /// A read-only list of <see cref="KeyboardShortcut"/> objects describing each supported shortcut.
    /// </returns>
    /// <remarks>
    /// This method enables runtime discovery of keyboard shortcuts for documentation,
    /// help systems, and accessibility tools. All shortcuts should be documented here,
    /// including those with modifier keys (Ctrl, Shift, Alt).
    /// </remarks>
    IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts();

    /// <summary>
    /// Attempts to set keyboard focus to this control.
    /// </summary>
    /// <returns>
    /// <c>true</c> if focus was successfully set; <c>false</c> if the control cannot receive focus.
    /// </returns>
    /// <remarks>
    /// This method should check <see cref="CanReceiveFocus"/> before attempting to set focus.
    /// It should also handle any necessary visual updates to show the focused state.
    /// </remarks>
    bool Focus();

    #endregion

    #region Commands (MVVM Support)

    /// <summary>
    /// Gets or sets the command to execute when the control receives keyboard focus.
    /// </summary>
    /// <remarks>
    /// The command parameter will be the control itself.
    /// This is useful for MVVM scenarios where focus changes need to trigger view model logic.
    /// </remarks>
    ICommand? GotFocusCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to execute when the control loses keyboard focus.
    /// </summary>
    /// <remarks>
    /// The command parameter will be the control itself.
    /// This is useful for triggering validation or save operations when focus leaves a control.
    /// </remarks>
    ICommand? LostFocusCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to execute when a key is pressed while the control has focus.
    /// </summary>
    /// <remarks>
    /// The command parameter will be the <see cref="KeyEventArgs"/>.
    /// If the command sets <see cref="KeyEventArgs.Handled"/> to true, further processing stops.
    /// This allows view models to intercept and handle keyboard events.
    /// </remarks>
    ICommand? KeyPressCommand { get; set; }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the control receives keyboard focus.
    /// </summary>
    event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <summary>
    /// Occurs when the control loses keyboard focus.
    /// </summary>
    event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusLost;

    /// <summary>
    /// Occurs when a key is pressed while the control has focus.
    /// </summary>
    /// <remarks>
    /// This event fires before <see cref="HandleKeyPress"/> processes the key.
    /// Handlers can set <see cref="KeyEventArgs.Handled"/> to true to prevent default handling.
    /// </remarks>
    event EventHandler<KeyEventArgs>? KeyPressed;

    /// <summary>
    /// Occurs when a key is released while the control has focus.
    /// </summary>
    event EventHandler<KeyEventArgs>? KeyReleased;

    #endregion
}

/// <summary>
/// Describes a keyboard shortcut supported by a control.
/// </summary>
/// <remarks>
/// This record is used by <see cref="IKeyboardNavigable.GetKeyboardShortcuts"/> to
/// document all keyboard shortcuts a control supports. This information can be used
/// for help systems, accessibility features, and shortcut customization UIs.
/// </remarks>
public record KeyboardShortcut
{
    /// <summary>
    /// Gets the primary key for this shortcut (e.g., Enter, Escape, A).
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the modifier keys required for this shortcut (e.g., "Ctrl", "Ctrl+Shift").
    /// </summary>
    /// <remarks>
    /// Use standard modifier names: Ctrl, Shift, Alt (or Option on macOS), Cmd (macOS only).
    /// Combine multiple modifiers with + (e.g., "Ctrl+Shift").
    /// Empty or null if no modifiers are required.
    /// </remarks>
    public string? Modifiers { get; init; }

    /// <summary>
    /// Gets a human-readable description of what this shortcut does.
    /// </summary>
    /// <remarks>
    /// This description should be suitable for display in help dialogs and tooltips.
    /// Use clear, action-oriented language (e.g., "Select all items", "Delete selected row").
    /// </remarks>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the category or group this shortcut belongs to.
    /// </summary>
    /// <remarks>
    /// Used to organize shortcuts in help displays. Common categories include:
    /// "Navigation", "Selection", "Editing", "Clipboard", "General".
    /// </remarks>
    public string? Category { get; init; }

    /// <summary>
    /// Gets a value indicating whether this shortcut is enabled.
    /// </summary>
    /// <remarks>
    /// Shortcuts may be conditionally enabled based on control state.
    /// Disabled shortcuts should still be documented but marked as unavailable.
    /// </remarks>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Gets the formatted display string for this shortcut (e.g., "Ctrl+A", "Enter").
    /// </summary>
    public string DisplayString => string.IsNullOrEmpty(Modifiers) ? Key : $"{Modifiers}+{Key}";
}

/// <summary>
/// Provides event data for keyboard events.
/// </summary>
public class KeyEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyEventArgs"/> class.
    /// </summary>
    /// <param name="key">The key that was pressed or released.</param>
    /// <param name="modifiers">The modifier keys that were held during the event.</param>
    public KeyEventArgs(string key, KeyModifiers modifiers = KeyModifiers.None)
    {
        Key = key;
        Modifiers = modifiers;
    }

    /// <summary>
    /// Gets the key that was pressed or released.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the modifier keys that were held during the event.
    /// </summary>
    public KeyModifiers Modifiers { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this event has been handled.
    /// </summary>
    /// <remarks>
    /// Set to <c>true</c> to prevent further processing of this key event.
    /// </remarks>
    public bool Handled { get; set; }

    /// <summary>
    /// Gets a value indicating whether the Control key (or Command on macOS) was held.
    /// </summary>
    public bool IsControlPressed => Modifiers.HasFlag(KeyModifiers.Control);

    /// <summary>
    /// Gets a value indicating whether the Shift key was held.
    /// </summary>
    public bool IsShiftPressed => Modifiers.HasFlag(KeyModifiers.Shift);

    /// <summary>
    /// Gets a value indicating whether the Alt key (or Option on macOS) was held.
    /// </summary>
    public bool IsAltPressed => Modifiers.HasFlag(KeyModifiers.Alt);

    /// <summary>
    /// Gets a value indicating whether the platform command key was held (Ctrl on Windows, Cmd on macOS).
    /// </summary>
    public bool IsPlatformCommandPressed => Modifiers.HasFlag(KeyModifiers.PlatformCommand);
}

/// <summary>
/// Specifies the modifier keys for keyboard events.
/// </summary>
[Flags]
public enum KeyModifiers
{
    /// <summary>
    /// No modifier keys.
    /// </summary>
    None = 0,

    /// <summary>
    /// The Control key (Ctrl on Windows/Linux, Control on macOS).
    /// </summary>
    Control = 1,

    /// <summary>
    /// The Shift key.
    /// </summary>
    Shift = 2,

    /// <summary>
    /// The Alt key (Alt on Windows/Linux, Option on macOS).
    /// </summary>
    Alt = 4,

    /// <summary>
    /// The platform-specific command key (Ctrl on Windows/Linux, Command on macOS).
    /// </summary>
    /// <remarks>
    /// Use this for shortcuts that should use Ctrl on Windows/Linux and Command on macOS.
    /// </remarks>
    PlatformCommand = 8
}

/// <summary>
/// Provides event data for keyboard focus events.
/// </summary>
/// <remarks>
/// This is distinct from <see cref="Microsoft.Maui.Controls.FocusEventArgs"/> to avoid naming conflicts
/// and to provide keyboard-specific focus information.
/// </remarks>
public class KeyboardFocusEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardFocusEventArgs"/> class.
    /// </summary>
    /// <param name="isFocused">Whether the control is gaining or losing focus.</param>
    public KeyboardFocusEventArgs(bool isFocused)
    {
        IsFocused = isFocused;
    }

    /// <summary>
    /// Gets a value indicating whether the control is gaining focus (true) or losing focus (false).
    /// </summary>
    public bool IsFocused { get; }
}
