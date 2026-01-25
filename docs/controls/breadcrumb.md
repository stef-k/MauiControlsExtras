# Breadcrumb

A breadcrumb navigation control showing the current location in a hierarchy.

## Features

- **Hierarchical Navigation** - Display path to current location
- **Clickable Items** - Navigate by clicking breadcrumb items
- **Home Icon** - Optional home icon at start
- **Collapsible** - Collapse middle items when path is long
- **Custom Separators** - Configure separator text or template
- **Keyboard Navigation** - Arrow keys and keyboard support

## Basic Usage

```xml
<extras:Breadcrumb ItemClickedCommand="{Binding NavigateCommand}">
    <extras:BreadcrumbItem Text="Home" Tag="/" />
    <extras:BreadcrumbItem Text="Products" Tag="/products" />
    <extras:BreadcrumbItem Text="Electronics" Tag="/products/electronics" />
    <extras:BreadcrumbItem Text="Phones" Tag="/products/electronics/phones" />
</extras:Breadcrumb>
```

## Data Binding

```xml
<extras:Breadcrumb
    ItemsSource="{Binding BreadcrumbPath}"
    ItemClickedCommand="{Binding NavigateCommand}" />
```

```csharp
// In ViewModel
public ObservableCollection<BreadcrumbItem> BreadcrumbPath { get; } = new()
{
    new BreadcrumbItem { Text = "Home", Tag = "/" },
    new BreadcrumbItem { Text = "Documents", Tag = "/documents" },
    new BreadcrumbItem { Text = "Reports", Tag = "/documents/reports" }
};
```

## Home Icon

```xml
<!-- Show home icon (default) -->
<extras:Breadcrumb ShowHomeIcon="True" HomeIcon="🏠" />

<!-- Custom home icon -->
<extras:Breadcrumb ShowHomeIcon="True" HomeIcon="⌂" />

<!-- Hide home icon -->
<extras:Breadcrumb ShowHomeIcon="False" />
```

## Custom Separator

```xml
<!-- Default slash separator -->
<extras:Breadcrumb Separator="/" />

<!-- Arrow separator -->
<extras:Breadcrumb Separator="›" />

<!-- Chevron separator -->
<extras:Breadcrumb Separator=">" />

<!-- Custom separator template -->
<extras:Breadcrumb>
    <extras:Breadcrumb.SeparatorTemplate>
        <DataTemplate>
            <Image Source="chevron.png" WidthRequest="16" HeightRequest="16" />
        </DataTemplate>
    </extras:Breadcrumb.SeparatorTemplate>
</extras:Breadcrumb>
```

## Collapsible Breadcrumbs

```xml
<!-- Collapse middle items when more than 4 items -->
<extras:Breadcrumb
    MaxVisibleItems="4"
    CollapsedIndicator="..." />
```

## Styling

```xml
<extras:Breadcrumb
    FontSize="14"
    ItemSpacing="8"
    ActiveItemColor="Blue"
    InactiveItemColor="Gray"
    SeparatorColor="LightGray"
    HoverUnderline="True" />
```

## Code-Behind Operations

```csharp
// Add items programmatically
breadcrumb.Items.Add(new BreadcrumbItem { Text = "New Folder", Tag = "/new" });

// Clear and rebuild
breadcrumb.Items.Clear();
breadcrumb.Items.Add(new BreadcrumbItem { Text = "Home", Tag = "/" });

// Navigate to item
breadcrumb.NavigateTo(2); // Navigate to 3rd item
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ← | Select previous item |
| → | Select next item |
| Enter | Navigate to selected item |
| Home | Navigate to first item |
| End | Navigate to last item |

## Events

| Event | Description |
|-------|-------------|
| ItemClicked | Breadcrumb item clicked |
| HomeClicked | Home icon clicked |

## Commands

| Command | Description |
|---------|-------------|
| ItemClickedCommand | Execute when item is clicked |
| HomeClickedCommand | Execute when home is clicked |

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Items | ObservableCollection&lt;BreadcrumbItem&gt; | empty | Breadcrumb items |
| Separator | string | "/" | Separator text |
| SeparatorTemplate | DataTemplate | null | Custom separator template |
| ItemTemplate | DataTemplate | null | Custom item template |
| ShowHomeIcon | bool | true | Show home icon |
| HomeIcon | string | "🏠" | Home icon content |
| ItemSpacing | double | 8 | Space between items |
| FontSize | double | 14 | Item font size |
| ActiveItemColor | Color | null | Current item color |
| InactiveItemColor | Color | null | Other items color |
| SeparatorColor | Color | null | Separator color |
| HoverUnderline | bool | true | Underline on hover |
| MaxVisibleItems | int | 0 | Max items (0 = all) |
| CollapsedIndicator | string | "..." | Collapsed items indicator |

## BreadcrumbItem Properties

| Property | Type | Description |
|----------|------|-------------|
| Text | string | Display text |
| Tag | object | Navigation data |
| Icon | string | Optional icon |
| IsEnabled | bool | Whether item is clickable |
