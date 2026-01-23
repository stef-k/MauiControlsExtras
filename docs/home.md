# MAUI Controls Extras

> Enhanced UI controls for .NET MAUI applications

## Overview

MAUI Controls Extras is a library that provides additional UI controls for .NET MAUI applications. These controls fill gaps in the standard MAUI control library, offering functionality commonly needed in mobile and cross-platform applications.

## Available Controls

### ComboBox

A feature-rich dropdown control with:

- Searchable/filterable dropdown list
- Complex object support via property paths
- Image/icon display
- Two-way data binding
- Theme-aware styling

[Learn more about ComboBox](controls/combobox.md)

## Features

- **Cross-Platform** - Works on Android, iOS, macOS Catalyst, and Windows
- **Theme-Aware** - Automatic light/dark mode support
- **MVVM-Friendly** - Full data binding support
- **Well-Documented** - Comprehensive API documentation
- **MIT Licensed** - Free for commercial and personal use

## Quick Example

```xml
<extras:ComboBox ItemsSource="{Binding Countries}"
                 SelectedItem="{Binding SelectedCountry, Mode=TwoWay}"
                 DisplayMemberPath="Name"
                 Placeholder="Select a country..." />
```

## Getting Started

1. [Install the NuGet package](installation.md)
2. [Follow the Quick Start guide](quickstart.md)
3. [Explore the API Reference](api/combobox.md)
