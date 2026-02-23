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

### ItemTemplate

Gets or sets a custom `DataTemplate` for rendering items. When set, this takes precedence over `DisplayMemberPath` and `IconMemberPath`.

```csharp
public DataTemplate? ItemTemplate { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `DataTemplate?` | `null` | Yes |

**Example:**

```xml
<extras:ComboBox ItemsSource="{Binding Users}" SelectedItem="{Binding SelectedUser}">
    <extras:ComboBox.ItemTemplate>
        <DataTemplate x:DataType="models:User">
            <HorizontalStackLayout Spacing="10" Padding="5">
                <Image Source="{Binding AvatarUrl}" WidthRequest="32" HeightRequest="32" />
                <VerticalStackLayout>
                    <Label Text="{Binding FullName}" FontAttributes="Bold" />
                    <Label Text="{Binding Email}" FontSize="12" TextColor="Gray" />
                </VerticalStackLayout>
            </HorizontalStackLayout>
        </DataTemplate>
    </extras:ComboBox.ItemTemplate>
</extras:ComboBox>
```

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

Gets or sets the accent color used for focus indication. When `null`, falls back to the theme's accent color.

```csharp
public Color? AccentColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (theme default) | Yes |

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

### PopupMode

Gets or sets whether the ComboBox uses popup mode for external popup handling.
When true, the dropdown raises `PopupRequested` instead of showing inline.
Used for constrained containers like DataGrid cells.

```csharp
public bool PopupMode { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

### IsSearchVisible

Gets or sets whether the search input is visible in the dropdown.
Set to `false` to hide the search UI for small item lists where search adds unnecessary complexity.

```csharp
public bool IsSearchVisible { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

**Example - Hide search for small lists:**

```xml
<extras:ComboBox ItemsSource="{Binding Priorities}"
                 SelectedItem="{Binding SelectedPriority}"
                 IsSearchVisible="False"
                 Placeholder="Select priority..." />
```

> **Note:** On iOS and Android, the software keyboard does not appear when the dropdown opens with search hidden. Desktop platforms (Windows, macOS) retain hidden keyboard-capture for keyboard navigation.

**Example - Dynamic toggle via binding:**

```xml
<CheckBox IsChecked="{Binding ShowSearch}" />
<extras:ComboBox ItemsSource="{Binding Items}"
                 IsSearchVisible="{Binding ShowSearch}"
                 SelectedItem="{Binding Selected}" />
```

**Example - Code-behind toggle:**

```csharp
// Hide search for lists with fewer than 10 items
myComboBox.IsSearchVisible = myComboBox.ItemsSource?.Cast<object>().Count() >= 10;
```

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

### PopupRequested

Occurs when `PopupMode` is true and the dropdown should be shown.
The parent container handles this event to show a popup overlay.

```csharp
public event EventHandler<ComboBoxPopupRequestEventArgs>? PopupRequested;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Source | ComboBox | The ComboBox that raised the event |
| AnchorBounds | Rect | Bounds of the anchor element relative to container |
| ItemsSource | IEnumerable | Items for the popup dropdown |
| DisplayMemberPath | string | Property path for item display |
| SelectedItem | object | Currently selected item |
| Placeholder | string | Placeholder text for search entry |
| IsSearchVisible | bool | Whether the search input should be visible |

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

### SetSelectedItemFromPopup(object? item)

Sets the selected item from an external source (e.g., popup overlay).
Used in `PopupMode` when the selection is made in the parent's popup.

```csharp
public void SetSelectedItemFromPopup(object? item)
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
                 IsSearchVisible="True"
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

### PopupMode (for constrained containers)

When embedding ComboBox in constrained containers like DataGrid cells, use PopupMode
to handle dropdown display externally:

```xml
<extras:ComboBox PopupMode="True"
                 PopupRequested="OnPopupRequested"
                 ItemsSource="{Binding Items}"
                 DisplayMemberPath="Name" />
```

```csharp
private void OnPopupRequested(object sender, ComboBoxPopupRequestEventArgs e)
{
    // Show popup overlay at the anchor bounds
    var popup = new ComboBoxPopupContent
    {
        ItemsSource = e.ItemsSource,
        DisplayMemberPath = e.DisplayMemberPath,
        SelectedItem = e.SelectedItem,
        IsSearchVisible = e.IsSearchVisible
    };

    popup.ItemSelected += (s, selectedItem) =>
    {
        e.Source.SetSelectedItemFromPopup(selectedItem);
        ClosePopup(popup);
    };

    ShowPopupAtBounds(popup, e.AnchorBounds);
}
```

### Hiding Search for Small Lists

For simple dropdowns with few items, hide the search box to provide a cleaner UI:

```xml
<!-- Status dropdown - no search needed -->
<extras:ComboBox ItemsSource="{Binding StatusOptions}"
                 SelectedItem="{Binding Status}"
                 IsSearchVisible="False"
                 Placeholder="Select status..." />
```

```csharp
// ViewModel
public ObservableCollection<string> StatusOptions { get; } =
    new(["Draft", "Pending", "Approved", "Rejected"]);
```

### Dynamic Search Visibility

Toggle search visibility based on item count or user preference:

```xml
<!-- XAML with runtime toggle -->
<VerticalStackLayout>
    <HorizontalStackLayout Spacing="10">
        <CheckBox IsChecked="{Binding IsSearchEnabled}" />
        <Label Text="Show Search Box" VerticalOptions="Center" />
    </HorizontalStackLayout>

    <extras:ComboBox ItemsSource="{Binding Countries}"
                     SelectedItem="{Binding SelectedCountry}"
                     DisplayMemberPath="Name"
                     IsSearchVisible="{Binding IsSearchEnabled}"
                     Placeholder="Select country..." />
</VerticalStackLayout>
```

```csharp
// ViewModel with dynamic toggle
public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSearchEnabled = true;

    [ObservableProperty]
    private ObservableCollection<Country> _countries;

    // Automatically hide search for small lists
    partial void OnCountriesChanged(ObservableCollection<Country> value)
    {
        IsSearchEnabled = value?.Count >= 10;
    }
}
```
