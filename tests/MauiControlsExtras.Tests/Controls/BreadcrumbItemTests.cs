using System.ComponentModel;
using MauiControlsExtras.Controls;
using MauiControlsExtras.Tests.Helpers;

namespace MauiControlsExtras.Tests.Controls;

public class BreadcrumbItemTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var item = new BreadcrumbItem();

        Assert.Null(item.Text);
        Assert.Null(item.Icon);
        Assert.Null(item.Tag);
        Assert.True(item.IsEnabled);
        Assert.False(item.IsCurrent);
        Assert.Null(item.Command);
        Assert.Null(item.CommandParameter);
    }

    [Fact]
    public void Constructor_WithText_SetsText()
    {
        var item = new BreadcrumbItem("Home");

        Assert.Equal("Home", item.Text);
    }

    [Fact]
    public void Constructor_WithTextAndIcon_SetsBoth()
    {
        var item = new BreadcrumbItem("Home", "🏠");

        Assert.Equal("Home", item.Text);
        Assert.Equal("🏠", item.Icon);
    }

    [Fact]
    public void Text_RaisesPropertyChanged()
    {
        var item = new BreadcrumbItem();
        var raised = false;
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BreadcrumbItem.Text)) raised = true;
        };

        item.Text = "Products";

        Assert.True(raised);
    }

    [Fact]
    public void Text_DoesNotRaise_WhenSameValue()
    {
        var item = new BreadcrumbItem("Home");
        var raised = false;
        item.PropertyChanged += (_, _) => raised = true;

        item.Text = "Home";

        Assert.False(raised);
    }

    [Fact]
    public void Icon_RaisesPropertyChanged()
    {
        var item = new BreadcrumbItem();
        var raised = false;
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BreadcrumbItem.Icon)) raised = true;
        };

        item.Icon = "📁";

        Assert.True(raised);
    }

    [Fact]
    public void Tag_RaisesPropertyChanged()
    {
        var item = new BreadcrumbItem();
        var raised = false;
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BreadcrumbItem.Tag)) raised = true;
        };

        item.Tag = new object();

        Assert.True(raised);
    }

    [Fact]
    public void IsEnabled_RaisesPropertyChanged()
    {
        var item = new BreadcrumbItem();
        var raised = false;
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BreadcrumbItem.IsEnabled)) raised = true;
        };

        item.IsEnabled = false;

        Assert.True(raised);
    }

    [Fact]
    public void Command_CanBeSetAndRetrieved()
    {
        var item = new BreadcrumbItem();
        var cmd = new TestCommand();

        item.Command = cmd;
        item.CommandParameter = "param";

        Assert.Same(cmd, item.Command);
        Assert.Equal("param", item.CommandParameter);
    }
}
