using MauiControlsExtras.Tests.Helpers;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests.Theming;

public class MauiControlsExtrasThemeTests : ThemeTestBase
{
    #region Current

    [Fact]
    public void Current_Default_IsNotNull()
    {
        Assert.NotNull(MauiControlsExtrasTheme.Current);
    }

    [Fact]
    public void Current_SetToNull_ResetsToDefault()
    {
        MauiControlsExtrasTheme.Current = null!;

        Assert.NotNull(MauiControlsExtrasTheme.Current);
        Assert.Equal(4, MauiControlsExtrasTheme.Current.CornerRadius);
    }

    [Fact]
    public void Current_SetToCustom_UpdatesValue()
    {
        var custom = new ControlsTheme { CornerRadius = 99 };
        MauiControlsExtrasTheme.Current = custom;

        Assert.Same(custom, MauiControlsExtrasTheme.Current);
    }

    #endregion

    #region ThemeChanged Event

    [Fact]
    public void SettingCurrent_RaisesThemeChanged()
    {
        var raised = false;
        MauiControlsExtrasTheme.ThemeChanged += OnChanged;

        try
        {
            MauiControlsExtrasTheme.Current = ControlsTheme.Modern;
            Assert.True(raised);
        }
        finally
        {
            MauiControlsExtrasTheme.ThemeChanged -= OnChanged;
        }

        void OnChanged(object? sender, EventArgs e) => raised = true;
    }

    [Fact]
    public void RaiseThemeChanged_RaisesEvent()
    {
        var count = 0;
        MauiControlsExtrasTheme.ThemeChanged += OnChanged;

        try
        {
            MauiControlsExtrasTheme.RaiseThemeChanged();
            Assert.Equal(1, count);

            MauiControlsExtrasTheme.RaiseThemeChanged();
            Assert.Equal(2, count);
        }
        finally
        {
            MauiControlsExtrasTheme.ThemeChanged -= OnChanged;
        }

        void OnChanged(object? sender, EventArgs e) => count++;
    }

    #endregion

    #region Apply / Reset

    [Fact]
    public void ApplyTheme_SetsCurrentToGivenTheme()
    {
        var theme = ControlsTheme.Compact;
        MauiControlsExtrasTheme.ApplyTheme(theme);

        Assert.Same(theme, MauiControlsExtrasTheme.Current);
    }

    [Fact]
    public void ResetToDefault_RestoresDefaultValues()
    {
        MauiControlsExtrasTheme.ApplyModernTheme();
        MauiControlsExtrasTheme.ResetToDefault();

        Assert.Equal(4, MauiControlsExtrasTheme.Current.CornerRadius);
        Assert.False(MauiControlsExtrasTheme.Current.HasShadow);
    }

    [Fact]
    public void ApplyModernTheme_SetsModernValues()
    {
        MauiControlsExtrasTheme.ApplyModernTheme();

        Assert.Equal(12, MauiControlsExtrasTheme.Current.CornerRadius);
        Assert.True(MauiControlsExtrasTheme.Current.HasShadow);
    }

    [Fact]
    public void ApplyCompactTheme_SetsCompactValues()
    {
        MauiControlsExtrasTheme.ApplyCompactTheme();

        Assert.Equal(2, MauiControlsExtrasTheme.Current.CornerRadius);
        Assert.Equal(13, MauiControlsExtrasTheme.Current.FontSize);
    }

    [Fact]
    public void ApplyFluentTheme_SetsFluentValues()
    {
        MauiControlsExtrasTheme.ApplyFluentTheme();

        Assert.Equal(Color.FromArgb("#0078D4"), MauiControlsExtrasTheme.Current.AccentColor);
    }

    [Fact]
    public void ApplyMaterial3Theme_SetsMaterial3Values()
    {
        MauiControlsExtrasTheme.ApplyMaterial3Theme();

        Assert.Equal(Color.FromArgb("#6750A4"), MauiControlsExtrasTheme.Current.AccentColor);
        Assert.Equal(12, MauiControlsExtrasTheme.Current.CornerRadius);
    }

    [Fact]
    public void ApplyHighContrastTheme_SetsHighContrastValues()
    {
        MauiControlsExtrasTheme.ApplyHighContrastTheme();

        Assert.Equal(0, MauiControlsExtrasTheme.Current.CornerRadius);
        Assert.Equal(2, MauiControlsExtrasTheme.Current.BorderThickness);
    }

    #endregion

    #region Quick Access Properties

    [Fact]
    public void DefaultAccentColor_ReflectsCurrent()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.AccentColor, MauiControlsExtrasTheme.DefaultAccentColor);
    }

    [Fact]
    public void DefaultCornerRadius_ReflectsCurrent()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.CornerRadius, MauiControlsExtrasTheme.DefaultCornerRadius);
    }

    [Fact]
    public void DefaultFontSize_ReflectsCurrent()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.FontSize, MauiControlsExtrasTheme.DefaultFontSize);
    }

    [Fact]
    public void DefaultAnimationDuration_ReflectsCurrent()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.AnimationDuration, MauiControlsExtrasTheme.DefaultAnimationDuration);
    }

    #endregion

    #region Customization Helpers

    [Fact]
    public void CreateCustomTheme_ReturnsModifiedClone()
    {
        var custom = MauiControlsExtrasTheme.CreateCustomTheme(t => t.CornerRadius = 50);

        Assert.Equal(50, custom.CornerRadius);
        Assert.Equal(4, MauiControlsExtrasTheme.Current.CornerRadius);
    }

    [Fact]
    public void ModifyCurrentTheme_ModifiesInPlace_AndRaisesEvent()
    {
        var raised = false;
        MauiControlsExtrasTheme.ThemeChanged += OnChanged;

        try
        {
            MauiControlsExtrasTheme.ModifyCurrentTheme(t => t.CornerRadius = 77);

            Assert.Equal(77, MauiControlsExtrasTheme.Current.CornerRadius);
            Assert.True(raised);
        }
        finally
        {
            MauiControlsExtrasTheme.ThemeChanged -= OnChanged;
        }

        void OnChanged(object? sender, EventArgs e) => raised = true;
    }

    #endregion

    #region Theme-Aware Color Helpers (Application.Current is null in tests -> Light variant)

    [Fact]
    public void GetForegroundColor_WithNullApp_ReturnsLightVariant()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.ForegroundColorLight, MauiControlsExtrasTheme.GetForegroundColor());
    }

    [Fact]
    public void GetBorderColor_WithNullApp_ReturnsLightVariant()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.BorderColorLight, MauiControlsExtrasTheme.GetBorderColor());
    }

    [Fact]
    public void GetBackgroundColor_WithNullApp_ReturnsLightVariant()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.BackgroundColorLight, MauiControlsExtrasTheme.GetBackgroundColor());
    }

    [Fact]
    public void GetSurfaceColor_WithNullApp_ReturnsLightVariant()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.SurfaceColorLight, MauiControlsExtrasTheme.GetSurfaceColor());
    }

    [Fact]
    public void GetPlaceholderColor_WithNullApp_ReturnsLightVariant()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.PlaceholderColorLight, MauiControlsExtrasTheme.GetPlaceholderColor());
    }

    [Fact]
    public void GetShadowColor_WithNullApp_ReturnsLightVariant()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.ShadowColorLight, MauiControlsExtrasTheme.GetShadowColor());
    }

    [Fact]
    public void GetDisabledBorderColor_WithNullApp_ReturnsLightVariant()
    {
        Assert.Equal(MauiControlsExtrasTheme.Current.DisabledBorderColorLight, MauiControlsExtrasTheme.GetDisabledBorderColor());
    }

    #endregion
}
