using MauiControlsExtras.Base;
using MauiControlsExtras.Tests.Helpers;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests.Base;

public class NavigationControlBaseTests : ThemeTestBase
{
    #region Property Defaults

    [Fact]
    public void ActiveColor_Default_IsNull()
    {
        var control = new TestableNavigationControl();
        Assert.Null(control.ActiveColor);
    }

    [Fact]
    public void InactiveColor_Default_IsNull()
    {
        var control = new TestableNavigationControl();
        Assert.Null(control.InactiveColor);
    }

    [Fact]
    public void VisitedColor_Default_IsNull()
    {
        var control = new TestableNavigationControl();
        Assert.Null(control.VisitedColor);
    }

    [Fact]
    public void DisabledNavigationColor_Default_IsNull()
    {
        var control = new TestableNavigationControl();
        Assert.Null(control.DisabledNavigationColor);
    }

    [Fact]
    public void ActiveBackgroundColor_Default_IsNull()
    {
        var control = new TestableNavigationControl();
        Assert.Null(control.ActiveBackgroundColor);
    }

    [Fact]
    public void ShowNavigationIndicator_Default_IsTrue()
    {
        var control = new TestableNavigationControl();
        Assert.True(control.ShowNavigationIndicator);
    }

    [Fact]
    public void NavigationIndicatorColor_Default_IsNull()
    {
        var control = new TestableNavigationControl();
        Assert.Null(control.NavigationIndicatorColor);
    }

    [Fact]
    public void NavigationIndicatorThickness_Default_Is3()
    {
        var control = new TestableNavigationControl();
        Assert.Equal(3.0, control.NavigationIndicatorThickness);
    }

    #endregion

    #region Effective Property Fallbacks

    [Fact]
    public void EffectiveActiveColor_WhenNull_ReturnsEffectiveAccentColor()
    {
        var control = new TestableNavigationControl();
        Assert.Equal(control.EffectiveAccentColor, control.EffectiveActiveColor);
    }

    [Fact]
    public void EffectiveInactiveColor_WhenNull_ReturnsGray()
    {
        var control = new TestableNavigationControl();
        Assert.Equal(Colors.Gray, control.EffectiveInactiveColor);
    }

    [Fact]
    public void EffectiveVisitedColor_WhenNull_ReturnsFadedAccent()
    {
        var control = new TestableNavigationControl();
        var expected = control.EffectiveAccentColor.WithAlpha(0.6f);
        Assert.Equal(expected, control.EffectiveVisitedColor);
    }

    [Fact]
    public void EffectiveDisabledNavigationColor_WhenNull_ReturnsLightGray()
    {
        var control = new TestableNavigationControl();
        Assert.Equal(Colors.LightGray, control.EffectiveDisabledNavigationColor);
    }

    [Fact]
    public void EffectiveActiveBackgroundColor_WhenNull_ReturnsAccentTint()
    {
        var control = new TestableNavigationControl();
        var expected = control.EffectiveAccentColor.WithAlpha(0.1f);
        Assert.Equal(expected, control.EffectiveActiveBackgroundColor);
    }

    [Fact]
    public void EffectiveNavigationIndicatorColor_WhenNull_ReturnsEffectiveAccentColor()
    {
        var control = new TestableNavigationControl();
        Assert.Equal(control.EffectiveAccentColor, control.EffectiveNavigationIndicatorColor);
    }

    #endregion

    #region Effective Property Overrides

    [Fact]
    public void EffectiveActiveColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableNavigationControl();
        control.ActiveColor = Colors.Green;
        Assert.Equal(Colors.Green, control.EffectiveActiveColor);
    }

    [Fact]
    public void EffectiveInactiveColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableNavigationControl();
        control.InactiveColor = Colors.DarkGray;
        Assert.Equal(Colors.DarkGray, control.EffectiveInactiveColor);
    }

    [Fact]
    public void EffectiveNavigationIndicatorColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableNavigationControl();
        control.NavigationIndicatorColor = Colors.Orange;
        Assert.Equal(Colors.Orange, control.EffectiveNavigationIndicatorColor);
    }

    #endregion

    #region PropertyChanged Notifications

    [Fact]
    public void SettingActiveColor_NotifiesEffective()
    {
        var control = new TestableNavigationControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.ActiveColor = Colors.Red;

        Assert.Contains(nameof(NavigationControlBase.EffectiveActiveColor), changed);
    }

    #endregion

    #region Theme Change

    [Fact]
    public void ThemeChanged_NotifiesNavigationEffectiveProperties()
    {
        var control = new TestableNavigationControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        MauiControlsExtrasTheme.ApplyModernTheme();

        Assert.Contains(nameof(NavigationControlBase.EffectiveActiveColor), changed);
        Assert.Contains(nameof(NavigationControlBase.EffectiveInactiveColor), changed);
        Assert.Contains(nameof(NavigationControlBase.EffectiveVisitedColor), changed);
        Assert.Contains(nameof(NavigationControlBase.EffectiveDisabledNavigationColor), changed);
        Assert.Contains(nameof(NavigationControlBase.EffectiveActiveBackgroundColor), changed);
        Assert.Contains(nameof(NavigationControlBase.EffectiveNavigationIndicatorColor), changed);
    }

    [Fact]
    public void ThemeChanged_EffectiveActiveColor_ReflectsNewAccent()
    {
        var control = new TestableNavigationControl();

        MauiControlsExtrasTheme.ApplyModernTheme();

        Assert.Equal(ControlsTheme.Modern.AccentColor, control.EffectiveActiveColor);
    }

    #endregion
}
