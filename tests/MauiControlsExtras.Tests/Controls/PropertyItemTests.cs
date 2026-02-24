using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MauiControlsExtras.Controls;
using MauiControlsExtras.Helpers;

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
    public void Value_Set_RaisesValueChangingEvent()
    {
        var target = new SampleObject { Name = "Old" };
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));
        PropertyValueChangingEventArgs? args = null;
        item.ValueChanging += (_, e) => args = e;

        item.Value = "New";

        Assert.NotNull(args);
        Assert.Equal("Old", args.CurrentValue);
        Assert.Equal("New", args.NewValue);
    }

    [Fact]
    public void Value_Set_WhenValueChangingCancelled_DoesNotUpdateValue()
    {
        var target = new SampleObject { Name = "Old" };
        var item = CreatePropertyItem(target, nameof(SampleObject.Name));
        item.ValueChanging += (_, e) => e.Cancel = true;

        item.Value = "New";

        Assert.Equal("Old", item.Value);
        Assert.Equal("Old", target.Name);
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

    [Fact]
    public void Value_Set_OnReadOnlyItem_DoesNotChangeValue()
    {
        var target = new SampleObject { Name = "Original" };
        var metadata = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            IsReadOnly = true,
            GetValue = obj => ((SampleObject)obj).Name
        };

        var item = new PropertyItem(metadata, target);
        var changedFired = false;
        item.ValueChanged += (_, _) => changedFired = true;

        item.Value = "Attempted";

        Assert.Equal("Original", item.Value);
        Assert.False(changedFired);
    }

    [Fact]
    public void Value_Set_Null_OnValueTypeProperty_SetsDefault()
    {
        var target = new SampleObject { RangeProp = 42 };
        var item = CreatePropertyItem(target, nameof(SampleObject.RangeProp));

        // Should not throw (Convert.ChangeType(null, int) would throw; ConvertToType handles it)
        item.Value = null;

        Assert.Equal(0, target.RangeProp);
        Assert.Null(item.Value); // raw value stored in PropertyItem
    }

    [Fact]
    public void Value_Set_StringOnIntProperty_Converts()
    {
        var target = new SampleObject { RangeProp = 0 };
        var item = CreatePropertyItem(target, nameof(SampleObject.RangeProp));

        item.Value = "50";

        Assert.Equal(50, target.RangeProp);
    }

    [Fact]
    public void Value_Set_RemainsUnchanged_WhenSetterThrowsUnfilteredException()
    {
        var target = new SampleObject { Name = "Original" };
        var metadata = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            GetValue = obj => ((SampleObject)obj).Name,
            SetValue = (_, _) => throw new InvalidOperationException("Setter failed")
        };

        var item = new PropertyItem(metadata, target);
        Assert.Equal("Original", item.Value);

        // InvalidOperationException is in the catch filter — value should NOT change
        item.Value = "Attempted";

        Assert.Equal("Original", item.Value);
    }

    [Fact]
    public void MetadataConstructor_CallsGetterOnce_WhenSubPropertiesPresent()
    {
        var callCount = 0;
        var dimensions = new SampleDimensions { Width = 10.0, Height = 20.0 };

        var metadata = new PropertyMetadataEntry
        {
            Name = "Size",
            PropertyType = typeof(SampleDimensions),
            GetValue = obj =>
            {
                callCount++;
                return dimensions;
            },
            IsReadOnly = true,
            SubProperties = new List<PropertyMetadataEntry>
            {
                new PropertyMetadataEntry
                {
                    Name = "Width",
                    PropertyType = typeof(double),
                    GetValue = obj => ((SampleDimensions)obj).Width,
                    SetValue = (obj, val) => ((SampleDimensions)obj).Width = (double)val!
                }
            }
        };

        var target = new SampleObject();
        _ = new PropertyItem(metadata, target);

        Assert.Equal(1, callCount);
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

    private class SampleDimensions
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
