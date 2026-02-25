using System.Diagnostics.CodeAnalysis;
using MauiControlsExtras.Behaviors;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Base;

/// <summary>
/// Base class for styled controls providing common appearance properties.
/// Includes support for theming, colors, borders, and shadow effects.
/// </summary>
public abstract class StyledControlBase : ContentView, IThemeAware
{
    private bool _isThemeSubscribed;

    #region Color Bindable Properties

    /// <summary>
    /// Identifies the <see cref="AccentColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AccentColorProperty = BindableProperty.Create(
        nameof(AccentColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnAccentColorChanged);

    /// <summary>
    /// Identifies the <see cref="ForegroundColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ForegroundColorProperty = BindableProperty.Create(
        nameof(ForegroundColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnForegroundColorChanged);

    /// <summary>
    /// Identifies the <see cref="DisabledColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisabledColorProperty = BindableProperty.Create(
        nameof(DisabledColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnDisabledColorChanged);

    /// <summary>
    /// Identifies the <see cref="ErrorColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ErrorColorProperty = BindableProperty.Create(
        nameof(ErrorColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnErrorColorChanged);

    /// <summary>
    /// Identifies the <see cref="SuccessColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SuccessColorProperty = BindableProperty.Create(
        nameof(SuccessColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnSuccessColorChanged);

    /// <summary>
    /// Identifies the <see cref="WarningColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty WarningColorProperty = BindableProperty.Create(
        nameof(WarningColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnWarningColorChanged);

    #endregion

    #region Layout Bindable Properties

    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius),
        typeof(double),
        typeof(StyledControlBase),
        -1.0,
        propertyChanged: OnCornerRadiusChanged);

    #endregion

    #region Border Bindable Properties

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
        -1.0,
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
    /// Identifies the <see cref="ErrorBorderColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ErrorBorderColorProperty = BindableProperty.Create(
        nameof(ErrorBorderColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnErrorBorderColorChanged);

    /// <summary>
    /// Identifies the <see cref="DisabledBorderColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty DisabledBorderColorProperty = BindableProperty.Create(
        nameof(DisabledBorderColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnDisabledBorderColorChanged);

    #endregion

    #region Shadow Bindable Properties

    /// <summary>
    /// Identifies the <see cref="HasShadow"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HasShadowProperty = BindableProperty.Create(
        nameof(HasShadow),
        typeof(bool),
        typeof(StyledControlBase),
        false,
        propertyChanged: OnHasShadowChanged);

    /// <summary>
    /// Identifies the <see cref="ShadowColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create(
        nameof(ShadowColor),
        typeof(Color),
        typeof(StyledControlBase),
        null,
        propertyChanged: OnShadowColorChanged);

    /// <summary>
    /// Identifies the <see cref="ShadowOffset"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShadowOffsetProperty = BindableProperty.Create(
        nameof(ShadowOffset),
        typeof(Point),
        typeof(StyledControlBase),
        new Point(0, 2),
        propertyChanged: OnShadowOffsetChanged);

    /// <summary>
    /// Identifies the <see cref="ShadowRadius"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShadowRadiusProperty = BindableProperty.Create(
        nameof(ShadowRadius),
        typeof(double),
        typeof(StyledControlBase),
        4.0,
        propertyChanged: OnShadowRadiusChanged);

    /// <summary>
    /// Identifies the <see cref="ShadowOpacity"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ShadowOpacityProperty = BindableProperty.Create(
        nameof(ShadowOpacity),
        typeof(double),
        typeof(StyledControlBase),
        0.2,
        propertyChanged: OnShadowOpacityChanged);

    /// <summary>
    /// Identifies the <see cref="Elevation"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ElevationProperty = BindableProperty.Create(
        nameof(Elevation),
        typeof(int),
        typeof(StyledControlBase),
        0,
        propertyChanged: OnElevationChanged);

    #endregion

    #region Color Properties

    /// <summary>
    /// Gets or sets the accent color used for focus indication and selection.
    /// When null, uses the theme default.
    /// </summary>
    public Color? AccentColor
    {
        get => (Color?)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground (text/icon) color.
    /// When null, uses the theme default.
    /// </summary>
    public Color? ForegroundColor
    {
        get => (Color?)GetValue(ForegroundColorProperty);
        set => SetValue(ForegroundColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color used when the control is disabled.
    /// When null, uses the theme default.
    /// </summary>
    public Color? DisabledColor
    {
        get => (Color?)GetValue(DisabledColorProperty);
        set => SetValue(DisabledColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color used for error states.
    /// When null, uses the theme default.
    /// </summary>
    public Color? ErrorColor
    {
        get => (Color?)GetValue(ErrorColorProperty);
        set => SetValue(ErrorColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color used for success states.
    /// When null, uses the theme default.
    /// </summary>
    public Color? SuccessColor
    {
        get => (Color?)GetValue(SuccessColorProperty);
        set => SetValue(SuccessColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color used for warning states.
    /// When null, uses the theme default.
    /// </summary>
    public Color? WarningColor
    {
        get => (Color?)GetValue(WarningColorProperty);
        set => SetValue(WarningColorProperty, value);
    }

    #endregion

    #region Layout Properties

    /// <summary>
    /// Gets or sets the corner radius for the control's border.
    /// When negative, uses the theme default.
    /// </summary>
    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region Border Properties

    /// <summary>
    /// Gets or sets the border color. When null, uses the theme default.
    /// </summary>
    public Color? BorderColor
    {
        get => (Color?)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the border thickness.
    /// When negative, uses the theme default.
    /// </summary>
    public double BorderThickness
    {
        get => (double)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the border color when the control has focus.
    /// When null, defaults to <see cref="EffectiveAccentColor"/>.
    /// </summary>
    public Color? FocusBorderColor
    {
        get => (Color?)GetValue(FocusBorderColorProperty);
        set => SetValue(FocusBorderColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the border color when the control is in an error state.
    /// When null, defaults to <see cref="EffectiveErrorColor"/>.
    /// </summary>
    public Color? ErrorBorderColor
    {
        get => (Color?)GetValue(ErrorBorderColorProperty);
        set => SetValue(ErrorBorderColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the border color when the control is disabled.
    /// When null, uses the theme default.
    /// </summary>
    public Color? DisabledBorderColor
    {
        get => (Color?)GetValue(DisabledBorderColorProperty);
        set => SetValue(DisabledBorderColorProperty, value);
    }

    #endregion

    #region Shadow Properties

    /// <summary>
    /// Gets or sets whether the control should display a shadow effect.
    /// </summary>
    public bool HasShadow
    {
        get => (bool)GetValue(HasShadowProperty);
        set => SetValue(HasShadowProperty, value);
    }

    /// <summary>
    /// Gets or sets the shadow color.
    /// When null, uses the theme default.
    /// </summary>
    public Color? ShadowColor
    {
        get => (Color?)GetValue(ShadowColorProperty);
        set => SetValue(ShadowColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the shadow offset.
    /// </summary>
    public Point ShadowOffset
    {
        get => (Point)GetValue(ShadowOffsetProperty);
        set => SetValue(ShadowOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the shadow blur radius.
    /// </summary>
    public double ShadowRadius
    {
        get => (double)GetValue(ShadowRadiusProperty);
        set => SetValue(ShadowRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the shadow opacity (0.0 to 1.0).
    /// </summary>
    public double ShadowOpacity
    {
        get => (double)GetValue(ShadowOpacityProperty);
        set => SetValue(ShadowOpacityProperty, value);
    }

    /// <summary>
    /// Gets or sets the Material-style elevation (0-24).
    /// Higher values create larger, softer shadows.
    /// </summary>
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    #endregion

    #region Effective Properties (with theme fallbacks)

    /// <summary>
    /// Gets the effective accent color, falling back to theme default when null.
    /// </summary>
    public Color EffectiveAccentColor => AccentColor ?? MauiControlsExtrasTheme.Current.AccentColor;

    /// <summary>
    /// Gets the effective foreground color, falling back to theme default when null.
    /// </summary>
    public Color EffectiveForegroundColor => ForegroundColor ?? MauiControlsExtrasTheme.GetForegroundColor();

    /// <summary>
    /// Gets the effective disabled color, falling back to theme default when null.
    /// </summary>
    public Color EffectiveDisabledColor => DisabledColor ?? MauiControlsExtrasTheme.Current.DisabledColor;

    /// <summary>
    /// Gets the effective error color, falling back to theme default when null.
    /// </summary>
    public Color EffectiveErrorColor => ErrorColor ?? MauiControlsExtrasTheme.Current.ErrorColor;

    /// <summary>
    /// Gets the effective success color, falling back to theme default when null.
    /// </summary>
    public Color EffectiveSuccessColor => SuccessColor ?? MauiControlsExtrasTheme.Current.SuccessColor;

    /// <summary>
    /// Gets the effective warning color, falling back to theme default when null.
    /// </summary>
    public Color EffectiveWarningColor => WarningColor ?? MauiControlsExtrasTheme.Current.WarningColor;

    /// <summary>
    /// Gets the effective corner radius, falling back to theme default when negative.
    /// </summary>
    public double EffectiveCornerRadius => CornerRadius >= 0 ? CornerRadius : MauiControlsExtrasTheme.Current.CornerRadius;

    /// <summary>
    /// Gets the effective border color, falling back to theme default when null.
    /// </summary>
    public Color EffectiveBorderColor => BorderColor ?? MauiControlsExtrasTheme.GetBorderColor();

    /// <summary>
    /// Gets the effective border thickness, falling back to theme default when negative.
    /// </summary>
    public double EffectiveBorderThickness => BorderThickness >= 0 ? BorderThickness : MauiControlsExtrasTheme.Current.BorderThickness;

    /// <summary>
    /// Gets the effective focus border color, falling back to accent color when null.
    /// </summary>
    public Color EffectiveFocusBorderColor => FocusBorderColor ?? EffectiveAccentColor;

    /// <summary>
    /// Gets the effective error border color, falling back to error color when null.
    /// </summary>
    public Color EffectiveErrorBorderColor => ErrorBorderColor ?? EffectiveErrorColor;

    /// <summary>
    /// Gets the effective disabled border color, falling back to theme default when null.
    /// </summary>
    public Color EffectiveDisabledBorderColor => DisabledBorderColor ?? MauiControlsExtrasTheme.Current.DisabledBorderColor;

    /// <summary>
    /// Gets the effective shadow color, falling back to theme default when null.
    /// </summary>
    public Color EffectiveShadowColor => ShadowColor ?? MauiControlsExtrasTheme.GetShadowColor();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="StyledControlBase"/> class.
    /// </summary>
    [DynamicDependency(nameof(EffectiveAccentColor), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveForegroundColor), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveDisabledColor), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveErrorColor), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveSuccessColor), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveWarningColor), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveCornerRadius), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveBorderColor), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveBorderThickness), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveFocusBorderColor), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveErrorBorderColor), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveDisabledBorderColor), typeof(StyledControlBase))]
    [DynamicDependency(nameof(EffectiveShadowColor), typeof(StyledControlBase))]
    protected StyledControlBase()
    {
        EnsureThemeSubscription();
        AttachKeyboardBehaviorIfNeeded();
    }

    #endregion

    #region Property Changed Handlers

    private static void OnAccentColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveAccentColor));
            control.OnPropertyChanged(nameof(EffectiveFocusBorderColor));
            control.OnAccentColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnForegroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveForegroundColor));
            control.OnForegroundColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnDisabledColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveDisabledColor));
            control.OnDisabledColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnErrorColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveErrorColor));
            control.OnPropertyChanged(nameof(EffectiveErrorBorderColor));
            control.OnErrorColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnSuccessColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveSuccessColor));
            control.OnSuccessColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnWarningColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveWarningColor));
            control.OnWarningColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveCornerRadius));
            control.OnCornerRadiusChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveBorderColor));
            control.OnBorderColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnBorderThicknessChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveBorderThickness));
            control.OnBorderThicknessChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnFocusBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveFocusBorderColor));
            control.OnFocusBorderColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnErrorBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveErrorBorderColor));
            control.OnErrorBorderColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnDisabledBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveDisabledBorderColor));
            control.OnDisabledBorderColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnHasShadowChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnHasShadowChanged((bool)oldValue, (bool)newValue);
        }
    }

    private static void OnShadowColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveShadowColor));
            control.OnShadowColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnShadowOffsetChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnShadowOffsetChanged((Point)oldValue, (Point)newValue);
        }
    }

    private static void OnShadowRadiusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnShadowRadiusChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnShadowOpacityChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnShadowOpacityChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnElevationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledControlBase control)
        {
            control.OnElevationChanged((int)oldValue, (int)newValue);
        }
    }

    #endregion

    #region Virtual Methods for Subclass Override

    /// <summary>Called when the <see cref="AccentColor"/> property changes.</summary>
    protected virtual void OnAccentColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="ForegroundColor"/> property changes.</summary>
    protected virtual void OnForegroundColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="DisabledColor"/> property changes.</summary>
    protected virtual void OnDisabledColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="ErrorColor"/> property changes.</summary>
    protected virtual void OnErrorColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="SuccessColor"/> property changes.</summary>
    protected virtual void OnSuccessColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="WarningColor"/> property changes.</summary>
    protected virtual void OnWarningColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="CornerRadius"/> property changes.</summary>
    protected virtual void OnCornerRadiusChanged(double oldValue, double newValue) { }

    /// <summary>Called when the <see cref="BorderColor"/> property changes.</summary>
    protected virtual void OnBorderColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="BorderThickness"/> property changes.</summary>
    protected virtual void OnBorderThicknessChanged(double oldValue, double newValue) { }

    /// <summary>Called when the <see cref="FocusBorderColor"/> property changes.</summary>
    protected virtual void OnFocusBorderColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="ErrorBorderColor"/> property changes.</summary>
    protected virtual void OnErrorBorderColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="DisabledBorderColor"/> property changes.</summary>
    protected virtual void OnDisabledBorderColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="HasShadow"/> property changes.</summary>
    protected virtual void OnHasShadowChanged(bool oldValue, bool newValue) { }

    /// <summary>Called when the <see cref="ShadowColor"/> property changes.</summary>
    protected virtual void OnShadowColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="ShadowOffset"/> property changes.</summary>
    protected virtual void OnShadowOffsetChanged(Point oldValue, Point newValue) { }

    /// <summary>Called when the <see cref="ShadowRadius"/> property changes.</summary>
    protected virtual void OnShadowRadiusChanged(double oldValue, double newValue) { }

    /// <summary>Called when the <see cref="ShadowOpacity"/> property changes.</summary>
    protected virtual void OnShadowOpacityChanged(double oldValue, double newValue) { }

    /// <summary>Called when the <see cref="Elevation"/> property changes.</summary>
    protected virtual void OnElevationChanged(int oldValue, int newValue) { }

    #endregion

    #region IThemeAware Implementation

    /// <summary>
    /// Called when the application theme changes.
    /// Override to implement theme-specific logic.
    /// </summary>
    public virtual void OnThemeChanged(AppTheme theme) { }

    private void OnGlobalThemeChanged(object? sender, EventArgs e)
    {
        // Notify all effective properties that may have changed
        OnPropertyChanged(nameof(EffectiveAccentColor));
        OnPropertyChanged(nameof(EffectiveForegroundColor));
        OnPropertyChanged(nameof(EffectiveDisabledColor));
        OnPropertyChanged(nameof(EffectiveErrorColor));
        OnPropertyChanged(nameof(EffectiveSuccessColor));
        OnPropertyChanged(nameof(EffectiveWarningColor));
        OnPropertyChanged(nameof(EffectiveCornerRadius));
        OnPropertyChanged(nameof(EffectiveBorderColor));
        OnPropertyChanged(nameof(EffectiveBorderThickness));
        OnPropertyChanged(nameof(EffectiveFocusBorderColor));
        OnPropertyChanged(nameof(EffectiveErrorBorderColor));
        OnPropertyChanged(nameof(EffectiveDisabledBorderColor));
        OnPropertyChanged(nameof(EffectiveShadowColor));

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
            RemoveThemeSubscription();
        }
        else
        {
            EnsureThemeSubscription();
            AttachKeyboardBehaviorIfNeeded();
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a Shadow object based on the current shadow properties.
    /// Returns null if <see cref="HasShadow"/> is false.
    /// </summary>
    protected Shadow? CreateShadow()
    {
        if (!HasShadow)
            return null;

        return new Shadow
        {
            Brush = new SolidColorBrush(EffectiveShadowColor),
            Offset = ShadowOffset,
            Radius = (float)ShadowRadius,
            Opacity = (float)ShadowOpacity
        };
    }

    /// <summary>
    /// Creates a Shadow object based on elevation (Material Design style).
    /// Higher elevation values create larger, softer shadows.
    /// </summary>
    protected Shadow? CreateElevationShadow()
    {
        if (Elevation <= 0)
            return null;

        // Material Design shadow calculation
        var offsetY = Math.Min(Elevation * 0.5, 12);
        var radius = Math.Min(Elevation * 1.5, 24);
        var opacity = Math.Max(0.1, 0.3 - (Elevation * 0.01));

        return new Shadow
        {
            Brush = new SolidColorBrush(EffectiveShadowColor),
            Offset = new Point(0, offsetY),
            Radius = (float)radius,
            Opacity = (float)opacity
        };
    }

    #endregion

    #region Private Helpers

    private void EnsureThemeSubscription()
    {
        if (_isThemeSubscribed)
        {
            return;
        }

        MauiControlsExtrasTheme.ThemeChanged += OnGlobalThemeChanged;
        _isThemeSubscribed = true;
    }

    private void RemoveThemeSubscription()
    {
        if (!_isThemeSubscribed)
        {
            return;
        }

        MauiControlsExtrasTheme.ThemeChanged -= OnGlobalThemeChanged;
        _isThemeSubscribed = false;
    }

    private void AttachKeyboardBehaviorIfNeeded()
    {
        if (this is not IKeyboardNavigable)
        {
            return;
        }

        if (Behaviors.OfType<KeyboardBehavior>().Any())
        {
            return;
        }

        Behaviors.Add(new KeyboardBehavior());
    }

    #endregion
}
