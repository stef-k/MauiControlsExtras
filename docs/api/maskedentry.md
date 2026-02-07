# MaskedEntry API Reference

Full API documentation for the `MauiControlsExtras.Controls.MaskedEntry` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class MaskedEntry : TextStyledControlBase, IValidatable, IKeyboardNavigable
```

## Inheritance

Inherits from [TextStyledControlBase](base-classes.md#textstyledcontrolbase). See base class documentation for inherited styling and typography properties.

## Interfaces

- [IValidatable](interfaces.md#ivalidatable) - Validation support
- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### Text

Gets or sets the current text value (includes mask literals if IncludeLiterals is true).

```csharp
public string? Text { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `string?` | `null` | Yes | TwoWay |

---

#### Value

Gets or sets the raw value without mask literals.

```csharp
public string? Value { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `string?` | `null` | Yes | TwoWay |

---

#### Mask

Gets or sets the input mask pattern.

```csharp
public string? Mask { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

**Mask Characters:**

| Character | Description |
|-----------|-------------|
| `0` | Required digit (0-9) |
| `9` | Optional digit (0-9) |
| `#` | Digit or space |
| `L` | Required letter (a-z, A-Z) |
| `?` | Optional letter |
| `A` | Required alphanumeric |
| `a` | Optional alphanumeric |
| `&` | Required any character |
| `C` | Optional any character |
| `\` | Escape next character (use literal) |

---

#### PromptChar

Gets or sets the character shown for unfilled positions.

```csharp
public char PromptChar { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `char` | `'_'` | Yes |

---

#### IncludeLiterals

Gets or sets whether mask literals are included in Text/Value.

```csharp
public bool IncludeLiterals { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### IncludePrompt

Gets or sets whether prompt characters are included in Text/Value.

```csharp
public bool IncludePrompt { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

### Display Properties

#### Placeholder

Gets or sets placeholder text shown when empty and unfocused.

```csharp
public string? Placeholder { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### ShowMaskOnFocus

Gets or sets whether the mask is shown only when focused.

```csharp
public bool ShowMaskOnFocus { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

### Behavior Properties

#### AllowOverwrite

Gets or sets whether typing overwrites existing characters.

```csharp
public bool AllowOverwrite { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### IsReadOnly

Gets or sets whether the entry is read-only.

```csharp
public bool IsReadOnly { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### SelectAllOnFocus

Gets or sets whether all text is selected when focused.

```csharp
public bool SelectAllOnFocus { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

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

#### IsComplete (Read-only)

Gets whether all required mask positions are filled.

```csharp
public bool IsComplete { get; }
```

| Type | Bindable |
|------|----------|
| `bool` | Yes |

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

### TextChanged

Occurs when the text value changes.

```csharp
public event EventHandler<TextChangedEventArgs>? TextChanged;
```

---

### ValueChanged

Occurs when the raw value changes.

```csharp
public event EventHandler<string?>? ValueChanged;
```

---

### Completed

Occurs when the user completes entry (Enter key or all positions filled).

```csharp
public event EventHandler? Completed;
```

---

### MaskCompleted

Occurs when all required mask positions are filled.

```csharp
public event EventHandler? MaskCompleted;
```

---

## Commands

### TextChangedCommand

Executed when text changes.

```csharp
public ICommand? TextChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `TextChangedEventArgs` |

---

### CompletedCommand

Executed when entry is completed.

```csharp
public ICommand? CompletedCommand { get; set; }
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

### Clear()

Clears all input, leaving only the mask.

```csharp
public void Clear()
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

### Focus()

Sets focus to the entry.

```csharp
public new bool Focus()
```

---

### SelectAll()

Selects all text in the entry.

```csharp
public void SelectAll()
```

---

## Predefined Masks

The control provides static constants for common mask patterns:

```csharp
public static class MaskedEntry
{
    public const string PhoneUS = "(000) 000-0000";
    public const string PhoneInternational = "+00 000 000 0000";
    public const string SSN = "000-00-0000";
    public const string ZipCode = "00000";
    public const string ZipCodePlus4 = "00000-0000";
    public const string CreditCard = "0000 0000 0000 0000";
    public const string Date = "00/00/0000";
    public const string Time12 = "00:00 LL";
    public const string Time24 = "00:00";
    public const string Currency = "$0,000.00";
}
```

---

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Arrow Left/Right | Move cursor |
| Home | Move to start |
| End | Move to end |
| Delete | Delete character at cursor |
| Backspace | Delete character before cursor |
| Ctrl+A | Select all |

---

## Usage Examples

### Phone Number

```xml
<extras:MaskedEntry Mask="(000) 000-0000"
                    Text="{Binding PhoneNumber, Mode=TwoWay}"
                    Placeholder="Enter phone number"
                    PromptChar="_" />
```

### Using Predefined Mask

```xml
<extras:MaskedEntry Mask="{x:Static extras:MaskedEntry.CreditCard}"
                    Value="{Binding CardNumber}"
                    PromptChar="•" />
```

### Date Input

```xml
<extras:MaskedEntry Mask="00/00/0000"
                    Value="{Binding DateOfBirth}"
                    Placeholder="MM/DD/YYYY"
                    IncludeLiterals="False" />
```

### Social Security Number

```xml
<extras:MaskedEntry Mask="000-00-0000"
                    Value="{Binding SSN}"
                    IsRequired="True"
                    RequiredErrorMessage="SSN is required"
                    ValidateCommand="{Binding OnValidatedCommand}" />
```

### Currency

```xml
<extras:MaskedEntry Mask="$0,000.00"
                    Value="{Binding Amount}"
                    FontFamily="Consolas" />
```

### Custom Mask with Letters

```xml
<!-- License plate format: ABC-1234 -->
<extras:MaskedEntry Mask="LLL-0000"
                    Value="{Binding LicensePlate}"
                    Placeholder="License plate" />
```

### Without Literals in Value

```xml
<!-- Value will be "1234567890" instead of "(123) 456-7890" -->
<extras:MaskedEntry Mask="(000) 000-0000"
                    Value="{Binding RawPhone}"
                    IncludeLiterals="False" />
```

### Code-Behind

```csharp
// Create masked entry
var entry = new MaskedEntry
{
    Mask = "(000) 000-0000",
    PromptChar = '_',
    IncludeLiterals = false,
    IsRequired = true
};

// Handle events
entry.TextChanged += (sender, args) =>
{
    Console.WriteLine($"Text: {args.NewTextValue}");
};

entry.MaskCompleted += (sender, args) =>
{
    // All required positions filled
    ProcessPhoneNumber(entry.Value);
};

entry.Completed += (sender, args) =>
{
    // User pressed Enter
    if (entry.IsComplete)
    {
        SubmitForm();
    }
};

// Validate
var result = entry.Validate();
if (!result.IsValid)
{
    ShowErrors(result.Errors);
}

// Check completion status
if (entry.IsComplete)
{
    var rawValue = entry.Value; // Without literals
    var fullText = entry.Text;  // With literals
}
```

### Dynamic Mask

```csharp
// Change mask based on selection
void OnCountryChanged(string country)
{
    maskedEntry.Mask = country switch
    {
        "US" => "(000) 000-0000",
        "UK" => "+44 0000 000000",
        "DE" => "+49 000 00000000",
        _ => "0000000000"
    };
    maskedEntry.Clear();
}
```
