using MauiControlsExtras.ContextMenu;
using MauiControlsExtras.Tests.Helpers;

namespace MauiControlsExtras.Tests.Controls;

public class ContextMenuItemTests
{
    [Fact]
    public void Execute_WithCommand_ExecutesCommand()
    {
        var cmd = new TestCommand();
        var item = new ContextMenuItem { Command = cmd };

        item.Execute();

        Assert.True(cmd.WasExecuted);
    }

    [Fact]
    public void Execute_WithCommandParameter_PassesParameter()
    {
        var cmd = new TestCommand();
        var item = new ContextMenuItem { Command = cmd, CommandParameter = "test" };

        item.Execute();

        Assert.Equal("test", cmd.LastParameter);
    }

    [Fact]
    public void Execute_WithAction_InvokesAction()
    {
        var invoked = false;
        var item = new ContextMenuItem { Action = () => invoked = true };

        item.Execute();

        Assert.True(invoked);
    }

    [Fact]
    public void Execute_WhenDisabled_DoesNotExecute()
    {
        var cmd = new TestCommand();
        var item = new ContextMenuItem { Command = cmd, IsEnabled = false };

        item.Execute();

        Assert.False(cmd.WasExecuted);
    }

    [Fact]
    public void Execute_WithCommandAndAction_PrefersCommand()
    {
        var cmdExecuted = false;
        var actionExecuted = false;
        var cmd = new TestCommand(_ => cmdExecuted = true);
        var item = new ContextMenuItem
        {
            Command = cmd,
            Action = () => actionExecuted = true
        };

        item.Execute();

        Assert.True(cmdExecuted);
        Assert.False(actionExecuted);
    }

    [Fact]
    public void Execute_WithCanExecuteFalse_FallsToAction()
    {
        var actionExecuted = false;
        var cmd = new TestCommand(canExecute: _ => false);
        var item = new ContextMenuItem
        {
            Command = cmd,
            Action = () => actionExecuted = true
        };

        item.Execute();

        Assert.True(actionExecuted);
    }

    [Fact]
    public void CanExecute_WhenDisabled_ReturnsFalse()
    {
        var cmd = new TestCommand();
        var item = new ContextMenuItem { Command = cmd, IsEnabled = false };

        Assert.False(item.CanExecute());
    }

    [Fact]
    public void CanExecute_WithCommand_DelegatesToCommand()
    {
        var cmd = new TestCommand(canExecute: _ => true);
        var item = new ContextMenuItem { Command = cmd };

        Assert.True(item.CanExecute());
    }

    [Fact]
    public void CanExecute_WithAction_ReturnsTrue()
    {
        var item = new ContextMenuItem { Action = () => { } };

        Assert.True(item.CanExecute());
    }

    [Fact]
    public void CanExecute_WithSubItems_ReturnsTrue()
    {
        var item = new ContextMenuItem();
        item.SubItems.Add(new ContextMenuItem { Text = "Child" });

        Assert.True(item.CanExecute());
    }

    [Fact]
    public void CanExecute_WithNothing_ReturnsFalse()
    {
        var item = new ContextMenuItem();

        Assert.False(item.CanExecute());
    }

    [Fact]
    public void Separator_Factory_CreatesSeparator()
    {
        var sep = ContextMenuItem.Separator();

        Assert.True(sep.IsSeparator);
    }

    [Fact]
    public void Create_WithAction_SetsProperties()
    {
        var item = ContextMenuItem.Create("Copy", () => { }, "📋", "Ctrl+C");

        Assert.Equal("Copy", item.Text);
        Assert.NotNull(item.Action);
        Assert.Equal("📋", item.IconGlyph);
        Assert.Equal("Ctrl+C", item.KeyboardShortcut);
    }

    [Fact]
    public void Create_WithCommand_SetsProperties()
    {
        var cmd = new TestCommand();
        var item = ContextMenuItem.Create("Save", cmd, "data", "💾", "Ctrl+S");

        Assert.Equal("Save", item.Text);
        Assert.Same(cmd, item.Command);
        Assert.Equal("data", item.CommandParameter);
        Assert.Equal("💾", item.IconGlyph);
        Assert.Equal("Ctrl+S", item.KeyboardShortcut);
    }

    [Fact]
    public void CreateSubMenu_CreatesItemWithSubItems()
    {
        var children = new[]
        {
            new ContextMenuItem { Text = "A" },
            new ContextMenuItem { Text = "B" }
        };

        var item = ContextMenuItem.CreateSubMenu("Format", children, "✎");

        Assert.Equal("Format", item.Text);
        Assert.Equal("✎", item.IconGlyph);
        Assert.True(item.HasSubItems);
        Assert.Equal(2, item.SubItems.Count);
    }

    [Fact]
    public void PropertyDefaults_AreCorrect()
    {
        var item = new ContextMenuItem();

        Assert.Null(item.Text);
        Assert.Null(item.Icon);
        Assert.Null(item.IconGlyph);
        Assert.Null(item.Command);
        Assert.Null(item.CommandParameter);
        Assert.True(item.IsEnabled);
        Assert.True(item.IsVisible);
        Assert.False(item.IsSeparator);
        Assert.Null(item.KeyboardShortcut);
        Assert.False(item.HasSubItems);
    }
}
