# MAUI Controls Extras

A comprehensive collection of 16 enterprise-grade UI controls for .NET MAUI applications that fill gaps in the standard control library.

[![NuGet](https://img.shields.io/nuget/v/StefK.MauiControlsExtras.svg)](https://www.nuget.org/packages/StefK.MauiControlsExtras/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> **Work in Progress**: This library is under active development. APIs may change between releases. See the [Changelog](./CHANGELOG.md) for breaking changes.

## Demo Application

A complete demo app showcasing all controls is available in [`samples/DemoApp/`](./samples/DemoApp/).

> **Note**: The demo uses direct event handlers for simplicity. For production apps, we recommend proper MVVM patterns with commands and view models.

## Available Controls

| Control | Description |
|---------|-------------|
| **Accordion** | Expandable/collapsible sections with single or multiple expand modes |
| **BindingNavigator** | Data navigation toolbar for browsing collections |
| **Breadcrumb** | Hierarchical navigation path display |
| **Calendar** | Date picker with single, multiple, and range selection modes |
| **ComboBox** | Searchable dropdown with complex object and icon support |
| **DataGridView** | Enterprise data grid with sorting, filtering, grouping, and editing |
| **MaskedEntry** | Text input with format masks (phone, date, SSN, etc.) |
| **MultiSelectComboBox** | Multi-selection dropdown with checkboxes |
| **NumericUpDown** | Numeric input with increment/decrement buttons |
| **PropertyGrid** | Property editor similar to Visual Studio Properties panel |
| **RangeSlider** | Dual-thumb slider for range selection |
| **Rating** | Star/icon-based rating control |
| **RichTextEditor** | WYSIWYG HTML/Markdown editor with Quill.js |
| **TokenEntry** | Tag/token input with autocomplete |
| **TreeView** | Hierarchical tree view with expand/collapse |
| **Wizard** | Step-by-step wizard/stepper for multi-page forms |

## Key Features

- **Cross-Platform** - Android, iOS, macOS Catalyst, and Windows
- **Keyboard Navigation** - Full keyboard support on desktop platforms
- **Mouse Interaction** - Click, double-click, right-click, hover, scroll
- **Clipboard Support** - Ctrl+C/V/X for copy/paste operations
- **Undo/Redo** - Ctrl+Z/Y for undoable operations
- **Theme-Aware** - Automatic light/dark mode support
- **MVVM-Friendly** - Full data binding with commands
- **MIT Licensed** - Free for commercial and personal use

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

### 2. Use the controls

```xml
<!-- ComboBox with search -->
<extras:ComboBox ItemsSource="{Binding Countries}"
                 SelectedItem="{Binding SelectedCountry, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 Placeholder="Select a country..." />

<!-- DataGrid with sorting, filtering, and various column types -->
<extras:DataGridView ItemsSource="{Binding Employees}"
                     CanUserEdit="True"
                     CanUserSort="True"
                     CanUserFilter="True">
    <extras:DataGridView.Columns>
        <extras:DataGridTextColumn Header="Name" Binding="Name" />
        <extras:DataGridTextColumn Header="Salary" Binding="Salary" Format="C0" />
        <extras:DataGridDatePickerColumn Header="Hire Date" Binding="HireDate" Format="d" />
        <extras:DataGridCheckBoxColumn Header="Active" Binding="IsActive" />
    </extras:DataGridView.Columns>
</extras:DataGridView>

<!-- RichTextEditor with dark theme support -->
<extras:RichTextEditor HtmlContent="{Binding Content, Mode=TwoWay}"
                       ThemeMode="Auto"
                       Placeholder="Start typing..." />

<!-- Calendar with date range selection -->
<extras:Calendar SelectionMode="Range"
                 RangeStart="{Binding StartDate, Mode=TwoWay}"
                 RangeEnd="{Binding EndDate, Mode=TwoWay}" />

<!-- TreeView with hierarchical data -->
<extras:TreeView ItemsSource="{Binding Folders}"
                 ChildrenPath="SubFolders"
                 DisplayMemberPath="Name" />
```

## Supported Platforms

- Android 5.0+ (API 21)
- iOS 15.0+
- macOS Catalyst 15.0+
- Windows 10.0.17763.0+

## Requirements

- .NET 10.0 or later
- .NET MAUI workload

## Documentation

Full documentation is available at: **https://stef-k.github.io/MauiControlsExtras/**

- [Getting Started](https://stef-k.github.io/MauiControlsExtras/#/quickstart)
- [Control Documentation](https://stef-k.github.io/MauiControlsExtras/#/controls/combobox)
- [API Reference](https://stef-k.github.io/MauiControlsExtras/#/api/combobox)
- [Changelog](https://stef-k.github.io/MauiControlsExtras/#/changelog)

## Contributing

Contributions are welcome! Please see the [Contributing Guide](https://stef-k.github.io/MauiControlsExtras/#/contributing) for guidelines.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
