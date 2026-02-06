namespace MauiControlsExtras.Helpers;

/// <summary>
/// Handles Android back button presses to close open popups/dropdowns.
/// Uses a stack (LIFO) so the most recently opened popup closes first.
/// Cross-platform stack logic; only the native callback hookup is Android-specific.
/// </summary>
internal static class AndroidBackButtonHandler
{
    private static readonly List<RegistrationEntry> _stack = [];

    /// <summary>
    /// Registers an owner with a close action. When the Android back button is pressed,
    /// the most recently registered (topmost) entry's close action is invoked.
    /// </summary>
    public static void Register(object owner, Action closeAction)
    {
        // Remove existing entry for this owner (re-register moves to top)
        RemoveOwner(owner);

        _stack.Add(new RegistrationEntry(owner, closeAction));

#if ANDROID
        EnsureCallbackEnabled();
#endif
    }

    /// <summary>
    /// Unregisters an owner. Safe to call even if the owner was never registered or already removed.
    /// </summary>
    public static void Unregister(object owner)
    {
        RemoveOwner(owner);
        CleanupStale();

#if ANDROID
        UpdateCallbackState();
#endif
    }

    /// <summary>
    /// Handles a back button press by popping the topmost entry and invoking its close action.
    /// Returns true if an entry was handled, false if the stack was empty.
    /// </summary>
    internal static bool HandleBackPress()
    {
        CleanupStale();

        if (_stack.Count == 0)
            return false;

        var top = _stack[^1];
        _stack.RemoveAt(_stack.Count - 1);

        if (top.OwnerRef.TryGetTarget(out _))
        {
            top.CloseAction.Invoke();
        }

#if ANDROID
        UpdateCallbackState();
#endif

        return true;
    }

    /// <summary>
    /// Removes entries whose owners have been garbage collected.
    /// </summary>
    internal static void CleanupStale()
    {
        _stack.RemoveAll(e => !e.OwnerRef.TryGetTarget(out _));
    }

    /// <summary>
    /// Clears all registrations. Used for testing.
    /// </summary>
    internal static void Reset()
    {
        _stack.Clear();

#if ANDROID
        UpdateCallbackState();
#endif
    }

    /// <summary>
    /// Gets the current registration count. Used for testing.
    /// </summary>
    internal static int Count => _stack.Count;

    private static void RemoveOwner(object owner)
    {
        _stack.RemoveAll(e =>
        {
            if (!e.OwnerRef.TryGetTarget(out var target))
                return true; // also clean up stale entries
            return ReferenceEquals(target, owner);
        });
    }

#if ANDROID
    private static AndroidX.Activity.OnBackPressedCallback? _callback;

    private static void EnsureCallbackEnabled()
    {
        if (_callback != null)
        {
            _callback.Enabled = true;
            return;
        }

        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        if (activity is not AndroidX.Activity.ComponentActivity componentActivity)
            return;

        _callback = new BackPressedCallback(() =>
        {
            HandleBackPress();
        });

        componentActivity.OnBackPressedDispatcher.AddCallback(componentActivity, _callback);
    }

    private static void UpdateCallbackState()
    {
        if (_callback != null)
        {
            _callback.Enabled = _stack.Count > 0;
        }
    }

    private sealed class BackPressedCallback : AndroidX.Activity.OnBackPressedCallback
    {
        private readonly Action _onBackPressed;

        public BackPressedCallback(Action onBackPressed) : base(enabled: true)
        {
            _onBackPressed = onBackPressed;
        }

        public override void HandleOnBackPressed()
        {
            _onBackPressed();
        }
    }
#endif

    private sealed class RegistrationEntry
    {
        public WeakReference<object> OwnerRef { get; }
        public Action CloseAction { get; }

        public RegistrationEntry(object owner, Action closeAction)
        {
            OwnerRef = new WeakReference<object>(owner);
            CloseAction = closeAction;
        }
    }
}
