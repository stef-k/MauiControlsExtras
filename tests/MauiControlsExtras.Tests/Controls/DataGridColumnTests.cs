using System.ComponentModel;
using MauiControlsExtras.Controls;

namespace MauiControlsExtras.Tests.Controls;

public class DataGridColumnTests
{
    [Fact]
    public void TextColumn_PropertyDefaults()
    {
        var col = new DataGridTextColumn();

        Assert.Equal(string.Empty, col.Header);
        Assert.Equal(-1, col.Width); // auto
        Assert.Equal(50, col.MinWidth);
        Assert.True(col.CanUserSort);
        Assert.True(col.CanUserFilter);
        Assert.True(col.CanUserEdit);
        Assert.True(col.CanUserResize);
        Assert.True(col.CanUserReorder);
        Assert.False(col.IsReadOnly);
        Assert.True(col.IsVisible);
    }

    [Fact]
    public void Header_RaisesPropertyChanged()
    {
        var col = new DataGridTextColumn();
        var raised = false;
        col.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DataGridColumn.Header)) raised = true;
        };

        col.Header = "Name";

        Assert.True(raised);
        Assert.Equal("Name", col.Header);
    }

    [Fact]
    public void Width_RaisesPropertyChanged()
    {
        var col = new DataGridTextColumn();
        var raised = false;
        col.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DataGridColumn.Width)) raised = true;
        };

        col.Width = 200;

        Assert.True(raised);
    }

    [Fact]
    public void SortDirection_RaisesPropertyChanged()
    {
        var col = new DataGridTextColumn();
        var raised = false;
        col.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DataGridColumn.SortDirection)) raised = true;
        };

        col.SortDirection = SortDirection.Ascending;

        Assert.True(raised);
    }

    [Fact]
    public void IsVisible_RaisesPropertyChanged()
    {
        var col = new DataGridTextColumn();
        var raised = false;
        col.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DataGridColumn.IsVisible)) raised = true;
        };

        col.IsVisible = false;

        Assert.True(raised);
        Assert.False(col.IsVisible);
    }

    [Fact]
    public void TextColumn_CanSetBinding()
    {
        var col = new DataGridTextColumn { Binding = "Name" };

        Assert.Equal("Name", col.Binding);
    }

    [Fact]
    public void CheckBoxColumn_DefaultBinding()
    {
        var col = new DataGridCheckBoxColumn();

        Assert.Equal(string.Empty, col.Binding);
    }

    [Fact]
    public void ImageColumn_DefaultImageSize()
    {
        var col = new DataGridImageColumn();

        Assert.Equal(32, col.ImageSize);
    }

    [Fact]
    public void TemplateColumn_HasNullTemplates()
    {
        var col = new DataGridTemplateColumn();

        Assert.Null(col.CellTemplate);
        Assert.Null(col.EditTemplate);
    }

    [Fact]
    public void AggregateType_CanBeSet()
    {
        var col = new DataGridTextColumn { AggregateType = DataGridAggregateType.Sum };

        Assert.Equal(DataGridAggregateType.Sum, col.AggregateType);
    }

    [Fact]
    public void ValidationFunc_CanBeSet()
    {
        var col = new DataGridTextColumn();
        col.ValidationFunc = (item, value) =>
            value != null
                ? MauiControlsExtras.Base.Validation.ValidationResult.Success
                : MauiControlsExtras.Base.Validation.ValidationResult.Failure("Required");

        Assert.NotNull(col.ValidationFunc);
    }

    [Fact]
    public void Validate_WithNoValidationFunc_ReturnsSuccess()
    {
        var col = new DataGridTextColumn();
        var item = new { Name = "Test" };

        var result = col.Validate(item, "some value");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithValidationFunc_DelegatesToFunc()
    {
        var col = new DataGridTextColumn();
        col.ValidationFunc = (item, value) =>
            string.IsNullOrEmpty(value?.ToString())
                ? MauiControlsExtras.Base.Validation.ValidationResult.Failure("Required")
                : MauiControlsExtras.Base.Validation.ValidationResult.Success;

        var item = new { Name = "Test" };

        var successResult = col.Validate(item, "valid");
        Assert.True(successResult.IsValid);

        var failResult = col.Validate(item, "");
        Assert.False(failResult.IsValid);
        Assert.Equal("Required", failResult.FirstError);
    }

    // --- FilterValues tests ---

    [Fact]
    public void FilterValues_DefaultsToNull()
    {
        var col = new DataGridTextColumn();
        Assert.Null(col.FilterValues);
    }

    [Fact]
    public void FilterValues_Set_RaisesPropertyChanged()
    {
        var col = new DataGridTextColumn();
        var raised = new List<string>();
        col.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        col.FilterValues = new object[] { "A", "B" };

        Assert.Contains(nameof(DataGridColumn.FilterValues), raised);
        Assert.Contains(nameof(DataGridColumn.IsFiltered), raised);
    }

    [Fact]
    public void FilterValues_NonEmpty_SetsIsFilteredTrue()
    {
        var col = new DataGridTextColumn();
        col.FilterValues = new object[] { "A" };
        Assert.True(col.IsFiltered);
    }

    [Fact]
    public void FilterValues_EmptyCollection_SetsIsFilteredFalse()
    {
        var col = new DataGridTextColumn();
        col.FilterValues = Enumerable.Empty<object>();
        Assert.False(col.IsFiltered);
    }

    [Fact]
    public void FilterValues_ResetToNull_SetsIsFilteredFalse()
    {
        var col = new DataGridTextColumn();
        col.FilterValues = new object[] { "A" };
        Assert.True(col.IsFiltered);

        col.FilterValues = null;
        Assert.False(col.IsFiltered);
    }

    // --- FilterText tests ---

    [Fact]
    public void FilterText_DefaultsToNull()
    {
        var col = new DataGridTextColumn();
        Assert.Null(col.FilterText);
    }

    [Fact]
    public void FilterText_Set_RaisesPropertyChanged()
    {
        var col = new DataGridTextColumn();
        var raised = new List<string>();
        col.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        col.FilterText = "search";

        Assert.Contains(nameof(DataGridColumn.FilterText), raised);
        Assert.Contains(nameof(DataGridColumn.IsFiltered), raised);
    }

    [Fact]
    public void FilterText_NonEmpty_SetsIsFilteredTrue()
    {
        var col = new DataGridTextColumn();
        col.FilterText = "search";
        Assert.True(col.IsFiltered);
    }

    [Fact]
    public void FilterText_EmptyString_SetsIsFilteredFalse()
    {
        var col = new DataGridTextColumn();
        col.FilterText = "";
        Assert.False(col.IsFiltered);
    }

    [Fact]
    public void FilterText_ResetToNull_SetsIsFilteredFalse()
    {
        var col = new DataGridTextColumn();
        col.FilterText = "search";
        Assert.True(col.IsFiltered);

        col.FilterText = null;
        Assert.False(col.IsFiltered);
    }

    // --- IsFiltered combined tests ---

    [Fact]
    public void IsFiltered_FalseByDefault()
    {
        var col = new DataGridTextColumn();
        Assert.False(col.IsFiltered);
    }

    [Fact]
    public void IsFiltered_TrueWithOnlyFilterValues()
    {
        var col = new DataGridTextColumn();
        col.FilterValues = new object[] { 1, 2 };
        Assert.True(col.IsFiltered);
        Assert.Null(col.FilterText);
    }

    [Fact]
    public void IsFiltered_TrueWithOnlyFilterText()
    {
        var col = new DataGridTextColumn();
        col.FilterText = "abc";
        Assert.True(col.IsFiltered);
        Assert.Null(col.FilterValues);
    }

    [Fact]
    public void IsFiltered_TrueWithBoth()
    {
        var col = new DataGridTextColumn();
        col.FilterValues = new object[] { "X" };
        col.FilterText = "Y";
        Assert.True(col.IsFiltered);
    }

    [Fact]
    public void IsFiltered_FalseWhenBothEmptyOrNull()
    {
        var col = new DataGridTextColumn();
        col.FilterValues = Enumerable.Empty<object>();
        col.FilterText = "";
        Assert.False(col.IsFiltered);
    }
}
