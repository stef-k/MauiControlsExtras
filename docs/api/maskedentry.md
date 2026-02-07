# MaskedEntry API Reference

API reference for `MauiControlsExtras.Controls.MaskedEntry`.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class MaskedEntry : TextStyledControlBase, IValidatable, IKeyboardNavigable, IClipboardSupport
```

## Core Properties

```csharp
public string? Text { get; set; }
public string? Mask { get; set; }
public char PromptChar { get; set; }
public string? Placeholder { get; set; }
public bool IncludeLiterals { get; set; }
public bool IsPassword { get; set; }
public bool IsMaskComplete { get; }
public string MaskedText { get; }
public string DisplayText { get; }
public Keyboard EntryKeyboard { get; }
```

## Validation Properties

```csharp
public bool IsRequired { get; set; }
public string RequiredErrorMessage { get; set; }
public bool ShowValidationIcon { get; set; }
public bool IsValid { get; }
public IReadOnlyList<string> ValidationErrors { get; }
```

## Commands

```csharp
public ICommand? TextChangedCommand { get; set; }
public ICommand? CompletedCommand { get; set; }
public ICommand? ValidateCommand { get; set; }
public ICommand? CopyCommand { get; set; }
public ICommand? CutCommand { get; set; }
public ICommand? PasteCommand { get; set; }
```

## Events

```csharp
public event EventHandler<TextChangedEventArgs>? TextChanged;
public event EventHandler? Completed;
public event EventHandler<bool>? ValidationChanged;
```

## Methods

```csharp
public void Clear();
public new bool Focus();
public new void Unfocus();
public ValidationResult Validate();
```

## Predefined Mask Constants

```csharp
public const string CreditCard = "0000 0000 0000 0000";
public const string DateUS = "00/00/0000";
public const string DateISO = "0000-00-00";
public const string TimeHHMM = "00:00";
public const string TimeHHMMSS = "00:00:00";
```

## Usage Examples

### Date Input

```xml
<extras:MaskedEntry Mask="00/00/0000"
                    Text="{Binding DateValue, Mode=TwoWay}"
                    Placeholder="MM/DD/YYYY" />
```

### Credit Card Input

```xml
<extras:MaskedEntry Mask="0000 0000 0000 0000"
                    Text="{Binding CardNumber, Mode=TwoWay}"
                    Placeholder="4111 1111 1111 1111" />
```

### Include Literals

```xml
<extras:MaskedEntry Mask="0000 0000 0000 0000"
                    IncludeLiterals="True"
                    Text="{Binding FormattedCardNumber, Mode=TwoWay}" />
```
