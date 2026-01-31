# BindingNavigator API Reference

Full API documentation for the `MauiControlsExtras.Controls.BindingNavigator` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class BindingNavigator : StyledControlBase, IKeyboardNavigable
```

## Inheritance

Inherits from [StyledControlBase](base-classes.md#styledcontrolbase). See base class documentation for inherited styling properties.

## Interfaces

- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### ItemsSource

Gets or sets the data source to navigate.

```csharp
public IEnumerable? ItemsSource { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `IEnumerable?` | `null` | Yes |

---

#### Position

Gets or sets the current position (0-based index).

```csharp
public int Position { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `int` | `0` | Yes | TwoWay |

---

#### DisplayPosition (Read-only)

Gets the display position (1-based for UI).

```csharp
public string DisplayPosition { get; }
```

---

#### Count (Read-only)

Gets the total count of items.

```csharp
public int Count { get; }
```

---

#### CurrentItem (Read-only)

Gets the current item at the current position.

```csharp
public object? CurrentItem { get; }
```

---

### Visibility Properties

#### ShowAddButton

Gets or sets whether the Add button is shown.

```csharp
public bool ShowAddButton { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### ShowDeleteButton

Gets or sets whether the Delete button is shown.

```csharp
public bool ShowDeleteButton { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### ShowSaveButtons

Gets or sets whether Save/Cancel buttons are shown.

```csharp
public bool ShowSaveButtons { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### ShowRefreshButton

Gets or sets whether the Refresh button is shown.

```csharp
public bool ShowRefreshButton { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### ShowSeparators

Gets or sets whether separators are shown between button groups.

```csharp
public bool ShowSeparators { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

### Appearance Properties

#### ButtonSize

Gets or sets the button size.

```csharp
public double ButtonSize { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `32` | Yes |

---

#### FirstButtonContent

Gets or sets the First button content.

```csharp
public string FirstButtonContent { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"⏮"` | Yes |

---

#### PreviousButtonContent

Gets or sets the Previous button content.

```csharp
public string PreviousButtonContent { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"◀"` | Yes |

---

#### NextButtonContent

Gets or sets the Next button content.

```csharp
public string NextButtonContent { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"▶"` | Yes |

---

#### LastButtonContent

Gets or sets the Last button content.

```csharp
public string LastButtonContent { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"⏭"` | Yes |

---

#### AddButtonContent

Gets or sets the Add button content.

```csharp
public string AddButtonContent { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"+"` | Yes |

---

#### DeleteButtonContent

Gets or sets the Delete button content.

```csharp
public string DeleteButtonContent { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"−"` | Yes |

---

#### SaveButtonContent

Gets or sets the Save button content.

```csharp
public string SaveButtonContent { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"💾"` | Yes |

---

#### CancelButtonContent

Gets or sets the Cancel button content.

```csharp
public string CancelButtonContent { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"✕"` | Yes |

---

#### RefreshButtonContent

Gets or sets the Refresh button content.

```csharp
public string RefreshButtonContent { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"↻"` | Yes |

---

### State Properties

#### CanGoFirst (Read-only)

Gets whether navigation to first is possible.

```csharp
public bool CanGoFirst { get; }
```

---

#### CanGoPrevious (Read-only)

Gets whether navigation to previous is possible.

```csharp
public bool CanGoPrevious { get; }
```

---

#### CanGoNext (Read-only)

Gets whether navigation to next is possible.

```csharp
public bool CanGoNext { get; }
```

---

#### CanGoLast (Read-only)

Gets whether navigation to last is possible.

```csharp
public bool CanGoLast { get; }
```

---

## Events

### PositionChanged

Occurs when the position changes.

```csharp
public event EventHandler<PositionChangedEventArgs>? PositionChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| OldPosition | `int` | Previous position |
| NewPosition | `int` | New position |
| CurrentItem | `object?` | Item at the new position |

---

### PositionChanging

Occurs before the position changes (cancelable).

```csharp
public event EventHandler<PositionChangingEventArgs>? PositionChanging;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| OldPosition | `int` | Previous position |
| NewPosition | `int` | Proposed new position |
| Cancel | `bool` | Set to true to cancel |

---

### Adding

Occurs when Add is requested.

```csharp
public event EventHandler<BindingNavigatorItemEventArgs>? Adding;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Item | `object?` | Item to add (null for new) |
| Index | `int` | Position to add at |
| Cancel | `bool` | Set to true to cancel |

---

### Deleting

Occurs when Delete is requested.

```csharp
public event EventHandler<BindingNavigatorItemEventArgs>? Deleting;
```

---

### Saving

Occurs when Save is requested.

```csharp
public event EventHandler? Saving;
```

---

### Cancelling

Occurs when Cancel is requested.

```csharp
public event EventHandler? Cancelling;
```

---

### Refreshing

Occurs when Refresh is requested.

```csharp
public event EventHandler? Refreshing;
```

---

## Commands

### PositionChangedCommand

Executed when position changes.

```csharp
public ICommand? PositionChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `PositionChangedEventArgs` |

---

### AddCommand

Executed when Add is clicked.

```csharp
public ICommand? AddCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `BindingNavigatorItemEventArgs` |

---

### DeleteCommand

Executed when Delete is clicked.

```csharp
public ICommand? DeleteCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `BindingNavigatorItemEventArgs` |

---

### SaveCommand

Executed when Save is clicked.

```csharp
public ICommand? SaveCommand { get; set; }
```

---

### CancelCommand

Executed when Cancel is clicked.

```csharp
public ICommand? CancelCommand { get; set; }
```

---

### RefreshCommand

Executed when Refresh is clicked.

```csharp
public ICommand? RefreshCommand { get; set; }
```

---

## Methods

### MoveFirst()

Moves to the first item.

```csharp
public void MoveFirst()
```

---

### MovePrevious()

Moves to the previous item.

```csharp
public void MovePrevious()
```

---

### MoveNext()

Moves to the next item.

```csharp
public void MoveNext()
```

---

### MoveLast()

Moves to the last item.

```csharp
public void MoveLast()
```

---

### NavigateTo(int position)

Navigates to a specific position.

```csharp
public bool NavigateTo(int position)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| position | `int` | The target position |

| Returns | Description |
|---------|-------------|
| `bool` | True if navigation succeeded |

---

### Add()

Adds a new item (raises Adding event).

```csharp
public void Add()
```

---

### Delete()

Deletes the current item (raises Deleting event).

```csharp
public void Delete()
```

---

### Refresh()

Refreshes the data (raises Refreshing event).

```csharp
public void Refresh()
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Home | Move to first |
| End | Move to last |
| Arrow Left | Move to previous |
| Arrow Right | Move to next |
| Page Up | Move to previous |
| Page Down | Move to next |
| Insert | Add new item |
| Delete | Delete current item |

---

## Usage Examples

### Basic Navigation

```xml
<extras:BindingNavigator ItemsSource="{Binding Items}"
                         Position="{Binding CurrentIndex}"
                         PositionChangedCommand="{Binding OnPositionChangedCommand}" />
```

### Navigation Only

```xml
<extras:BindingNavigator ItemsSource="{Binding Customers}"
                         Position="{Binding CurrentCustomerIndex}"
                         ShowAddButton="False"
                         ShowDeleteButton="False" />
```

### With CRUD Operations

```xml
<extras:BindingNavigator ItemsSource="{Binding Records}"
                         Position="{Binding CurrentIndex}"
                         ShowAddButton="True"
                         ShowDeleteButton="True"
                         ShowSaveButtons="True"
                         ShowRefreshButton="True"
                         AddCommand="{Binding AddRecordCommand}"
                         DeleteCommand="{Binding DeleteRecordCommand}"
                         SaveCommand="{Binding SaveCommand}"
                         RefreshCommand="{Binding RefreshCommand}" />
```

### Custom Button Content

```xml
<extras:BindingNavigator ItemsSource="{Binding Items}"
                         FirstButtonContent="First"
                         PreviousButtonContent="Prev"
                         NextButtonContent="Next"
                         LastButtonContent="Last"
                         AddButtonContent="New"
                         DeleteButtonContent="Remove"
                         ButtonSize="40" />
```

### Custom Appearance

```xml
<extras:BindingNavigator ItemsSource="{Binding Items}"
                         Position="{Binding Index}"
                         AccentColor="#2196F3"
                         BackgroundColor="#F5F5F5"
                         CornerRadius="8"
                         ShowSeparators="False" />
```

### Code-Behind

```csharp
// Create binding navigator
var navigator = new BindingNavigator
{
    ShowAddButton = true,
    ShowDeleteButton = true,
    ShowSaveButtons = true,
    ShowRefreshButton = true
};

// Set data source
navigator.ItemsSource = customers;

// Handle position changes
navigator.PositionChanged += (sender, args) =>
{
    Console.WriteLine($"Moved from {args.OldPosition} to {args.NewPosition}");
    LoadCustomerDetails(args.CurrentItem);
};

// Handle position changing (cancelable)
navigator.PositionChanging += (sender, args) =>
{
    if (HasUnsavedChanges)
    {
        args.Cancel = true;
        ShowSavePrompt();
    }
};

// Handle add
navigator.Adding += (sender, args) =>
{
    var newCustomer = new Customer();
    customers.Add(newCustomer);
    navigator.NavigateTo(customers.Count - 1);
};

// Handle delete
navigator.Deleting += (sender, args) =>
{
    if (ShowConfirmation("Delete this customer?"))
    {
        customers.RemoveAt(args.Index);
    }
    else
    {
        args.Cancel = true;
    }
};

// Handle save
navigator.Saving += (sender, args) =>
{
    SaveChanges();
};

// Handle refresh
navigator.Refreshing += (sender, args) =>
{
    ReloadData();
};

// Programmatic navigation
navigator.MoveFirst();
navigator.MovePrevious();
navigator.MoveNext();
navigator.MoveLast();
navigator.NavigateTo(5);

// Access current state
var current = navigator.CurrentItem;
var count = navigator.Count;
var position = navigator.Position;
```

### MVVM Pattern

```csharp
// ViewModel
public class CustomerListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private int _currentIndex;

    [ObservableProperty]
    private Customer? _selectedCustomer;

    partial void OnCurrentIndexChanged(int value)
    {
        if (value >= 0 && value < Customers.Count)
        {
            SelectedCustomer = Customers[value];
        }
    }

    [RelayCommand]
    private void OnPositionChanged(PositionChangedEventArgs args)
    {
        // Load details for the new item
        if (args.CurrentItem is Customer customer)
        {
            _ = LoadCustomerDetailsAsync(customer);
        }
    }

    [RelayCommand]
    private async Task AddRecord(BindingNavigatorItemEventArgs args)
    {
        var newCustomer = await _customerService.CreateNewAsync();
        Customers.Add(newCustomer);
        CurrentIndex = Customers.Count - 1;
    }

    [RelayCommand]
    private async Task DeleteRecord(BindingNavigatorItemEventArgs args)
    {
        if (args.Item is Customer customer)
        {
            await _customerService.DeleteAsync(customer.Id);
            Customers.Remove(customer);
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        await _customerService.SaveChangesAsync();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        var customers = await _customerService.GetAllAsync();
        Customers = new ObservableCollection<Customer>(customers);
        CurrentIndex = 0;
    }
}
```

### Master-Detail Layout

```xml
<Grid RowDefinitions="Auto,*">
    <!-- Navigator toolbar -->
    <extras:BindingNavigator Grid.Row="0"
                             ItemsSource="{Binding Orders}"
                             Position="{Binding CurrentOrderIndex}"
                             ShowSaveButtons="True"
                             AddCommand="{Binding AddOrderCommand}"
                             DeleteCommand="{Binding DeleteOrderCommand}"
                             SaveCommand="{Binding SaveOrderCommand}" />

    <!-- Detail view -->
    <ScrollView Grid.Row="1">
        <VerticalStackLayout Padding="16">
            <Label Text="Order Details" FontSize="18" FontAttributes="Bold" />
            <Entry Text="{Binding CurrentOrder.CustomerName}" Placeholder="Customer Name" />
            <DatePicker Date="{Binding CurrentOrder.OrderDate}" />
            <Entry Text="{Binding CurrentOrder.Total}" Keyboard="Numeric" />
        </VerticalStackLayout>
    </ScrollView>
</Grid>
```

