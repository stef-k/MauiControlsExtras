using MauiControlsExtras.Behaviors;
using MauiControlsExtras.Tests.Helpers;
using MauiControlsExtras.Theming;

namespace MauiControlsExtras.Tests.Behaviors;

public class HoverBehaviorTests : ThemeTestBase
{
    [Fact]
    public void HoverColor_Default_IsNull()
    {
        var behavior = new HoverBehavior();
        Assert.Null(behavior.HoverColor);
    }

    [Fact]
    public void HoverColor_CanBeSet()
    {
        var behavior = new HoverBehavior();
        behavior.HoverColor = Colors.Red;
        Assert.Equal(Colors.Red, behavior.HoverColor);
    }

    [Fact]
    public void OnAttachedTo_AddsPointerGestureRecognizer()
    {
        var view = new BoxView();
        var behavior = new HoverBehavior();

        view.Behaviors.Add(behavior);

        Assert.Single(view.GestureRecognizers);
        Assert.IsType<PointerGestureRecognizer>(view.GestureRecognizers[0]);
    }

    [Fact]
    public void OnDetachingFrom_RemovesPointerGestureRecognizer()
    {
        var view = new BoxView();
        var behavior = new HoverBehavior();

        view.Behaviors.Add(behavior);
        Assert.Single(view.GestureRecognizers);

        view.Behaviors.Remove(behavior);
        Assert.Empty(view.GestureRecognizers);
    }

    [Fact]
    public void Apply_AddsBehaviorToView()
    {
        var view = new BoxView();

        HoverBehavior.Apply(view);

        Assert.Single(view.Behaviors);
        Assert.IsType<HoverBehavior>(view.Behaviors[0]);
    }

    [Fact]
    public void Apply_WithCustomColor_SetsBehaviorHoverColor()
    {
        var view = new BoxView();
        var customColor = Colors.Green;

        HoverBehavior.Apply(view, customColor);

        var behavior = Assert.IsType<HoverBehavior>(view.Behaviors[0]);
        Assert.Equal(customColor, behavior.HoverColor);
    }

    [Fact]
    public void Apply_WithNullColor_LeavesHoverColorNull()
    {
        var view = new BoxView();

        HoverBehavior.Apply(view);

        var behavior = Assert.IsType<HoverBehavior>(view.Behaviors[0]);
        Assert.Null(behavior.HoverColor);
    }
}
