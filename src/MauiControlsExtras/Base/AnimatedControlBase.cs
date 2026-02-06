using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Base;

/// <summary>
/// Base class for animated controls providing animation properties.
/// Extends <see cref="StyledControlBase"/> with animation-specific properties.
/// </summary>
public abstract class AnimatedControlBase : StyledControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="AnimationDuration"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AnimationDurationProperty = BindableProperty.Create(
        nameof(AnimationDuration),
        typeof(int),
        typeof(AnimatedControlBase),
        -1,
        propertyChanged: OnAnimationDurationChanged);

    /// <summary>
    /// Identifies the <see cref="AnimationEasing"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AnimationEasingProperty = BindableProperty.Create(
        nameof(AnimationEasing),
        typeof(Easing),
        typeof(AnimatedControlBase),
        null,
        propertyChanged: OnAnimationEasingChanged);

    /// <summary>
    /// Identifies the <see cref="EnableAnimations"/> bindable property.
    /// </summary>
    public static readonly BindableProperty EnableAnimationsProperty = BindableProperty.Create(
        nameof(EnableAnimations),
        typeof(bool?),
        typeof(AnimatedControlBase),
        null,
        propertyChanged: OnEnableAnimationsChanged);

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the animation duration in milliseconds. When negative, uses theme default.
    /// </summary>
    public int AnimationDuration
    {
        get => (int)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// Gets or sets the animation easing function. When null, uses theme default.
    /// </summary>
    public Easing? AnimationEasing
    {
        get => (Easing?)GetValue(AnimationEasingProperty);
        set => SetValue(AnimationEasingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether animations are enabled. When null, uses theme default.
    /// </summary>
    public bool? EnableAnimations
    {
        get => (bool?)GetValue(EnableAnimationsProperty);
        set => SetValue(EnableAnimationsProperty, value);
    }

    #endregion

    #region Effective Properties

    /// <summary>
    /// Gets the effective animation duration in milliseconds, falling back to theme default.
    /// </summary>
    public int EffectiveAnimationDuration =>
        AnimationDuration >= 0 ? AnimationDuration : MauiControlsExtrasTheme.Current.AnimationDuration;

    /// <summary>
    /// Gets the effective animation duration as a TimeSpan.
    /// </summary>
    public TimeSpan EffectiveAnimationTimeSpan =>
        TimeSpan.FromMilliseconds(EffectiveAnimationDuration);

    /// <summary>
    /// Gets the effective animation easing, falling back to theme default.
    /// </summary>
    public Easing EffectiveAnimationEasing =>
        AnimationEasing ?? MauiControlsExtrasTheme.Current.AnimationEasing;

    /// <summary>
    /// Gets whether animations are effectively enabled, falling back to theme default.
    /// </summary>
    public bool EffectiveEnableAnimations =>
        EnableAnimations ?? MauiControlsExtrasTheme.Current.EnableAnimations;

    #endregion

    #region Theme Change

    /// <inheritdoc />
    public override void OnThemeChanged(AppTheme theme)
    {
        base.OnThemeChanged(theme);
        OnPropertyChanged(nameof(EffectiveAnimationDuration));
        OnPropertyChanged(nameof(EffectiveAnimationTimeSpan));
        OnPropertyChanged(nameof(EffectiveAnimationEasing));
        OnPropertyChanged(nameof(EffectiveEnableAnimations));
    }

    #endregion

    #region Property Changed Handlers

    private static void OnAnimationDurationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AnimatedControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveAnimationDuration));
            control.OnPropertyChanged(nameof(EffectiveAnimationTimeSpan));
            control.OnAnimationDurationChanged((int)oldValue, (int)newValue);
        }
    }

    private static void OnAnimationEasingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AnimatedControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveAnimationEasing));
            control.OnAnimationEasingChanged((Easing?)oldValue, (Easing?)newValue);
        }
    }

    private static void OnEnableAnimationsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AnimatedControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveEnableAnimations));
            control.OnEnableAnimationsChanged((bool?)oldValue, (bool?)newValue);
        }
    }

    #endregion

    #region Virtual Methods for Subclass Override

    /// <summary>Called when the <see cref="AnimationDuration"/> property changes.</summary>
    protected virtual void OnAnimationDurationChanged(int oldValue, int newValue) { }

    /// <summary>Called when the <see cref="AnimationEasing"/> property changes.</summary>
    protected virtual void OnAnimationEasingChanged(Easing? oldValue, Easing? newValue) { }

    /// <summary>Called when the <see cref="EnableAnimations"/> property changes.</summary>
    protected virtual void OnEnableAnimationsChanged(bool? oldValue, bool? newValue) { }

    #endregion

    #region Animation Helper Methods

    /// <summary>
    /// Animates a property using the control's animation settings.
    /// </summary>
    /// <param name="name">The animation name for tracking.</param>
    /// <param name="callback">The animation callback receiving values from 0 to 1.</param>
    /// <param name="finished">Optional callback when animation completes.</param>
    /// <returns>True if animation started, false if animations are disabled.</returns>
    protected bool AnimateProperty(string name, Action<double> callback, Action? finished = null)
    {
        if (!EffectiveEnableAnimations)
        {
            callback(1.0);
            finished?.Invoke();
            return false;
        }

        this.Animate(
            name,
            callback,
            length: (uint)EffectiveAnimationDuration,
            easing: EffectiveAnimationEasing,
            finished: (_, _) => finished?.Invoke());

        return true;
    }

    /// <summary>
    /// Animates from one value to another using the control's animation settings.
    /// </summary>
    /// <param name="name">The animation name for tracking.</param>
    /// <param name="start">The starting value.</param>
    /// <param name="end">The ending value.</param>
    /// <param name="callback">The animation callback receiving interpolated values.</param>
    /// <param name="finished">Optional callback when animation completes.</param>
    /// <returns>True if animation started, false if animations are disabled.</returns>
    protected bool AnimateValue(string name, double start, double end, Action<double> callback, Action? finished = null)
    {
        if (!EffectiveEnableAnimations)
        {
            callback(end);
            finished?.Invoke();
            return false;
        }

        this.Animate(
            name,
            new Animation(v => callback(v), start, end, EffectiveAnimationEasing),
            length: (uint)EffectiveAnimationDuration,
            finished: (_, _) => finished?.Invoke());

        return true;
    }

    /// <summary>
    /// Cancels a running animation by name.
    /// </summary>
    /// <param name="name">The animation name to cancel.</param>
    protected void CancelAnimation(string name)
    {
        this.AbortAnimation(name);
    }

    /// <summary>
    /// Runs an animation task asynchronously using the control's animation settings.
    /// </summary>
    /// <param name="callback">The animation callback receiving values from 0 to 1.</param>
    /// <returns>A task that completes when the animation finishes.</returns>
    protected Task AnimateAsync(Action<double> callback)
    {
        if (!EffectiveEnableAnimations)
        {
            callback(1.0);
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        var animationName = Guid.NewGuid().ToString();

        this.Animate(
            animationName,
            callback,
            length: (uint)EffectiveAnimationDuration,
            easing: EffectiveAnimationEasing,
            finished: (_, _) => tcs.TrySetResult());

        return tcs.Task;
    }

    #endregion
}
