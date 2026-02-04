using MauiControlsExtras.Base;
using MauiControlsExtras.Tests.Helpers;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests.Base;

public class TextStyledControlBaseTests : ThemeTestBase
{
    #region Property Defaults

    [Fact]
    public void FontFamily_Default_IsNull()
    {
        var control = new TestableTextStyledControl();
        Assert.Null(control.FontFamily);
    }

    [Fact]
    public void FontSize_Default_IsNegativeOne()
    {
        var control = new TestableTextStyledControl();
        Assert.Equal(-1.0, control.FontSize);
    }

    [Fact]
    public void FontAttributes_Default_IsNone()
    {
        var control = new TestableTextStyledControl();
        Assert.Equal(FontAttributes.None, control.FontAttributes);
    }

    [Fact]
    public void TextColor_Default_IsNull()
    {
        var control = new TestableTextStyledControl();
        Assert.Null(control.TextColor);
    }

    [Fact]
    public void PlaceholderColor_Default_IsNull()
    {
        var control = new TestableTextStyledControl();
        Assert.Null(control.PlaceholderColor);
    }

    [Fact]
    public void TextDecorations_Default_IsNone()
    {
        var control = new TestableTextStyledControl();
        Assert.Equal(TextDecorations.None, control.TextDecorations);
    }

    [Fact]
    public void LineHeight_Default_Is1_2()
    {
        var control = new TestableTextStyledControl();
        Assert.Equal(1.2, control.LineHeight);
    }

    [Fact]
    public void CharacterSpacing_Default_IsZero()
    {
        var control = new TestableTextStyledControl();
        Assert.Equal(0.0, control.CharacterSpacing);
    }

    #endregion

    #region Effective Property Fallbacks

    [Fact]
    public void EffectiveFontFamily_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableTextStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.FontFamily, control.EffectiveFontFamily);
    }

    [Fact]
    public void EffectiveFontSize_WhenNegative_ReturnsThemeDefault()
    {
        var control = new TestableTextStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.FontSize, control.EffectiveFontSize);
    }

    [Fact]
    public void EffectiveTextColor_WhenNull_ReturnsEffectiveForegroundColor()
    {
        var control = new TestableTextStyledControl();
        Assert.Equal(control.EffectiveForegroundColor, control.EffectiveTextColor);
    }

    [Fact]
    public void EffectivePlaceholderColor_WhenNull_ReturnsLightThemeDefault()
    {
        var control = new TestableTextStyledControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.PlaceholderColorLight, control.EffectivePlaceholderColor);
    }

    #endregion

    #region Effective Property Overrides

    [Fact]
    public void EffectiveFontFamily_WhenSet_ReturnsSetValue()
    {
        var control = new TestableTextStyledControl();
        control.FontFamily = "Arial";
        Assert.Equal("Arial", control.EffectiveFontFamily);
    }

    [Fact]
    public void EffectiveFontSize_WhenSetPositive_ReturnsSetValue()
    {
        var control = new TestableTextStyledControl();
        control.FontSize = 20;
        Assert.Equal(20, control.EffectiveFontSize);
    }

    [Fact]
    public void EffectiveTextColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableTextStyledControl();
        control.TextColor = Colors.Blue;
        Assert.Equal(Colors.Blue, control.EffectiveTextColor);
    }

    [Fact]
    public void EffectivePlaceholderColor_WhenSet_ReturnsSetValue()
    {
        var control = new TestableTextStyledControl();
        control.PlaceholderColor = Colors.LightGray;
        Assert.Equal(Colors.LightGray, control.EffectivePlaceholderColor);
    }

    #endregion

    #region PropertyChanged Notifications

    [Fact]
    public void SettingFontFamily_NotifiesEffectiveFontFamily()
    {
        var control = new TestableTextStyledControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.FontFamily = "Consolas";

        Assert.Contains(nameof(TextStyledControlBase.EffectiveFontFamily), changed);
    }

    [Fact]
    public void SettingFontSize_NotifiesEffectiveFontSize()
    {
        var control = new TestableTextStyledControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.FontSize = 18;

        Assert.Contains(nameof(TextStyledControlBase.EffectiveFontSize), changed);
    }

    [Fact]
    public void SettingTextColor_NotifiesEffectiveTextColor()
    {
        var control = new TestableTextStyledControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.TextColor = Colors.Red;

        Assert.Contains(nameof(TextStyledControlBase.EffectiveTextColor), changed);
    }

    #endregion

    #region Theme Change

    [Fact]
    public void ThemeChanged_NotifiesTextEffectiveProperties()
    {
        var control = new TestableTextStyledControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        MauiControlsExtrasTheme.ApplyCompactTheme();

        Assert.Contains(nameof(TextStyledControlBase.EffectiveFontFamily), changed);
        Assert.Contains(nameof(TextStyledControlBase.EffectiveFontSize), changed);
        Assert.Contains(nameof(TextStyledControlBase.EffectiveTextColor), changed);
        Assert.Contains(nameof(TextStyledControlBase.EffectivePlaceholderColor), changed);
    }

    [Fact]
    public void ThemeChanged_EffectiveFontSize_ReflectsNewTheme()
    {
        var control = new TestableTextStyledControl();

        MauiControlsExtrasTheme.ApplyCompactTheme();

        Assert.Equal(13, control.EffectiveFontSize);
    }

    #endregion
}
