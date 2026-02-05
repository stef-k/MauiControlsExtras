using MauiControlsExtras.Base;

namespace MauiControlsExtras.Helpers;

/// <summary>
/// Bridges native mobile clipboard operations (context menu Copy/Cut/Paste) to
/// <see cref="IClipboardSupport"/> commands. On mobile, native context menus perform
/// clipboard operations directly on the native view, bypassing the MVVM command layer.
/// This bridge detects those operations and fires the corresponding commands.
/// </summary>
internal static class MobileClipboardBridge
{
    /// <summary>
    /// Attaches platform-specific clipboard operation detection to the given entry
    /// and fires the corresponding commands on the owner.
    /// </summary>
    /// <param name="entry">The MAUI Entry whose native view will be monitored.</param>
    /// <param name="owner">The control implementing <see cref="IClipboardSupport"/>.</param>
    public static void Setup(Entry entry, IClipboardSupport owner)
    {
        if (entry.Handler?.PlatformView == null) return;

#if ANDROID
        SetupAndroid(entry, owner);
#elif IOS || MACCATALYST
        SetupApple(entry, owner);
#endif
    }

#if ANDROID
    private static void SetupAndroid(Entry entry, IClipboardSupport owner)
    {
        if (entry.Handler?.PlatformView is not AndroidX.AppCompat.Widget.AppCompatEditText editText)
            return;

        var callback = new ClipboardActionModeCallback(owner);
        editText.CustomSelectionActionModeCallback = callback;
        editText.CustomInsertionActionModeCallback = callback;
    }

    private sealed class ClipboardActionModeCallback : Java.Lang.Object, Android.Views.ActionMode.ICallback
    {
        // Standard Android resource IDs for clipboard actions
        private const int CopyId = 16908321;   // android.R.id.copy
        private const int CutId = 16908320;     // android.R.id.cut
        private const int PasteId = 16908322;   // android.R.id.paste

        private readonly WeakReference<IClipboardSupport> _ownerRef;

        public ClipboardActionModeCallback(IClipboardSupport owner)
        {
            _ownerRef = new WeakReference<IClipboardSupport>(owner);
        }

        public bool OnActionItemClicked(Android.Views.ActionMode? mode, Android.Views.IMenuItem? item)
        {
            if (item == null || !_ownerRef.TryGetTarget(out var owner))
                return false;

            var id = item.ItemId;

            if (id == CopyId)
                owner.CopyCommand?.Execute(owner.GetClipboardContent());
            else if (id == CutId)
                owner.CutCommand?.Execute(owner.GetClipboardContent());
            else if (id == PasteId)
                owner.PasteCommand?.Execute(null);

            // Return false to let the native action proceed
            return false;
        }

        public bool OnCreateActionMode(Android.Views.ActionMode? mode, Android.Views.IMenu? menu) => true;

        public void OnDestroyActionMode(Android.Views.ActionMode? mode) { }

        public bool OnPrepareActionMode(Android.Views.ActionMode? mode, Android.Views.IMenu? menu) => false;
    }
#endif

#if IOS || MACCATALYST
    private static void SetupApple(Entry entry, IClipboardSupport owner)
    {
        if (entry.Handler?.PlatformView is not UIKit.UITextField textField)
            return;

        var observer = new AppleClipboardObserver(textField, entry, owner);
        // Store observer reference on the entry to prevent GC and allow cleanup
        SetObserver(entry, observer);
    }

    // Use attached-property-like storage via ConditionalWeakTable to avoid leaks
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Entry, AppleClipboardObserver> _observers = new();

    private static void SetObserver(Entry entry, AppleClipboardObserver observer)
    {
        // Remove previous observer if re-setup occurs (e.g. handler reset)
        if (_observers.TryGetValue(entry, out var previous))
        {
            previous.Dispose();
            _observers.Remove(entry);
        }
        _observers.Add(entry, observer);
    }

    private sealed class AppleClipboardObserver : IDisposable
    {
        private readonly WeakReference<UIKit.UITextField> _textFieldRef;
        private readonly WeakReference<IClipboardSupport> _ownerRef;
        private Foundation.NSObject? _pasteboardNotification;
        private string? _previousText;
        private bool _disposed;

        public AppleClipboardObserver(UIKit.UITextField textField, Entry entry, IClipboardSupport owner)
        {
            _textFieldRef = new WeakReference<UIKit.UITextField>(textField);
            _ownerRef = new WeakReference<IClipboardSupport>(owner);
            _previousText = entry.Text;

            // Observe pasteboard changes for Copy/Cut detection
            _pasteboardNotification = Foundation.NSNotificationCenter.DefaultCenter.AddObserver(
                UIKit.UIPasteboard.ChangedNotification,
                OnPasteboardChanged);

            // Observe text changes for Paste detection
            entry.TextChanged += OnEntryTextChanged;
        }

        private void OnPasteboardChanged(Foundation.NSNotification notification)
        {
            if (!_textFieldRef.TryGetTarget(out var textField) || !textField.IsFirstResponder)
                return;

            if (!_ownerRef.TryGetTarget(out var owner))
                return;

            // Capture text before clipboard change to compare
            var textBeforeClipboard = _previousText;

            // Defer check to allow the native operation to complete
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!_textFieldRef.TryGetTarget(out var tf))
                    return;

                if (!_ownerRef.TryGetTarget(out var o))
                    return;

                var currentText = tf.Text ?? string.Empty;
                var before = textBeforeClipboard ?? string.Empty;

                if (currentText.Length < before.Length)
                {
                    // Text shortened → Cut
                    o.CutCommand?.Execute(o.GetClipboardContent());
                }
                else
                {
                    // Text unchanged or same length → Copy
                    o.CopyCommand?.Execute(o.GetClipboardContent());
                }

                _previousText = currentText;
            });
        }

        private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            var oldText = _previousText ?? string.Empty;
            var newText = e.NewTextValue ?? string.Empty;
            _previousText = newText;

            // Detect paste: text grew by more than 1 character in a single change
            if (newText.Length - oldText.Length > 1)
            {
                if (!_ownerRef.TryGetTarget(out var owner))
                    return;

                if (!_textFieldRef.TryGetTarget(out var textField) || !textField.IsFirstResponder)
                    return;

                owner.PasteCommand?.Execute(null);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_pasteboardNotification != null)
            {
                Foundation.NSNotificationCenter.DefaultCenter.RemoveObserver(_pasteboardNotification);
                _pasteboardNotification.Dispose();
                _pasteboardNotification = null;
            }
        }
    }
#endif
}
