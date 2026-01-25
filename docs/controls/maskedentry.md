# MaskedEntry

A text input control with format masking for structured data entry.

## Features

- **Format Masks** - Predefined and custom masks
- **Input Validation** - Automatic character validation
- **Placeholder Characters** - Show mask format visually
- **Keyboard Navigation** - Full keyboard support

## Basic Usage

```xml
<extras:MaskedEntry
    Mask="(000) 000-0000"
    Text="{Binding PhoneNumber, Mode=TwoWay}"
    Placeholder="Phone number" />
```

## Mask Characters

| Character | Description |
|-----------|-------------|
| `0` | Required digit (0-9) |
| `9` | Optional digit |
| `#` | Digit or space |
| `L` | Required letter (A-Z, a-z) |
| `?` | Optional letter |
| `&` | Required character |
| `C` | Optional character |
| `A` | Alphanumeric (required) |
| `a` | Alphanumeric (optional) |

## Common Masks

### Phone Numbers

```xml
<extras:MaskedEntry Mask="(000) 000-0000" />
<extras:MaskedEntry Mask="+0 (000) 000-0000" />
```

### Dates

```xml
<extras:MaskedEntry Mask="00/00/0000" />
<extras:MaskedEntry Mask="0000-00-00" />
```

### Credit Cards

```xml
<extras:MaskedEntry Mask="0000 0000 0000 0000" />
```

### Social Security

```xml
<extras:MaskedEntry Mask="000-00-0000" />
```

### ZIP Codes

```xml
<extras:MaskedEntry Mask="00000" />
<extras:MaskedEntry Mask="00000-0000" />
```

### Time

```xml
<extras:MaskedEntry Mask="00:00" />
<extras:MaskedEntry Mask="00:00:00" />
```

## Placeholder Character

```xml
<extras:MaskedEntry
    Mask="(000) 000-0000"
    PromptChar="_"
    ShowPrompt="True" />
```

## Getting Unmasked Value

```xml
<extras:MaskedEntry
    Mask="(000) 000-0000"
    Text="{Binding FormattedPhone}"
    UnmaskedText="{Binding RawPhone}" />
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| ← / → | Move between positions |
| Home / End | Start/end of input |
| Tab | Next mask section |
| Backspace / Delete | Clear character |

## Events

| Event | Description |
|-------|-------------|
| TextChanged | Text value changed |
| Completed | User pressed Enter |
| MaskCompleted | All required characters entered |

## Properties

| Property | Type | Description |
|----------|------|-------------|
| Mask | string | Mask pattern |
| Text | string | Formatted text value |
| UnmaskedText | string | Raw text without mask |
| PromptChar | char | Placeholder character |
| ShowPrompt | bool | Show placeholders |
| IncludeLiterals | bool | Include mask literals in value |
