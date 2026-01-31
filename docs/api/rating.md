# Rating API Reference

Full API documentation for the `MauiControlsExtras.Controls.Rating` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class Rating : StyledControlBase, IValidatable, IKeyboardNavigable
```

## Inheritance

Inherits from [StyledControlBase](base-classes.md#styledcontrolbase). See base class documentation for inherited styling properties.

## Interfaces

- [IValidatable](interfaces.md#ivalidatable) - Validation support
- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### Value

Gets or sets the current rating value.

```csharp
public double Value { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `double` | `0` | Yes | TwoWay |

---

#### Maximum

Gets or sets the maximum rating value.

```csharp
public double Maximum { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `5` | Yes |

---

#### Precision

Gets or sets the rating precision (Full, Half, or Quarter).

```csharp
public RatingPrecision Precision { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `RatingPrecision` | `Full` | Yes |

---

#### IsReadOnly

Gets or sets whether the rating is read-only.

```csharp
public bool IsReadOnly { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

### Appearance Properties

#### Icon

Gets or sets the icon type to display (Star, Heart, or Custom).

```csharp
public RatingIcon Icon { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `RatingIcon` | `Star` | Yes |

---

#### CustomIcon

Gets or sets the custom icon glyph character when Icon is Custom.

```csharp
public string? CustomIcon { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### IconSize

Gets or sets the size of each rating icon.

```csharp
public double IconSize { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `24` | Yes |

---

#### Spacing

Gets or sets the spacing between icons.

```csharp
public double Spacing { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `4` | Yes |

---

#### FilledColor

Gets or sets the color for filled (selected) icons.

```csharp
public Color? FilledColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses AccentColor) | Yes |

---

#### EmptyColor

Gets or sets the color for empty (unselected) icons.

```csharp
public Color? EmptyColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses muted foreground) | Yes |

---

### Validation Properties

#### IsRequired

Gets or sets whether a value is required.

```csharp
public bool IsRequired { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### MinimumValue

Gets or sets the minimum valid rating value (when IsRequired is true).

```csharp
public double MinimumValue { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `1` | Yes |

---

#### RequiredErrorMessage

Gets or sets the error message when required validation fails.

```csharp
public string RequiredErrorMessage { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Rating is required"` | Yes |

---

#### IsValid (Read-only)

Gets whether the current value passes validation.

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

### ValueChanged

Occurs when the rating value changes.

```csharp
public event EventHandler<double>? ValueChanged;
```

**Event Args:** The new rating value.

---

## Commands

### ValueChangedCommand

Executed when the rating value changes.

```csharp
public ICommand? ValueChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Value | `double` |

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

### Validate()

Performs validation and returns the result.

```csharp
public ValidationResult Validate()
```

| Returns | Description |
|---------|-------------|
| `ValidationResult` | Contains IsValid and any error messages |

---

### Clear()

Resets the rating to zero.

```csharp
public void Clear()
```

---

## Enumerations

### RatingPrecision

```csharp
public enum RatingPrecision
{
    Full,     // Only whole values (1, 2, 3, etc.)
    Half,     // Half values allowed (1, 1.5, 2, etc.)
    Quarter   // Quarter values allowed (1, 1.25, 1.5, etc.)
}
```

### RatingIcon

```csharp
public enum RatingIcon
{
    Star,     // ★ star icons
    Heart,    // ♥ heart icons
    Custom    // Use CustomIcon property
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Arrow Left | Decrease rating by precision step |
| Arrow Right | Increase rating by precision step |
| Home | Set rating to minimum (0 or MinimumValue) |
| End | Set rating to maximum |
| 0-9 | Set rating directly (0-5 for 5-star) |
| Delete | Clear rating to 0 |

---

## Usage Examples

### Basic Rating

```xml
<extras:Rating Value="{Binding UserRating, Mode=TwoWay}"
               Maximum="5"
               Precision="Half" />
```

### Custom Appearance

```xml
<extras:Rating Value="3.5"
               Maximum="5"
               Icon="Heart"
               IconSize="32"
               Spacing="8"
               FilledColor="#E91E63"
               EmptyColor="#FFCDD2" />
```

### With Custom Icon

```xml
<extras:Rating Value="{Binding Score}"
               Maximum="10"
               Icon="Custom"
               CustomIcon="●"
               FilledColor="#FFD700"
               EmptyColor="#E0E0E0" />
```

### With Validation

```xml
<extras:Rating Value="{Binding Rating}"
               IsRequired="True"
               MinimumValue="1"
               RequiredErrorMessage="Please rate this item"
               ValidateCommand="{Binding OnRatingValidated}" />
```

### Read-Only Display

```xml
<extras:Rating Value="{Binding AverageRating}"
               IsReadOnly="True"
               Precision="Quarter" />
```

### Code-Behind

```csharp
// Create rating programmatically
var rating = new Rating
{
    Maximum = 5,
    Precision = RatingPrecision.Half,
    Icon = RatingIcon.Star,
    IconSize = 28
};

// Handle value changes
rating.ValueChanged += (sender, newValue) =>
{
    Console.WriteLine($"New rating: {newValue}");
};

// Set value
rating.Value = 4.5;

// Validate
var result = rating.Validate();
if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine(error);
    }
}
```
