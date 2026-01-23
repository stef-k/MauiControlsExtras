# ComboBox Control

The ComboBox control provides a dropdown selection experience similar to WinForms ComboBox, with additional features for mobile and cross-platform scenarios.

## Features

- **Searchable Dropdown** - Built-in search with debounced input (100ms)
- **Complex Object Support** - Use `DisplayMemberPath` and `ValueMemberPath`
- **Image Support** - Display icons alongside text via `IconMemberPath`
- **Two-Way Binding** - Full support for MVVM patterns
- **Theme-Aware** - Automatic light/dark mode styling
- **Customizable** - Accent colors, placeholder text, visible item count

## Basic Usage

```xml
<extras:ComboBox ItemsSource="{Binding Items}"
                 SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 Placeholder="Select an item..." />
```

## Using with Complex Objects

When your items are complex objects, use property paths:

```csharp
public class Activity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
```

```xml
<extras:ComboBox ItemsSource="{Binding Activities}"
                 SelectedItem="{Binding SelectedActivity, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 ValueMemberPath="Id"
                 Placeholder="Select an activity..." />
```

## Displaying Icons

To show images alongside text, set `IconMemberPath`:

```csharp
public class IconOption
{
    public string DisplayName { get; set; }
    public string IconImageSource { get; set; } // Path to image in Resources/Raw
}
```

```xml
<extras:ComboBox ItemsSource="{Binding Icons}"
                 SelectedItem="{Binding SelectedIcon, Mode=TwoWay}"
                 DisplayMemberPath="DisplayName"
                 IconMemberPath="IconImageSource"
                 VisibleItemCount="6" />
```

> **Note:** Icon images should be placed in the `Resources/Raw` folder of your MAUI project.

## Default Values

Set a default selection when the control loads:

```xml
<extras:ComboBox ItemsSource="{Binding Priorities}"
                 SelectedItem="{Binding Priority, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 ValueMemberPath="Code"
                 DefaultValue="normal" />
```

The `DefaultValue` will match against (in order):
1. `ValueMemberPath` property value
2. `DisplayMemberPath` property value
3. The item itself

## Customizing Appearance

### Accent Color

Change the focus border color:

```xml
<extras:ComboBox AccentColor="#FF5722"
                 ... />
```

### Visible Items

Control dropdown height:

```xml
<extras:ComboBox VisibleItemCount="8"
                 ... />
```

## Programmatic Control

### Open/Close

```csharp
myComboBox.Open();
myComboBox.Close();
```

### Clear Selection

```csharp
myComboBox.ClearSelection();
```

### Refresh Items

```csharp
myComboBox.RefreshItems();
```

## Events

### SelectionChanged

Fired when the selection changes:

```csharp
private void ComboBox_SelectionChanged(object sender, object? e)
{
    if (e is MyItem item)
    {
        // Handle selection
    }
}
```

### Opened/Closed

Track dropdown state:

```csharp
private void ComboBox_Opened(object sender, EventArgs e)
{
    // Dropdown opened
}

private void ComboBox_Closed(object sender, EventArgs e)
{
    // Dropdown closed
}
```

## Styling

The ComboBox automatically adapts to light/dark themes. Key colors:

| Element | Light Theme | Dark Theme |
|---------|-------------|------------|
| Background | White | #424242 |
| Border | #BDBDBD | #9E9E9E |
| Text | #212121 | White |
| Placeholder | #6B7280 | #9CA3AF |
| Focus Border | AccentColor | AccentColor |

## Best Practices

1. **Use DisplayMemberPath** for complex objects instead of overriding ToString()
2. **Keep VisibleItemCount reasonable** (5-8 items) for mobile
3. **Provide meaningful Placeholder text** to guide users
4. **Use ValueMemberPath** when binding to IDs or codes
5. **Handle null selection** gracefully in your ViewModel
