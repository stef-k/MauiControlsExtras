using MauiControlsExtras.Base;
using MauiControlsExtras.Tests.Helpers;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests.Base;

public class ListStyledControlBaseTests : ThemeTestBase
{
    #region Property Defaults

    [Fact]
    public void AlternatingRowColor_Default_IsNull()
    {
        var control = new TestableListStyledControl();
        Assert.Null(control.AlternatingRowColor);
    }

    [Fact]
    public void SelectedItemBackgroundColor_Default_IsNull()
    {
        var control = new TestableListStyledControl();
        Assert.Null(control.SelectedItemBackgroundColor);
    }

    [Fact]
    public void SelectedItemTextColor_Default_IsNull()
    {
        var control = new TestableListStyledControl();
        Assert.Null(control.SelectedItemTextColor);
    }

    [Fact]
    public void HoverColor_Default_IsNull()
    {
        var control = new TestableListStyledControl();
        Assert.Null(control.HoverColor);
    }

    [Fact]
    public void SeparatorColor_Default_IsNull()
    {
        var control = new TestableListStyledControl();
        Assert.Null(control.SeparatorColor);
    }

    [Fact]
    public void SeparatorVisibility_Default_IsTrue()
    {
        var control = new TestableListStyledControl();
        Assert.True(control.SeparatorVisibility);
    }

    [Fact]
    public void SeparatorThickness_Default_Is1()
    {
        var control = new TestableListStyledControl();
        Assert.Equal(1.0, control.SeparatorThickness);
    }

    [Fact]
    public void ItemSpacing_Default_IsZero()
    {
        var control = new TestableListStyledControl();
        Assert.Equal(0.0, control.ItemSpacing);
    }

    #endregion

    #region Effective Property Fallbacks

    [Fact]
    public void EffectiveSelectedItemBackgroundColor_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableListStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.SelectedBackgroundColor, control.EffectiveSelectedItemBackgroundColor);
    }

    [Fact]
    public void EffectiveSelectedItemTextColor_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableListStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.SelectedForegroundColor, control.EffectiveSelectedItemTextColor);
    }

    [Fact]
    public void EffectiveHoverColor_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableListStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.HoverColor, control.EffectiveHoverColor);
    }

    [Fact]
    public void EffectiveSeparatorColor_WhenNull_ReturnsBorderColorWithHalfAlpha()
    {
        var control = new TestableListStyledControl();
        var expected = control.EffectiveBorderColor.WithAlpha(0.5f);
        Assert.Equal(expected, control.EffectiveSeparatorColor);
    }

    #endregion

    #region Effective Property Overrides

    [Fact]
    public void EffectiveSelectedItemBackgroundColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableListStyledControl();
        control.SelectedItemBackgroundColor = Colors.LightBlue;
        Assert.Equal(Colors.LightBlue, control.EffectiveSelectedItemBackgroundColor);
    }

    [Fact]
    public void EffectiveSelectedItemTextColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableListStyledControl();
        control.SelectedItemTextColor = Colors.White;
        Assert.Equal(Colors.White, control.EffectiveSelectedItemTextColor);
    }

    [Fact]
    public void EffectiveHoverColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableListStyledControl();
        control.HoverColor = Colors.LightYellow;
        Assert.Equal(Colors.LightYellow, control.EffectiveHoverColor);
    }

    [Fact]
    public void EffectiveSeparatorColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableListStyledControl();
        control.SeparatorColor = Colors.DarkGray;
        Assert.Equal(Colors.DarkGray, control.EffectiveSeparatorColor);
    }

    #endregion

    #region PropertyChanged Notifications

    [Fact]
    public void SettingSelectedItemBackgroundColor_NotifiesEffective()
    {
        var control = new TestableListStyledControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.SelectedItemBackgroundColor = Colors.Blue;

        Assert.Contains(nameof(ListStyledControlBase.EffectiveSelectedItemBackgroundColor), changed);
    }

    #endregion

    #region Theme Change

    [Fact]
    public void ThemeChanged_NotifiesListEffectiveProperties()
    {
        var control = new TestableListStyledControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        MauiControlsExtrasTheme.ApplyModernTheme();

        Assert.Contains(nameof(ListStyledControlBase.EffectiveSelectedItemBackgroundColor), changed);
        Assert.Contains(nameof(ListStyledControlBase.EffectiveSelectedItemTextColor), changed);
        Assert.Contains(nameof(ListStyledControlBase.EffectiveHoverColor), changed);
        Assert.Contains(nameof(ListStyledControlBase.EffectiveSeparatorColor), changed);
    }

    #endregion
}
