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
| Alt+↓ | Open dropdown |
| Escape | Close dropdown |
| ↑ / ↓ | Navigate items |
| Space | Toggle checkbox |
| Enter | Close dropdown |
| Type text | Filter items |

## Events

| Event | Description |
|-------|-------------|
| SelectionChanged | Selection has changed |
| DropdownOpened | Dropdown was opened |
| DropdownClosed | Dropdown was closed |

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
