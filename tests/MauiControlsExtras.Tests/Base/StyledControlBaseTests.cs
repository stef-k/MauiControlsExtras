using MauiControlsExtras.Base;
using MauiControlsExtras.Tests.Helpers;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests.Base;

public class StyledControlBaseTests : ThemeTestBase
{
    #region Color Property Defaults

    [Fact]
    public void AccentColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.AccentColor);
    }

    [Fact]
    public void ForegroundColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.ForegroundColor);
    }

    [Fact]
    public void DisabledColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.DisabledColor);
    }

    [Fact]
    public void ErrorColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.ErrorColor);
    }

    [Fact]
    public void SuccessColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.SuccessColor);
    }

    [Fact]
    public void WarningColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.WarningColor);
    }

    #endregion

    #region Layout/Border Defaults

    [Fact]
    public void CornerRadius_Default_IsNegativeOne()
    {
        var control = new TestableStyledControl();
        Assert.Equal(-1.0, control.CornerRadius);
    }

    [Fact]
    public void BorderColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.BorderColor);
    }

    [Fact]
    public void BorderThickness_Default_IsNegativeOne()
    {
        var control = new TestableStyledControl();
        Assert.Equal(-1.0, control.BorderThickness);
    }

    [Fact]
    public void FocusBorderColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.FocusBorderColor);
    }

    [Fact]
    public void ErrorBorderColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.ErrorBorderColor);
    }

    [Fact]
    public void DisabledBorderColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.DisabledBorderColor);
    }

    #endregion

    #region Shadow Defaults

    [Fact]
    public void HasShadow_Default_IsFalse()
    {
        var control = new TestableStyledControl();
        Assert.False(control.HasShadow);
    }

    [Fact]
    public void ShadowColor_Default_IsNull()
    {
        var control = new TestableStyledControl();
        Assert.Null(control.ShadowColor);
    }

    [Fact]
    public void ShadowOffset_Default_Is0_2()
    {
        var control = new TestableStyledControl();
        Assert.Equal(new Point(0, 2), control.ShadowOffset);
    }

    [Fact]
    public void ShadowRadius_Default_Is4()
    {
        var control = new TestableStyledControl();
        Assert.Equal(4.0, control.ShadowRadius);
    }

    [Fact]
    public void ShadowOpacity_Default_Is0_2()
    {
        var control = new TestableStyledControl();
        Assert.Equal(0.2, control.ShadowOpacity);
    }

    [Fact]
    public void Elevation_Default_Is0()
    {
        var control = new TestableStyledControl();
        Assert.Equal(0, control.Elevation);
    }

    #endregion

    #region Effective Property Fallbacks (null -> theme default)

    [Fact]
    public void EffectiveAccentColor_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.AccentColor, control.EffectiveAccentColor);
    }

    [Fact]
    public void EffectiveDisabledColor_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.DisabledColor, control.EffectiveDisabledColor);
    }

    [Fact]
    public void EffectiveErrorColor_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.ErrorColor, control.EffectiveErrorColor);
    }

    [Fact]
    public void EffectiveSuccessColor_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.SuccessColor, control.EffectiveSuccessColor);
    }

    [Fact]
    public void EffectiveWarningColor_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.WarningColor, control.EffectiveWarningColor);
    }

    [Fact]
    public void EffectiveForegroundColor_WhenNull_ReturnsLightThemeDefault()
    {
        var control = new TestableStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.ForegroundColorLight, control.EffectiveForegroundColor);
    }

    [Fact]
    public void EffectiveCornerRadius_WhenNegative_ReturnsThemeDefault()
    {
        var control = new TestableStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.CornerRadius, control.EffectiveCornerRadius);
    }

    [Fact]
    public void EffectiveBorderColor_WhenNull_ReturnsLightThemeDefault()
    {
        var control = new TestableStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.BorderColorLight, control.EffectiveBorderColor);
    }

    [Fact]
    public void EffectiveBorderThickness_WhenNegative_ReturnsThemeDefault()
    {
        var control = new TestableStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.BorderThickness, control.EffectiveBorderThickness);
    }

    [Fact]
    public void EffectiveFocusBorderColor_WhenNull_ReturnsEffectiveAccentColor()
    {
        var control = new TestableStyledControl();
        Assert.Equal(control.EffectiveAccentColor, control.EffectiveFocusBorderColor);
    }

    [Fact]
    public void EffectiveErrorBorderColor_WhenNull_ReturnsEffectiveErrorColor()
    {
        var control = new TestableStyledControl();
        Assert.Equal(control.EffectiveErrorColor, control.EffectiveErrorBorderColor);
    }

    [Fact]
    public void EffectiveDisabledBorderColor_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableStyledControl();
        // Application.Current is null -> DisabledBorderColor uses Light variant
        Assert.Equal(MauiControlsExtrasTheme.Current.DisabledBorderColorLight, control.EffectiveDisabledBorderColor);
    }

    [Fact]
    public void EffectiveShadowColor_WhenNull_ReturnsLightThemeDefault()
    {
        var control = new TestableStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.ShadowColorLight, control.EffectiveShadowColor);
    }

    #endregion

    #region Effective Property Overrides (set value used instead of theme)

    [Fact]
    public void EffectiveAccentColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableStyledControl();
        control.AccentColor = Colors.Red;
        Assert.Equal(Colors.Red, control.EffectiveAccentColor);
    }

    [Fact]
    public void EffectiveErrorColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableStyledControl();
        control.ErrorColor = Colors.Fuchsia;
        Assert.Equal(Colors.Fuchsia, control.EffectiveErrorColor);
    }

    [Fact]
    public void EffectiveCornerRadius_WhenSetPositive_ReturnsSetValue()
    {
        var control = new TestableStyledControl();
        control.CornerRadius = 20;
        Assert.Equal(20, control.EffectiveCornerRadius);
    }

    [Fact]
    public void EffectiveCornerRadius_WhenSetToZero_ReturnsZero()
    {
        var control = new TestableStyledControl();
        control.CornerRadius = 0;
        Assert.Equal(0, control.EffectiveCornerRadius);
    }

    [Fact]
    public void EffectiveBorderThickness_WhenSetPositive_ReturnsSetValue()
    {
        var control = new TestableStyledControl();
        control.BorderThickness = 3;
        Assert.Equal(3, control.EffectiveBorderThickness);
    }

    [Fact]
    public void EffectiveFocusBorderColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableStyledControl();
        control.FocusBorderColor = Colors.Green;
        Assert.Equal(Colors.Green, control.EffectiveFocusBorderColor);
    }

    #endregion

    #region PropertyChanged Notifications on Direct Set

    [Fact]
    public void SettingAccentColor_NotifiesEffectiveAccentColor()
    {
        var control = new TestableStyledControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.AccentColor = Colors.Red;

        Assert.Contains(nameof(StyledControlBase.EffectiveAccentColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveFocusBorderColor), changed);
    }

    [Fact]
    public void SettingErrorColor_NotifiesEffectiveErrorColor_And_EffectiveErrorBorderColor()
    {
        var control = new TestableStyledControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.ErrorColor = Colors.Red;

        Assert.Contains(nameof(StyledControlBase.EffectiveErrorColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveErrorBorderColor), changed);
    }

    #endregion

    #region Theme Change Notifications

    [Fact]
    public void ThemeChanged_NotifiesAllEffectiveProperties()
    {
        var control = new TestableStyledControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        MauiControlsExtrasTheme.ApplyModernTheme();

        Assert.Contains(nameof(StyledControlBase.EffectiveAccentColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveForegroundColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveDisabledColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveErrorColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveSuccessColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveWarningColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveCornerRadius), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveBorderColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveBorderThickness), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveFocusBorderColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveErrorBorderColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveDisabledBorderColor), changed);
        Assert.Contains(nameof(StyledControlBase.EffectiveShadowColor), changed);
    }

    [Fact]
    public void ThemeChanged_EffectiveValues_ReflectNewTheme()
    {
        var control = new TestableStyledControl();

        MauiControlsExtrasTheme.ApplyModernTheme();

        Assert.Equal(ControlsTheme.Modern.AccentColor, control.EffectiveAccentColor);
        Assert.Equal(ControlsTheme.Modern.CornerRadius, control.EffectiveCornerRadius);
    }

    [Fact]
    public void ThemeChanged_WithOverriddenProperty_KeepsOverride()
    {
        var control = new TestableStyledControl();
        control.AccentColor = Colors.Red;

        MauiControlsExtrasTheme.ApplyModernTheme();

        Assert.Equal(Colors.Red, control.EffectiveAccentColor);
    }

    #endregion
}
