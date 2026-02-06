using System.Windows.Input;
using MauiControlsExtras.Base;

namespace MauiControlsExtras.Tests.Helpers;

/// <summary>
/// Concrete styled control that implements <see cref="IKeyboardNavigable"/> for keyboard behavior tests.
/// </summary>
public class TestableKeyboardNavigableStyledControl : StyledControlBase, IKeyboardNavigable
{
    public bool CanReceiveFocus => true;

    public bool IsKeyboardNavigationEnabled { get; set; } = true;

    public bool HasKeyboardFocus { get; private set; }

    public ICommand? GotFocusCommand { get; set; }

    public ICommand? LostFocusCommand { get; set; }

    public ICommand? KeyPressCommand { get; set; }

    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusGained;

    public event EventHandler<KeyboardFocusEventArgs>? KeyboardFocusLost;

    public event EventHandler<KeyEventArgs>? KeyPressed;

    public event EventHandler<KeyEventArgs>? KeyReleased;

    public bool HandleKeyPress(KeyEventArgs e)
    {
        KeyPressed?.Invoke(this, e);
        return e.Handled;
    }

    public IReadOnlyList<KeyboardShortcut> GetKeyboardShortcuts() => Array.Empty<KeyboardShortcut>();

    public new bool Focus()
    {
        HasKeyboardFocus = true;
        KeyboardFocusGained?.Invoke(this, new KeyboardFocusEventArgs(true));
        return true;
    }
}
