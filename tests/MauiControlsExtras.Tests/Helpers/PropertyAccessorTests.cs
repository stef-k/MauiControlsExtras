using MauiControlsExtras.Helpers;

namespace MauiControlsExtras.Tests.Helpers;

public class PropertyAccessorTests
{
    private class TestModel
    {
        public string Name { get; set; } = "Test";
        public int Age { get; set; } = 25;
        public bool IsActive { get; set; } = true;
        public DateTime Created { get; set; } = new(2024, 1, 1);
        public string ReadOnly => "ReadOnly";
    }

    private class OtherModel
    {
        public string Title { get; set; } = "Other";
        public double Price { get; set; } = 9.99;
    }

    [Fact]
    public void GetValue_ReturnsPropertyValue()
    {
        var model = new TestModel { Name = "Hello" };

        var result = PropertyAccessor.GetValue(model, "Name");

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenPropertyNotFound()
    {
        var model = new TestModel();

        var result = PropertyAccessor.GetValue(model, "NonExistent");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GetValue_ReturnsNull_WhenPathEmpty(string? path)
    {
        var model = new TestModel();

        var result = PropertyAccessor.GetValue(model, path!);

        Assert.Null(result);
    }

    [Fact]
    public void GetValue_CachesPropertyInfo()
    {
        var model = new TestModel { Name = "First" };

        // Call twice - second should use cache
        var result1 = PropertyAccessor.GetValue(model, "Name");
        model.Name = "Second";
        var result2 = PropertyAccessor.GetValue(model, "Name");

        Assert.Equal("First", result1);
        Assert.Equal("Second", result2);
    }

    [Fact]
    public void GetValue_HandlesMultipleTypes()
    {
        var model1 = new TestModel { Name = "Test" };
        var model2 = new OtherModel { Title = "Other" };

        var result1 = PropertyAccessor.GetValue(model1, "Name");
        var result2 = PropertyAccessor.GetValue(model2, "Title");

        Assert.Equal("Test", result1);
        Assert.Equal("Other", result2);
    }

    [Fact]
    public void SetValue_UpdatesProperty()
    {
        var model = new TestModel();

        var result = PropertyAccessor.SetValue(model, "Name", "Updated");

        Assert.True(result);
        Assert.Equal("Updated", model.Name);
    }

    [Fact]
    public void SetValue_ReturnsFalse_WhenPropertyNotFound()
    {
        var model = new TestModel();

        var result = PropertyAccessor.SetValue(model, "NonExistent", "value");

        Assert.False(result);
    }

    [Fact]
    public void SetValue_ReturnsFalse_WhenReadOnly()
    {
        var model = new TestModel();

        var result = PropertyAccessor.SetValue(model, "ReadOnly", "value");

        Assert.False(result);
    }

    [Theory]
    [InlineData(typeof(int), 0)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(double), 0.0)]
    [InlineData(typeof(float), 0f)]
    [InlineData(typeof(long), 0L)]
    public void GetDefaultValue_ReturnsCorrectDefaults(Type type, object expected)
    {
        var result = PropertyAccessor.GetDefaultValue(type);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetDefaultValue_ReturnsZero_ForDecimal()
    {
        var result = PropertyAccessor.GetDefaultValue(typeof(decimal));

        Assert.Equal(0m, result);
    }

    [Fact]
    public void GetDefaultValue_ReturnsDefault_ForDateTime()
    {
        var result = PropertyAccessor.GetDefaultValue(typeof(DateTime));

        Assert.Equal(default(DateTime), result);
    }

    [Fact]
    public void GetDefaultValue_ReturnsNull_ForReferenceTypes()
    {
        var result = PropertyAccessor.GetDefaultValue(typeof(string));

        Assert.Null(result);
    }

    [Fact]
    public void GetDefaultValue_ReturnsNull_ForNullableTypes()
    {
        var result = PropertyAccessor.GetDefaultValue(typeof(int?));

        Assert.Null(result);
    }

    [Theory]
    [InlineData("42", typeof(int), 42)]
    [InlineData("3.14", typeof(double), 3.14)]
    [InlineData("true", typeof(bool), true)]
    [InlineData("99.99", typeof(decimal), "99.99")]
    public void ConvertToType_HandlesCommonTypes(object input, Type targetType, object expected)
    {
        var result = PropertyAccessor.ConvertToType(input, targetType);

        if (expected is string s && targetType == typeof(decimal))
            Assert.Equal(decimal.Parse(s), result);
        else
            Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertToType_HandlesNull_ForValueType()
    {
        var result = PropertyAccessor.ConvertToType(null, typeof(int));

        Assert.Equal(0, result);
    }

    [Fact]
    public void ConvertToType_HandlesNull_ForReferenceType()
    {
        var result = PropertyAccessor.ConvertToType(null, typeof(string));

        Assert.Null(result);
    }

    [Fact]
    public void ConvertToType_ReturnsNull_ForNullableWithEmptyString()
    {
        var result = PropertyAccessor.ConvertToType("", typeof(int?));

        Assert.Null(result);
    }
}
