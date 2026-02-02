using DemoApp.ViewModels;
using MauiControlsExtras.Base;
using MauiControlsExtras.ContextMenu;
using MauiControlsExtras.Controls;

namespace DemoApp.Views;

public partial class TreeViewDemoPage : ContentPage
{
    private readonly TreeViewDemoViewModel _viewModel;

    public TreeViewDemoPage(TreeViewDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Set default picker selections
        accentColorPicker.SelectedIndex = 0;
        borderWidthPicker.SelectedIndex = 1;
    }

    private void OnSelectionChanged(object? sender, MauiControlsExtras.Base.SelectionChangedEventArgs e)
    {
        if (e.NewSelection is Models.FolderItem folder)
        {
            selectionStatusLabel.Text = $"Selected: {folder.Name}";
            _viewModel.UpdateStatus($"Selected: {folder.Name}");
        }
        else
        {
            selectionStatusLabel.Text = "Selected: (none)";
        }
    }

    private void OnShowCheckBoxesChanged(object? sender, CheckedChangedEventArgs e)
    {
        // Update is handled via binding
        _viewModel.UpdateStatus($"Show Checkboxes: {e.Value}");
    }

    private void OnShowLinesChanged(object? sender, CheckedChangedEventArgs e)
    {
        // Update is handled via binding
        _viewModel.UpdateStatus($"Show Tree Lines: {e.Value}");
    }

    private void OnExpandAllClicked(object? sender, EventArgs e)
    {
        interactiveTreeView.ExpandAll();
        _viewModel.UpdateStatus("Expanded all nodes");
    }

    private void OnCollapseAllClicked(object? sender, EventArgs e)
    {
        interactiveTreeView.CollapseAll();
        _viewModel.UpdateStatus("Collapsed all nodes");
    }

    private void OnItemChecked(object? sender, TreeViewItemEventArgs e)
    {
        // Count checked items
        var checkedItems = interactiveTreeView.GetCheckedItems().ToList();
        checkedItemsLabel.Text = $"Checked items: {checkedItems.Count}";
        _viewModel.UpdateStatus($"Item '{(e.Item as Models.FolderItem)?.Name}' check state changed. Total checked: {checkedItems.Count}");
    }

    private void OnContextMenuOpening(object? sender, ContextMenuOpeningEventArgs e)
    {
        // Cast to TreeView-specific event args for node info
        if (e is TreeViewContextMenuOpeningEventArgs treeArgs && treeArgs.Node != null)
        {
            var nodeName = treeArgs.DataItem is Models.FolderItem folder ? folder.Name : "Unknown";
            contextMenuStatusLabel.Text = $"Context menu opened for: {nodeName} (Level {treeArgs.Level})";

            // Add custom menu items
            e.Items.Add("Rename", () =>
            {
                _viewModel.UpdateStatus($"Rename clicked for: {nodeName}");
            }, "\uE8AC"); // Edit icon

            e.Items.Add("Delete", () =>
            {
                _viewModel.UpdateStatus($"Delete clicked for: {nodeName}");
            }, "\uE74D"); // Delete icon
        }
    }

    private void OnAccentColorChanged(object? sender, EventArgs e)
    {
        var color = accentColorPicker.SelectedIndex switch
        {
            0 => Color.FromArgb("#512BD4"), // Blue (Default)
            1 => Color.FromArgb("#4CAF50"), // Green
            2 => Color.FromArgb("#F44336"), // Red
            3 => Color.FromArgb("#9C27B0"), // Purple
            4 => Color.FromArgb("#FF9800"), // Orange
            _ => Color.FromArgb("#512BD4")
        };

        styledTreeView.AccentColor = color;
        _viewModel.UpdateStatus($"Accent color changed to: {accentColorPicker.SelectedItem}");
    }

    private void OnBorderWidthChanged(object? sender, EventArgs e)
    {
        var width = borderWidthPicker.SelectedIndex switch
        {
            0 => 0.0,
            1 => 1.0,
            2 => 2.0,
            3 => 3.0,
            _ => 1.0
        };

        styledTreeView.BorderThickness = width;
        _viewModel.UpdateStatus($"Border width changed to: {borderWidthPicker.SelectedItem}");
    }
}
