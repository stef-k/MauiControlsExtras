using MauiControlsExtras.Base;
using MauiControlsExtras.Tests.Helpers;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests.Base;

public class AnimatedControlBaseTests : ThemeTestBase
{
    #region Property Defaults

    [Fact]
    public void AnimationDuration_Default_IsNegativeOne()
    {
        var control = new TestableAnimatedControl();
        Assert.Equal(-1, control.AnimationDuration);
    }

    [Fact]
    public void AnimationEasing_Default_IsNull()
    {
        var control = new TestableAnimatedControl();
        Assert.Null(control.AnimationEasing);
    }

    [Fact]
    public void EnableAnimations_Default_IsNull()
    {
        var control = new TestableAnimatedControl();
        Assert.Null(control.EnableAnimations);
    }

    #endregion

    #region Effective Property Fallbacks

    [Fact]
    public void EffectiveAnimationDuration_WhenNegative_ReturnsThemeDefault()
    {
        var control = new TestableAnimatedControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.AnimationDuration, control.EffectiveAnimationDuration);
    }

    [Fact]
    public void EffectiveAnimationTimeSpan_ReflectsEffectiveDuration()
    {
        var control = new TestableAnimatedControl();
        Assert.Equal(TimeSpan.FromMilliseconds(250), control.EffectiveAnimationTimeSpan);
    }

    [Fact]
    public void EffectiveAnimationEasing_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableAnimatedControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.AnimationEasing, control.EffectiveAnimationEasing);
    }

    [Fact]
    public void EffectiveEnableAnimations_WhenNull_ReturnsThemeDefault()
    {
        var control = new TestableAnimatedControl();
        Assert.Equal(MauiControlsExtrasTheme.Current.EnableAnimations, control.EffectiveEnableAnimations);
    }

    #endregion

    #region Effective Property Overrides

    [Fact]
    public void EffectiveAnimationDuration_WhenSetPositive_ReturnsSetValue()
    {
        var control = new TestableAnimatedControl();
        control.AnimationDuration = 500;
        Assert.Equal(500, control.EffectiveAnimationDuration);
    }

    [Fact]
    public void EffectiveAnimationDuration_WhenSetToZero_ReturnsZero()
    {
        var control = new TestableAnimatedControl();
        control.AnimationDuration = 0;
        Assert.Equal(0, control.EffectiveAnimationDuration);
    }

    [Fact]
    public void EffectiveAnimationEasing_WhenSet_ReturnsSetValue()
    {
        var control = new TestableAnimatedControl();
        control.AnimationEasing = Easing.Linear;
        Assert.Equal(Easing.Linear, control.EffectiveAnimationEasing);
    }

    [Fact]
    public void EffectiveEnableAnimations_WhenSetFalse_ReturnsFalse()
    {
        var control = new TestableAnimatedControl();
        control.EnableAnimations = false;
        Assert.False(control.EffectiveEnableAnimations);
    }

    [Fact]
    public void EffectiveAnimationTimeSpan_WhenDurationSet_ReflectsNewValue()
    {
        var control = new TestableAnimatedControl();
        control.AnimationDuration = 100;
        Assert.Equal(TimeSpan.FromMilliseconds(100), control.EffectiveAnimationTimeSpan);
    }

    #endregion

    #region PropertyChanged Notifications

    [Fact]
    public void SettingAnimationDuration_NotifiesEffective()
    {
        var control = new TestableAnimatedControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.AnimationDuration = 300;

        Assert.Contains(nameof(AnimatedControlBase.EffectiveAnimationDuration), changed);
        Assert.Contains(nameof(AnimatedControlBase.EffectiveAnimationTimeSpan), changed);
    }

    [Fact]
    public void SettingAnimationEasing_NotifiesEffective()
    {
        var control = new TestableAnimatedControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.AnimationEasing = Easing.BounceIn;

        Assert.Contains(nameof(AnimatedControlBase.EffectiveAnimationEasing), changed);
    }

    [Fact]
    public void SettingEnableAnimations_NotifiesEffective()
    {
        var control = new TestableAnimatedControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        control.EnableAnimations = false;

        Assert.Contains(nameof(AnimatedControlBase.EffectiveEnableAnimations), changed);
    }

    #endregion

    #region Theme Change

    [Fact]
    public void ThemeChanged_EffectiveAnimationDuration_ReflectsNewTheme()
    {
        var control = new TestableAnimatedControl();

        MauiControlsExtrasTheme.ModifyCurrentTheme(t => t.AnimationDuration = 100);

        Assert.Equal(100, control.EffectiveAnimationDuration);
    }

    [Fact]
    public void ThemeChanged_WithOverriddenDuration_KeepsOverride()
    {
        var control = new TestableAnimatedControl();
        control.AnimationDuration = 500;

        MauiControlsExtrasTheme.ModifyCurrentTheme(t => t.AnimationDuration = 100);

        Assert.Equal(500, control.EffectiveAnimationDuration);
    }

    [Fact]
    public void ThemeChanged_NotifiesAllEffectiveAnimationProperties()
    {
        var control = new TestableAnimatedControl();
        var changed = new List<string>();
        control.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        MauiControlsExtrasTheme.ModifyCurrentTheme(t =>
        {
            t.AnimationDuration = 175;
            t.EnableAnimations = false;
            t.AnimationEasing = Easing.Linear;
        });

        Assert.Contains(nameof(AnimatedControlBase.EffectiveAnimationDuration), changed);
        Assert.Contains(nameof(AnimatedControlBase.EffectiveAnimationTimeSpan), changed);
        Assert.Contains(nameof(AnimatedControlBase.EffectiveAnimationEasing), changed);
        Assert.Contains(nameof(AnimatedControlBase.EffectiveEnableAnimations), changed);
    }

    #endregion
}
