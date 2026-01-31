# NumericUpDown API Reference

Full API documentation for the `MauiControlsExtras.Controls.NumericUpDown` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class NumericUpDown : TextStyledControlBase, IValidatable, IKeyboardNavigable
```

## Inheritance

Inherits from [TextStyledControlBase](base-classes.md#textstyledcontrolbase). See base class documentation for inherited styling and typography properties.

## Interfaces

- [IValidatable](interfaces.md#ivalidatable) - Validation support
- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### Value

Gets or sets the current numeric value.

```csharp
public double Value { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `double` | `0` | Yes | TwoWay |

---

#### Minimum

Gets or sets the minimum allowed value.

```csharp
public double Minimum { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `double.MinValue` | Yes |

---

#### Maximum

Gets or sets the maximum allowed value.

```csharp
public double Maximum { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `double.MaxValue` | Yes |

---

#### Step

Gets or sets the increment/decrement step value.

```csharp
public double Step { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `1` | Yes |

---

#### DecimalPlaces

Gets or sets the number of decimal places to display.

```csharp
public int DecimalPlaces { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `0` | Yes |

---

### Display Properties

#### Format

Gets or sets the numeric format string (e.g., "N2", "C", "P").

```csharp
public string? Format { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### Placeholder

Gets or sets placeholder text shown when empty.

```csharp
public string? Placeholder { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### Prefix

Gets or sets text displayed before the value (e.g., "$").

```csharp
public string? Prefix { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### Suffix

Gets or sets text displayed after the value (e.g., "kg").

```csharp
public string? Suffix { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

### Button Properties

#### ButtonPlacement

Gets or sets the position of increment/decrement buttons.

```csharp
public ButtonPlacement ButtonPlacement { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `ButtonPlacement` | `Right` | Yes |

---

#### ShowButtons

Gets or sets whether the increment/decrement buttons are visible.

```csharp
public bool ShowButtons { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### IsReadOnly

Gets or sets whether the control is read-only.

```csharp
public bool IsReadOnly { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

### Behavior Properties

#### AllowNull

Gets or sets whether null/empty values are allowed.

```csharp
public bool AllowNull { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### WrapAround

Gets or sets whether values wrap from max to min and vice versa.

```csharp
public bool WrapAround { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### RepeatDelay

Gets or sets the delay before auto-repeat starts (ms).

```csharp
public int RepeatDelay { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `500` | Yes |

---

#### RepeatInterval

Gets or sets the interval between auto-repeat increments (ms).

```csharp
public int RepeatInterval { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `50` | Yes |

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

#### RequiredErrorMessage

Gets or sets the error message when required validation fails.

```csharp
public string RequiredErrorMessage { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"Value is required"` | Yes |

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

Occurs when the value changes.

```csharp
public event EventHandler<double>? ValueChanged;
```

**Event Args:** The new numeric value.

---

## Commands

### ValueChangedCommand

Executed when the value changes.

```csharp
public ICommand? ValueChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Value | `double` |

---

### IncrementCommand

Executed when increment is requested.

```csharp
public ICommand? IncrementCommand { get; set; }
```

---

### DecrementCommand

Executed when decrement is requested.

```csharp
public ICommand? DecrementCommand { get; set; }
```

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

### Increment()

Increases the value by Step.

```csharp
public void Increment()
```

---

### Decrement()

Decreases the value by Step.

```csharp
public void Decrement()
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

### ButtonPlacement

```csharp
public enum ButtonPlacement
{
    Left,       // Buttons on left side
    Right,      // Buttons on right side (default)
    LeftRight,  // Decrement left, increment right
    Stacked     // Buttons stacked vertically
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Arrow Up | Increment value by step |
| Arrow Down | Decrement value by step |
| Page Up | Increment value by 10 × step |
| Page Down | Decrement value by 10 × step |
| Home | Set value to minimum |
| End | Set value to maximum |

---

## Usage Examples

### Basic Usage

```xml
<extras:NumericUpDown Value="{Binding Quantity, Mode=TwoWay}"
                      Minimum="0"
                      Maximum="100"
                      Step="1" />
```

### With Decimal Places

```xml
<extras:NumericUpDown Value="{Binding Price}"
                      Minimum="0"
                      Maximum="9999.99"
                      Step="0.01"
                      DecimalPlaces="2"
                      Prefix="$" />
```

### With Format String

```xml
<extras:NumericUpDown Value="{Binding Percentage}"
                      Minimum="0"
                      Maximum="1"
                      Step="0.01"
                      Format="P0" />
```

### Different Button Placements

```xml
<!-- Buttons on left and right -->
<extras:NumericUpDown Value="{Binding Count}"
                      ButtonPlacement="LeftRight" />

<!-- Stacked buttons -->
<extras:NumericUpDown Value="{Binding Level}"
                      ButtonPlacement="Stacked" />
```

### With Wrap Around

```xml
<extras:NumericUpDown Value="{Binding Hour}"
                      Minimum="0"
                      Maximum="23"
                      WrapAround="True" />
```

### With Validation

```xml
<extras:NumericUpDown Value="{Binding Age}"
                      Minimum="0"
                      Maximum="150"
                      IsRequired="True"
                      RequiredErrorMessage="Age is required"
                      ValidateCommand="{Binding OnValidated}" />
```

### Code-Behind

```csharp
// Create control programmatically
var numeric = new NumericUpDown
{
    Minimum = 0,
    Maximum = 100,
    Step = 5,
    DecimalPlaces = 0,
    ButtonPlacement = ButtonPlacement.Right
};

// Handle value changes
numeric.ValueChanged += (sender, newValue) =>
{
    Console.WriteLine($"New value: {newValue}");
};

// Programmatic control
numeric.Increment();
numeric.Decrement();

// Validate
var result = numeric.Validate();
if (!result.IsValid)
{
    DisplayErrors(result.Errors);
}
```
