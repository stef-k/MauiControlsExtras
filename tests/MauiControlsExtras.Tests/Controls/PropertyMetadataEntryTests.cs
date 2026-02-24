using MauiControlsExtras.Controls;

namespace MauiControlsExtras.Tests.Controls;

public class PropertyMetadataEntryTests
{
    private class TestProduct
    {
        public string Name { get; set; } = "Widget";
        public decimal Price { get; set; } = 9.99m;
        public bool InStock { get; set; } = true;
    }

    [Fact]
    public void RegisterMetadata_StoresEntries()
    {
        try
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
        finally
        {
            // Cleanup: re-register empty to not pollute other tests
            PropertyMetadataRegistry.Register(typeof(TestProduct));
        }
    }

    [Fact]
    public void RegisterMetadata_Generic_StoresEntries()
    {
        try
        {
            PropertyMetadataRegistry.Register<TestProduct>(
                new PropertyMetadataEntry
                {
                    Name = "Name",
                    PropertyType = typeof(string),
                    GetValue = obj => ((TestProduct)obj).Name
                });

            Assert.True(PropertyMetadataRegistry.HasMetadata(typeof(TestProduct)));
        }
        finally
        {
            PropertyMetadataRegistry.Register(typeof(TestProduct));
        }
    }

    [Fact]
    public void HasMetadata_ReturnsTrueForRegistered()
    {
        try
        {
            PropertyMetadataRegistry.Register<TestProduct>(
                new PropertyMetadataEntry
                {
                    Name = "Name",
                    PropertyType = typeof(string),
                    GetValue = obj => ((TestProduct)obj).Name
                });

            Assert.True(PropertyMetadataRegistry.HasMetadata(typeof(TestProduct)));
        }
        finally
        {
            PropertyMetadataRegistry.Register(typeof(TestProduct));
        }
    }

    [Fact]
    public void HasMetadata_ReturnsFalseForUnregistered()
    {
        Assert.False(PropertyMetadataRegistry.HasMetadata(typeof(string)));
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
            }
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
            GetValue = obj => ((TestProduct)obj).Name
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
            GetValue = obj => ((TestProduct)obj).Name
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
            GetValue = obj => ((TestProduct)obj).Name
        };

        var item = new PropertyItem(metadata, target);
        Assert.Equal("Original", item.Value);

        target.Name = "Changed";
        item.RefreshValue();

        Assert.Equal("Changed", item.Value);
    }
}
