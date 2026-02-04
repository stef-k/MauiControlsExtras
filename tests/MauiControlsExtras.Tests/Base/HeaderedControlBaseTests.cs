using MauiControlsExtras.Base;
using MauiControlsExtras.Tests.Helpers;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests.Base;

public class HeaderedControlBaseTests : ThemeTestBase
{
    #region Property Defaults

    [Fact]
    public void HeaderBackgroundColor_Default_IsNull()
    {
        var control = new TestableHeaderedControl();
        Assert.Null(control.HeaderBackgroundColor);
    }

    [Fact]
    public void HeaderTextColor_Default_IsNull()
    {
        var control = new TestableHeaderedControl();
        Assert.Null(control.HeaderTextColor);
    }

    [Fact]
    public void HeaderFontSize_Default_Is16()
    {
        var control = new TestableHeaderedControl();
        Assert.Equal(16.0, control.HeaderFontSize);
    }

    [Fact]
    public void HeaderFontAttributes_Default_IsBold()
    {
        var control = new TestableHeaderedControl();
        Assert.Equal(FontAttributes.Bold, control.HeaderFontAttributes);
    }

    [Fact]
    public void HeaderFontFamily_Default_IsNull()
    {
        var control = new TestableHeaderedControl();
        Assert.Null(control.HeaderFontFamily);
    }

    [Fact]
    public void HeaderPadding_Default_Is12_8()
    {
        var control = new TestableHeaderedControl();
        Assert.Equal(new Thickness(12, 8), control.HeaderPadding);
    }

    [Fact]
    public void HeaderHeight_Default_IsNegativeOne()
    {
        var control = new TestableHeaderedControl();
        Assert.Equal(-1.0, control.HeaderHeight);
    }

    [Fact]
    public void HeaderBorderColor_Default_IsNull()
    {
        var control = new TestableHeaderedControl();
        Assert.Null(control.HeaderBorderColor);
    }

    [Fact]
    public void HeaderBorderThickness_Default_IsBottomOnly()
    {
        var control = new TestableHeaderedControl();
        Assert.Equal(new Thickness(0, 0, 0, 1), control.HeaderBorderThickness);
    }

    #endregion

    #region Effective Property Fallbacks

    [Fact]
    public void EffectiveHeaderBackgroundColor_WhenNull_ReturnsSurfaceColor()
    {
        var control = new TestableHeaderedControl();
        Assert.Equal(MauiControlsExtrasTheme.GetSurfaceColor(), control.EffectiveHeaderBackgroundColor);
    }

    [Fact]
    public void EffectiveHeaderTextColor_WhenNull_ReturnsEffectiveForegroundColor()
    {
        var control = new TestableHeaderedControl();
        Assert.Equal(control.EffectiveForegroundColor, control.EffectiveHeaderTextColor);
    }

    [Fact]
    public void EffectiveHeaderFontFamily_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableHeaderedControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.FontFamily, control.EffectiveHeaderFontFamily);
    }

    [Fact]
    public void EffectiveHeaderBorderColor_WhenNull_ReturnsEffectiveBorderColor()
    {
        var control = new TestableHeaderedControl();
        Assert.Equal(control.EffectiveBorderColor, control.EffectiveHeaderBorderColor);
    }

    #endregion

    #region Effective Property Overrides

    [Fact]
    public void EffectiveHeaderBackgroundColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableHeaderedControl();
        control.HeaderBackgroundColor = Colors.Navy;
        Assert.Equal(Colors.Navy, control.EffectiveHeaderBackgroundColor);
    }

    [Fact]
    public void EffectiveHeaderTextColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableHeaderedControl();
        control.HeaderTextColor = Colors.White;
        Assert.Equal(Colors.White, control.EffectiveHeaderTextColor);
    }

    [Fact]
    public void EffectiveHeaderFontFamily_WhenSet_ReturnsSetValue()
    {
        var control = new TestableHeaderedControl();
        control.HeaderFontFamily = "Georgia";
        Assert.Equal("Georgia", control.EffectiveHeaderFontFamily);
    }

    [Fact]
    public void EffectiveHeaderBorderColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableHeaderedControl();
        control.HeaderBorderColor = Colors.DarkGray;
        Assert.Equal(Colors.DarkGray, control.EffectiveHeaderBorderColor);
    }

    #endregion

    #region PropertyChanged Notifications

    [Fact]
    public void SettingHeaderBackgroundColor_NotifiesEffective()
    {
        var control = new TestableHeaderedControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.HeaderBackgroundColor = Colors.Blue;

        Assert.Contains(nameof(HeaderedControlBase.EffectiveHeaderBackgroundColor), changed);
    }

    [Fact]
    public void SettingHeaderTextColor_NotifiesEffective()
    {
        var control = new TestableHeaderedControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.HeaderTextColor = Colors.White;

        Assert.Contains(nameof(HeaderedControlBase.EffectiveHeaderTextColor), changed);
    }

    #endregion

    #region Theme Change

    [Fact]
    public void ThemeChanged_NotifiesHeaderEffectiveProperties()
    {
        var control = new TestableHeaderedControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        MauiControlsExtrasTheme.ApplyModernTheme();

        Assert.Contains(nameof(HeaderedControlBase.EffectiveHeaderBackgroundColor), changed);
        Assert.Contains(nameof(HeaderedControlBase.EffectiveHeaderTextColor), changed);
        Assert.Contains(nameof(HeaderedControlBase.EffectiveHeaderFontFamily), changed);
        Assert.Contains(nameof(HeaderedControlBase.EffectiveHeaderBorderColor), changed);
    }

    #endregion
}
