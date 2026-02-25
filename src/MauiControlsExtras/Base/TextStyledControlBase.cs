using System.Diagnostics.CodeAnalysis;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Base;

/// <summary>
/// Base class for text-containing controls providing typography properties.
/// Extends <see cref="StyledControlBase"/> with font and text styling.
/// </summary>
public abstract class TextStyledControlBase : StyledControlBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextStyledControlBase"/> class.
    /// </summary>
    [DynamicDependency(nameof(EffectiveFontFamily), typeof(TextStyledControlBase))]
    [DynamicDependency(nameof(EffectiveFontSize), typeof(TextStyledControlBase))]
    [DynamicDependency(nameof(EffectiveTextColor), typeof(TextStyledControlBase))]
    [DynamicDependency(nameof(EffectivePlaceholderColor), typeof(TextStyledControlBase))]
    protected TextStyledControlBase() { }

    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="FontFamily"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(
        nameof(FontFamily),
        typeof(string),
        typeof(TextStyledControlBase),
        null,
        propertyChanged: OnFontFamilyChanged);

    /// <summary>
    /// Identifies the <see cref="FontSize"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(TextStyledControlBase),
        -1.0,
        propertyChanged: OnFontSizeChanged);

    /// <summary>
    /// Identifies the <see cref="FontAttributes"/> bindable property.
    /// </summary>
    public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(
        nameof(FontAttributes),
        typeof(FontAttributes),
        typeof(TextStyledControlBase),
        FontAttributes.None,
        propertyChanged: OnFontAttributesChanged);

    /// <summary>
    /// Identifies the <see cref="TextColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(TextStyledControlBase),
        null,
        propertyChanged: OnTextColorChanged);

    /// <summary>
    /// Identifies the <see cref="PlaceholderColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(
        nameof(PlaceholderColor),
        typeof(Color),
        typeof(TextStyledControlBase),
        null,
        propertyChanged: OnPlaceholderColorChanged);

    /// <summary>
    /// Identifies the <see cref="TextDecorations"/> bindable property.
    /// </summary>
    public static readonly BindableProperty TextDecorationsProperty = BindableProperty.Create(
        nameof(TextDecorations),
        typeof(TextDecorations),
        typeof(TextStyledControlBase),
        TextDecorations.None,
        propertyChanged: OnTextDecorationsChanged);

    /// <summary>
    /// Identifies the <see cref="LineHeight"/> bindable property.
    /// </summary>
    public static readonly BindableProperty LineHeightProperty = BindableProperty.Create(
        nameof(LineHeight),
        typeof(double),
        typeof(TextStyledControlBase),
        1.2,
        propertyChanged: OnLineHeightChanged);

    /// <summary>
    /// Identifies the <see cref="CharacterSpacing"/> bindable property.
    /// </summary>
    public static readonly BindableProperty CharacterSpacingProperty = BindableProperty.Create(
        nameof(CharacterSpacing),
        typeof(double),
        typeof(TextStyledControlBase),
        0.0,
        propertyChanged: OnCharacterSpacingChanged);

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the font family. When null, uses the theme default.
    /// </summary>
    public string? FontFamily
    {
        get => (string?)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size. When negative, uses the theme default.
    /// </summary>
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the font attributes (bold, italic).
    /// </summary>
    public FontAttributes FontAttributes
    {
        get => (FontAttributes)GetValue(FontAttributesProperty);
        set => SetValue(FontAttributesProperty, value);
    }

    /// <summary>
    /// Gets or sets the text color. When null, uses the theme foreground color.
    /// </summary>
    public Color? TextColor
    {
        get => (Color?)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text color. When null, uses the theme default.
    /// </summary>
    public Color? PlaceholderColor
    {
        get => (Color?)GetValue(PlaceholderColorProperty);
        set => SetValue(PlaceholderColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the text decorations (underline, strikethrough).
    /// </summary>
    public TextDecorations TextDecorations
    {
        get => (TextDecorations)GetValue(TextDecorationsProperty);
        set => SetValue(TextDecorationsProperty, value);
    }

    /// <summary>
    /// Gets or sets the line height multiplier.
    /// </summary>
    public double LineHeight
    {
        get => (double)GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the character spacing.
    /// </summary>
    public double CharacterSpacing
    {
        get => (double)GetValue(CharacterSpacingProperty);
        set => SetValue(CharacterSpacingProperty, value);
    }

    #endregion

    #region Effective Properties

    /// <summary>
    /// Gets the effective font family, falling back to theme default when null.
    /// </summary>
    public string? EffectiveFontFamily => FontFamily ?? MauiControlsExtrasTheme.Current.FontFamily;

    /// <summary>
    /// Gets the effective font size, falling back to theme default when negative.
    /// </summary>
    public double EffectiveFontSize => FontSize >= 0 ? FontSize : MauiControlsExtrasTheme.Current.FontSize;

    /// <summary>
    /// Gets the effective text color, falling back to theme foreground color when null.
    /// </summary>
    public Color EffectiveTextColor => TextColor ?? EffectiveForegroundColor;

    /// <summary>
    /// Gets the effective placeholder color, falling back to theme default when null.
    /// </summary>
    public Color EffectivePlaceholderColor => PlaceholderColor ?? MauiControlsExtrasTheme.GetPlaceholderColor();

    #endregion

    #region Theme Change

    /// <inheritdoc />
    public override void OnThemeChanged(AppTheme theme)
    {
        base.OnThemeChanged(theme);
        OnPropertyChanged(nameof(EffectiveFontFamily));
        OnPropertyChanged(nameof(EffectiveFontSize));
        OnPropertyChanged(nameof(EffectiveTextColor));
        OnPropertyChanged(nameof(EffectivePlaceholderColor));
    }

    #endregion

    #region Property Changed Handlers

    private static void OnFontFamilyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TextStyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveFontFamily));
            control.OnFontFamilyChanged((string?)oldValue, (string?)newValue);
        }
    }

    private static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TextStyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveFontSize));
            control.OnFontSizeChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnFontAttributesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TextStyledControlBase control)
        {
            control.OnFontAttributesChanged((FontAttributes)oldValue, (FontAttributes)newValue);
        }
    }

    private static void OnTextColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TextStyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveTextColor));
            control.OnTextColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnPlaceholderColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TextStyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectivePlaceholderColor));
            control.OnPlaceholderColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnTextDecorationsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TextStyledControlBase control)
        {
            control.OnTextDecorationsChanged((TextDecorations)oldValue, (TextDecorations)newValue);
        }
    }

    private static void OnLineHeightChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TextStyledControlBase control)
        {
            control.OnLineHeightChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnCharacterSpacingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TextStyledControlBase control)
        {
            control.OnCharacterSpacingChanged((double)oldValue, (double)newValue);
        }
    }

    #endregion

    #region Virtual Methods for Subclass Override

    /// <summary>Called when the <see cref="FontFamily"/> property changes.</summary>
    protected virtual void OnFontFamilyChanged(string? oldValue, string? newValue) { }

    /// <summary>Called when the <see cref="FontSize"/> property changes.</summary>
    protected virtual void OnFontSizeChanged(double oldValue, double newValue) { }

    /// <summary>Called when the <see cref="FontAttributes"/> property changes.</summary>
    protected virtual void OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue) { }

    /// <summary>Called when the <see cref="TextColor"/> property changes.</summary>
    protected virtual void OnTextColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="PlaceholderColor"/> property changes.</summary>
    protected virtual void OnPlaceholderColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="TextDecorations"/> property changes.</summary>
    protected virtual void OnTextDecorationsChanged(TextDecorations oldValue, TextDecorations newValue) { }

    /// <summary>Called when the <see cref="LineHeight"/> property changes.</summary>
    protected virtual void OnLineHeightChanged(double oldValue, double newValue) { }

    /// <summary>Called when the <see cref="CharacterSpacing"/> property changes.</summary>
    protected virtual void OnCharacterSpacingChanged(double oldValue, double newValue) { }

    #endregion
}
