# ComboBox API Reference

Full API documentation for the `MauiControlsExtras.Controls.ComboBox` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class ComboBox : ContentView
```

## Properties

### ItemsSource

Gets or sets the collection of items to display in the dropdown.

```csharp
public IEnumerable? ItemsSource { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `IEnumerable` | `null` | Yes |

---

### SelectedItem

Gets or sets the currently selected item.

```csharp
public object? SelectedItem { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `object` | `null` | Yes | TwoWay |

---

### SelectedValue

Gets or sets the selected value based on `ValueMemberPath`.

```csharp
public object? SelectedValue { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `object` | `null` | Yes | TwoWay |

---

### DisplayMemberPath

Gets or sets the property path to use for display text.

```csharp
public string? DisplayMemberPath { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `null` | Yes |

---

### ValueMemberPath

Gets or sets the property path to use for the selected value.

```csharp
public string? ValueMemberPath { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `null` | Yes |

---

### IconMemberPath

Gets or sets the property path to use for item icons.

```csharp
public string? IconMemberPath { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `null` | Yes |

---

### Placeholder

Gets or sets the placeholder text shown when no item is selected.

```csharp
public string Placeholder { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Select an item"` | Yes |

---

### DefaultValue

Gets or sets the default value to select when items are loaded.

```csharp
public object? DefaultValue { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `object` | `null` | Yes |

---

### VisibleItemCount

Gets or sets the number of items visible in the dropdown without scrolling.

```csharp
public int VisibleItemCount { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `5` | Yes |

---

### AccentColor

Gets or sets the accent color used for focus indication.

```csharp
public Color AccentColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color` | `#0078D4` | Yes |

---

### HasSelection (Read-only)

Gets whether an item is currently selected.

```csharp
public bool HasSelection { get; }
```

| Type | Bindable |
|------|----------|
| `bool` | Yes |

---

### IsExpanded (Read-only)

Gets whether the dropdown is currently expanded.

```csharp
public bool IsExpanded { get; }
```

| Type | Bindable |
|------|----------|
| `bool` | No |

---

## Events

### SelectionChanged

Occurs when the selected item changes.

```csharp
public event EventHandler<object?>? SelectionChanged;
```

**Event Args:** The newly selected item, or `null` if selection was cleared.

---

### Opened

Occurs when the dropdown opens.

```csharp
public event EventHandler? Opened;
```

---

### Closed

Occurs when the dropdown closes.

```csharp
public event EventHandler? Closed;
```

---

## Methods

### Open()

Programmatically opens the dropdown.

```csharp
public void Open()
```

---

### Close()

Programmatically closes the dropdown.

```csharp
public void Close()
```

---

### ClearSelection()

Clears the current selection.

```csharp
public void ClearSelection()
```

---

### RefreshItems()

Refreshes the dropdown items from the current ItemsSource.

```csharp
public void RefreshItems()
```

---

## Usage Examples

### Basic Binding

```xml
<extras:ComboBox ItemsSource="{Binding Items}"
                 SelectedItem="{Binding Selected, Mode=TwoWay}"
                 DisplayMemberPath="Name" />
```

### Full Featured

```xml
<extras:ComboBox ItemsSource="{Binding Options}"
                 SelectedItem="{Binding SelectedOption, Mode=TwoWay}"
                 SelectedValue="{Binding SelectedOptionId, Mode=TwoWay}"
                 DisplayMemberPath="DisplayName"
                 ValueMemberPath="Id"
                 IconMemberPath="IconPath"
                 Placeholder="Choose an option..."
                 DefaultValue="default"
                 VisibleItemCount="6"
                 AccentColor="#FF5722"
                 SelectionChanged="OnSelectionChanged"
                 Opened="OnDropdownOpened"
                 Closed="OnDropdownClosed" />
```

### Code-Behind

```csharp
// Open dropdown
myComboBox.Open();

// Close dropdown
myComboBox.Close();

// Clear selection
myComboBox.ClearSelection();

// Check state
if (myComboBox.HasSelection)
{
    var item = myComboBox.SelectedItem;
}
```
