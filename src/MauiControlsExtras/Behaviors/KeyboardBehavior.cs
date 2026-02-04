using MauiControlsExtras.Base;

namespace MauiControlsExtras.Behaviors;

/// <summary>
/// A behavior that provides keyboard event handling for MAUI controls.
/// Attach this behavior to any View to receive keyboard events on desktop platforms.
/// </summary>
/// <remarks>
/// <para>
/// This behavior bridges the gap between MAUI's cross-platform input system and
/// the <see cref="IKeyboardNavigable"/> interface, providing consistent keyboard
/// handling across Windows and macOS.
/// </para>
/// <para>
/// Usage:
/// <code>
/// &lt;ContentView x:Name="myControl"&gt;
///     &lt;ContentView.Behaviors&gt;
///         &lt;behaviors:KeyboardBehavior KeyPressed="OnKeyPressed" /&gt;
///     &lt;/ContentView.Behaviors&gt;
/// &lt;/ContentView&gt;
/// </code>
/// </para>
/// </remarks>
public class KeyboardBehavior : Behavior<View>
{
    private View? _attachedView;

    #region Bindable Properties

    /// <summary>
    /// Identifies the IsEnabled bindable property.
    /// </summary>
    public static readonly BindableProperty IsEnabledProperty =
        BindableProperty.Create(
            nameof(IsEnabled),
            typeof(bool),
            typeof(KeyboardBehavior),
            true);

    /// <summary>
    /// Gets or sets a value indicating whether keyboard handling is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// Identifies the HandleTabKey bindable property.
    /// </summary>
    public static readonly BindableProperty HandleTabKeyProperty =
        BindableProperty.Create(
            nameof(HandleTabKey),
            typeof(bool),
            typeof(KeyboardBehavior),
            false);

    /// <summary>
    /// Gets or sets a value indicating whether Tab key events should be captured.
    /// </summary>
    /// <remarks>
    /// Set to true only if the control needs to handle Tab internally (e.g., for cell navigation in a grid).
    /// Normally Tab should be left to the platform for focus navigation.
    /// </remarks>
    public bool HandleTabKey
    {
        get => (bool)GetValue(HandleTabKeyProperty);
        set => SetValue(HandleTabKeyProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when a key is pressed while the attached view has focus.
    /// </summary>
    public event EventHandler<KeyEventArgs>? KeyPressed;

    /// <summary>
    /// Occurs when a key is released while the attached view has focus.
    /// </summary>
    /// <remarks>
    /// Note: Key release events may not be available on all platforms (e.g., Mac Catalyst).
    /// This event is primarily useful on Windows.
    /// </remarks>
#pragma warning disable CS0067 // Event is never used on some platforms
    public event EventHandler<KeyEventArgs>? KeyReleased;
#pragma warning restore CS0067

    #endregion

    /// <inheritdoc/>
    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        _attachedView = bindable;

        // Hook into focus events to track when we should process keys
        bindable.Focused += OnViewFocused;
        bindable.Unfocused += OnViewUnfocused;

        // Set up platform-specific keyboard handling
        SetupPlatformKeyboardHandling(bindable);
    }

    /// <inheritdoc/>
    protected override void OnDetachingFrom(View bindable)
    {
        bindable.Focused -= OnViewFocused;
        bindable.Unfocused -= OnViewUnfocused;

        CleanupPlatformKeyboardHandling(bindable);

        _attachedView = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnViewFocused(object? sender, Microsoft.Maui.Controls.FocusEventArgs e)
    {
        // View gained focus - keyboard events will now be processed
    }

    private void OnViewUnfocused(object? sender, Microsoft.Maui.Controls.FocusEventArgs e)
    {
        // View lost focus - stop processing keyboard events
    }

    private void SetupPlatformKeyboardHandling(View view)
    {
#if WINDOWS
        SetupWindowsKeyboardHandling(view);
#elif MACCATALYST
        SetupMacKeyboardHandling(view);
#endif
    }

    private void CleanupPlatformKeyboardHandling(View view)
    {
#if WINDOWS
        CleanupWindowsKeyboardHandling(view);
#elif MACCATALYST
        CleanupMacKeyboardHandling(view);
#endif
    }

#if WINDOWS
    private void SetupWindowsKeyboardHandling(View view)
    {
        view.HandlerChanged += OnHandlerChangedWindows;
        if (view.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
        {
            AttachWindowsKeyboardEvents(element);
        }
    }

    private void CleanupWindowsKeyboardHandling(View view)
    {
        view.HandlerChanged -= OnHandlerChangedWindows;
        if (view.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
        {
            DetachWindowsKeyboardEvents(element);
        }
    }

    private void OnHandlerChangedWindows(object? sender, EventArgs e)
    {
        if (sender is View view && view.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
        {
            AttachWindowsKeyboardEvents(element);
        }
    }

    private void AttachWindowsKeyboardEvents(Microsoft.UI.Xaml.UIElement element)
    {
        element.KeyDown += OnWindowsKeyDown;
        element.KeyUp += OnWindowsKeyUp;

        // Make the element focusable if it isn't already
        if (!element.IsTabStop)
        {
            element.IsTabStop = true;
        }
    }

    private void DetachWindowsKeyboardEvents(Microsoft.UI.Xaml.UIElement element)
    {
        element.KeyDown -= OnWindowsKeyDown;
        element.KeyUp -= OnWindowsKeyUp;
    }

    private void OnWindowsKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (!IsEnabled) return;

        var key = ConvertWindowsKey(e.Key);
        if (key == "Tab" && !HandleTabKey) return;

        var modifiers = GetWindowsModifiers();
        var args = new KeyEventArgs(key, modifiers);

        KeyPressed?.Invoke(this, args);

        // If the attached view implements IKeyboardNavigable, delegate to it
        if (!args.Handled && _attachedView is IKeyboardNavigable navigable)
        {
            args.Handled = navigable.HandleKeyPress(args);
        }

        e.Handled = args.Handled;
    }

    private void OnWindowsKeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (!IsEnabled) return;

        var key = ConvertWindowsKey(e.Key);
        var modifiers = GetWindowsModifiers();
        var args = new KeyEventArgs(key, modifiers);

        KeyReleased?.Invoke(this, args);
        e.Handled = args.Handled;
    }

    private static string ConvertWindowsKey(Windows.System.VirtualKey key)
    {
        return key switch
        {
            Windows.System.VirtualKey.Enter => "Enter",
            Windows.System.VirtualKey.Escape => "Escape",
            Windows.System.VirtualKey.Tab => "Tab",
            Windows.System.VirtualKey.Space => "Space",
            Windows.System.VirtualKey.Back => "Backspace",
            Windows.System.VirtualKey.Delete => "Delete",
            Windows.System.VirtualKey.Insert => "Insert",
            Windows.System.VirtualKey.Home => "Home",
            Windows.System.VirtualKey.End => "End",
            Windows.System.VirtualKey.PageUp => "PageUp",
            Windows.System.VirtualKey.PageDown => "PageDown",
            Windows.System.VirtualKey.Up => "ArrowUp",
            Windows.System.VirtualKey.Down => "ArrowDown",
            Windows.System.VirtualKey.Left => "ArrowLeft",
            Windows.System.VirtualKey.Right => "ArrowRight",
            Windows.System.VirtualKey.F1 => "F1",
            Windows.System.VirtualKey.F2 => "F2",
            Windows.System.VirtualKey.F3 => "F3",
            Windows.System.VirtualKey.F4 => "F4",
            Windows.System.VirtualKey.F5 => "F5",
            Windows.System.VirtualKey.F6 => "F6",
            Windows.System.VirtualKey.F7 => "F7",
            Windows.System.VirtualKey.F8 => "F8",
            Windows.System.VirtualKey.F9 => "F9",
            Windows.System.VirtualKey.F10 => "F10",
            Windows.System.VirtualKey.F11 => "F11",
            Windows.System.VirtualKey.F12 => "F12",
            Windows.System.VirtualKey.A => "A",
            Windows.System.VirtualKey.C => "C",
            Windows.System.VirtualKey.V => "V",
            Windows.System.VirtualKey.X => "X",
            Windows.System.VirtualKey.Z => "Z",
            Windows.System.VirtualKey.Y => "Y",
            _ => key.ToString()
        };
    }

    private static KeyModifiers GetWindowsModifiers()
    {
        var modifiers = KeyModifiers.None;
        var coreWindow = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread;

        if (coreWindow(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            modifiers |= KeyModifiers.Control | KeyModifiers.PlatformCommand;
        }
        if (coreWindow(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            modifiers |= KeyModifiers.Shift;
        }
        if (coreWindow(Windows.System.VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            modifiers |= KeyModifiers.Alt;
        }

        return modifiers;
    }
#endif

#if MACCATALYST
    private void SetupMacKeyboardHandling(View view)
    {
        view.HandlerChanged += OnHandlerChangedMac;
        if (view.Handler?.PlatformView is UIKit.UIView uiView)
        {
            AttachMacKeyboardEvents(uiView);
        }
    }

    private void CleanupMacKeyboardHandling(View view)
    {
        view.HandlerChanged -= OnHandlerChangedMac;
        // UIKit doesn't require explicit detachment for key commands
    }

    private void OnHandlerChangedMac(object? sender, EventArgs e)
    {
        if (sender is View view && view.Handler?.PlatformView is UIKit.UIView uiView)
        {
            AttachMacKeyboardEvents(uiView);
        }
    }

    private void AttachMacKeyboardEvents(UIKit.UIView uiView)
    {
        // On Mac Catalyst, keyboard events are handled through UIKeyCommand
        // which requires the view or its responder chain to implement key commands.
        // For now, we set up the responder chain to enable keyboard focus.
        uiView.UserInteractionEnabled = true;
    }

    /// <summary>
    /// Creates key commands for Mac Catalyst keyboard handling.
    /// Call this from your custom view's KeyCommands property override.
    /// </summary>
    public UIKit.UIKeyCommand[] GetMacKeyCommands()
    {
        var commands = new List<UIKit.UIKeyCommand>();

        // Arrow keys
        commands.Add(CreateMacKeyCommand(UIKit.UIKeyCommand.UpArrow, 0, "ArrowUp"));
        commands.Add(CreateMacKeyCommand(UIKit.UIKeyCommand.DownArrow, 0, "ArrowDown"));
        commands.Add(CreateMacKeyCommand(UIKit.UIKeyCommand.LeftArrow, 0, "ArrowLeft"));
        commands.Add(CreateMacKeyCommand(UIKit.UIKeyCommand.RightArrow, 0, "ArrowRight"));

        // Common shortcuts with Command key
        commands.Add(CreateMacKeyCommand("a", UIKit.UIKeyModifierFlags.Command, "A"));
        commands.Add(CreateMacKeyCommand("c", UIKit.UIKeyModifierFlags.Command, "C"));
        commands.Add(CreateMacKeyCommand("v", UIKit.UIKeyModifierFlags.Command, "V"));
        commands.Add(CreateMacKeyCommand("x", UIKit.UIKeyModifierFlags.Command, "X"));
        commands.Add(CreateMacKeyCommand("z", UIKit.UIKeyModifierFlags.Command, "Z"));

        // Enter and Escape
        commands.Add(CreateMacKeyCommand("\r", 0, "Enter"));
        commands.Add(CreateMacKeyCommand(UIKit.UIKeyCommand.Escape, 0, "Escape"));

        // Tab (if enabled)
        if (HandleTabKey)
        {
            commands.Add(CreateMacKeyCommand("\t", 0, "Tab"));
        }

        return commands.ToArray();
    }

    private UIKit.UIKeyCommand CreateMacKeyCommand(string input, UIKit.UIKeyModifierFlags modifiers, string keyName)
    {
        var selector = new ObjCRuntime.Selector("handleKeyCommand:");
        return UIKit.UIKeyCommand.Create(new Foundation.NSString(input), modifiers, selector);
    }

    /// <summary>
    /// Handles a Mac key command. Call this from your view's key command handler.
    /// </summary>
    public void HandleMacKeyCommand(UIKit.UIKeyCommand command)
    {
        if (!IsEnabled) return;

        var key = ConvertMacKey(command);
        var modifiers = ConvertMacModifiers(command.ModifierFlags);
        var args = new KeyEventArgs(key, modifiers);

        KeyPressed?.Invoke(this, args);

        if (!args.Handled && _attachedView is IKeyboardNavigable navigable)
        {
            args.Handled = navigable.HandleKeyPress(args);
        }
    }

    private static string ConvertMacKey(UIKit.UIKeyCommand command)
    {
        var input = command.Input;

        if (input == UIKit.UIKeyCommand.UpArrow) return "ArrowUp";
        if (input == UIKit.UIKeyCommand.DownArrow) return "ArrowDown";
        if (input == UIKit.UIKeyCommand.LeftArrow) return "ArrowLeft";
        if (input == UIKit.UIKeyCommand.RightArrow) return "ArrowRight";
        if (input == UIKit.UIKeyCommand.Escape) return "Escape";
        if (input == "\r") return "Enter";
        if (input == "\t") return "Tab";
        if (input == " ") return "Space";
        if (input == "\x7f") return "Backspace";

        if (string.IsNullOrEmpty(input))
            return "";
        return ((string)input).ToUpperInvariant();
    }

    private static KeyModifiers ConvertMacModifiers(UIKit.UIKeyModifierFlags flags)
    {
        var modifiers = KeyModifiers.None;

        if (flags.HasFlag(UIKit.UIKeyModifierFlags.Command))
        {
            modifiers |= KeyModifiers.PlatformCommand;
        }
        if (flags.HasFlag(UIKit.UIKeyModifierFlags.Control))
        {
            modifiers |= KeyModifiers.Control;
        }
        if (flags.HasFlag(UIKit.UIKeyModifierFlags.Shift))
        {
            modifiers |= KeyModifiers.Shift;
        }
        if (flags.HasFlag(UIKit.UIKeyModifierFlags.Alternate))
        {
            modifiers |= KeyModifiers.Alt;
        }

        return modifiers;
    }
#endif

    /// <summary>
    /// Simulates a key press event. Useful for testing or programmatic input.
    /// </summary>
    /// <param name="key">The key to simulate.</param>
    /// <param name="modifiers">The modifier keys to include.</param>
    /// <returns>True if the event was handled; false otherwise.</returns>
    public bool SimulateKeyPress(string key, KeyModifiers modifiers = KeyModifiers.None)
    {
        if (!IsEnabled) return false;

        var args = new KeyEventArgs(key, modifiers);
        KeyPressed?.Invoke(this, args);

        if (!args.Handled && _attachedView is IKeyboardNavigable navigable)
        {
            args.Handled = navigable.HandleKeyPress(args);
        }

        return args.Handled;
    }
}
