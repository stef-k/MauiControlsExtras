using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Base;

/// <summary>
/// Base class for styled controls providing common appearance properties.
/// Includes support for theming, corner radius, borders, and shadow effects.
/// </summary>
public abstract class StyledControlBase : ContentView, IThemeAware
{
    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="AccentColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AccentColorProperty = BindableProperty.Create(
        nameof(AccentColor),
        typeof(Color),
        typeof(StyledControlBase),
        Color.FromArgb("#0078D4"),
        propertyChanged: OnAccentColorChanged);

    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius),
        typeof(double),
        typeof(StyledControlBase),
        8.0,
        propertyChanged: OnCornerRadiusChanged);

    /// <summary>
    /// Identifies the <see cref="BorderColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
        nameof(BorderColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnBorderColorChanged);

    /// <summary>
    /// Identifies the <see cref="BorderThickness"/> bindable property.
    /// </summary>
    public static readonly BindableProperty BorderThicknessProperty = BindableProperty.Create(
        nameof(BorderThickness),
        typeof(double),
        typeof(StyledControlBase),
        1.5,
        propertyChanged: OnBorderThicknessChanged);

    /// <summary>
    /// Identifies the <see cref="FocusBorderColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FocusBorderColorProperty = BindableProperty.Create(
        nameof(FocusBorderColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnFocusBorderColorChanged);

    /// <summary>
    /// Identifies the <see cref="HasShadow"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HasShadowProperty = BindableProperty.Create(
        nameof(HasShadow),
        typeof(bool),
        typeof(StyledControlBase),
        false,
        propertyChanged: OnHasShadowChanged);

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the accent color used for focus indication and selection.
    /// </summary>
    /// <value>The accent color. Default is blue (#0078D4).</value>
    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius for the control's border.
    /// </summary>
    /// <value>The corner radius value. Default is 8.</value>
    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the border color. When null, uses the theme default.
    /// </summary>
    /// <value>The border color, or null to use theme default.</value>
    public Color? BorderColor
    {
        get => (Color?)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the border thickness.
    /// </summary>
    /// <value>The border thickness. Default is 1.5.</value>
    public double BorderThickness
    {
        get => (double)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the border color when the control has focus.
    /// When null, defaults to <see cref="AccentColor"/>.
    /// </summary>
    /// <value>The focus border color, or null to use AccentColor.</value>
    public Color? FocusBorderColor
    {
        get => (Color?)GetValue(FocusBorderColorProperty);
        set => SetValue(FocusBorderColorProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the control should display a shadow effect.
    /// </summary>
    /// <value>True to show shadow, false otherwise. Default is false.</value>
    public bool HasShadow
    {
        get => (bool)GetValue(HasShadowProperty);
        set => SetValue(HasShadowProperty, value);
    }

    /// <summary>
    /// Gets the effective border color, falling back to theme default when <see cref="BorderColor"/> is null.
    /// </summary>
    public Color EffectiveBorderColor => BorderColor ?? MauiControlsExtrasTheme.GetBorderColor();

    /// <summary>
    /// Gets the effective focus border color, falling back to <see cref="AccentColor"/> when <see cref="FocusBorderColor"/> is null.
    /// </summary>
    public Color EffectiveFocusBorderColor => FocusBorderColor ?? AccentColor;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="StyledControlBase"/> class.
    /// </summary>
    protected StyledControlBase()
    {
        MauiControlsExtrasTheme.ThemeChanged += OnGlobalThemeChanged;
    }

    #endregion

    #region Property Changed Handlers

    private static void OnAccentColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnAccentColorChanged((Color)oldValue, (Color)newValue);
        }
    }

    private static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnCornerRadiusChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnBorderColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnBorderThicknessChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnBorderThicknessChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnFocusBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnFocusBorderColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnHasShadowChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnHasShadowChanged((bool)oldValue, (bool)newValue);
        }
    }

    #endregion

    #region Virtual Methods for Subclass Override

    /// <summary>
    /// Called when the <see cref="AccentColor"/> property changes.
    /// Override to respond to accent color changes.
    /// </summary>
    protected virtual void OnAccentColorChanged(Color oldValue, Color newValue) { }

    /// <summary>
    /// Called when the <see cref="CornerRadius"/> property changes.
    /// Override to respond to corner radius changes.
    /// </summary>
    protected virtual void OnCornerRadiusChanged(double oldValue, double newValue) { }

    /// <summary>
    /// Called when the <see cref="BorderColor"/> property changes.
    /// Override to respond to border color changes.
    /// </summary>
    protected virtual void OnBorderColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>
    /// Called when the <see cref="BorderThickness"/> property changes.
    /// Override to respond to border thickness changes.
    /// </summary>
    protected virtual void OnBorderThicknessChanged(double oldValue, double newValue) { }

    /// <summary>
    /// Called when the <see cref="FocusBorderColor"/> property changes.
    /// Override to respond to focus border color changes.
    /// </summary>
    protected virtual void OnFocusBorderColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>
    /// Called when the <see cref="HasShadow"/> property changes.
    /// Override to respond to shadow visibility changes.
    /// </summary>
    protected virtual void OnHasShadowChanged(bool oldValue, bool newValue) { }

    #endregion

    #region IThemeAware Implementation

    /// <summary>
    /// Called when the application theme changes.
    /// Override to implement theme-specific logic.
    /// </summary>
    /// <param name="theme">The new application theme.</param>
    public virtual void OnThemeChanged(AppTheme theme) { }

    private void OnGlobalThemeChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(EffectiveBorderColor));
        OnThemeChanged(Application.Current?.RequestedTheme ?? AppTheme.Unspecified);
    }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Called when the element is removed from the visual tree.
    /// Unsubscribes from theme change events.
    /// </summary>
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler == null)
        {
            MauiControlsExtrasTheme.ThemeChanged -= OnGlobalThemeChanged;
        }
    }

    #endregion
}
