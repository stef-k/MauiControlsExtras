# MaskedEntry

A text input control with format masking for structured data entry.

## Features

- **Format Masks** - Predefined and custom masks
- **Input Validation** - Automatic character validation
- **Prompt Characters** - Show mask format visually
- **Keyboard Navigation** - Full keyboard support
- **IValidatable Support** - Built-in validation

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
| `A` | Required letter (A-Z, a-z) |
| `a` | Optional letter |
| `L` | Required letter, auto uppercase |
| `?` | Optional letter, auto uppercase |
| `&` | Required any character |
| `C` | Optional any character |
| `\` | Escape next character as literal |

Any other character is treated as a literal and displayed as-is in the mask.

## Predefined Mask Constants

The control provides predefined mask constants for common formats:

```csharp
MaskedEntry.PhoneUS      // (000) 000-0000
MaskedEntry.PhoneIntl    // +00 000 000 0000
MaskedEntry.CreditCard   // 0000 0000 0000 0000
MaskedEntry.DateUS       // 00/00/0000
MaskedEntry.DateISO      // 0000-00-00
MaskedEntry.TimeHHMM     // 00:00
MaskedEntry.TimeHHMMSS   // 00:00:00
MaskedEntry.SSN          // 000-00-0000
MaskedEntry.ZipUS        // 00000-9999
MaskedEntry.ZipCA        // A0A 0A0
```

### Usage

```xml
<extras:MaskedEntry Mask="{x:Static extras:MaskedEntry.PhoneUS}" />
```

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

## Prompt Character

The prompt character shows unfilled positions in the mask:

```xml
<extras:MaskedEntry
    Mask="(000) 000-0000"
    PromptChar="_" />
```

## Text Values

- `Text` - The raw unmasked value (user input only)
- `MaskedText` - The formatted value with mask literals applied

```xml
<extras:MaskedEntry
    Mask="(000) 000-0000"
    Text="{Binding RawPhone}" />
<!-- If user types 5551234567, Text = "5551234567", MaskedText = "(555) 123-4567" -->
```

To include mask literals in the `Text` value:

```xml
<extras:MaskedEntry
    Mask="(000) 000-0000"
    IncludeLiterals="True"
    Text="{Binding FormattedPhone}" />
<!-- Text will be "(555) 123-4567" instead of "5551234567" -->
```

## Validation

MaskedEntry implements `IValidatable` for built-in validation support.

```xml
<extras:MaskedEntry
    Mask="(000) 000-0000"
    IsRequired="True"
    RequiredErrorMessage="Phone number is required"
    ShowValidationIcon="True" />
```

### Checking Validation State

```csharp
if (!maskedEntry.IsValid)
{
    foreach (var error in maskedEntry.ValidationErrors)
    {
        Debug.WriteLine(error);
    }
}

// Check if mask is completely filled
if (maskedEntry.IsMaskComplete)
{
    // All required positions have valid input
}
```

## Password Mode

```xml
<extras:MaskedEntry
    Mask="0000"
    IsPassword="True" />
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Tab | Move to next field |
| Backspace | Delete previous character |
| Delete | Delete next character |

## Events

| Event | Description |
|-------|-------------|
| TextChanged | Text value changed |
| Completed | Mask is completely filled or Enter pressed |
| ValidationChanged | IsValid state changed |

## Commands

| Command | Parameter | Description |
|---------|-----------|-------------|
| TextChangedCommand | string (new text) | Fires when text changes |
| CompletedCommand | string (text) | Fires when mask is complete |
| ValidateCommand | ValidationResult | Fires after validation |
| GotFocusCommand | object (control) | Fires when control receives focus |
| LostFocusCommand | object (control) | Fires when control loses focus |
| KeyPressCommand | KeyEventArgs | Fires on key press |

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Mask | string | null | Mask pattern |
| Text | string | null | Raw unmasked text value |
| MaskedText | string | (read-only) | Formatted text with mask applied |
| PromptChar | char | `_` | Character for unfilled positions |
| Placeholder | string | null | Placeholder text when empty |
| IncludeLiterals | bool | false | Include mask literals in Text value |
| IsPassword | bool | false | Obscure input characters |
| IsMaskComplete | bool | (read-only) | True when all required positions filled |

### Validation Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsRequired | bool | false | Whether input is required |
| RequiredErrorMessage | string | "This field is required." | Message when required validation fails |
| ShowValidationIcon | bool | true | Show validation status icon |
| IsValid | bool | (read-only) | Current validation state |
| ValidationErrors | IReadOnlyList&lt;string&gt; | (read-only) | List of validation error messages |

### Inherited from TextStyledControlBase

| Property | Type | Description |
|----------|------|-------------|
| FontFamily | string | Font family name |
| FontSize | double | Font size |
| FontAttributes | FontAttributes | Bold, Italic, etc. |
| TextColor | Color | Text color |
| PlaceholderColor | Color | Placeholder text color |
