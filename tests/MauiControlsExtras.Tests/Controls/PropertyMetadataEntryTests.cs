using MauiControlsExtras.Controls;
using MauiControlsExtras.Helpers;

namespace MauiControlsExtras.Tests.Controls;

public class PropertyMetadataEntryTests : IDisposable
{
    public void Dispose()
    {
        PropertyMetadataRegistry.Clear();
    }

    private class TestProduct
    {
        public string Name { get; set; } = "Widget";
        public decimal Price { get; set; } = 9.99m;
        public bool InStock { get; set; } = true;
    }

    private class TestDimensions
    {
        public double Width { get; set; } = 100.0;
        public double Height { get; set; } = 200.0;
    }

    private class TestProductWithDimensions
    {
        public string Name { get; set; } = "Widget";
        public TestDimensions Size { get; set; } = new();
    }

    private struct TestStructDimensions
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }

    private class TestProductWithStructDimensions
    {
        public string Name { get; set; } = "Widget";
        public TestStructDimensions Size { get; set; } = new() { Width = 10, Height = 20 };
    }

    private class TestProductWithNullDimensions
    {
        public string Name { get; set; } = "Widget";
        public TestDimensions? Size { get; set; }
    }

    [Fact]
    public void RegisterMetadata_StoresEntries()
    {
        var entries = new[]
        {
            new PropertyMetadataEntry
            {
                Name = "Name",
                PropertyType = typeof(string),
                GetValue = obj => ((TestProduct)obj).Name,
                SetValue = (obj, val) => ((TestProduct)obj).Name = (string)val!
            }
        };

        PropertyMetadataRegistry.Register(typeof(TestProduct), entries);

        Assert.True(PropertyMetadataRegistry.HasMetadata(typeof(TestProduct)));
    }

    [Fact]
    public void RegisterMetadata_Generic_StoresEntries()
    {
        PropertyMetadataRegistry.Register<TestProduct>(
            new PropertyMetadataEntry
            {
                Name = "Name",
                PropertyType = typeof(string),
                GetValue = obj => ((TestProduct)obj).Name,
                IsReadOnly = true
            });

        Assert.True(PropertyMetadataRegistry.HasMetadata(typeof(TestProduct)));
    }

    [Fact]
    public void HasMetadata_ReturnsTrueForRegistered()
    {
        PropertyMetadataRegistry.Register<TestProduct>(
            new PropertyMetadataEntry
            {
                Name = "Name",
                PropertyType = typeof(string),
                GetValue = obj => ((TestProduct)obj).Name,
                IsReadOnly = true
            });

        Assert.True(PropertyMetadataRegistry.HasMetadata(typeof(TestProduct)));
    }

    [Fact]
    public void HasMetadata_ReturnsFalseForUnregistered()
    {
        Assert.False(PropertyMetadataRegistry.HasMetadata(typeof(string)));
    }

    [Fact]
    public void HasMetadata_ReturnsFalse_AfterUnregister()
    {
        PropertyMetadataRegistry.Register<TestProduct>(
            new PropertyMetadataEntry
            {
                Name = "Name",
                PropertyType = typeof(string),
                GetValue = obj => ((TestProduct)obj).Name,
                IsReadOnly = true
            });

        PropertyMetadataRegistry.Unregister(typeof(TestProduct));

        Assert.False(PropertyMetadataRegistry.HasMetadata(typeof(TestProduct)));
    }

    [Fact]
    public void PropertyItem_FromMetadata_ReadsValues()
    {
        var metadata = new PropertyMetadataEntry
        {
            Name = "Price",
            DisplayName = "Product Price",
            Description = "The price of the product",
            Category = "Pricing",
            IsReadOnly = false,
            PropertyType = typeof(decimal),
            Minimum = 0m,
            Maximum = 10000m,
            GetValue = obj => ((TestProduct)obj).Price,
            SetValue = (obj, val) => ((TestProduct)obj).Price = (decimal)val!
        };

        var target = new TestProduct { Price = 19.99m };
        var item = new PropertyItem(metadata, target);

        Assert.Equal("Price", item.Name);
        Assert.Equal("Product Price", item.DisplayName);
        Assert.Equal("The price of the product", item.Description);
        Assert.Equal("Pricing", item.Category);
        Assert.False(item.IsReadOnly);
        Assert.Equal(typeof(decimal), item.PropertyType);
        Assert.Equal(0m, item.Minimum);
        Assert.Equal(10000m, item.Maximum);
        Assert.Equal(19.99m, item.Value);
    }

    [Fact]
    public void PropertyItem_FromMetadata_GetValue_UsesFunc()
    {
        var funcCalled = false;
        var metadata = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            GetValue = obj =>
            {
                funcCalled = true;
                return ((TestProduct)obj).Name;
            },
            IsReadOnly = true
        };

        var target = new TestProduct { Name = "TestWidget" };
        var item = new PropertyItem(metadata, target);

        Assert.True(funcCalled);
        Assert.Equal("TestWidget", item.Value);
    }

    [Fact]
    public void PropertyItem_FromMetadata_SetValue_UsesAction()
    {
        var target = new TestProduct { Name = "Original" };
        var metadata = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            GetValue = obj => ((TestProduct)obj).Name,
            SetValue = (obj, val) => ((TestProduct)obj).Name = (string)val!
        };

        var item = new PropertyItem(metadata, target);
        item.Value = "Updated";

        Assert.Equal("Updated", target.Name);
        Assert.Equal("Updated", item.Value);
    }

    [Fact]
    public void PropertyItem_FromMetadata_SetValue_RaisesEvents()
    {
        var target = new TestProduct { Price = 10m };
        var metadata = new PropertyMetadataEntry
        {
            Name = "Price",
            PropertyType = typeof(decimal),
            GetValue = obj => ((TestProduct)obj).Price,
            SetValue = (obj, val) => ((TestProduct)obj).Price = (decimal)val!
        };

        var item = new PropertyItem(metadata, target);

        var changingFired = false;
        var changedFired = false;
        item.ValueChanging += (_, args) => changingFired = true;
        item.ValueChanged += (_, args) =>
        {
            changedFired = true;
            Assert.Equal(10m, args.OldValue);
            Assert.Equal(25m, args.NewValue);
        };

        item.Value = 25m;

        Assert.True(changingFired);
        Assert.True(changedFired);
    }

    [Fact]
    public void PropertyItem_FromMetadata_DefaultCategory()
    {
        var metadata = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            GetValue = obj => ((TestProduct)obj).Name,
            IsReadOnly = true
        };

        var item = new PropertyItem(metadata, new TestProduct());

        Assert.Equal("Misc", item.Category);
    }

    [Fact]
    public void PropertyItem_FromMetadata_DefaultDisplayName()
    {
        var metadata = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            GetValue = obj => ((TestProduct)obj).Name,
            IsReadOnly = true
        };

        var item = new PropertyItem(metadata, new TestProduct());

        Assert.Equal("Name", item.DisplayName);
    }

    [Fact]
    public void PropertyItem_FromMetadata_RefreshValue()
    {
        var target = new TestProduct { Name = "Original" };
        var metadata = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            GetValue = obj => ((TestProduct)obj).Name,
            IsReadOnly = true
        };

        var item = new PropertyItem(metadata, target);
        Assert.Equal("Original", item.Value);

        target.Name = "Changed";
        item.RefreshValue();

        Assert.Equal("Changed", item.Value);
    }

    [Fact]
    public void PropertyItem_FromMetadata_SubProperties_UseCorrectTarget()
    {
        var target = new TestProductWithDimensions { Size = new TestDimensions { Width = 50.0, Height = 75.0 } };

        var metadata = new PropertyMetadataEntry
        {
            Name = "Size",
            PropertyType = typeof(TestDimensions),
            GetValue = obj => ((TestProductWithDimensions)obj).Size,
            IsReadOnly = true,
            SubProperties = new List<PropertyMetadataEntry>
            {
                new PropertyMetadataEntry
                {
                    Name = "Width",
                    PropertyType = typeof(double),
                    GetValue = obj => ((TestDimensions)obj).Width,
                    SetValue = (obj, val) => ((TestDimensions)obj).Width = (double)val!
                },
                new PropertyMetadataEntry
                {
                    Name = "Height",
                    PropertyType = typeof(double),
                    GetValue = obj => ((TestDimensions)obj).Height,
                    SetValue = (obj, val) => ((TestDimensions)obj).Height = (double)val!
                }
            }
        };

        var item = new PropertyItem(metadata, target);

        Assert.True(item.IsExpandable);
        Assert.Equal(2, item.SubProperties.Count);
        Assert.Equal(50.0, item.SubProperties[0].Value);
        Assert.Equal(75.0, item.SubProperties[1].Value);

        // Write-through: sub-property setter targets the Size object, not root
        item.SubProperties[0].Value = 999.0;
        Assert.Equal(999.0, target.Size.Width);
    }

    [Fact]
    public void PropertyItem_FromMetadata_Throws_WhenWritableButNoSetter()
    {
        var metadata = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            IsReadOnly = false,
            GetValue = obj => ((TestProduct)obj).Name,
            SetValue = null
        };

        var target = new TestProduct();

        var ex = Assert.Throws<ArgumentException>(() => new PropertyItem(metadata, target));
        Assert.Contains("IsReadOnly=false but SetValue is null", ex.Message);
    }

    [Fact]
    public void PropertyItem_FromMetadata_StructSubProperties_NotExpandable()
    {
        var target = new TestProductWithStructDimensions { Size = new TestStructDimensions { Width = 50.0, Height = 75.0 } };

        var metadata = new PropertyMetadataEntry
        {
            Name = "Size",
            PropertyType = typeof(TestStructDimensions),
            GetValue = obj => ((TestProductWithStructDimensions)obj).Size,
            IsReadOnly = true,
            SubProperties = new List<PropertyMetadataEntry>
            {
                new PropertyMetadataEntry
                {
                    Name = "Width",
                    PropertyType = typeof(double),
                    GetValue = obj => ((TestStructDimensions)obj).Width,
                    SetValue = (obj, val) => { /* cannot propagate back to parent struct */ }
                },
                new PropertyMetadataEntry
                {
                    Name = "Height",
                    PropertyType = typeof(double),
                    GetValue = obj => ((TestStructDimensions)obj).Height,
                    SetValue = (obj, val) => { /* cannot propagate back to parent struct */ }
                }
            }
        };

        var item = new PropertyItem(metadata, target);

        Assert.False(item.IsExpandable);
        Assert.Empty(item.SubProperties);
    }

    [Fact]
    public void PropertyItem_FromMetadata_SubProperties_HandlesNullSubTarget()
    {
        var target = new TestProductWithNullDimensions { Size = null };

        var metadata = new PropertyMetadataEntry
        {
            Name = "Size",
            PropertyType = typeof(TestDimensions),
            GetValue = obj => ((TestProductWithNullDimensions)obj).Size,
            IsReadOnly = true,
            SubProperties = new List<PropertyMetadataEntry>
            {
                new PropertyMetadataEntry
                {
                    Name = "Width",
                    PropertyType = typeof(double),
                    GetValue = obj => ((TestDimensions)obj).Width,
                    SetValue = (obj, val) => ((TestDimensions)obj).Width = (double)val!
                }
            }
        };

        var item = new PropertyItem(metadata, target);

        Assert.False(item.IsExpandable);
        Assert.Empty(item.SubProperties);
    }

    [Fact]
    public void Register_ThrowsArgumentException_WhenNoEntries()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PropertyMetadataRegistry.Register(typeof(TestProduct)));

        Assert.Contains("At least one PropertyMetadataEntry", ex.Message);
    }

    [Fact]
    public void Register_ThrowsArgumentException_WhenEmptyArray()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PropertyMetadataRegistry.Register(typeof(TestProduct), Array.Empty<PropertyMetadataEntry>()));

        Assert.Contains("At least one PropertyMetadataEntry", ex.Message);
    }

    [Fact]
    public void Register_DuplicateType_ThrowsInvalidOperationException()
    {
        var entry = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            GetValue = obj => ((TestProduct)obj).Name,
            IsReadOnly = true
        };

        PropertyMetadataRegistry.Register(typeof(TestProduct), entry);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            PropertyMetadataRegistry.Register(typeof(TestProduct), entry));

        Assert.Contains("already registered", ex.Message);
    }

    [Fact]
    public void Register_AfterUnregister_Succeeds()
    {
        var entry = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            GetValue = obj => ((TestProduct)obj).Name,
            IsReadOnly = true
        };

        PropertyMetadataRegistry.Register(typeof(TestProduct), entry);
        PropertyMetadataRegistry.Unregister(typeof(TestProduct));
        PropertyMetadataRegistry.Register(typeof(TestProduct), entry);

        Assert.True(PropertyMetadataRegistry.HasMetadata(typeof(TestProduct)));
    }

    [Fact]
    public void PropertyItem_FromMetadata_GetterThrows_SetsValueToNull()
    {
        var metadata = new PropertyMetadataEntry
        {
            Name = "Name",
            PropertyType = typeof(string),
            GetValue = _ => throw new InvalidOperationException("Getter failed"),
            IsReadOnly = true
        };

        var target = new TestProduct();
        var item = new PropertyItem(metadata, target);

        Assert.Null(item.Value);
    }

    [Fact]
    public void PropertyItem_FromMetadata_SubPropertyGetterThrows_NotExpandable()
    {
        var target = new TestProductWithDimensions();

        var metadata = new PropertyMetadataEntry
        {
            Name = "Size",
            PropertyType = typeof(TestDimensions),
            GetValue = _ => throw new InvalidOperationException("Getter failed"),
            IsReadOnly = true,
            SubProperties = new List<PropertyMetadataEntry>
            {
                new PropertyMetadataEntry
                {
                    Name = "Width",
                    PropertyType = typeof(double),
                    GetValue = obj => ((TestDimensions)obj).Width,
                    SetValue = (obj, val) => ((TestDimensions)obj).Width = (double)val!
                }
            }
        };

        var item = new PropertyItem(metadata, target);

        Assert.False(item.IsExpandable);
        Assert.Empty(item.SubProperties);
    }
}
