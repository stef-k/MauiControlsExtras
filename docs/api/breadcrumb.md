# Breadcrumb API Reference

Full API documentation for the `MauiControlsExtras.Controls.Breadcrumb` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class Breadcrumb : StyledControlBase, IKeyboardNavigable
```

## Inheritance

Inherits from [StyledControlBase](base-classes.md#styledcontrolbase). See base class documentation for inherited styling properties.

## Interfaces

- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### Items

Gets the collection of breadcrumb items.

```csharp
public BreadcrumbItemCollection Items { get; }
```

| Type | Bindable |
|------|----------|
| `BreadcrumbItemCollection` | No (collection property) |

---

#### ItemsSource

Gets or sets the source collection for data binding.

```csharp
public IEnumerable? ItemsSource { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `IEnumerable?` | `null` | Yes |

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

#### SelectedItem

Gets or sets the currently selected (navigated to) item.

```csharp
public object? SelectedItem { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `object?` | `null` | Yes | TwoWay |

---

### Appearance Properties

#### Separator

Gets or sets the separator text or character between items.

```csharp
public string Separator { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"/"` | Yes |

---

#### SeparatorColor

Gets or sets the separator text color.

```csharp
public Color? SeparatorColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses muted foreground) | Yes |

---

#### ShowHomeIcon

Gets or sets whether a home icon is shown at the start.

```csharp
public bool ShowHomeIcon { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### HomeIcon

Gets or sets the home icon glyph character.

```csharp
public string HomeIcon { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"🏠"` | Yes |

---

#### MaxVisibleItems

Gets or sets the maximum number of visible items before collapsing.

```csharp
public int MaxVisibleItems { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `0` (no limit) | Yes |

---

#### EllipsisText

Gets or sets the text shown for collapsed items.

```csharp
public string EllipsisText { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"..."` | Yes |

---

#### ItemFontSize

Gets or sets the font size for breadcrumb items.

```csharp
public double ItemFontSize { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `14` | Yes |

---

#### CurrentItemColor

Gets or sets the color of the current (last) item.

```csharp
public Color? CurrentItemColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses foreground) | Yes |

---

#### LinkColor

Gets or sets the color of clickable items.

```csharp
public Color? LinkColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses accent) | Yes |

---

#### IsCurrentItemClickable

Gets or sets whether the last item is clickable.

```csharp
public bool IsCurrentItemClickable { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

## Events

### ItemClicked

Occurs when a breadcrumb item is clicked.

```csharp
public event EventHandler<BreadcrumbItemClickedEventArgs>? ItemClicked;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Item | `object` | The clicked item |
| Index | `int` | Index of the clicked item |
| IsHome | `bool` | Whether the home icon was clicked |

---

### NavigationRequested

Occurs when navigation to a breadcrumb item is requested.

```csharp
public event EventHandler<BreadcrumbNavigationEventArgs>? NavigationRequested;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| TargetItem | `object` | Item to navigate to |
| Path | `IReadOnlyList<object>` | Full path to the target |
| Cancel | `bool` | Set to true to cancel navigation |

---

## Commands

### ItemClickedCommand

Executed when an item is clicked.

```csharp
public ICommand? ItemClickedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `BreadcrumbItemClickedEventArgs` |

---

### NavigateCommand

Executed when navigation is requested.

```csharp
public ICommand? NavigateCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Item | `object` |

---

## Methods

### NavigateTo(object item)

Navigates to the specified item, removing items after it.

```csharp
public void NavigateTo(object item)
```

---

### NavigateTo(int index)

Navigates to the item at the specified index.

```csharp
public void NavigateTo(int index)
```

---

### Push(object item)

Adds a new item to the end of the breadcrumb trail.

```csharp
public void Push(object item)
```

---

### Pop()

Removes and returns the last item from the breadcrumb trail.

```csharp
public object? Pop()
```

---

### Clear()

Clears all items from the breadcrumb trail.

```csharp
public void Clear()
```

---

## Supporting Types

### BreadcrumbItem

Represents a single item in the breadcrumb trail.

```csharp
public class BreadcrumbItem
{
    public string Text { get; set; }
    public object? Data { get; set; }
    public string? Icon { get; set; }
    public bool IsEnabled { get; set; } = true;
}
```

### BreadcrumbItemCollection

A collection of BreadcrumbItem objects with helper methods.

```csharp
public class BreadcrumbItemCollection : ObservableCollection<BreadcrumbItem>
{
    void Add(string text, object? data = null);
    void Push(BreadcrumbItem item);
    BreadcrumbItem? Pop();
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Arrow Left | Navigate to previous item |
| Arrow Right | Navigate to next item |
| Home | Navigate to first item |
| End | Navigate to last item |
| Enter | Select focused item |
| Backspace | Navigate up one level (pop) |

---

## Usage Examples

### Basic Usage

```xml
<extras:Breadcrumb ItemClicked="OnBreadcrumbItemClicked">
    <extras:Breadcrumb.Items>
        <extras:BreadcrumbItem Text="Home" Data="home" />
        <extras:BreadcrumbItem Text="Products" Data="products" />
        <extras:BreadcrumbItem Text="Electronics" Data="electronics" />
    </extras:Breadcrumb.Items>
</extras:Breadcrumb>
```

### Data Bound

```xml
<extras:Breadcrumb ItemsSource="{Binding NavigationPath}"
                   DisplayMemberPath="Name"
                   SelectedItem="{Binding CurrentLocation}"
                   NavigateCommand="{Binding NavigateToCommand}" />
```

### Custom Appearance

```xml
<extras:Breadcrumb Separator="›"
                   SeparatorColor="#9E9E9E"
                   ShowHomeIcon="True"
                   HomeIcon="🏠"
                   LinkColor="#1976D2"
                   CurrentItemColor="#212121"
                   ItemFontSize="16" />
```

### With Item Limit

```xml
<!-- Only show first, ellipsis, and last 2 items when path is long -->
<extras:Breadcrumb ItemsSource="{Binding Path}"
                   MaxVisibleItems="4"
                   EllipsisText="..." />
```

### Code-Behind

```csharp
// Create breadcrumb
var breadcrumb = new Breadcrumb
{
    Separator = "/",
    ShowHomeIcon = true
};

// Add items
breadcrumb.Items.Add("Home", "/");
breadcrumb.Items.Add("Documents", "/documents");
breadcrumb.Items.Add("Projects", "/documents/projects");

// Handle navigation
breadcrumb.ItemClicked += (sender, args) =>
{
    if (args.IsHome)
    {
        NavigateToHome();
    }
    else
    {
        NavigateTo(args.Item);
    }
};

// Programmatic navigation
breadcrumb.Push(new BreadcrumbItem { Text = "Report.pdf", Data = "/documents/projects/report.pdf" });
breadcrumb.NavigateTo(1); // Go back to "Documents"
breadcrumb.Pop(); // Remove last item
```

### MVVM Pattern

```csharp
// ViewModel
public class NavigationViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<PathItem> _navigationPath = new();

    [RelayCommand]
    private void NavigateTo(object item)
    {
        if (item is PathItem pathItem)
        {
            // Find index and truncate path
            var index = NavigationPath.IndexOf(pathItem);
            while (NavigationPath.Count > index + 1)
            {
                NavigationPath.RemoveAt(NavigationPath.Count - 1);
            }

            // Load content for this path
            LoadContent(pathItem.Path);
        }
    }
}
```
