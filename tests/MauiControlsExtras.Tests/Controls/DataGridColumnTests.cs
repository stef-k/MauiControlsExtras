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
}
