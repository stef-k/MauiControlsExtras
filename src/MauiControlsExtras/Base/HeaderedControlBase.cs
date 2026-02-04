using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Base;

/// <summary>
/// Base class for controls with headers providing header styling properties.
/// Extends <see cref="StyledControlBase"/> with header-specific properties.
/// </summary>
public abstract class HeaderedControlBase : StyledControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="HeaderBackgroundColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderBackgroundColorProperty = BindableProperty.Create(
        nameof(HeaderBackgroundColor),
        typeof(Color),
        typeof(HeaderedControlBase),
        null,
        propertyChanged: OnHeaderBackgroundColorChanged);

    /// <summary>
    /// Identifies the <see cref="HeaderTextColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderTextColorProperty = BindableProperty.Create(
        nameof(HeaderTextColor),
        typeof(Color),
        typeof(HeaderedControlBase),
        null,
        propertyChanged: OnHeaderTextColorChanged);

    /// <summary>
    /// Identifies the <see cref="HeaderFontSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderFontSizeProperty = BindableProperty.Create(
        nameof(HeaderFontSize),
        typeof(double),
        typeof(HeaderedControlBase),
        16.0,
        propertyChanged: OnHeaderFontSizeChanged);

    /// <summary>
    /// Identifies the <see cref="HeaderFontAttributes"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderFontAttributesProperty = BindableProperty.Create(
        nameof(HeaderFontAttributes),
        typeof(FontAttributes),
        typeof(HeaderedControlBase),
        FontAttributes.Bold,
        propertyChanged: OnHeaderFontAttributesChanged);

    /// <summary>
    /// Identifies the <see cref="HeaderFontFamily"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderFontFamilyProperty = BindableProperty.Create(
        nameof(HeaderFontFamily),
        typeof(string),
        typeof(HeaderedControlBase),
        null,
        propertyChanged: OnHeaderFontFamilyChanged);

    /// <summary>
    /// Identifies the <see cref="HeaderPadding"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderPaddingProperty = BindableProperty.Create(
        nameof(HeaderPadding),
        typeof(Thickness),
        typeof(HeaderedControlBase),
        new Thickness(12, 8),
        propertyChanged: OnHeaderPaddingChanged);

    /// <summary>
    /// Identifies the <see cref="HeaderHeight"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderHeightProperty = BindableProperty.Create(
        nameof(HeaderHeight),
        typeof(double),
        typeof(HeaderedControlBase),
        -1.0,
        propertyChanged: OnHeaderHeightChanged);

    /// <summary>
    /// Identifies the <see cref="HeaderBorderColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderBorderColorProperty = BindableProperty.Create(
        nameof(HeaderBorderColor),
        typeof(Color),
        typeof(HeaderedControlBase),
        null,
        propertyChanged: OnHeaderBorderColorChanged);

    /// <summary>
    /// Identifies the <see cref="HeaderBorderThickness"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HeaderBorderThicknessProperty = BindableProperty.Create(
        nameof(HeaderBorderThickness),
        typeof(Thickness),
        typeof(HeaderedControlBase),
        new Thickness(0, 0, 0, 1),
        propertyChanged: OnHeaderBorderThicknessChanged);

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the header background color. When null, uses a subtle surface color.
    /// </summary>
    public Color? HeaderBackgroundColor
    {
        get => (Color?)GetValue(HeaderBackgroundColorProperty);
        set => SetValue(HeaderBackgroundColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the header text color. When null, uses the foreground color.
    /// </summary>
    public Color? HeaderTextColor
    {
        get => (Color?)GetValue(HeaderTextColorProperty);
        set => SetValue(HeaderTextColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the header font size.
    /// </summary>
    public double HeaderFontSize
    {
        get => (double)GetValue(HeaderFontSizeProperty);
        set => SetValue(HeaderFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the header font attributes (bold, italic).
    /// </summary>
    public FontAttributes HeaderFontAttributes
    {
        get => (FontAttributes)GetValue(HeaderFontAttributesProperty);
        set => SetValue(HeaderFontAttributesProperty, value);
    }

    /// <summary>
    /// Gets or sets the header font family. When null, uses the theme default.
    /// </summary>
    public string? HeaderFontFamily
    {
        get => (string?)GetValue(HeaderFontFamilyProperty);
        set => SetValue(HeaderFontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the header internal padding.
    /// </summary>
    public Thickness HeaderPadding
    {
        get => (Thickness)GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the header height. When negative, auto-sizes based on content.
    /// </summary>
    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the header border color. When null, uses the control border color.
    /// </summary>
    public Color? HeaderBorderColor
    {
        get => (Color?)GetValue(HeaderBorderColorProperty);
        set => SetValue(HeaderBorderColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the header border thickness (typically bottom border only).
    /// </summary>
    public Thickness HeaderBorderThickness
    {
        get => (Thickness)GetValue(HeaderBorderThicknessProperty);
        set => SetValue(HeaderBorderThicknessProperty, value);
    }

    #endregion

    #region Effective Properties

    /// <summary>
    /// Gets the effective header background color, falling back to surface color.
    /// </summary>
    public Color EffectiveHeaderBackgroundColor =>
        HeaderBackgroundColor ?? MauiControlsExtrasTheme.GetSurfaceColor();

    /// <summary>
    /// Gets the effective header text color, falling back to foreground color.
    /// </summary>
    public Color EffectiveHeaderTextColor =>
        HeaderTextColor ?? EffectiveForegroundColor;

    /// <summary>
    /// Gets the effective header font family, falling back to theme default.
    /// </summary>
    public string? EffectiveHeaderFontFamily =>
        HeaderFontFamily ?? MauiControlsExtrasTheme.Current.FontFamily;

    /// <summary>
    /// Gets the effective header border color, falling back to control border color.
    /// </summary>
    public Color EffectiveHeaderBorderColor =>
        HeaderBorderColor ?? EffectiveBorderColor;

    #endregion

    #region Theme Change

    /// <inheritdoc />
    public override void OnThemeChanged(AppTheme theme)
    {
        base.OnThemeChanged(theme);
        OnPropertyChanged(nameof(EffectiveHeaderBackgroundColor));
        OnPropertyChanged(nameof(EffectiveHeaderTextColor));
        OnPropertyChanged(nameof(EffectiveHeaderFontFamily));
        OnPropertyChanged(nameof(EffectiveHeaderBorderColor));
    }

    #endregion

    #region Property Changed Handlers

    private static void OnHeaderBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeaderedControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveHeaderBackgroundColor));
            control.OnHeaderBackgroundColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnHeaderTextColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeaderedControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveHeaderTextColor));
            control.OnHeaderTextColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnHeaderFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeaderedControlBase control)
        {
            control.OnHeaderFontSizeChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnHeaderFontAttributesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeaderedControlBase control)
        {
            control.OnHeaderFontAttributesChanged((FontAttributes)oldValue, (FontAttributes)newValue);
        }
    }

    private static void OnHeaderFontFamilyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeaderedControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveHeaderFontFamily));
            control.OnHeaderFontFamilyChanged((string?)oldValue, (string?)newValue);
        }
    }

    private static void OnHeaderPaddingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeaderedControlBase control)
        {
            control.OnHeaderPaddingChanged((Thickness)oldValue, (Thickness)newValue);
        }
    }

    private static void OnHeaderHeightChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeaderedControlBase control)
        {
            control.OnHeaderHeightChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnHeaderBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeaderedControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveHeaderBorderColor));
            control.OnHeaderBorderColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnHeaderBorderThicknessChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeaderedControlBase control)
        {
            control.OnHeaderBorderThicknessChanged((Thickness)oldValue, (Thickness)newValue);
        }
    }

    #endregion

    #region Virtual Methods for Subclass Override

    /// <summary>Called when the <see cref="HeaderBackgroundColor"/> property changes.</summary>
    protected virtual void OnHeaderBackgroundColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="HeaderTextColor"/> property changes.</summary>
    protected virtual void OnHeaderTextColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="HeaderFontSize"/> property changes.</summary>
    protected virtual void OnHeaderFontSizeChanged(double oldValue, double newValue) { }

    /// <summary>Called when the <see cref="HeaderFontAttributes"/> property changes.</summary>
    protected virtual void OnHeaderFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) { }

    /// <summary>Called when the <see cref="HeaderFontFamily"/> property changes.</summary>
    protected virtual void OnHeaderFontFamilyChanged(string? oldValue, string? newValue) { }

    /// <summary>Called when the <see cref="HeaderPadding"/> property changes.</summary>
    protected virtual void OnHeaderPaddingChanged(Thickness oldValue, Thickness newValue) { }

    /// <summary>Called when the <see cref="HeaderHeight"/> property changes.</summary>
    protected virtual void OnHeaderHeightChanged(double oldValue, double newValue) { }

    /// <summary>Called when the <see cref="HeaderBorderColor"/> property changes.</summary>
    protected virtual void OnHeaderBorderColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="HeaderBorderThickness"/> property changes.</summary>
    protected virtual void OnHeaderBorderThicknessChanged(Thickness oldValue, Thickness newValue) { }

    #endregion
}
