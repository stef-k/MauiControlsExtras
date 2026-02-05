using MauiControlsExtras.ContextMenu;
using MauiControlsExtras.Tests.Helpers;

namespace MauiControlsExtras.Tests.Controls;

public class ContextMenuItemCollectionTests
{
    [Fact]
    public void AddSeparator_AddsSeparatorItem()
    {
        var collection = new ContextMenuItemCollection();

        collection.AddSeparator();

        Assert.Single(collection);
        Assert.True(collection[0].IsSeparator);
    }

    [Fact]
    public void Add_WithAction_CreatesAndAddsItem()
    {
        var collection = new ContextMenuItemCollection();

        var item = collection.Add("Copy", () => { }, "📋", "Ctrl+C");

        Assert.Single(collection);
        Assert.Equal("Copy", item.Text);
        Assert.Equal("📋", item.IconGlyph);
        Assert.Equal("Ctrl+C", item.KeyboardShortcut);
    }

    [Fact]
    public void Add_WithCommand_CreatesAndAddsItem()
    {
        var collection = new ContextMenuItemCollection();
        var cmd = new TestCommand();

        var item = collection.Add("Save", cmd, "data", "💾");

        Assert.Single(collection);
        Assert.Same(cmd, item.Command);
        Assert.Equal("data", item.CommandParameter);
    }

    [Fact]
    public void AddSubMenu_CreatesSubmenuItem()
    {
        var collection = new ContextMenuItemCollection();
        var children = new[] { new ContextMenuItem { Text = "A" } };

        var item = collection.AddSubMenu("Format", children);

        Assert.Single(collection);
        Assert.True(item.HasSubItems);
        Assert.Single(item.SubItems);
    }

    [Fact]
    public void AddSubMenu_WithBuilder_PopulatesSubItems()
    {
        var collection = new ContextMenuItemCollection();

        var item = collection.AddSubMenu("Edit", sub =>
        {
            sub.Add("Cut", () => { });
            sub.Add("Copy", () => { });
        });

        Assert.Equal(2, item.SubItems.Count);
    }

    [Fact]
    public void AddRange_AddsMultipleItems()
    {
        var collection = new ContextMenuItemCollection();
        var items = new[]
        {
            new ContextMenuItem { Text = "A" },
            new ContextMenuItem { Text = "B" },
            new ContextMenuItem { Text = "C" }
        };

        collection.AddRange(items);

        Assert.Equal(3, collection.Count);
    }

    [Fact]
    public void FindByText_FindsVisibleItem()
    {
        var collection = new ContextMenuItemCollection();
        collection.Add(new ContextMenuItem { Text = "Copy", IsVisible = true });
        collection.Add(new ContextMenuItem { Text = "Paste", IsVisible = false });

        var found = collection.FindByText("Copy");
        var notFound = collection.FindByText("Paste");

        Assert.NotNull(found);
        Assert.Equal("Copy", found.Text);
        Assert.Null(notFound);
    }

    [Fact]
    public void FindByText_ReturnsNull_WhenNotFound()
    {
        var collection = new ContextMenuItemCollection();

        Assert.Null(collection.FindByText("Missing"));
    }

    [Fact]
    public void InsertSeparator_InsertsAtIndex()
    {
        var collection = new ContextMenuItemCollection
        {
            new ContextMenuItem { Text = "A" },
            new ContextMenuItem { Text = "B" }
        };

        collection.InsertSeparator(1);

        Assert.Equal(3, collection.Count);
        Assert.True(collection[1].IsSeparator);
    }

    [Fact]
    public void GetVisibleItems_FiltersInvisible()
    {
        var collection = new ContextMenuItemCollection
        {
            new ContextMenuItem { Text = "Visible", IsVisible = true },
            new ContextMenuItem { Text = "Hidden", IsVisible = false },
            new ContextMenuItem { Text = "AlsoVisible", IsVisible = true }
        };

        var visible = collection.GetVisibleItems().ToList();

        Assert.Equal(2, visible.Count);
        Assert.DoesNotContain(visible, i => i.Text == "Hidden");
    }
}
