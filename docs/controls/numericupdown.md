# NumericUpDown

A numeric input control with increment/decrement buttons.

## Features

- **Increment/Decrement** - Buttons to adjust value
- **Min/Max Constraints** - Enforce value limits
- **Step Size** - Configurable increment amount
- **Formatting** - Custom number formats
- **Validation** - Built-in validation support
- **Keyboard Navigation** - Full keyboard support
- **Button Placement** - Configurable button positions

## Basic Usage

```xml
<extras:NumericUpDown
    Value="{Binding Quantity, Mode=TwoWay}"
    Minimum="0"
    Maximum="100"
    Step="1" />
```

## Formatting

```xml
<extras:NumericUpDown
    Value="{Binding Price}"
    Format="C2"
    Minimum="0"
    Maximum="10000" />
```

### Common Formats

| Format | Example |
|--------|---------|
| `N0` | 1,234 |
| `N2` | 1,234.56 |
| `C0` | $1,234 |
| `C2` | $1,234.56 |
| `P0` | 50% |
| `P2` | 50.00% |

## Decimal Places

Use `DecimalPlaces` to control the number of decimal places displayed when `Format` is not set:

```xml
<extras:NumericUpDown
    Value="{Binding Temperature}"
    DecimalPlaces="2"
    Step="0.1" />
```

## Button Placement

The `ButtonPlacement` property controls where the increment/decrement buttons appear:

```xml
<extras:NumericUpDown
    Value="{Binding Quantity}"
    ButtonPlacement="LeftAndRight" />
```

### ButtonPlacement Values

| Value | Description |
|-------|-------------|
| `Right` | Buttons stacked vertically on the right side (default) |
| `Left` | Buttons stacked vertically on the left side |
| `LeftAndRight` | Decrement on left, increment on right |
| `Stacked` | Buttons stacked vertically (up on top, down on bottom) on the right |

## Placeholder

Display placeholder text when the value is null:

```xml
<extras:NumericUpDown
    Value="{Binding OptionalQuantity}"
    Placeholder="Enter quantity..." />
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ↑ | Increment by Step |
| ↓ | Decrement by Step |
| Page Up | Large increment (Step × 10) |
| Page Down | Large decrement (Step × 10) |
| Home | Set to minimum |
| End | Set to maximum |
| Enter | Commit value |
| Escape | Revert |
| Mouse Wheel | Increment/Decrement |

## Events

| Event | Description |
|-------|-------------|
| ValueChanged | Value has changed |

## Commands

| Command | Type | Description |
|---------|------|-------------|
| ValueChangedCommand | ICommand | Execute when value changes |
| ValueChangedCommandParameter | object | Parameter passed to ValueChangedCommand |
| IncrementCommand | ICommand | Execute when value is incremented |
| DecrementCommand | ICommand | Execute when value is decremented |
| ValidateCommand | ICommand | Execute when validation occurs |

## Validation

NumericUpDown implements `IValidatable` for built-in validation support.

```xml
<extras:NumericUpDown
    Value="{Binding Quantity}"
    IsRequired="True"
    RequiredErrorMessage="Quantity is required"
    ValidateCommand="{Binding ValidateQuantityCommand}" />
```

### Checking Validation State

```csharp
if (!numericUpDown.IsValid)
{
    foreach (var error in numericUpDown.ValidationErrors)
    {
        Debug.WriteLine(error);
    }
}

// Trigger validation manually
var result = numericUpDown.Validate();
```

### Validation Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsRequired | bool | false | Whether a value is required |
| RequiredErrorMessage | string | "This field is required." | Error message when required but null |
| IsValid | bool | (read-only) | Current validation state |
| ValidationErrors | IReadOnlyList&lt;string&gt; | (read-only) | List of validation error messages |
| ValidateCommand | ICommand | null | Command executed when validation occurs |

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Value | double? | null | Current value (nullable) |
| Minimum | double | double.MinValue | Minimum allowed value |
| Maximum | double | double.MaxValue | Maximum allowed value |
| Step | double | 1.0 | Increment/decrement amount |
| DecimalPlaces | int | 0 | Number of decimal places to display |
| Format | string | null | Custom number format string (e.g., "C2") |
| ButtonPlacement | ButtonPlacement | Right | Button position style |
| Placeholder | string | null | Placeholder text when value is null |
| IsReadOnly | bool | false | Prevent direct text editing |
| IsRequired | bool | false | Whether a value is required |
| RequiredErrorMessage | string | "This field is required." | Error message for required validation |
