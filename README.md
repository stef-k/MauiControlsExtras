# MAUI Controls Extras

A collection of enhanced UI controls for .NET MAUI applications that fill gaps in the standard control library.

[![NuGet](https://img.shields.io/nuget/v/StefK.MauiControlsExtras.svg)](https://www.nuget.org/packages/StefK.MauiControlsExtras/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

### ComboBox Control

A feature-rich dropdown control similar to WinForms ComboBox, with modern mobile-friendly features:

- **Searchable/Filterable** - Built-in search with debounced input
- **Complex Object Support** - Use `DisplayMemberPath` and `ValueMemberPath` for data binding
- **Image/Icon Support** - Display images alongside text via `IconMemberPath`
- **Two-Way Binding** - Full support for `SelectedItem` and `SelectedValue` binding
- **Theme-Aware** - Automatic light/dark mode styling
- **Customizable** - Placeholder text, accent colors, visible item count
- **Clear Selection** - Built-in clear button

## Installation

### NuGet Package Manager

```
Install-Package StefK.MauiControlsExtras
```

### .NET CLI

```
dotnet add package StefK.MauiControlsExtras
```

## Quick Start

### 1. Add the namespace

```xml
xmlns:extras="clr-namespace:MauiControlsExtras.Controls;assembly=MauiControlsExtras"
```

### 2. Basic Usage

```xml
<extras:ComboBox ItemsSource="{Binding Countries}"
                 SelectedItem="{Binding SelectedCountry, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 Placeholder="Select a country..." />
```

### 3. With Icons

```xml
<extras:ComboBox ItemsSource="{Binding Icons}"
                 SelectedItem="{Binding SelectedIcon, Mode=TwoWay}"
                 DisplayMemberPath="DisplayName"
                 IconMemberPath="ImagePath"
                 Placeholder="Select an icon..."
                 VisibleItemCount="6" />
```

## API Reference

### ComboBox Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ItemsSource` | `IEnumerable` | `null` | The collection of items to display |
| `SelectedItem` | `object` | `null` | The currently selected item (two-way) |
| `SelectedValue` | `object` | `null` | The selected value based on ValueMemberPath (two-way) |
| `DisplayMemberPath` | `string` | `null` | Property path for display text |
| `ValueMemberPath` | `string` | `null` | Property path for the value |
| `IconMemberPath` | `string` | `null` | Property path for item icons |
| `Placeholder` | `string` | `"Select an item"` | Text shown when no selection |
| `DefaultValue` | `object` | `null` | Value to select on initialization |
| `VisibleItemCount` | `int` | `5` | Number of visible items in dropdown |
| `AccentColor` | `Color` | `#0078D4` | Color used for focus indication |
| `HasSelection` | `bool` | `false` | Whether an item is selected (read-only) |
| `IsExpanded` | `bool` | `false` | Whether dropdown is open (read-only) |

### ComboBox Events

| Event | Description |
|-------|-------------|
| `SelectionChanged` | Raised when the selected item changes |
| `Opened` | Raised when the dropdown opens |
| `Closed` | Raised when the dropdown closes |

### ComboBox Methods

| Method | Description |
|--------|-------------|
| `Open()` | Programmatically opens the dropdown |
| `Close()` | Programmatically closes the dropdown |
| `ClearSelection()` | Clears the current selection |
| `RefreshItems()` | Refreshes the filtered items list |

## Examples

### Activity Selection (Simple)

```xml
<extras:ComboBox ItemsSource="{Binding ActivityTypes}"
                 SelectedItem="{Binding SelectedActivity, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 Placeholder="Select an Activity"
                 VisibleItemCount="5" />
```

### Icon Picker (With Images)

```xml
<extras:ComboBox ItemsSource="{Binding Icons}"
                 SelectedItem="{Binding SelectedIconOption, Mode=TwoWay}"
                 DisplayMemberPath="DisplayName"
                 IconMemberPath="IconImageSource"
                 Placeholder="Select an icon..."
                 VisibleItemCount="6" />
```

### With Default Value

```xml
<extras:ComboBox ItemsSource="{Binding Priorities}"
                 SelectedItem="{Binding Priority, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 ValueMemberPath="Id"
                 DefaultValue="normal"
                 Placeholder="Select priority" />
```

## Supported Platforms

- Android 5.0+ (API 21)
- iOS 15.0+
- macOS Catalyst 15.0+
- Windows 10.0.17763.0+

## Requirements

- .NET 10.0 or later
- .NET MAUI

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Documentation

Full documentation is available at: https://stef-k.github.io/MauiControlsExtras/
