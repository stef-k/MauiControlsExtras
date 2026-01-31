# MultiSelectComboBox

A dropdown control that allows selecting multiple items with checkboxes.

## Features

- **Multiple Selection** - Select multiple items via checkboxes
- **Search/Filter** - Filter items by typing
- **Select All** - Quick select/deselect all option
- **Custom Display** - Configure how selections are shown
- **Data Binding** - Full MVVM support
- **Keyboard Navigation** - Full keyboard support

## Basic Usage

```xml
<extras:MultiSelectComboBox
    ItemsSource="{Binding Categories}"
    SelectedItems="{Binding SelectedCategories}"
    DisplayMemberPath="Name"
    Placeholder="Select categories..." />
```

## Display Options

```xml
<extras:MultiSelectComboBox
    ItemsSource="{Binding Items}"
    SelectedItems="{Binding SelectedItems}"
    DisplayMode="Chips"
    MaxDisplayedChips="3"
    Separator=", " />
```

### Display Modes

- `Text` - Comma-separated text
- `Chips` - Individual removable chips
- `Count` - "3 items selected"

## Select All

```xml
<extras:MultiSelectComboBox
    ItemsSource="{Binding Items}"
    ShowSelectAll="True"
    SelectAllText="Select All" />
```

## Filtering

```xml
<extras:MultiSelectComboBox
    ItemsSource="{Binding Items}"
    IsSearchable="True"
    FilterMemberPath="Name"
    SearchPlaceholder="Type to search..." />
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ↓ | Open dropdown / Move to next item |
| ↑ | Move to previous item |
| Enter | Open dropdown |
| Space | Toggle selection on highlighted item |
| Escape | Close dropdown |
| Home | Move to first item |
| End | Move to last item |
| Ctrl+A | Select all |

## Events

| Event | Description |
|-------|-------------|
| SelectionChanged | Selection has changed |
| DropdownOpened | Dropdown was opened |
| DropdownClosed | Dropdown was closed |

## Validation

MultiSelectComboBox implements `IValidatable` for built-in validation support.

```xml
<extras:MultiSelectComboBox
    ItemsSource="{Binding Categories}"
    SelectedItems="{Binding SelectedCategories}"
    IsRequired="True"
    RequiredErrorMessage="Please select at least one category"
    ValidateCommand="{Binding OnValidationCommand}" />
```

### Checking Validation State

```csharp
if (!multiSelectComboBox.IsValid)
{
    foreach (var error in multiSelectComboBox.ValidationErrors)
    {
        Debug.WriteLine(error);
    }
}

// Trigger validation manually
var result = multiSelectComboBox.Validate();
```

### Validation Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsRequired | bool | false | Whether at least one selection is required |
| RequiredErrorMessage | string | "This field is required." | Error message when required but nothing selected |
| IsValid | bool | (read-only) | Current validation state |
| ValidationErrors | IReadOnlyList&lt;string&gt; | (read-only) | List of validation error messages |
| ValidateCommand | ICommand | null | Command executed when validation occurs |

## Properties

| Property | Type | Description |
|----------|------|-------------|
| ItemsSource | IEnumerable | Items to display |
| SelectedItems | IList | Selected items |
| DisplayMemberPath | string | Property to display |
| ShowSelectAll | bool | Show "Select All" option |
| DisplayMode | DisplayMode | How to show selections |
| IsSearchable | bool | Enable filtering |
| MaxDropdownHeight | double | Max dropdown height |
