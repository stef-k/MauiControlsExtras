using MauiControlsExtras.ContextMenu;
using MauiControlsExtras.Controls;

namespace MauiControlsExtras.Tests.Controls;

public class EventArgsTests
{
    #region Accordion Event Args

    [Fact]
    public void AccordionItemExpandedEventArgs_StoresProperties()
    {
        var item = new AccordionItem { Header = "Test" };
        var args = new AccordionItemExpandedEventArgs(item, 2, true);

        Assert.Same(item, args.Item);
        Assert.Equal(2, args.Index);
        Assert.True(args.IsExpanded);
    }

    [Fact]
    public void AccordionItemExpandingEventArgs_IsCancelable()
    {
        var item = new AccordionItem();
        var args = new AccordionItemExpandingEventArgs(item, 0, true);

        Assert.False(args.Cancel);
        args.Cancel = true;
        Assert.True(args.Cancel);
        Assert.True(args.IsExpanding);
    }

    #endregion

    #region Wizard Event Args

    [Fact]
    public void WizardStepChangedEventArgs_StoresProperties()
    {
        var oldStep = new WizardStep { Title = "Old" };
        var newStep = new WizardStep { Title = "New" };
        var args = new WizardStepChangedEventArgs(oldStep, newStep, 0, 1);

        Assert.Same(oldStep, args.OldStep);
        Assert.Same(newStep, args.NewStep);
        Assert.Equal(0, args.OldIndex);
        Assert.Equal(1, args.NewIndex);
    }

    [Fact]
    public void WizardStepChangingEventArgs_IsCancelable()
    {
        var args = new WizardStepChangingEventArgs(null, null, 0, 1);

        Assert.False(args.Cancel);
        args.Cancel = true;
        Assert.True(args.Cancel);
    }

    [Fact]
    public void WizardFinishedEventArgs_StoresProperties()
    {
        var steps = new List<WizardStep> { new(), new() };
        var args = new WizardFinishedEventArgs(true, steps);

        Assert.True(args.WasCancelled);
        Assert.Equal(2, args.Steps.Count);
    }

    [Fact]
    public void WizardFinishingEventArgs_IsCancelable()
    {
        var steps = new List<WizardStep>();
        var args = new WizardFinishingEventArgs(steps);

        Assert.False(args.Cancel);
        args.Cancel = true;
        Assert.True(args.Cancel);
    }

    [Fact]
    public void WizardCancellingEventArgs_StoresCurrentIndex()
    {
        var args = new WizardCancellingEventArgs(3);

        Assert.Equal(3, args.CurrentIndex);
        Assert.False(args.Cancel);
    }

    [Fact]
    public void WizardStepValidatingEventArgs_ComputesDirection()
    {
        var forward = new WizardStepValidatingEventArgs(null, null, 0, 2);
        Assert.Equal(WizardNavigationDirection.Forward, forward.Direction);

        var backward = new WizardStepValidatingEventArgs(null, null, 2, 0);
        Assert.Equal(WizardNavigationDirection.Backward, backward.Direction);
    }

    [Fact]
    public void WizardStepValidatingEventArgs_HasMutableValidationErrors()
    {
        var args = new WizardStepValidatingEventArgs(null, null, 0, 1);
        Assert.Empty(args.ValidationErrors);

        args.ValidationErrors.Add("Field required");
        Assert.Single(args.ValidationErrors);
    }

    [Fact]
    public void WizardStepValidationEventArgs_StoresStepAndMessage()
    {
        var step = new WizardStep { Title = "Step 1" };
        var args = new WizardStepValidationEventArgs(step, "Invalid input");

        Assert.Same(step, args.Step);
        Assert.Equal("Invalid input", args.Message);
    }

    #endregion

    #region Breadcrumb Event Args

    [Fact]
    public void BreadcrumbItemClickedEventArgs_StoresProperties()
    {
        var item = new BreadcrumbItem("Home");
        var args = new BreadcrumbItemClickedEventArgs(item, 0);

        Assert.Same(item, args.Item);
        Assert.Equal(0, args.Index);
    }

    [Fact]
    public void BreadcrumbNavigatingEventArgs_IsCancelable()
    {
        var item = new BreadcrumbItem("Products");
        var args = new BreadcrumbNavigatingEventArgs(item, 1);

        Assert.Same(item, args.TargetItem);
        Assert.Equal(1, args.TargetIndex);
        Assert.False(args.Cancel);
        args.Cancel = true;
        Assert.True(args.Cancel);
    }

    #endregion

    #region Calendar Event Args

    [Fact]
    public void CalendarDateSelectedEventArgs_StoresProperties()
    {
        var date = new DateTime(2025, 6, 15);
        var args = new CalendarDateSelectedEventArgs(date, true);

        Assert.Equal(date, args.Date);
        Assert.True(args.IsSelected);
    }

    [Fact]
    public void CalendarDateSelectingEventArgs_IsCancelable()
    {
        var date = new DateTime(2025, 12, 25);
        var args = new CalendarDateSelectingEventArgs(date, false);

        Assert.Equal(date, args.Date);
        Assert.False(args.IsSelecting);
        Assert.False(args.Cancel);
        args.Cancel = true;
        Assert.True(args.Cancel);
    }

    [Fact]
    public void CalendarDisplayDateChangedEventArgs_StoresProperties()
    {
        var oldDate = new DateTime(2025, 1, 1);
        var newDate = new DateTime(2025, 2, 1);
        var args = new CalendarDisplayDateChangedEventArgs(oldDate, newDate);

        Assert.Equal(oldDate, args.OldDate);
        Assert.Equal(newDate, args.NewDate);
    }

    #endregion

    #region DataGrid Event Args

    [Fact]
    public void DataGridCellEditEventArgs_StoresProperties()
    {
        var column = new DataGridTextColumn { Header = "Name" };
        var item = new { Name = "Test" };
        var args = new DataGridCellEditEventArgs(item, column, 0, 0, "old", "new");

        Assert.Same(item, args.Item);
        Assert.Same(column, args.Column);
        Assert.Equal(0, args.RowIndex);
        Assert.Equal(0, args.ColumnIndex);
        Assert.Equal("old", args.OldValue);
        Assert.Equal("new", args.NewValue);
        Assert.False(args.Cancel);
    }

    [Fact]
    public void DataGridRowEditEventArgs_StoresProperties()
    {
        var item = new { Name = "Test" };
        var columns = new List<DataGridColumn> { new DataGridTextColumn() };
        var args = new DataGridRowEditEventArgs(item, 5, columns);

        Assert.Same(item, args.Item);
        Assert.Equal(5, args.RowIndex);
        Assert.Single(args.EditedColumns);
    }

    [Fact]
    public void DataGridColumnEventArgs_StoresProperties()
    {
        var column = new DataGridTextColumn { Header = "Age" };
        var args = new DataGridColumnEventArgs(column, 1);

        Assert.Same(column, args.Column);
        Assert.Equal(1, args.ColumnIndex);
    }

    [Fact]
    public void DataGridContextMenuEventArgs_StoresProperties()
    {
        var item = new { Name = "Test" };
        var column = new DataGridTextColumn { Header = "Name" };
        var args = new DataGridContextMenuEventArgs(item, column, 2, 1);

        Assert.Same(item, args.Item);
        Assert.Same(column, args.Column);
        Assert.Equal(2, args.RowIndex);
        Assert.Equal(1, args.ColumnIndex);
        Assert.False(args.Cancel);
        Assert.Null(args.CustomMenuItems);
    }

    [Fact]
    public void DataGridContextMenuOpeningEventArgs_StoresProperties()
    {
        var items = new ContextMenuItemCollection();
        var position = new Point(100, 200);
        var item = new { Name = "Test" };
        var column = new DataGridTextColumn { Header = "Name" };

        var args = new DataGridContextMenuOpeningEventArgs(
            items, position, item, column, 3, 2, "TestValue");

        Assert.Same(items, args.Items);
        Assert.Equal(position, args.Position);
        Assert.Same(item, args.Item);
        Assert.Same(column, args.Column);
        Assert.Equal(3, args.RowIndex);
        Assert.Equal(2, args.ColumnIndex);
        Assert.Equal("TestValue", args.CellValue);
        Assert.False(args.IsHeader);
        Assert.True(args.IsDataCell);
    }

    [Fact]
    public void DataGridContextMenuOpeningEventArgs_IsHeader_WhenRowIndexMinusOne()
    {
        var args = new DataGridContextMenuOpeningEventArgs(
            Point.Zero, null, new DataGridTextColumn(), -1, 0);

        Assert.True(args.IsHeader);
        Assert.False(args.IsDataCell);
    }

    [Fact]
    public void DataGridCellValidationEventArgs_StoresProperties()
    {
        var column = new DataGridTextColumn { Header = "Name" };
        var item = new { Name = "Test" };
        var args = new DataGridCellValidationEventArgs(item, column, "old", "new");

        Assert.Same(item, args.Item);
        Assert.Same(column, args.Column);
        Assert.Equal("old", args.OldValue);
        Assert.Equal("new", args.NewValue);
    }

    [Fact]
    public void DataGridCellValidationEventArgs_IsValid_DefaultTrue()
    {
        var column = new DataGridTextColumn();
        var args = new DataGridCellValidationEventArgs(new object(), column, null, null);

        Assert.True(args.IsValid);
        Assert.Null(args.ErrorMessage);
    }

    [Fact]
    public void DataGridCellValidationEventArgs_CanSetInvalid()
    {
        var column = new DataGridTextColumn();
        var args = new DataGridCellValidationEventArgs(new object(), column, null, null);

        args.IsValid = false;
        args.ErrorMessage = "Value is required";

        Assert.False(args.IsValid);
        Assert.Equal("Value is required", args.ErrorMessage);
    }

    [Fact]
    public void DataGridContextMenuOpeningEventArgs_Cancel()
    {
        var args = new DataGridContextMenuOpeningEventArgs(
            Point.Zero, null, null, -1, -1);

        Assert.False(args.Cancel);
        args.Cancel = true;
        Assert.True(args.Cancel);
    }

    #endregion

    #region BindingNavigator Event Args

    [Fact]
    public void PositionChangedEventArgs_StoresProperties()
    {
        var args = new MauiControlsExtras.Controls.PositionChangedEventArgs(0, 5, "item5");

        Assert.Equal(0, args.OldPosition);
        Assert.Equal(5, args.NewPosition);
        Assert.Equal("item5", args.Item);
    }

    [Fact]
    public void PositionChangingEventArgs_IsCancelable()
    {
        var args = new MauiControlsExtras.Controls.PositionChangingEventArgs(3, 4);

        Assert.Equal(3, args.CurrentPosition);
        Assert.Equal(4, args.NewPosition);
        Assert.False(args.Cancel);
        args.Cancel = true;
        Assert.True(args.Cancel);
    }

    [Fact]
    public void BindingNavigatorItemEventArgs_IsCancelable()
    {
        var args = new MauiControlsExtras.Controls.BindingNavigatorItemEventArgs("record", 10);

        Assert.Equal("record", args.Item);
        Assert.Equal(10, args.Position);
        Assert.False(args.Cancel);
    }

    #endregion

    #region PropertyGrid Event Args

    [Fact]
    public void PropertyValueChangedEventArgs_StoresProperties()
    {
        var prop = CreateTestPropertyItem();
        var args = new PropertyValueChangedEventArgs(prop, "old", "new");

        Assert.Same(prop, args.Property);
        Assert.Equal("old", args.OldValue);
        Assert.Equal("new", args.NewValue);
    }

    [Fact]
    public void PropertyValueChangingEventArgs_IsCancelable()
    {
        var prop = CreateTestPropertyItem();
        var args = new PropertyValueChangingEventArgs(prop, "current", "proposed");

        Assert.Same(prop, args.Property);
        Assert.Equal("current", args.CurrentValue);
        Assert.Equal("proposed", args.NewValue);
        Assert.False(args.Cancel);
        args.Cancel = true;
        Assert.True(args.Cancel);
    }

    [Fact]
    public void SelectedObjectChangedEventArgs_StoresProperties()
    {
        var oldObj = new object();
        var newObj = new object();
        var args = new SelectedObjectChangedEventArgs(oldObj, newObj);

        Assert.Same(oldObj, args.OldObject);
        Assert.Same(newObj, args.NewObject);
    }

    [Fact]
    public void PropertySelectionChangedEventArgs_StoresProperties()
    {
        var args = new PropertySelectionChangedEventArgs(null, null);

        Assert.Null(args.OldProperty);
        Assert.Null(args.NewProperty);
    }

    #endregion

    #region SelectionChangedEventArgs (ISelectable)

    [Fact]
    public void SelectionChangedEventArgs_StoresProperties()
    {
        var args = new MauiControlsExtras.Base.SelectionChangedEventArgs("old", "new");

        Assert.Equal("old", args.OldSelection);
        Assert.Equal("new", args.NewSelection);
    }

    [Fact]
    public void SelectionChangedEventArgs_SelectionCleared_WhenNewIsNull()
    {
        var args = new MauiControlsExtras.Base.SelectionChangedEventArgs("old", null);

        Assert.True(args.SelectionCleared);
        Assert.False(args.IsFirstSelection);
    }

    [Fact]
    public void SelectionChangedEventArgs_IsFirstSelection_WhenOldIsNull()
    {
        var args = new MauiControlsExtras.Base.SelectionChangedEventArgs(null, "new");

        Assert.True(args.IsFirstSelection);
        Assert.False(args.SelectionCleared);
    }

    [Fact]
    public void SelectionChangedEventArgs_BothNull_NeitherClearedNorFirst()
    {
        var args = new MauiControlsExtras.Base.SelectionChangedEventArgs(null, null);

        Assert.False(args.SelectionCleared);
        Assert.False(args.IsFirstSelection);
    }

    #endregion

    #region RichTextEditor Event Args

    [Fact]
    public void RichTextContentChangedEventArgs_StoresProperties()
    {
        var args = new RichTextContentChangedEventArgs("<p>old</p>", "<p>new</p>", ContentFormat.Html);

        Assert.Equal("<p>old</p>", args.OldContent);
        Assert.Equal("<p>new</p>", args.NewContent);
        Assert.Equal(ContentFormat.Html, args.Format);
    }

    [Fact]
    public void RichTextSelectionChangedEventArgs_HasSelectionComputed()
    {
        var withSelection = new RichTextSelectionChangedEventArgs(5, 10, "hello there");
        Assert.True(withSelection.HasSelection);
        Assert.Equal(5, withSelection.Start);
        Assert.Equal(10, withSelection.Length);

        var noSelection = new RichTextSelectionChangedEventArgs(5, 0, null);
        Assert.False(noSelection.HasSelection);
    }

    [Fact]
    public void RichTextLinkTappedEventArgs_StoresProperties()
    {
        var args = new RichTextLinkTappedEventArgs("https://example.com", "Example");

        Assert.Equal("https://example.com", args.Url);
        Assert.Equal("Example", args.Text);
        Assert.False(args.Handled);
    }

    [Fact]
    public void RichTextImageRequestedEventArgs_IsMutable()
    {
        var args = new RichTextImageRequestedEventArgs();

        Assert.Null(args.ImageUrl);
        args.ImageUrl = "img.png";
        args.AltText = "Alt";
        args.Handled = true;

        Assert.Equal("img.png", args.ImageUrl);
        Assert.Equal("Alt", args.AltText);
        Assert.True(args.Handled);
    }

    [Fact]
    public void RichTextFocusChangedEventArgs_StoresIsFocused()
    {
        var focused = new RichTextFocusChangedEventArgs(true);
        Assert.True(focused.IsFocused);

        var unfocused = new RichTextFocusChangedEventArgs(false);
        Assert.False(unfocused.IsFocused);
    }

    #endregion

    #region TokenEntry Event Args

    [Fact]
    public void TokenClipboardEventArgs_StoresProperties()
    {
        var tokens = new[] { "tag1", "tag2" };
        var args = new TokenClipboardEventArgs(TokenClipboardOperation.Copy, tokens, "tag1, tag2");

        Assert.Equal(TokenClipboardOperation.Copy, args.Operation);
        Assert.Equal(2, args.Tokens.Count);
        Assert.Equal("tag1, tag2", args.Content);
        Assert.False(args.Cancel);
    }

    [Fact]
    public void TokenClipboardEventArgs_SingleTokenConstructor()
    {
        var args = new TokenClipboardEventArgs(TokenClipboardOperation.Cut, "tag1");

        Assert.Equal(TokenClipboardOperation.Cut, args.Operation);
        Assert.Single(args.Tokens);
        Assert.Equal("tag1", args.Content);
    }

    [Fact]
    public void TokenClipboardEventArgs_PasteSkipTracking()
    {
        var args = new TokenClipboardEventArgs(TokenClipboardOperation.Paste, new[] { "a", "b" }, "a, b");

        Assert.Empty(args.SkippedTokens);
        Assert.Empty(args.SkipReasons);
        Assert.Equal(0, args.SuccessCount);
    }

    #endregion

    private static PropertyItem CreateTestPropertyItem()
    {
        var target = new SampleTarget { Name = "Test" };
        var propInfo = typeof(SampleTarget).GetProperty(nameof(SampleTarget.Name))!;
        return new PropertyItem(propInfo, target);
    }

    private class SampleTarget
    {
        public string Name { get; set; } = "";
    }
}
