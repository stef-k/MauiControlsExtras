# Base Classes API Reference

MAUI Controls Extras controls inherit from specialized base classes that provide common styling, theming, and behavior properties. This document describes these base classes and their inherited properties.

## Namespace

```csharp
using MauiControlsExtras.Base;
```

## Inheritance Hierarchy

```
ContentView
└── StyledControlBase
    ├── TextStyledControlBase
    ├── ListStyledControlBase
    └── HeaderedControlBase
```

---

## StyledControlBase

Base class for all styled controls providing common appearance properties including theming, colors, borders, and shadow effects.

```csharp
public abstract class StyledControlBase : ContentView, IThemeAware
```

### Controls Using This Base

- Accordion
- Breadcrumb
- Calendar
- BindingNavigator
- RangeSlider
- Rating

### Color Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| AccentColor | Color? | `null` | Accent color for focus/selection. Falls back to theme default |
| ForegroundColor | Color? | `null` | Text/icon color. Falls back to theme default |
| DisabledColor | Color? | `null` | Color when disabled. Falls back to theme default |
| ErrorColor | Color? | `null` | Color for error states. Falls back to theme default |
| SuccessColor | Color? | `null` | Color for success states. Falls back to theme default |
| WarningColor | Color? | `null` | Color for warning states. Falls back to theme default |

### Border Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| BorderColor | Color? | `null` | Border color. Falls back to theme default |
| BorderThickness | double | `-1.0` | Border thickness. Negative uses theme default |
| FocusBorderColor | Color? | `null` | Border color when focused. Falls back to AccentColor |
| ErrorBorderColor | Color? | `null` | Border color in error state. Falls back to ErrorColor |
| DisabledBorderColor | Color? | `null` | Border color when disabled. Falls back to theme default |
| CornerRadius | double | `-1.0` | Corner radius. Negative uses theme default |

### Shadow Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| HasShadow | bool | `false` | Whether to display shadow effect |
| ShadowColor | Color? | `null` | Shadow color. Falls back to theme default |
| ShadowOffset | Point | `(0, 2)` | Shadow X/Y offset |
| ShadowRadius | double | `4.0` | Shadow blur radius |
| ShadowOpacity | double | `0.2` | Shadow opacity (0.0 to 1.0) |
| Elevation | int | `0` | Material-style elevation (0-24) |

### Effective Properties (Read-only)

These computed properties provide the actual value used, falling back to theme defaults when the property is null or negative:

| Property | Type | Description |
|----------|------|-------------|
| EffectiveAccentColor | Color | Actual accent color with theme fallback |
| EffectiveForegroundColor | Color | Actual foreground color with theme fallback |
| EffectiveDisabledColor | Color | Actual disabled color with theme fallback |
| EffectiveErrorColor | Color | Actual error color with theme fallback |
| EffectiveSuccessColor | Color | Actual success color with theme fallback |
| EffectiveWarningColor | Color | Actual warning color with theme fallback |
| EffectiveBorderColor | Color | Actual border color with theme fallback |
| EffectiveBorderThickness | double | Actual border thickness with theme fallback |
| EffectiveFocusBorderColor | Color | Actual focus border color |
| EffectiveErrorBorderColor | Color | Actual error border color |
| EffectiveDisabledBorderColor | Color | Actual disabled border color |
| EffectiveCornerRadius | double | Actual corner radius with theme fallback |
| EffectiveShadowColor | Color | Actual shadow color with theme fallback |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| OnThemeChanged(AppTheme) | void | Called when application theme changes |

---

## TextStyledControlBase

Base class for text-containing controls providing typography properties. Extends `StyledControlBase`.

```csharp
public abstract class TextStyledControlBase : StyledControlBase
```

### Controls Using This Base

- MultiSelectComboBox
- NumericUpDown
- RichTextEditor
- TokenEntry

### Inherited Properties

All properties from [StyledControlBase](#styledcontrolbase).

### Typography Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| FontFamily | string? | `null` | Font family. Falls back to theme default |
| FontSize | double | `-1.0` | Font size. Negative uses theme default |
| FontAttributes | FontAttributes | `None` | Bold, Italic, or None |
| TextColor | Color? | `null` | Text color. Falls back to ForegroundColor |
| PlaceholderColor | Color? | `null` | Placeholder text color. Falls back to theme default |
| TextDecorations | TextDecorations | `None` | Underline, Strikethrough, or None |
| LineHeight | double | `1.2` | Line height multiplier |
| CharacterSpacing | double | `0.0` | Spacing between characters |

### Effective Properties (Read-only)

| Property | Type | Description |
|----------|------|-------------|
| EffectiveFontFamily | string? | Actual font family with theme fallback |
| EffectiveFontSize | double | Actual font size with theme fallback |
| EffectiveTextColor | Color | Actual text color with theme fallback |
| EffectivePlaceholderColor | Color | Actual placeholder color with theme fallback |

---

## ListStyledControlBase

Base class for list/collection controls providing selection and separator styling. Extends `StyledControlBase`.

```csharp
public abstract class ListStyledControlBase : StyledControlBase
```

### Controls Using This Base

- TreeView

### Inherited Properties

All properties from [StyledControlBase](#styledcontrolbase).

### List Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| AlternatingRowColor | Color? | `null` | Background for alternating rows. Null disables |
| SelectedItemBackgroundColor | Color? | `null` | Selected item background. Falls back to theme |
| SelectedItemTextColor | Color? | `null` | Selected item text color. Falls back to theme |
| HoverColor | Color? | `null` | Background on hover. Falls back to theme |
| SeparatorColor | Color? | `null` | Color between items. Falls back to muted border |
| SeparatorVisibility | bool | `true` | Whether separators are visible |
| SeparatorThickness | double | `1.0` | Separator line thickness |
| ItemSpacing | double | `0.0` | Spacing between items |

### Effective Properties (Read-only)

| Property | Type | Description |
|----------|------|-------------|
| EffectiveSelectedItemBackgroundColor | Color | Actual selection background with theme fallback |
| EffectiveSelectedItemTextColor | Color | Actual selection text color with theme fallback |
| EffectiveHoverColor | Color | Actual hover color with theme fallback |
| EffectiveSeparatorColor | Color | Actual separator color with theme fallback |

---

## HeaderedControlBase

Base class for controls with headers providing header styling properties. Extends `StyledControlBase`.

```csharp
public abstract class HeaderedControlBase : StyledControlBase
```

### Controls Using This Base

- PropertyGrid
- Wizard

### Inherited Properties

All properties from [StyledControlBase](#styledcontrolbase).

### Header Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| HeaderBackgroundColor | Color? | `null` | Header background. Falls back to surface color |
| HeaderTextColor | Color? | `null` | Header text color. Falls back to foreground |
| HeaderFontSize | double | `16.0` | Header font size |
| HeaderFontAttributes | FontAttributes | `Bold` | Header font attributes |
| HeaderFontFamily | string? | `null` | Header font family. Falls back to theme |
| HeaderPadding | Thickness | `(12, 8)` | Header internal padding |
| HeaderHeight | double | `-1.0` | Header height. Negative auto-sizes |
| HeaderBorderColor | Color? | `null` | Header border color. Falls back to control border |
| HeaderBorderThickness | Thickness | `(0, 0, 0, 1)` | Header border (typically bottom only) |

### Effective Properties (Read-only)

| Property | Type | Description |
|----------|------|-------------|
| EffectiveHeaderBackgroundColor | Color | Actual header background with fallback |
| EffectiveHeaderTextColor | Color | Actual header text color with fallback |
| EffectiveHeaderFontFamily | string? | Actual header font with fallback |
| EffectiveHeaderBorderColor | Color | Actual header border color with fallback |

---

## Usage Example

### Customizing Inherited Properties

All controls inherit these styling properties. Here's how to customize them:

```xml
<!-- Customizing a control that inherits from StyledControlBase -->
<extras:Rating
    Value="3.5"
    AccentColor="#FF5722"
    ForegroundColor="#212121"
    BorderColor="#CCCCCC"
    CornerRadius="8"
    HasShadow="True"
    Elevation="4" />

<!-- Customizing a control that inherits from TextStyledControlBase -->
<extras:NumericUpDown
    Value="42"
    FontFamily="Consolas"
    FontSize="16"
    TextColor="#1A237E"
    PlaceholderColor="#9E9E9E"
    BorderColor="#3F51B5" />

<!-- Customizing a control that inherits from HeaderedControlBase -->
<extras:Wizard
    HeaderBackgroundColor="#F5F5F5"
    HeaderTextColor="#212121"
    HeaderFontSize="18"
    HeaderFontAttributes="Bold"
    AccentColor="#2196F3" />
```

### Theme-Aware Styling

When you leave properties as `null` or negative values, controls automatically use theme defaults:

```xml
<!-- These use theme defaults for all styling -->
<extras:Rating Value="4" />
<extras:NumericUpDown Value="100" />
<extras:TreeView ItemsSource="{Binding Items}" />
```

```csharp
// Programmatically check effective values
var rating = new Rating();
Color actualAccent = rating.EffectiveAccentColor;  // Theme default
Color actualBorder = rating.EffectiveBorderColor;  // Theme default

// Override specific properties
rating.AccentColor = Colors.Purple;
Color newAccent = rating.EffectiveAccentColor;  // Now Colors.Purple
```
