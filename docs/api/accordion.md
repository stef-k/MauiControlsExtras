# Accordion API Reference

Full API documentation for the `MauiControlsExtras.Controls.Accordion` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class Accordion : StyledControlBase, IKeyboardNavigable
```

## Inheritance

Inherits from [StyledControlBase](base-classes.md#styledcontrolbase). See base class documentation for inherited styling properties.

## Interfaces

- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### Items

Gets the collection of accordion items.

```csharp
public AccordionItemCollection Items { get; }
```

| Type | Bindable |
|------|----------|
| `AccordionItemCollection` | No (collection property) |

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

#### ItemTemplate

Gets or sets the template for accordion item content.

```csharp
public DataTemplate? ItemTemplate { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `DataTemplate?` | `null` | Yes |

---

#### HeaderTemplate

Gets or sets the template for accordion item headers.

```csharp
public DataTemplate? HeaderTemplate { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `DataTemplate?` | `null` | Yes |

---

#### ExpandMode

Gets or sets whether multiple items can be expanded simultaneously.

```csharp
public AccordionExpandMode ExpandMode { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `AccordionExpandMode` | `Single` | Yes |

---

#### SelectedItem

Gets or sets the currently selected/expanded item.

```csharp
public object? SelectedItem { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `object?` | `null` | Yes | TwoWay |

---

#### SelectedIndex

Gets or sets the index of the selected item.

```csharp
public int SelectedIndex { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `int` | `-1` | Yes | TwoWay |

---

### Appearance Properties

#### IconPosition

Gets or sets the position of the expand/collapse icon.

```csharp
public ExpandIconPosition IconPosition { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `ExpandIconPosition` | `Right` | Yes |

---

#### ExpandedIcon

Gets or sets the icon for expanded state.

```csharp
public string ExpandedIcon { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"▼"` | Yes |

---

#### CollapsedIcon

Gets or sets the icon for collapsed state.

```csharp
public string CollapsedIcon { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"▶"` | Yes |

---

#### HeaderHeight

Gets or sets the height of item headers.

```csharp
public double HeaderHeight { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `48` | Yes |

---

#### HeaderBackgroundColor

Gets or sets the header background color.

```csharp
public Color? HeaderBackgroundColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses theme) | Yes |

---

#### HeaderTextColor

Gets or sets the header text color.

```csharp
public Color? HeaderTextColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses foreground) | Yes |

---

#### ExpandedHeaderBackgroundColor

Gets or sets the header background when expanded.

```csharp
public Color? ExpandedHeaderBackgroundColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` | Yes |

---

#### ItemSpacing

Gets or sets the spacing between items.

```csharp
public double ItemSpacing { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `0` | Yes |

---

### Behavior Properties

#### AnimateExpansion

Gets or sets whether expansion/collapse is animated.

```csharp
public bool AnimateExpansion { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### AnimationDuration

Gets or sets the animation duration in milliseconds.

```csharp
public int AnimationDuration { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `250` | Yes |

---

#### AllowCollapseAll

Gets or sets whether all items can be collapsed (only applies to Single mode).

```csharp
public bool AllowCollapseAll { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

## Events

### ItemExpanded

Occurs when an item is expanded.

```csharp
public event EventHandler<AccordionItemEventArgs>? ItemExpanded;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Item | `object` | The expanded item |
| Index | `int` | Index of the item |

---

### ItemCollapsed

Occurs when an item is collapsed.

```csharp
public event EventHandler<AccordionItemEventArgs>? ItemCollapsed;
```

---

### SelectionChanged

Occurs when the selected item changes.

```csharp
public event EventHandler<AccordionSelectionChangedEventArgs>? SelectionChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| OldItem | `object?` | Previously selected item |
| NewItem | `object?` | Newly selected item |
| OldIndex | `int` | Previous selection index |
| NewIndex | `int` | New selection index |

---

## Commands

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

### SelectionChangedCommand

Executed when selection changes.

```csharp
public ICommand? SelectionChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `AccordionSelectionChangedEventArgs` |

---

## Methods

### ExpandItem(int index)

Expands the item at the specified index.

```csharp
public void ExpandItem(int index)
```

---

### CollapseItem(int index)

Collapses the item at the specified index.

```csharp
public void CollapseItem(int index)
```

---

### ToggleItem(int index)

Toggles the expansion state of the item at the specified index.

```csharp
public void ToggleItem(int index)
```

---

### ExpandAll()

Expands all items (only works in Multiple mode).

```csharp
public void ExpandAll()
```

---

### CollapseAll()

Collapses all items.

```csharp
public void CollapseAll()
```

---

### IsExpanded(int index)

Returns whether the item at the specified index is expanded.

```csharp
public bool IsExpanded(int index)
```

---

## Enumerations

### AccordionExpandMode

```csharp
public enum AccordionExpandMode
{
    Single,    // Only one item can be expanded at a time
    Multiple   // Multiple items can be expanded simultaneously
}
```

### ExpandIconPosition

```csharp
public enum ExpandIconPosition
{
    Left,   // Icon on the left side of header
    Right,  // Icon on the right side of header
    None    // No expand/collapse icon
}
```

---

## Supporting Types

### AccordionItem

Represents a single item in the accordion.

```csharp
public class AccordionItem : ContentView
{
    // Properties
    public string? Header { get; set; }
    public View? HeaderContent { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsEnabled { get; set; }
    public string? Icon { get; set; }

    // Content is inherited from ContentView
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Arrow Up | Move focus to previous item |
| Arrow Down | Move focus to next item |
| Enter/Space | Toggle focused item expansion |
| Home | Move focus to first item |
| End | Move focus to last item |

---

## Usage Examples

### Basic Usage

```xml
<extras:Accordion>
    <extras:AccordionItem Header="Section 1">
        <Label Text="Content for section 1" />
    </extras:AccordionItem>
    <extras:AccordionItem Header="Section 2">
        <Label Text="Content for section 2" />
    </extras:AccordionItem>
    <extras:AccordionItem Header="Section 3">
        <Label Text="Content for section 3" />
    </extras:AccordionItem>
</extras:Accordion>
```

### Data Bound

```xml
<extras:Accordion ItemsSource="{Binding Sections}"
                  SelectedItem="{Binding SelectedSection}"
                  ItemExpandedCommand="{Binding OnSectionExpandedCommand}">
    <extras:Accordion.HeaderTemplate>
        <DataTemplate>
            <HorizontalStackLayout Spacing="8">
                <Label Text="{Binding Icon}" FontSize="16" />
                <Label Text="{Binding Title}" FontSize="14" FontAttributes="Bold" />
            </HorizontalStackLayout>
        </DataTemplate>
    </extras:Accordion.HeaderTemplate>
    <extras:Accordion.ItemTemplate>
        <DataTemplate>
            <Label Text="{Binding Content}" Padding="16" />
        </DataTemplate>
    </extras:Accordion.ItemTemplate>
</extras:Accordion>
```

### Multiple Expansion Mode

```xml
<extras:Accordion ExpandMode="Multiple">
    <extras:AccordionItem Header="FAQ 1" IsExpanded="True">
        <Label Text="Answer 1" />
    </extras:AccordionItem>
    <extras:AccordionItem Header="FAQ 2" IsExpanded="True">
        <Label Text="Answer 2" />
    </extras:AccordionItem>
</extras:Accordion>
```

### Custom Appearance

```xml
<extras:Accordion IconPosition="Left"
                  ExpandedIcon="−"
                  CollapsedIcon="+"
                  HeaderHeight="56"
                  HeaderBackgroundColor="#F5F5F5"
                  ExpandedHeaderBackgroundColor="#E3F2FD"
                  HeaderTextColor="#212121"
                  ItemSpacing="4"
                  CornerRadius="8">
    <!-- Items -->
</extras:Accordion>
```

### Without Animation

```xml
<extras:Accordion AnimateExpansion="False">
    <!-- Items -->
</extras:Accordion>
```

### Code-Behind

```csharp
// Create accordion
var accordion = new Accordion
{
    ExpandMode = AccordionExpandMode.Single,
    AnimateExpansion = true,
    AnimationDuration = 300
};

// Add items
accordion.Items.Add(new AccordionItem
{
    Header = "Getting Started",
    Content = new Label { Text = "Welcome to the app..." }
});

accordion.Items.Add(new AccordionItem
{
    Header = "Features",
    Content = new VerticalStackLayout
    {
        Children =
        {
            new Label { Text = "Feature 1" },
            new Label { Text = "Feature 2" }
        }
    }
});

// Handle events
accordion.ItemExpanded += (sender, args) =>
{
    Console.WriteLine($"Expanded: {args.Index}");
};

// Programmatic control
accordion.ExpandItem(0);
accordion.ToggleItem(1);
bool isExpanded = accordion.IsExpanded(0);
```
