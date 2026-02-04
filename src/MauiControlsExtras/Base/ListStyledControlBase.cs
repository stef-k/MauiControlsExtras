using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Base;

/// <summary>
/// Base class for list/collection controls providing selection and separator styling.
/// Extends <see cref="StyledControlBase"/> with list-specific properties.
/// </summary>
public abstract class ListStyledControlBase : StyledControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Identifies the <see cref="AlternatingRowColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty AlternatingRowColorProperty = BindableProperty.Create(
        nameof(AlternatingRowColor),
        typeof(Color),
        typeof(ListStyledControlBase),
        null,
        propertyChanged: OnAlternatingRowColorChanged);

    /// <summary>
    /// Identifies the <see cref="SelectedItemBackgroundColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedItemBackgroundColorProperty = BindableProperty.Create(
        nameof(SelectedItemBackgroundColor),
        typeof(Color),
        typeof(ListStyledControlBase),
        null,
        propertyChanged: OnSelectedItemBackgroundColorChanged);

    /// <summary>
    /// Identifies the <see cref="SelectedItemTextColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SelectedItemTextColorProperty = BindableProperty.Create(
        nameof(SelectedItemTextColor),
        typeof(Color),
        typeof(ListStyledControlBase),
        null,
        propertyChanged: OnSelectedItemTextColorChanged);

    /// <summary>
    /// Identifies the <see cref="HoverColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty HoverColorProperty = BindableProperty.Create(
        nameof(HoverColor),
        typeof(Color),
        typeof(ListStyledControlBase),
        null,
        propertyChanged: OnHoverColorChanged);

    /// <summary>
    /// Identifies the <see cref="SeparatorColor"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SeparatorColorProperty = BindableProperty.Create(
        nameof(SeparatorColor),
        typeof(Color),
        typeof(ListStyledControlBase),
        null,
        propertyChanged: OnSeparatorColorChanged);

    /// <summary>
    /// Identifies the <see cref="SeparatorVisibility"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SeparatorVisibilityProperty = BindableProperty.Create(
        nameof(SeparatorVisibility),
        typeof(bool),
        typeof(ListStyledControlBase),
        true,
        propertyChanged: OnSeparatorVisibilityChanged);

    /// <summary>
    /// Identifies the <see cref="SeparatorThickness"/> bindable property.
    /// </summary>
    public static readonly BindableProperty SeparatorThicknessProperty = BindableProperty.Create(
        nameof(SeparatorThickness),
        typeof(double),
        typeof(ListStyledControlBase),
        1.0,
        propertyChanged: OnSeparatorThicknessChanged);

    /// <summary>
    /// Identifies the <see cref="ItemSpacing"/> bindable property.
    /// </summary>
    public static readonly BindableProperty ItemSpacingProperty = BindableProperty.Create(
        nameof(ItemSpacing),
        typeof(double),
        typeof(ListStyledControlBase),
        0.0,
        propertyChanged: OnItemSpacingChanged);

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the background color for alternating rows. Null disables alternating colors.
    /// </summary>
    public Color? AlternatingRowColor
    {
        get => (Color?)GetValue(AlternatingRowColorProperty);
        set => SetValue(AlternatingRowColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the background color for selected items. When null, uses theme default.
    /// </summary>
    public Color? SelectedItemBackgroundColor
    {
        get => (Color?)GetValue(SelectedItemBackgroundColorProperty);
        set => SetValue(SelectedItemBackgroundColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the text color for selected items. When null, uses theme default.
    /// </summary>
    public Color? SelectedItemTextColor
    {
        get => (Color?)GetValue(SelectedItemTextColorProperty);
        set => SetValue(SelectedItemTextColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the background color when hovering over items. When null, uses theme default.
    /// </summary>
    public Color? HoverColor
    {
        get => (Color?)GetValue(HoverColorProperty);
        set => SetValue(HoverColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the color of separators between items. When null, uses a muted border color.
    /// </summary>
    public Color? SeparatorColor
    {
        get => (Color?)GetValue(SeparatorColorProperty);
        set => SetValue(SeparatorColorProperty, value);
    }

    /// <summary>
    /// Gets or sets whether separators between items are visible.
    /// </summary>
    public bool SeparatorVisibility
    {
        get => (bool)GetValue(SeparatorVisibilityProperty);
        set => SetValue(SeparatorVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of separators between items.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between items.
    /// </summary>
    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    #endregion

    #region Effective Properties

    /// <summary>
    /// Gets the effective selected item background color, falling back to theme default.
    /// </summary>
    public Color EffectiveSelectedItemBackgroundColor =>
        SelectedItemBackgroundColor ?? MauiControlsExtrasTheme.Current.SelectedBackgroundColor;

    /// <summary>
    /// Gets the effective selected item text color, falling back to theme default.
    /// </summary>
    public Color EffectiveSelectedItemTextColor =>
        SelectedItemTextColor ?? MauiControlsExtrasTheme.Current.SelectedForegroundColor;

    /// <summary>
    /// Gets the effective hover color, falling back to theme default.
    /// </summary>
    public Color EffectiveHoverColor =>
        HoverColor ?? MauiControlsExtrasTheme.Current.HoverColor;

    /// <summary>
    /// Gets the effective separator color, falling back to a muted border color.
    /// </summary>
    public Color EffectiveSeparatorColor =>
        SeparatorColor ?? EffectiveBorderColor.WithAlpha(0.5f);

    #endregion

    #region Theme Change

    /// <inheritdoc />
    public override void OnThemeChanged(AppTheme theme)
    {
        base.OnThemeChanged(theme);
        OnPropertyChanged(nameof(EffectiveSelectedItemBackgroundColor));
        OnPropertyChanged(nameof(EffectiveSelectedItemTextColor));
        OnPropertyChanged(nameof(EffectiveHoverColor));
        OnPropertyChanged(nameof(EffectiveSeparatorColor));
    }

    #endregion

    #region Property Changed Handlers

    private static void OnAlternatingRowColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListStyledControlBase control)
        {
            control.OnAlternatingRowColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnSelectedItemBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListStyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveSelectedItemBackgroundColor));
            control.OnSelectedItemBackgroundColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnSelectedItemTextColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListStyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveSelectedItemTextColor));
            control.OnSelectedItemTextColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnHoverColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListStyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveHoverColor));
            control.OnHoverColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnSeparatorColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListStyledControlBase control)
        {
            control.OnPropertyChanged(nameof(EffectiveSeparatorColor));
            control.OnSeparatorColorChanged((Color?)oldValue, (Color?)newValue);
        }
    }

    private static void OnSeparatorVisibilityChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListStyledControlBase control)
        {
            control.OnSeparatorVisibilityChanged((bool)oldValue, (bool)newValue);
        }
    }

    private static void OnSeparatorThicknessChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListStyledControlBase control)
        {
            control.OnSeparatorThicknessChanged((double)oldValue, (double)newValue);
        }
    }

    private static void OnItemSpacingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListStyledControlBase control)
        {
            control.OnItemSpacingChanged((double)oldValue, (double)newValue);
        }
    }

    #endregion

    #region Virtual Methods for Subclass Override

    /// <summary>Called when the <see cref="AlternatingRowColor"/> property changes.</summary>
    protected virtual void OnAlternatingRowColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="SelectedItemBackgroundColor"/> property changes.</summary>
    protected virtual void OnSelectedItemBackgroundColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="SelectedItemTextColor"/> property changes.</summary>
    protected virtual void OnSelectedItemTextColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="HoverColor"/> property changes.</summary>
    protected virtual void OnHoverColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="SeparatorColor"/> property changes.</summary>
    protected virtual void OnSeparatorColorChanged(Color? oldValue, Color? newValue) { }

    /// <summary>Called when the <see cref="SeparatorVisibility"/> property changes.</summary>
    protected virtual void OnSeparatorVisibilityChanged(bool oldValue, bool newValue) { }

    /// <summary>Called when the <see cref="SeparatorThickness"/> property changes.</summary>
    protected virtual void OnSeparatorThicknessChanged(double oldValue, double newValue) { }

    /// <summary>Called when the <see cref="ItemSpacing"/> property changes.</summary>
    protected virtual void OnItemSpacingChanged(double oldValue, double newValue) { }

    #endregion
}
