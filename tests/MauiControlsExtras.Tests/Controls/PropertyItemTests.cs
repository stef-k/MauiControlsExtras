using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MauiControlsExtras.Controls;

namespace MauiControlsExtras.Tests.Controls;

public class PropertyItemTests
{
    [Fact]
    public void Constructor_ReadsPropertyValue()
    {
        var target = new SampleObject { Name = "Hello" };
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));

        Assert.Equal("Hello", item.Value);
    }

    [Fact]
    public void Constructor_ReadsDisplayName()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.DisplayNameProp));

        Assert.Equal("Friendly Name", item.DisplayName);
    }

    [Fact]
    public void Constructor_FallsBackToPropertyName_WhenNoDisplayNameAttr()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));

        Assert.Equal("Name", item.DisplayName);
    }

    [Fact]
    public void Constructor_ReadsDescription()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.DisplayNameProp));

        Assert.Equal("A test description", item.Description);
    }

    [Fact]
    public void Constructor_ReadsCategory()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.CategorizedProp));

        Assert.Equal("Layout", item.Category);
    }

    [Fact]
    public void Constructor_DefaultsCategory_ToMisc()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));

        Assert.Equal("Misc", item.Category);
    }

    [Fact]
    public void Constructor_DetectsReadOnly()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.ReadOnlyProp));

        Assert.True(item.IsReadOnly);
    }

    [Fact]
    public void Constructor_ReadsRange()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.RangeProp));

        Assert.Equal(0, item.Minimum);
        Assert.Equal(100, item.Maximum);
    }

    [Fact]
    public void Value_Set_UpdatesTargetObject()
    {
        var target = new SampleObject { Name = "Old" };
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));

        item.Value = "New";

        Assert.Equal("New", target.Name);
    }

    [Fact]
    public void Value_Set_RaisesValueChangedEvent()
    {
        var target = new SampleObject { Name = "Old" };
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));
        PropertyValueChangedEventArgs? args = null;
        item.ValueChanged += (_, e) => args = e;

        item.Value = "New";

        Assert.NotNull(args);
        Assert.Equal("Old", args.OldValue);
        Assert.Equal("New", args.NewValue);
    }

    [Fact]
    public void Value_Set_RaisesPropertyChanged()
    {
        var target = new SampleObject { Name = "Old" };
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));
        var changed = new List<string>();
        item.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        item.Value = "New";

        Assert.Contains("Value", changed);
        Assert.Contains("DisplayValue", changed);
    }

    [Fact]
    public void DisplayValue_FormatsNull()
    {
        var target = new SampleObject { Name = null! };
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));

        Assert.Equal("(null)", item.DisplayValue);
    }

    [Fact]
    public void RefreshValue_UpdatesFromTarget()
    {
        var target = new SampleObject { Name = "Initial" };
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));

        target.Name = "Changed";
        item.RefreshValue();

        Assert.Equal("Changed", item.Value);
    }

    [Fact]
    public void IsExpanded_RaisesPropertyChanged()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));
        var raised = false;
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PropertyItem.IsExpanded)) raised = true;
        };

        item.IsExpanded = true;

        Assert.True(raised);
    }

    [Fact]
    public void IsSelected_RaisesPropertyChanged()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));
        var raised = false;
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PropertyItem.IsSelected)) raised = true;
        };

        item.IsSelected = true;

        Assert.True(raised);
    }

    [Fact]
    public void TypeName_ReturnsStringForString()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));

        Assert.Equal("string", item.TypeName);
    }

    [Fact]
    public void TypeName_ReturnsIntForInt()
    {
        var target = new SampleObject();
        var item = CreatePropertyItem(target, nameof(SampleObject.RangeProp));

        Assert.Equal("int", item.TypeName);
    }

    private static PropertyItem CreatePropertyItem(object target, string propertyName)
    {
        var propInfo = target.GetType().GetProperty(propertyName)!;
        return new PropertyItem(propInfo, target);
    }

    private class SampleObject
    {
        public string Name { get; set; } = "";

        [DisplayName("Friendly Name")]
        [Description("A test description")]
        public string DisplayNameProp { get; set; } = "";

        [Category("Layout")]
        public double CategorizedProp { get; set; }

        [ReadOnly(true)]
        public string ReadOnlyProp { get; set; } = "Readonly";

        [Range(0, 100)]
        public int RangeProp { get; set; }
    }
}
