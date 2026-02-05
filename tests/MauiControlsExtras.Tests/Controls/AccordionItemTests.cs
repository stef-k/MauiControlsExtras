using MauiControlsExtras.Controls;
using MauiControlsExtras.Tests.Helpers;

namespace MauiControlsExtras.Tests.Controls;

public class AccordionItemTests
{
    [Fact]
    public void Expand_WhenCollapsed_SetsIsExpandedTrue()
    {
        var item = new AccordionItem();

        item.Expand();

        Assert.True(item.IsExpanded);
    }

    [Fact]
    public void Expand_WhenAlreadyExpanded_DoesNothing()
    {
        var item = new AccordionItem { IsExpanded = true };
        var cmd = new TestCommand();
        item.ExpandCommand = cmd;

        item.Expand();

        Assert.False(cmd.WasExecuted);
    }

    [Fact]
    public void Expand_WhenDisabled_DoesNotExpand()
    {
        var item = new AccordionItem { IsEnabled = false };

        item.Expand();

        Assert.False(item.IsExpanded);
    }

    [Fact]
    public void Expand_ExecutesExpandCommand()
    {
        var item = new AccordionItem();
        var cmd = new TestCommand();
        item.ExpandCommand = cmd;

        item.Expand();

        Assert.True(cmd.WasExecuted);
        Assert.Equal(1, cmd.ExecuteCount);
    }

    [Fact]
    public void Expand_UsesExpandCommandParameter_WhenSet()
    {
        var item = new AccordionItem();
        var cmd = new TestCommand();
        item.ExpandCommand = cmd;
        item.ExpandCommandParameter = "custom-param";

        item.Expand();

        Assert.Equal("custom-param", cmd.LastParameter);
    }

    [Fact]
    public void Expand_PassesSelfAsParameter_WhenNoParameterSet()
    {
        var item = new AccordionItem();
        var cmd = new TestCommand();
        item.ExpandCommand = cmd;

        item.Expand();

        Assert.Same(item, cmd.LastParameter);
    }

    [Fact]
    public void Expand_RaisesExpandedEvent()
    {
        var item = new AccordionItem();
        var raised = false;
        item.Expanded += (_, _) => raised = true;

        item.Expand();

        Assert.True(raised);
    }

    [Fact]
    public void Collapse_WhenExpanded_SetsIsExpandedFalse()
    {
        var item = new AccordionItem { IsExpanded = true };

        item.Collapse();

        Assert.False(item.IsExpanded);
    }

    [Fact]
    public void Collapse_WhenAlreadyCollapsed_DoesNothing()
    {
        var item = new AccordionItem();
        var cmd = new TestCommand();
        item.CollapseCommand = cmd;

        item.Collapse();

        Assert.False(cmd.WasExecuted);
    }

    [Fact]
    public void Collapse_ExecutesCollapseCommand()
    {
        var item = new AccordionItem { IsExpanded = true };
        var cmd = new TestCommand();
        item.CollapseCommand = cmd;

        item.Collapse();

        Assert.True(cmd.WasExecuted);
    }

    [Fact]
    public void Collapse_UsesCollapseCommandParameter_WhenSet()
    {
        var item = new AccordionItem { IsExpanded = true };
        var cmd = new TestCommand();
        item.CollapseCommand = cmd;
        item.CollapseCommandParameter = 42;

        item.Collapse();

        Assert.Equal(42, cmd.LastParameter);
    }

    [Fact]
    public void Collapse_RaisesCollapsedEvent()
    {
        var item = new AccordionItem { IsExpanded = true };
        var raised = false;
        item.Collapsed += (_, _) => raised = true;

        item.Collapse();

        Assert.True(raised);
    }

    [Fact]
    public void Toggle_FromCollapsed_Expands()
    {
        var item = new AccordionItem();

        item.Toggle();

        Assert.True(item.IsExpanded);
    }

    [Fact]
    public void Toggle_FromExpanded_Collapses()
    {
        var item = new AccordionItem { IsExpanded = true };

        item.Toggle();

        Assert.False(item.IsExpanded);
    }

    [Fact]
    public void ExpanderIcon_ReturnsCorrectGlyph()
    {
        var item = new AccordionItem();
        Assert.Equal("▶", item.ExpanderIcon);

        item.IsExpanded = true;
        Assert.Equal("▼", item.ExpanderIcon);
    }

    [Fact]
    public void PropertyDefaults_AreCorrect()
    {
        var item = new AccordionItem();

        Assert.Null(item.Header);
        Assert.Null(item.HeaderTemplate);
        Assert.Null(item.Icon);
        Assert.True(item.IsEnabled);
        Assert.False(item.IsExpanded);
        Assert.Null(item.ExpandCommand);
        Assert.Null(item.CollapseCommand);
    }
}
