using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Behaviors;

/// <summary>
/// A behavior that provides hover feedback on desktop platforms (Windows/macOS).
/// Attach this behavior to any View to show a subtle background highlight on pointer hover.
/// Uses <see cref="PointerGestureRecognizer"/> which is a no-op on touch-only platforms.
/// </summary>
public class HoverBehavior : Behavior<View>
{
    private View? _attachedView;
    private PointerGestureRecognizer? _recognizer;
    private Color? _originalBackgroundColor;

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="HoverColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HoverColorProperty =
        BindableProperty.Create(
            nameof(HoverColor),
            typeof(Color),
            typeof(HoverBehavior),
            null);

    /// <summary>
    /// Gets or sets the color to apply on hover.
    /// When null, falls back to <see cref="MauiControlsExtrasTheme.Current"/>.<see cref="ControlsTheme.HoverColor"/>.
    /// </summary>
    public Color? HoverColor
    {
        get => (Color?)GetValue(HoverColorProperty);
        set => SetValue(HoverColorProperty, value);
    }

    #endregion

    /// <summary>
    /// Gets the effective hover color, resolving the theme fallback.
    /// </summary>
    private Color EffectiveHoverColor =>
        HoverColor ?? MauiControlsExtrasTheme.Current.HoverColor;

    /// <inheritdoc/>
    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        _attachedView = bindable;

        _recognizer = new PointerGestureRecognizer();
        _recognizer.PointerEntered += OnPointerEntered;
        _recognizer.PointerExited += OnPointerExited;
        bindable.GestureRecognizers.Add(_recognizer);
    }

    /// <inheritdoc/>
    protected override void OnDetachingFrom(View bindable)
    {
        if (_recognizer != null)
        {
            _recognizer.PointerEntered -= OnPointerEntered;
            _recognizer.PointerExited -= OnPointerExited;
            bindable.GestureRecognizers.Remove(_recognizer);
            _recognizer = null;
        }

        _originalBackgroundColor = null;
        _attachedView = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (_attachedView == null) return;

        _originalBackgroundColor = _attachedView.BackgroundColor;
        _attachedView.BackgroundColor = EffectiveHoverColor;
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (_attachedView == null) return;

        _attachedView.BackgroundColor = _originalBackgroundColor;
        _originalBackgroundColor = null;
    }

    /// <summary>
    /// Convenience method to apply a <see cref="HoverBehavior"/> to a view in a single call.
    /// </summary>
    /// <param name="view">The view to attach hover feedback to.</param>
    /// <param name="hoverColor">Optional custom hover color. When null, uses theme default.</param>
    public static void Apply(View view, Color? hoverColor = null)
    {
        var behavior = new HoverBehavior();
        if (hoverColor != null)
        {
            behavior.HoverColor = hoverColor;
        }
        view.Behaviors.Add(behavior);
    }
}
