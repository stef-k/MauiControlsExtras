using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests.Theming;

public class ControlsThemeTests
{
    #region Default Values

    [Fact]
    public void Default_AccentColor_IsExpected()
    {
        var theme = new ControlsTheme();
        Assert.Equal(Color.FromArgb("#0078D4"), theme.AccentColor);
    }

    [Fact]
    public void Default_ErrorColor_IsExpected()
    {
        var theme = new ControlsTheme();
        Assert.Equal(Color.FromArgb("#D32F2F"), theme.ErrorColor);
    }

    [Fact]
    public void Default_SuccessColor_IsExpected()
    {
        var theme = new ControlsTheme();
        Assert.Equal(Color.FromArgb("#388E3C"), theme.SuccessColor);
    }

    [Fact]
    public void Default_WarningColor_IsExpected()
    {
        var theme = new ControlsTheme();
        Assert.Equal(Color.FromArgb("#F57C00"), theme.WarningColor);
    }

    [Fact]
    public void Default_DisabledColor_IsGray()
    {
        var theme = new ControlsTheme();
        Assert.Equal(Colors.Gray, theme.DisabledColor);
    }

    [Fact]
    public void Default_CornerRadius_Is4()
    {
        var theme = new ControlsTheme();
        Assert.Equal(4, theme.CornerRadius);
    }

    [Fact]
    public void Default_BorderThickness_Is1()
    {
        var theme = new ControlsTheme();
        Assert.Equal(1, theme.BorderThickness);
    }

    [Fact]
    public void Default_HasShadow_IsFalse()
    {
        var theme = new ControlsTheme();
        Assert.False(theme.HasShadow);
    }

    [Fact]
    public void Default_FontSize_Is14()
    {
        var theme = new ControlsTheme();
        Assert.Equal(14, theme.FontSize);
    }

    [Fact]
    public void Default_FontFamily_IsNull()
    {
        var theme = new ControlsTheme();
        Assert.Null(theme.FontFamily);
    }

    [Fact]
    public void Default_AnimationDuration_Is250()
    {
        var theme = new ControlsTheme();
        Assert.Equal(250, theme.AnimationDuration);
    }

    [Fact]
    public void Default_AnimationEasing_IsCubicInOut()
    {
        var theme = new ControlsTheme();
        Assert.Equal(Easing.CubicInOut, theme.AnimationEasing);
    }

    [Fact]
    public void Default_EnableAnimations_IsTrue()
    {
        var theme = new ControlsTheme();
        Assert.True(theme.EnableAnimations);
    }

    [Fact]
    public void Default_ForegroundColorLight_IsExpected()
    {
        var theme = new ControlsTheme();
        Assert.Equal(Color.FromArgb("#212121"), theme.ForegroundColorLight);
    }

    [Fact]
    public void Default_ForegroundColorDark_IsWhite()
    {
        var theme = new ControlsTheme();
        Assert.Equal(Color.FromArgb("#FFFFFF"), theme.ForegroundColorDark);
    }

    #endregion

    #region Predefined Themes

    [Fact]
    public void Modern_HasCornerRadius12()
    {
        var theme = ControlsTheme.Modern;
        Assert.Equal(12, theme.CornerRadius);
    }

    [Fact]
    public void Modern_HasShadow()
    {
        var theme = ControlsTheme.Modern;
        Assert.True(theme.HasShadow);
    }

    [Fact]
    public void Modern_AccentColor_IsExpected()
    {
        var theme = ControlsTheme.Modern;
        Assert.Equal(Color.FromArgb("#6200EE"), theme.AccentColor);
    }

    [Fact]
    public void Compact_HasCornerRadius2()
    {
        var theme = ControlsTheme.Compact;
        Assert.Equal(2, theme.CornerRadius);
    }

    [Fact]
    public void Compact_FontSize_Is13()
    {
        var theme = ControlsTheme.Compact;
        Assert.Equal(13, theme.FontSize);
    }

    [Fact]
    public void Compact_HasNoShadow()
    {
        var theme = ControlsTheme.Compact;
        Assert.False(theme.HasShadow);
    }

    [Fact]
    public void Fluent_AccentColor_IsExpected()
    {
        var theme = ControlsTheme.Fluent;
        Assert.Equal(Color.FromArgb("#0078D4"), theme.AccentColor);
    }

    [Fact]
    public void Material3_CornerRadius_Is12()
    {
        var theme = ControlsTheme.Material3;
        Assert.Equal(12, theme.CornerRadius);
    }

    [Fact]
    public void Material3_AccentColor_IsExpected()
    {
        var theme = ControlsTheme.Material3;
        Assert.Equal(Color.FromArgb("#6750A4"), theme.AccentColor);
    }

    [Fact]
    public void HighContrast_CornerRadius_Is0()
    {
        var theme = ControlsTheme.HighContrast;
        Assert.Equal(0, theme.CornerRadius);
    }

    [Fact]
    public void HighContrast_BorderThickness_Is2()
    {
        var theme = ControlsTheme.HighContrast;
        Assert.Equal(2, theme.BorderThickness);
    }

    #endregion

    #region Clone

    [Fact]
    public void Clone_ReturnsNewInstance()
    {
        var original = new ControlsTheme();
        var clone = original.Clone();

        Assert.NotSame(original, clone);
    }

    [Fact]
    public void Clone_PreservesValues()
    {
        var original = new ControlsTheme { CornerRadius = 99, FontSize = 20 };
        var clone = original.Clone();

        Assert.Equal(99, clone.CornerRadius);
        Assert.Equal(20, clone.FontSize);
    }

    [Fact]
    public void Clone_ModifyingClone_DoesNotAffectOriginal()
    {
        var original = new ControlsTheme();
        var clone = original.Clone();

        clone.CornerRadius = 99;

        Assert.Equal(4, original.CornerRadius);
        Assert.Equal(99, clone.CornerRadius);
    }

    #endregion

    #region Static Factories

    [Fact]
    public void Default_ReturnsNewInstanceEachTime()
    {
        var a = ControlsTheme.Default;
        var b = ControlsTheme.Default;

        Assert.NotSame(a, b);
    }

    [Fact]
    public void Modern_ReturnsNewInstanceEachTime()
    {
        var a = ControlsTheme.Modern;
        var b = ControlsTheme.Modern;

        Assert.NotSame(a, b);
    }

    #endregion
}
