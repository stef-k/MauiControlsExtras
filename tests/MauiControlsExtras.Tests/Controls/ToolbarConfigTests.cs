using MauiControlsExtras.Controls;

namespace MauiControlsExtras.Tests.Controls;

public class ToolbarConfigTests
{
    [Fact]
    public void Standard_HasDefaultButtons()
    {
        var config = ToolbarConfig.Standard;

        Assert.True(config.Bold);
        Assert.True(config.Italic);
        Assert.True(config.Underline);
        Assert.True(config.BulletList);
        Assert.True(config.NumberedList);
        Assert.True(config.Heading);
        Assert.True(config.Quote);
        Assert.True(config.Link);
        Assert.True(config.Undo);
        Assert.True(config.Redo);
        Assert.True(config.ClearFormatting);
        Assert.False(config.Strikethrough);
        Assert.False(config.CodeBlock);
        Assert.False(config.Image);
    }

    [Fact]
    public void Minimal_HasOnlyBoldItalicLink()
    {
        var config = ToolbarConfig.Minimal;

        Assert.True(config.Bold);
        Assert.True(config.Italic);
        Assert.True(config.Link);
        Assert.False(config.Underline);
        Assert.False(config.BulletList);
        Assert.False(config.NumberedList);
        Assert.False(config.Heading);
        Assert.False(config.Quote);
        Assert.False(config.Undo);
        Assert.False(config.Redo);
        Assert.False(config.ClearFormatting);
    }

    [Fact]
    public void Full_HasAllButtonsEnabled()
    {
        var config = ToolbarConfig.Full;

        Assert.True(config.Bold);
        Assert.True(config.Italic);
        Assert.True(config.Underline);
        Assert.True(config.Strikethrough);
        Assert.True(config.BulletList);
        Assert.True(config.NumberedList);
        Assert.True(config.Heading);
        Assert.True(config.Quote);
        Assert.True(config.CodeBlock);
        Assert.True(config.Link);
        Assert.True(config.Image);
        Assert.True(config.Undo);
        Assert.True(config.Redo);
        Assert.True(config.ClearFormatting);
    }

    [Fact]
    public void None_HasAllButtonsDisabled()
    {
        var config = ToolbarConfig.None;

        Assert.False(config.Bold);
        Assert.False(config.Italic);
        Assert.False(config.Underline);
        Assert.False(config.Strikethrough);
        Assert.False(config.BulletList);
        Assert.False(config.NumberedList);
        Assert.False(config.Heading);
        Assert.False(config.Quote);
        Assert.False(config.CodeBlock);
        Assert.False(config.Link);
        Assert.False(config.Image);
        Assert.False(config.Undo);
        Assert.False(config.Redo);
        Assert.False(config.ClearFormatting);
    }
}
