using System.ComponentModel;
using MauiControlsExtras.Controls;

namespace MauiControlsExtras.Tests.Controls;

public class PropertyCategoryTests
{
    [Fact]
    public void Constructor_SetsNameAndProperties()
    {
        var props = Array.Empty<PropertyItem>();
        var category = new PropertyCategory("Layout", props);

        Assert.Equal("Layout", category.Name);
        Assert.Same(props, category.Properties);
    }

    [Fact]
    public void PropertyCount_ReturnsCount()
    {
        var target = new TestObj();
        var items = new[]
        {
            new PropertyItem(typeof(TestObj).GetProperty(nameof(TestObj.A))!, target),
            new PropertyItem(typeof(TestObj).GetProperty(nameof(TestObj.B))!, target)
        };
        var category = new PropertyCategory("Test", items);

        Assert.Equal(2, category.PropertyCount);
    }

    [Fact]
    public void IsExpanded_DefaultsToTrue()
    {
        var category = new PropertyCategory("Test", Array.Empty<PropertyItem>());

        Assert.True(category.IsExpanded);
    }

    [Fact]
    public void IsExpanded_RaisesPropertyChanged()
    {
        var category = new PropertyCategory("Test", Array.Empty<PropertyItem>());
        var raised = false;
        category.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PropertyCategory.IsExpanded)) raised = true;
        };

        category.IsExpanded = false;

        Assert.True(raised);
    }

    [Fact]
    public void ExpanderIcon_ReflectsState()
    {
        var category = new PropertyCategory("Test", Array.Empty<PropertyItem>());

        Assert.Equal("▼", category.ExpanderIcon); // expanded

        category.IsExpanded = false;
        Assert.Equal("▶", category.ExpanderIcon); // collapsed
    }

    [Fact]
    public void ToggleExpanded_TogglesState()
    {
        var category = new PropertyCategory("Test", Array.Empty<PropertyItem>());
        Assert.True(category.IsExpanded);

        category.ToggleExpanded();
        Assert.False(category.IsExpanded);

        category.ToggleExpanded();
        Assert.True(category.IsExpanded);
    }

    [Fact]
    public void ToggleExpanded_RaisesExpanderIconChanged()
    {
        var category = new PropertyCategory("Test", Array.Empty<PropertyItem>());
        var changed = new List<string>();
        category.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        category.ToggleExpanded();

        Assert.Contains("ExpanderIcon", changed);
    }

    private class TestObj
    {
        public string A { get; set; } = "";
        public int B { get; set; }
    }
}
