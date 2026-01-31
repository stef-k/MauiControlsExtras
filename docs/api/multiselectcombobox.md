# MultiSelectComboBox API Reference

Full API documentation for the `MauiControlsExtras.Controls.MultiSelectComboBox` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class MultiSelectComboBox : TextStyledControlBase, IValidatable, IKeyboardNavigable
```

## Inheritance

Inherits from [TextStyledControlBase](base-classes.md#textstyledcontrolbase). See base class documentation for inherited styling and typography properties.

## Interfaces

- [IValidatable](interfaces.md#ivalidatable) - Validation support
- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### ItemsSource

Gets or sets the collection of items to display.

```csharp
public IEnumerable? ItemsSource { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `IEnumerable?` | `null` | Yes |

---

#### SelectedItems

Gets or sets the collection of selected items.

```csharp
public IList? SelectedItems { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `IList?` | `null` | Yes | TwoWay |

---

#### SelectedValues

Gets or sets the selected values based on ValueMemberPath.

```csharp
public IList? SelectedValues { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `IList?` | `null` | Yes | TwoWay |

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

#### ValueMemberPath

Gets or sets the property path for values.

```csharp
public string? ValueMemberPath { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### MaxSelections

Gets or sets the maximum number of selections allowed.

```csharp
public int MaxSelections { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `0` (unlimited) | Yes |

---

### Display Properties

#### Placeholder

Gets or sets placeholder text when no items are selected.

```csharp
public string Placeholder { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Select items..."` | Yes |

---

#### SelectAllText

Gets or sets the text for the "Select All" option.

```csharp
public string SelectAllText { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Select All"` | Yes |

---

#### ShowSelectAllOption

Gets or sets whether "Select All" option is shown.

```csharp
public bool ShowSelectAllOption { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### ShowSearchBox

Gets or sets whether the search box is visible.

```csharp
public bool ShowSearchBox { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### SearchPlaceholder

Gets or sets the search box placeholder text.

```csharp
public string SearchPlaceholder { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Search..."` | Yes |

---

#### DisplayFormat

Gets or sets how selected items are displayed in the collapsed state.

```csharp
public SelectionDisplayFormat DisplayFormat { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `SelectionDisplayFormat` | `Chips` | Yes |

---

#### MaxDisplayChips

Gets or sets the maximum number of chips to display before showing "+N more".

```csharp
public int MaxDisplayChips { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `3` | Yes |

---

#### VisibleItemCount

Gets or sets the number of items visible without scrolling.

```csharp
public int VisibleItemCount { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `6` | Yes |

---

### Appearance Properties

#### ChipBackgroundColor

Gets or sets the background color of selection chips.

```csharp
public Color? ChipBackgroundColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses accent with alpha) | Yes |

---

#### ChipTextColor

Gets or sets the text color of selection chips.

```csharp
public Color? ChipTextColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` | Yes |

---

#### ShowRemoveChipButton

Gets or sets whether chips display a remove button.

```csharp
public bool ShowRemoveChipButton { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

### State Properties

#### IsExpanded (Read-only)

Gets whether the dropdown is currently expanded.

```csharp
public bool IsExpanded { get; }
```

| Type | Bindable |
|------|----------|
| `bool` | No |

---

#### SelectionCount (Read-only)

Gets the number of selected items.

```csharp
public int SelectionCount { get; }
```

| Type | Bindable |
|------|----------|
| `int` | Yes |

---

#### HasSelection (Read-only)

Gets whether any items are selected.

```csharp
public bool HasSelection { get; }
```

| Type | Bindable |
|------|----------|
| `bool` | Yes |

---

#### IsAllSelected (Read-only)

Gets whether all items are selected.

```csharp
public bool IsAllSelected { get; }
```

| Type | Bindable |
|------|----------|
| `bool` | Yes |

---

### Validation Properties

#### IsRequired

Gets or sets whether at least one selection is required.

```csharp
public bool IsRequired { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### MinimumSelections

Gets or sets the minimum number of selections required.

```csharp
public int MinimumSelections { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `0` | Yes |

---

#### RequiredErrorMessage

Gets or sets the error message when required validation fails.

```csharp
public string RequiredErrorMessage { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"At least one selection is required"` | Yes |

---

#### IsValid (Read-only)

Gets whether the current selection passes validation.

```csharp
public bool IsValid { get; }
```

| Type | Bindable |
|------|----------|
| `bool` | Yes |

---

#### ValidationErrors (Read-only)

Gets the list of current validation error messages.

```csharp
public IReadOnlyList<string> ValidationErrors { get; }
```

| Type | Bindable |
|------|----------|
| `IReadOnlyList<string>` | Yes |

---

## Events

### SelectionChanged

Occurs when the selection changes.

```csharp
public event EventHandler<MultiSelectSelectionChangedEventArgs>? SelectionChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| AddedItems | `IReadOnlyList<object>` | Newly selected items |
| RemovedItems | `IReadOnlyList<object>` | Deselected items |
| SelectedItems | `IReadOnlyList<object>` | All selected items |

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

## Commands

### SelectionChangedCommand

Executed when selection changes.

```csharp
public ICommand? SelectionChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `MultiSelectSelectionChangedEventArgs` |

---

### ValidateCommand

Executed after validation completes.

```csharp
public ICommand? ValidateCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Result | `ValidationResult` |

---

## Methods

### Open()

Opens the dropdown.

```csharp
public void Open()
```

---

### Close()

Closes the dropdown.

```csharp
public void Close()
```

---

### SelectAll()

Selects all items.

```csharp
public void SelectAll()
```

---

### ClearSelection()

Clears all selections.

```csharp
public void ClearSelection()
```

---

### SelectItem(object item)

Selects the specified item.

```csharp
public bool SelectItem(object item)
```

---

### DeselectItem(object item)

Deselects the specified item.

```csharp
public bool DeselectItem(object item)
```

---

### ToggleItem(object item)

Toggles the selection state of the specified item.

```csharp
public void ToggleItem(object item)
```

---

### IsSelected(object item)

Returns whether the specified item is selected.

```csharp
public bool IsSelected(object item)
```

---

### Validate()

Performs validation and returns the result.

```csharp
public ValidationResult Validate()
```

| Returns | Description |
|---------|-------------|
| `ValidationResult` | Contains IsValid and any error messages |

---

## Enumerations

### SelectionDisplayFormat

```csharp
public enum SelectionDisplayFormat
{
    Chips,      // Show selected items as chips
    Count,      // Show "N items selected"
    Summary,    // Show first items + "+N more"
    Text        // Show comma-separated list
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Arrow Down | Open dropdown / Move to next item |
| Arrow Up | Move to previous item |
| Space | Toggle selection of focused item |
| Enter | Toggle selection / Close dropdown |
| Escape | Close dropdown |
| Home | Move to first item |
| End | Move to last item |
| Ctrl+A | Select all items |
| Backspace | Remove last selected chip |

---

## Usage Examples

### Basic Multi-Select

```xml
<extras:MultiSelectComboBox ItemsSource="{Binding Categories}"
                            SelectedItems="{Binding SelectedCategories}"
                            DisplayMemberPath="Name"
                            Placeholder="Select categories..." />
```

### With Value Binding

```xml
<extras:MultiSelectComboBox ItemsSource="{Binding Countries}"
                            SelectedValues="{Binding SelectedCountryCodes}"
                            DisplayMemberPath="Name"
                            ValueMemberPath="Code" />
```

### Limited Selections

```xml
<extras:MultiSelectComboBox ItemsSource="{Binding Options}"
                            SelectedItems="{Binding Chosen}"
                            MaxSelections="3"
                            Placeholder="Choose up to 3..." />
```

### Custom Display Format

```xml
<!-- Show count instead of chips -->
<extras:MultiSelectComboBox ItemsSource="{Binding Items}"
                            SelectedItems="{Binding Selected}"
                            DisplayFormat="Count" />

<!-- Show summary with overflow -->
<extras:MultiSelectComboBox ItemsSource="{Binding Items}"
                            SelectedItems="{Binding Selected}"
                            DisplayFormat="Summary"
                            MaxDisplayChips="2" />
```

### With Search

```xml
<extras:MultiSelectComboBox ItemsSource="{Binding AllUsers}"
                            SelectedItems="{Binding SelectedUsers}"
                            DisplayMemberPath="FullName"
                            ShowSearchBox="True"
                            SearchPlaceholder="Search users..."
                            VisibleItemCount="8" />
```

### Custom Appearance

```xml
<extras:MultiSelectComboBox ItemsSource="{Binding Tags}"
                            SelectedItems="{Binding SelectedTags}"
                            ChipBackgroundColor="#E3F2FD"
                            ChipTextColor="#1565C0"
                            ShowRemoveChipButton="True"
                            AccentColor="#2196F3"
                            CornerRadius="8" />
```

### With Validation

```xml
<extras:MultiSelectComboBox ItemsSource="{Binding Departments}"
                            SelectedItems="{Binding SelectedDepts}"
                            IsRequired="True"
                            MinimumSelections="2"
                            RequiredErrorMessage="Select at least 2 departments"
                            ValidateCommand="{Binding OnValidatedCommand}" />
```

### Without Select All

```xml
<extras:MultiSelectComboBox ItemsSource="{Binding Items}"
                            SelectedItems="{Binding Selected}"
                            ShowSelectAllOption="False"
                            ShowSearchBox="False" />
```

### Code-Behind

```csharp
// Create multi-select
var multiSelect = new MultiSelectComboBox
{
    DisplayMemberPath = "Name",
    ValueMemberPath = "Id",
    MaxSelections = 5,
    ShowSelectAllOption = true,
    ShowSearchBox = true
};

// Set items source
multiSelect.ItemsSource = GetAvailableItems();

// Initialize selections
multiSelect.SelectedItems = new ObservableCollection<object>
{
    items[0], items[2]
};

// Handle selection changes
multiSelect.SelectionChanged += (sender, args) =>
{
    Console.WriteLine($"Selected: {args.SelectedItems.Count} items");
    foreach (var item in args.AddedItems)
    {
        Console.WriteLine($"  Added: {item}");
    }
};

// Programmatic control
multiSelect.SelectAll();
multiSelect.ClearSelection();
multiSelect.SelectItem(specificItem);
multiSelect.DeselectItem(specificItem);

// Check state
bool isSelected = multiSelect.IsSelected(item);
int count = multiSelect.SelectionCount;
bool allSelected = multiSelect.IsAllSelected;

// Validate
var result = multiSelect.Validate();
if (!result.IsValid)
{
    ShowErrors(result.Errors);
}
```

### MVVM Pattern

```csharp
// ViewModel
public class FilterViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Category> _categories;

    [ObservableProperty]
    private ObservableCollection<object> _selectedCategories = new();

    [RelayCommand]
    private void OnSelectionChanged(MultiSelectSelectionChangedEventArgs args)
    {
        // React to selection changes
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var categoryIds = SelectedCategories
            .Cast<Category>()
            .Select(c => c.Id)
            .ToList();

        FilteredItems = AllItems
            .Where(i => categoryIds.Contains(i.CategoryId))
            .ToList();
    }
}
```
