# NumericUpDown

A numeric input control with increment/decrement buttons.

## Features

- **Increment/Decrement** - Buttons to adjust value
- **Min/Max Constraints** - Enforce value limits
- **Step Size** - Configurable increment amount
- **Formatting** - Custom number formats
- **Validation** - Built-in validation support
- **Keyboard Navigation** - Full keyboard support

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

## Large Step

```xml
<extras:NumericUpDown
    Value="{Binding Quantity}"
    Step="1"
    LargeStep="10" />
```

Use Page Up/Down for large step increments.

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ↑ | Increment |
| ↓ | Decrement |
| Page Up | Large increment |
| Page Down | Large decrement |
| Home | Set to minimum |
| End | Set to maximum |
| Enter | Commit value |
| Escape | Revert |
| Mouse Wheel | Increment/Decrement |

## Events

| Event | Description |
|-------|-------------|
| ValueChanged | Value has changed |
| Incremented | Value was incremented |
| Decremented | Value was decremented |

## Commands

| Command | Description |
|---------|-------------|
| ValueChangedCommand | Execute when value changes |
| IncrementCommand | Execute on increment |
| DecrementCommand | Execute on decrement |

## Properties

| Property | Type | Description |
|----------|------|-------------|
| Value | double | Current value |
| Minimum | double | Minimum allowed value |
| Maximum | double | Maximum allowed value |
| Step | double | Increment amount |
| LargeStep | double | Page increment amount |
| Format | string | Number format string |
| IsReadOnly | bool | Prevent editing |
