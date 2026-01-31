# PropertyGrid API Reference

Full API documentation for the `MauiControlsExtras.Controls.PropertyGrid` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class PropertyGrid : HeaderedControlBase, IKeyboardNavigable
```

## Inheritance

Inherits from [HeaderedControlBase](base-classes.md#headeredcontrolbase). See base class documentation for inherited styling and header properties.

## Interfaces

- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### SelectedObject

Gets or sets the object to edit.

```csharp
public object? SelectedObject { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `object?` | `null` | Yes | TwoWay |

---

#### IsReadOnly

Gets or sets whether the property grid is read-only.

```csharp
public bool IsReadOnly { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### SortMode

Gets or sets the property sort mode.

```csharp
public PropertySortMode SortMode { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `PropertySortMode` | `Categorized` | Yes |

---

#### SearchText

Gets or sets the search filter text.

```csharp
public string? SearchText { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

### Display Properties

#### ShowCategories

Gets or sets whether properties are grouped by category.

```csharp
public bool ShowCategories { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### ShowDescription

Gets or sets whether the description panel is shown.

```csharp
public bool ShowDescription { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### ShowSearchBox

Gets or sets whether the search box is shown.

```csharp
public bool ShowSearchBox { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### ExpandAllCategories

Gets or sets whether all categories start expanded.

```csharp
public bool ExpandAllCategories { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

### State Properties

#### HasSearchText (Read-only)

Gets whether there is search text.

```csharp
public bool HasSearchText { get; }
```

---

#### Categories (Read-only)

Gets the property categories.

```csharp
public IReadOnlyList<PropertyCategory> Categories { get; }
```

---

#### SelectedProperty (Read-only)

Gets the currently selected property.

```csharp
public PropertyItem? SelectedProperty { get; }
```

---

## Events

### PropertyValueChanged

Occurs when a property value changes.

```csharp
public event EventHandler<PropertyValueChangedEventArgs>? PropertyValueChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| PropertyName | `string` | Name of the changed property |
| OldValue | `object?` | Previous value |
| NewValue | `object?` | New value |

---

### PropertyValueChanging

Occurs before a property value changes (cancelable).

```csharp
public event EventHandler<PropertyValueChangingEventArgs>? PropertyValueChanging;
```

---

### SelectedObjectChanged

Occurs when the selected object changes.

```csharp
public event EventHandler<SelectedObjectChangedEventArgs>? SelectedObjectChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| OldObject | `object?` | Previous object |
| NewObject | `object?` | New object |

---

### PropertySelectionChanged

Occurs when the property selection changes.

```csharp
public event EventHandler<PropertySelectionChangedEventArgs>? PropertySelectionChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| OldProperty | `PropertyItem?` | Previously selected property |
| NewProperty | `PropertyItem?` | Newly selected property |

---

## Commands

### PropertyChangedCommand

Executed when a property value changes.

```csharp
public ICommand? PropertyChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `PropertyValueChangedEventArgs` |

---

### SelectedObjectChangedCommand

Executed when the selected object changes.

```csharp
public ICommand? SelectedObjectChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `SelectedObjectChangedEventArgs` |

---

### PropertySelectionChangedCommand

Executed when property selection changes.

```csharp
public ICommand? PropertySelectionChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `PropertySelectionChangedEventArgs` |

---

## Methods

### Refresh()

Refreshes the property grid from the current object.

```csharp
public void Refresh()
```

---

### ExpandAll()

Expands all categories.

```csharp
public void ExpandAll()
```

---

### CollapseAll()

Collapses all categories.

```csharp
public void CollapseAll()
```

---

## Enumerations

### PropertySortMode

```csharp
public enum PropertySortMode
{
    Categorized,   // Properties are grouped by category
    Alphabetical,  // Properties are sorted alphabetically
    None           // No sorting, properties appear in declaration order
}
```

---

## Supporting Types

### PropertyCategory

Represents a category of properties.

```csharp
public class PropertyCategory
{
    public string Name { get; }
    public IReadOnlyList<PropertyItem> Properties { get; }
    public bool IsExpanded { get; set; }
}
```

### PropertyItem

Represents a single property.

```csharp
public class PropertyItem : INotifyPropertyChanged
{
    public string Name { get; }
    public string DisplayName { get; }
    public string? Description { get; }
    public string Category { get; }
    public Type PropertyType { get; }
    public string TypeName { get; }
    public object? Value { get; set; }
    public string DisplayValue { get; }
    public bool IsReadOnly { get; }
    public bool IsSelected { get; set; }
    public bool IsExpandable { get; }
    public bool IsExpanded { get; set; }
    public object? Minimum { get; }
    public object? Maximum { get; }
    public Type? EditorType { get; }
}
```

### IPropertyEditor

Interface for custom property editors.

```csharp
public interface IPropertyEditor
{
    View CreateEditor(PropertyItem property);
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Arrow Up | Select previous property |
| Arrow Down | Select next property |
| Arrow Left | Collapse property |
| Arrow Right | Expand property |
| Home | Select first property |
| End | Select last property |

---

## Usage Examples

### Basic Usage

```xml
<extras:PropertyGrid SelectedObject="{Binding SelectedItem}"
                     PropertyChangedCommand="{Binding OnPropertyChangedCommand}" />
```

### Categorized View

```xml
<extras:PropertyGrid SelectedObject="{Binding Settings}"
                     SortMode="Categorized"
                     ShowCategories="True"
                     ShowDescription="True"
                     ExpandAllCategories="True" />
```

### Alphabetical View

```xml
<extras:PropertyGrid SelectedObject="{Binding Settings}"
                     SortMode="Alphabetical"
                     ShowCategories="False" />
```

### With Search

```xml
<extras:PropertyGrid SelectedObject="{Binding Configuration}"
                     ShowSearchBox="True"
                     SearchText="{Binding PropertyFilter}" />
```

### Read-Only Mode

```xml
<extras:PropertyGrid SelectedObject="{Binding ReadOnlyData}"
                     IsReadOnly="True"
                     ShowDescription="True" />
```

### Custom Appearance

```xml
<extras:PropertyGrid SelectedObject="{Binding Item}"
                     HeaderText="Properties"
                     HeaderFontSize="16"
                     HeaderFontAttributes="Bold"
                     ShowHeaderDivider="True"
                     CornerRadius="8"
                     AccentColor="#2196F3" />
```

### Code-Behind

```csharp
// Create property grid
var propertyGrid = new PropertyGrid
{
    SortMode = PropertySortMode.Categorized,
    ShowCategories = true,
    ShowDescription = true,
    ShowSearchBox = true,
    ExpandAllCategories = false
};

// Set object to edit
propertyGrid.SelectedObject = new MyConfigObject();

// Handle property changes
propertyGrid.PropertyValueChanged += (sender, args) =>
{
    Console.WriteLine($"Property '{args.PropertyName}' changed from {args.OldValue} to {args.NewValue}");
    SaveConfiguration();
};

// Handle object changes
propertyGrid.SelectedObjectChanged += (sender, args) =>
{
    Console.WriteLine($"Object changed: {args.NewObject?.GetType().Name}");
};

// Handle property selection
propertyGrid.PropertySelectionChanged += (sender, args) =>
{
    if (args.NewProperty != null)
    {
        ShowPropertyHelp(args.NewProperty.Name);
    }
};

// Refresh after external changes
propertyGrid.Refresh();

// Expand/collapse categories
propertyGrid.ExpandAll();
propertyGrid.CollapseAll();
```

### Custom Object with Attributes

```csharp
// Object with property attributes
public class AppSettings
{
    [Category("General")]
    [DisplayName("Application Name")]
    [Description("The display name of the application")]
    public string Name { get; set; } = "My App";

    [Category("General")]
    [DisplayName("Version")]
    [ReadOnly(true)]
    public string Version { get; set; } = "1.0.0";

    [Category("Appearance")]
    [DisplayName("Theme")]
    public AppTheme Theme { get; set; } = AppTheme.Light;

    [Category("Appearance")]
    [DisplayName("Primary Color")]
    public Color PrimaryColor { get; set; } = Colors.Blue;

    [Category("Network")]
    [DisplayName("API URL")]
    [Description("The base URL for API calls")]
    public string ApiUrl { get; set; } = "https://api.example.com";

    [Category("Network")]
    [DisplayName("Timeout (seconds)")]
    [Range(1, 300)]
    public int Timeout { get; set; } = 30;

    [Browsable(false)]
    public string InternalId { get; set; } // Hidden from PropertyGrid
}
```

### MVVM Pattern

```csharp
// ViewModel
public class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private AppSettings _settings;

    [RelayCommand]
    private void OnPropertyChanged(PropertyValueChangedEventArgs args)
    {
        // Auto-save on property change
        _ = _settingsService.SaveAsync(Settings);

        // Log the change
        _logger.LogInformation("Setting changed: {Property} = {Value}",
            args.PropertyName, args.NewValue);
    }

    [RelayCommand]
    private void OnObjectChanged(SelectedObjectChangedEventArgs args)
    {
        // React to object changes
        if (args.NewObject is AppSettings newSettings)
        {
            ValidateSettings(newSettings);
        }
    }
}
```

