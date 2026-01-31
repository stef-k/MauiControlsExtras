# TreeView API Reference

Full API documentation for the `MauiControlsExtras.Controls.TreeView` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class TreeView : ListStyledControlBase, IKeyboardNavigable
```

## Inheritance

Inherits from [ListStyledControlBase](base-classes.md#liststyledcontrolbase). See base class documentation for inherited styling and list properties.

## Interfaces

- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### ItemsSource

Gets or sets the root items collection.

```csharp
public IEnumerable? ItemsSource { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `IEnumerable?` | `null` | Yes |

---

#### ChildrenPath

Gets or sets the property path to child items.

```csharp
public string ChildrenPath { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Children"` | Yes |

---

#### DisplayMemberPath

Gets or sets the property path for display text.

```csharp
public string? DisplayMemberPath { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### IconMemberPath

Gets or sets the property path for item icons.

```csharp
public string? IconMemberPath { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### SelectedItem

Gets or sets the currently selected item.

```csharp
public object? SelectedItem { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `object?` | `null` | Yes | TwoWay |

---

#### SelectedItems

Gets or sets the selected items collection for multi-select mode.

```csharp
public IList? SelectedItems { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `IList?` | `null` | Yes | TwoWay |

---

#### SelectionMode

Gets or sets the selection mode.

```csharp
public TreeViewSelectionMode SelectionMode { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `TreeViewSelectionMode` | `Single` | Yes |

---

#### ItemTemplate

Gets or sets the custom item template.

```csharp
public DataTemplate? ItemTemplate { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `DataTemplate?` | `null` | Yes |

---

### Appearance Properties

#### IndentSize

Gets or sets the indent size in pixels per level.

```csharp
public double IndentSize { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `20` | Yes |

---

#### ShowLines

Gets or sets whether to show tree connector lines.

```csharp
public bool ShowLines { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### ShowCheckBoxes

Gets or sets whether to show selection checkboxes.

```csharp
public bool ShowCheckBoxes { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### CheckBoxMode

Gets or sets the checkbox behavior mode.

```csharp
public CheckBoxMode CheckBoxMode { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `CheckBoxMode` | `Independent` | Yes |

---

### Lazy Loading Properties

#### LoadChildrenCommand

Gets or sets the command for lazy loading children.

```csharp
public ICommand? LoadChildrenCommand { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `ICommand?` | `null` | Yes |

---

#### IsExpandedPath

Gets or sets the property path to bind expanded state.

```csharp
public string? IsExpandedPath { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### HasChildrenPath

Gets or sets the property path to determine if item has children (for lazy loading).

```csharp
public string? HasChildrenPath { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

### State Properties

#### FlattenedItems (Read-only)

Gets the flattened items collection for display.

```csharp
public ObservableCollection<TreeViewNode> FlattenedItems { get; }
```

---

#### FocusedNode (Read-only)

Gets the currently focused node.

```csharp
public TreeViewNode? FocusedNode { get; }
```

---

## Events

### SelectionChanged

Occurs when the selection changes.

```csharp
public event EventHandler<object?>? SelectionChanged;
```

---

### ItemExpanded

Occurs when an item is expanded.

```csharp
public event EventHandler<TreeViewItemEventArgs>? ItemExpanded;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Item | `object` | The data item |
| Node | `TreeViewNode` | The tree view node |

---

### ItemCollapsed

Occurs when an item is collapsed.

```csharp
public event EventHandler<TreeViewItemEventArgs>? ItemCollapsed;
```

---

### ItemTapped

Occurs when an item is tapped.

```csharp
public event EventHandler<TreeViewItemEventArgs>? ItemTapped;
```

---

### ItemDoubleTapped

Occurs when an item is double-tapped.

```csharp
public event EventHandler<TreeViewItemEventArgs>? ItemDoubleTapped;
```

---

### ItemChecked

Occurs when an item's check state changes.

```csharp
public event EventHandler<TreeViewItemEventArgs>? ItemChecked;
```

---

## Commands

### SelectionChangedCommand

Executed when selection changes.

```csharp
public ICommand? SelectionChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Item | `object?` |

---

### SelectionChangedCommandParameter

Gets or sets the parameter for SelectionChangedCommand.

```csharp
public object? SelectionChangedCommandParameter { get; set; }
```

---

### ItemExpandedCommand

Executed when an item is expanded.

```csharp
public ICommand? ItemExpandedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Item | `object` |

---

### ItemCollapsedCommand

Executed when an item is collapsed.

```csharp
public ICommand? ItemCollapsedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Item | `object` |

---

### ItemTappedCommand

Executed when an item is tapped.

```csharp
public ICommand? ItemTappedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Item | `object` |

---

### ItemDoubleTappedCommand

Executed when an item is double-tapped.

```csharp
public ICommand? ItemDoubleTappedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Item | `object` |

---

### ItemCheckedCommand

Executed when an item's check state changes.

```csharp
public ICommand? ItemCheckedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Item | `object` |

---

## Methods

### ExpandAll()

Expands all nodes in the tree.

```csharp
public void ExpandAll()
```

---

### CollapseAll()

Collapses all nodes in the tree.

```csharp
public void CollapseAll()
```

---

### ExpandTo(object item)

Expands the path to a specific item.

```csharp
public void ExpandTo(object item)
```

---

### ScrollTo(object item)

Scrolls to make an item visible.

```csharp
public void ScrollTo(object item)
```

---

### GetCheckedItems()

Gets all checked items.

```csharp
public IEnumerable<object> GetCheckedItems()
```

---

### SetChecked(object item, bool isChecked)

Sets the check state for an item.

```csharp
public void SetChecked(object item, bool isChecked)
```

---

## Enumerations

### TreeViewSelectionMode

```csharp
public enum TreeViewSelectionMode
{
    None,       // No selection allowed
    Single,     // Single item selection
    Multiple    // Multiple item selection
}
```

### CheckBoxMode

```csharp
public enum CheckBoxMode
{
    Independent,   // Checkboxes are independent
    Cascade,       // Checking parent checks all children
    TriState       // Parent shows partial state when some children checked
}
```

### CheckState

```csharp
public enum CheckState
{
    Unchecked,      // Unchecked state
    Checked,        // Checked state
    Indeterminate   // Indeterminate state (some children checked)
}
```

---

## Supporting Types

### TreeViewNode

Represents a node in the flattened tree view.

```csharp
public class TreeViewNode : INotifyPropertyChanged
{
    public object DataItem { get; }
    public TreeViewNode? Parent { get; }
    public int Level { get; }
    public ObservableCollection<TreeViewNode> Children { get; }
    public bool IsExpanded { get; set; }
    public bool IsSelected { get; set; }
    public CheckState CheckState { get; set; }
    public ImageSource? Icon { get; set; }
    public bool HasChildren { get; }
    public bool HasPotentialChildren { get; set; }
    public bool ChildrenLoaded { get; set; }
}
```

### TreeViewItemEventArgs

```csharp
public class TreeViewItemEventArgs : EventArgs
{
    public object Item { get; }
    public TreeViewNode Node { get; }
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Arrow Up | Move to previous item |
| Arrow Down | Move to next item |
| Arrow Left | Collapse node or move to parent |
| Arrow Right | Expand node or move to first child |
| Home | Move to first item |
| End | Move to last item |
| Page Up | Move up one page |
| Page Down | Move down one page |
| Enter | Expand/collapse node |
| Space | Toggle selection or checkbox |
| + | Expand current node |
| - | Collapse current node |
| * | Expand current node and all children |

---

## Usage Examples

### Basic TreeView

```xml
<extras:TreeView ItemsSource="{Binding RootNodes}"
                 ChildrenPath="Children"
                 DisplayMemberPath="Name"
                 SelectedItem="{Binding SelectedNode}" />
```

### With Checkboxes

```xml
<extras:TreeView ItemsSource="{Binding Categories}"
                 ChildrenPath="SubCategories"
                 DisplayMemberPath="Name"
                 ShowCheckBoxes="True"
                 CheckBoxMode="TriState"
                 ItemCheckedCommand="{Binding OnItemCheckedCommand}" />
```

### With Custom Template

```xml
<extras:TreeView ItemsSource="{Binding Files}"
                 ChildrenPath="Children"
                 SelectionMode="Multiple"
                 SelectedItems="{Binding SelectedFiles}">
    <extras:TreeView.ItemTemplate>
        <DataTemplate>
            <HorizontalStackLayout Spacing="8">
                <Image Source="{Binding Icon}" WidthRequest="16" HeightRequest="16" />
                <Label Text="{Binding Name}" />
                <Label Text="{Binding Size}" FontSize="10" TextColor="Gray" />
            </HorizontalStackLayout>
        </DataTemplate>
    </extras:TreeView.ItemTemplate>
</extras:TreeView>
```

### With Lazy Loading

```xml
<extras:TreeView ItemsSource="{Binding RootFolders}"
                 ChildrenPath="SubFolders"
                 DisplayMemberPath="Name"
                 HasChildrenPath="HasSubFolders"
                 LoadChildrenCommand="{Binding LoadSubFoldersCommand}" />
```

### With Icons

```xml
<extras:TreeView ItemsSource="{Binding TreeNodes}"
                 ChildrenPath="Children"
                 DisplayMemberPath="Name"
                 IconMemberPath="IconPath"
                 IndentSize="24"
                 ShowLines="True" />
```

### Code-Behind

```csharp
// Create tree view
var treeView = new TreeView
{
    ChildrenPath = "Children",
    DisplayMemberPath = "Name",
    SelectionMode = TreeViewSelectionMode.Single,
    ShowCheckBoxes = true,
    CheckBoxMode = CheckBoxMode.Cascade,
    IndentSize = 20
};

// Set data source
treeView.ItemsSource = GetRootNodes();

// Handle events
treeView.SelectionChanged += (sender, item) =>
{
    Console.WriteLine($"Selected: {item}");
};

treeView.ItemExpanded += (sender, args) =>
{
    Console.WriteLine($"Expanded: {args.Item}");
};

treeView.ItemChecked += (sender, args) =>
{
    Console.WriteLine($"Checked: {args.Node.CheckState}");
};

// Programmatic control
treeView.ExpandAll();
treeView.CollapseAll();
treeView.ExpandTo(specificItem);
treeView.ScrollTo(targetItem);

// Get checked items
var checkedItems = treeView.GetCheckedItems().ToList();

// Set checked state
treeView.SetChecked(item, true);
```

### MVVM Pattern

```csharp
// ViewModel
public class FileExplorerViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<FolderNode> _rootFolders;

    [ObservableProperty]
    private FolderNode? _selectedFolder;

    [RelayCommand]
    private async Task LoadSubFolders(FolderNode folder)
    {
        // Lazy load children
        var children = await _fileService.GetSubFoldersAsync(folder.Path);
        folder.SubFolders = new ObservableCollection<FolderNode>(children);
    }

    [RelayCommand]
    private void OnItemExpanded(object item)
    {
        if (item is FolderNode folder)
        {
            LogAnalytics("folder_expanded", folder.Path);
        }
    }

    [RelayCommand]
    private void OnItemChecked(object item)
    {
        // Update selection state
        RefreshSelectedCount();
    }
}
```

