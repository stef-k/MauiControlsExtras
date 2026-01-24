using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Base;

/// <summary>
/// Base class for navigation controls providing navigation state colors.
/// Extends <see cref="StyledControlBase"/> with navigation-specific properties.
/// </summary>
public abstract class NavigationControlBase : StyledControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="ActiveColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ActiveColorProperty = BindableProperty.Create(
        nameof(ActiveColor),
        typeof(Color),
        typeof(NavigationControlBase),
        null,
        propertyChanged: OnActiveColorChanged);

    /// <summary>
    /// Identifies the <see cref="InactiveColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty InactiveColorProperty = BindableProperty.Create(
        nameof(InactiveColor),
        typeof(Color),
        typeof(NavigationControlBase),
        null,
        propertyChanged: OnInactiveColorChanged);

    /// <summary>
    /// Identifies the <see cref="VisitedColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty VisitedColorProperty = BindableProperty.Create(
        nameof(VisitedColor),
        typeof(Color),
        typeof(NavigationControlBase),
        null,
        propertyChanged: OnVisitedColorChanged);

    /// <summary>
    /// Identifies the <see cref="DisabledNavigationColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisabledNavigationColorProperty = BindableProperty.Create(
        nameof(DisabledNavigationColor),
        typeof(Color),
        typeof(NavigationControlBase),
        null,
        propertyChanged: OnDisabledNavigationColorChanged);

    /// <summary>
    /// Identifies the <see cref="ActiveBackgroundColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ActiveBackgroundColorProperty = BindableProperty.Create(
        nameof(ActiveBackgroundColor),
        typeof(Color),
        typeof(NavigationControlBase),
        null,
        propertyChanged: OnActiveBackgroundColorChanged);

    /// <summary>
    /// Identifies the <see cref="ShowNavigationIndicator"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShowNavigationIndicatorProperty = BindableProperty.Create(
        nameof(ShowNavigationIndicator),
        typeof(bool),
        typeof(NavigationControlBase),
        true,
        propertyChanged: OnShowNavigationIndicatorChanged);

    /// <summary>
    /// Identifies the <see cref="NavigationIndicatorColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty NavigationIndicatorColorProperty = BindableProperty.Create(
        nameof(NavigationIndicatorColor),
        typeof(Color),
        typeof(NavigationControlBase),
        null,
        propertyChanged: OnNavigationIndicatorColorChanged);

    /// <summary>
    /// Identifies the <see cref="NavigationIndicatorThickness"/> bindable property.
    /// </summary>
    public static readonly BindableProperty NavigationIndicatorThicknessProperty = BindableProperty.Create(
        nameof(NavigationIndicatorThickness),
        typeof(double),
        typeof(NavigationControlBase),
        3.0,
        propertyChanged: OnNavigationIndicatorThicknessChanged);

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the color for active/current navigation items. When null, uses accent color.
    /// </summary>
    public Color? ActiveColor
    {
        get => (Color?)GetValue(ActiveColorProperty);
        set => SetValue(ActiveColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color for inactive navigation items. When null, uses a muted gray.
    /// </summary>
    public Color? InactiveColor
    {
        get => (Color?)GetValue(InactiveColorProperty);
        set => SetValue(InactiveColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color for previously visited items. When null, uses a faded accent color.
    /// </summary>
    public Color? VisitedColor
    {
        get => (Color?)GetValue(VisitedColorProperty);
        set => SetValue(VisitedColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color for disabled navigation items. When null, uses light gray.
    /// </summary>
    public Color? DisabledNavigationColor
    {
        get => (Color?)GetValue(DisabledNavigationColorProperty);
        set => SetValue(DisabledNavigationColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the background color for active navigation items. When null, uses transparent with accent tint.
    /// </summary>
    public Color? ActiveBackgroundColor
    {
        get => (Color?)GetValue(ActiveBackgroundColorProperty);
        set => SetValue(ActiveBackgroundColorProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show a visual indicator for the active item (underline, pill, etc.).
    /// </summary>
    public bool ShowNavigationIndicator
    {
        get => (bool)GetValue(ShowNavigationIndicatorProperty);
        set => SetValue(ShowNavigationIndicatorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color of the navigation indicator. When null, uses accent color.
    /// </summary>
    public Color? NavigationIndicatorColor
    {
        get => (Color?)GetValue(NavigationIndicatorColorProperty);
        set => SetValue(NavigationIndicatorColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the navigation indicator.
    /// </summary>
    public double NavigationIndicatorThickness
    {
        get => (double)GetValue(NavigationIndicatorThicknessProperty);
        set => SetValue(NavigationIndicatorThicknessProperty, value);
    }

    #endregion

    #region Effective Properties

    /// <summary>
    /// Gets the effective active color, falling back to accent color.
    /// </summary>
    public Color EffectiveActiveColor => ActiveColor ?? EffectiveAccentColor;

    /// <summary>
    /// Gets the effective inactive color, falling back to gray.
    /// </summary>
    public Color EffectiveInactiveColor => InactiveColor ?? Colors.Gray;

    /// <summary>
    /// Gets the effective visited color, falling back to a faded accent color.
    /// </summary>
    public Color EffectiveVisitedColor => VisitedColor ?? EffectiveAccentColor.WithAlpha(0.6f);

    /// <summary>
    /// Gets the effective disabled navigation color, falling back to light gray.
    /// </summary>
    public Color EffectiveDisabledNavigationColor => DisabledNavigationColor ?? Colors.LightGray;

    /// <summary>
    /// Gets the effective active background color, falling back to a subtle accent tint.
    /// </summary>
    public Color EffectiveActiveBackgroundColor => ActiveBackgroundColor ?? EffectiveAccentColor.WithAlpha(0.1f);

    /// <summary>
    /// Gets the effective navigation indicator color, falling back to accent color.
    /// </summary>
    public Color EffectiveNavigationIndicatorColor => NavigationIndicatorColor ?? EffectiveAccentColor;

    #endregion

    #region Property Changed Handlers

    private static void OnActiveColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavigationControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveActiveColor));
            control.OnActiveColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnInactiveColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavigationControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveInactiveColor));
            control.OnInactiveColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnVisitedColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavigationControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveVisitedColor));
            control.OnVisitedColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnDisabledNavigationColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavigationControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveDisabledNavigationColor));
            control.OnDisabledNavigationColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnActiveBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavigationControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveActiveBackgroundColor));
            control.OnActiveBackgroundColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnShowNavigationIndicatorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavigationControlBase control)
        {
            control.OnShowNavigationIndicatorChanged((bool)oldValue, (bool)newValue);
        }
    }

    private static void OnNavigationIndicatorColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavigationControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveNavigationIndicatorColor));
            control.OnNavigationIndicatorColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnNavigationIndicatorThicknessChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavigationControlBase control)
        {
            control.OnNavigationIndicatorThicknessChanged((double)oldValue, (double)newValue);
        }
    }

    #endregion

    #region Virtual Methods for Subclass Override

    /// <summary>Called when the <see cref="ActiveColor"/> property changes.</summary>
    protected virtual void OnActiveColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="InactiveColor"/> property changes.</summary>
    protected virtual void OnInactiveColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="VisitedColor"/> property changes.</summary>
    protected virtual void OnVisitedColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="DisabledNavigationColor"/> property changes.</summary>
    protected virtual void OnDisabledNavigationColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="ActiveBackgroundColor"/> property changes.</summary>
    protected virtual void OnActiveBackgroundColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="ShowNavigationIndicator"/> property changes.</summary>
    protected virtual void OnShowNavigationIndicatorChanged(bool oldValue, bool newValue) { }

    /// <summary>Called when the <see cref="NavigationIndicatorColor"/> property changes.</summary>
    protected virtual void OnNavigationIndicatorColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="NavigationIndicatorThickness"/> property changes.</summary>
    protected virtual void OnNavigationIndicatorThicknessChanged(double oldValue, double newValue) { }

    #endregion
}
