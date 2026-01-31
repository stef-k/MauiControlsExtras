# TokenEntry API Reference

Full API documentation for the `MauiControlsExtras.Controls.TokenEntry` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class TokenEntry : TextStyledControlBase, IValidatable, IKeyboardNavigable
```

## Inheritance

Inherits from [TextStyledControlBase](base-classes.md#textstyledcontrolbase). See base class documentation for inherited styling and typography properties.

## Interfaces

- [IValidatable](interfaces.md#ivalidatable) - Validation support
- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support

---

## Properties

### Core Properties

#### Tokens

Gets or sets the collection of tokens.

```csharp
public IList<string>? Tokens { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `IList<string>?` | `null` | Yes | TwoWay |

---

#### Text

Gets or sets the current text input before it becomes a token.

```csharp
public string? Text { get; set; }
```

| Type | Default | Bindable | Binding Mode |
|------|---------|----------|--------------|
| `string?` | `null` | Yes | TwoWay |

---

#### Delimiter

Gets or sets the character(s) that trigger token creation.

```csharp
public string Delimiter { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `","` | Yes |

---

#### MaxTokens

Gets or sets the maximum number of tokens allowed.

```csharp
public int MaxTokens { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `0` (unlimited) | Yes |

---

#### AllowDuplicates

Gets or sets whether duplicate tokens are allowed.

```csharp
public bool AllowDuplicates { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

### Suggestions Properties

#### SuggestionsSource

Gets or sets the collection of suggestions for autocomplete.

```csharp
public IEnumerable<string>? SuggestionsSource { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `IEnumerable<string>?` | `null` | Yes |

---

#### ShowSuggestions

Gets or sets whether suggestions dropdown is shown.

```csharp
public bool ShowSuggestions { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### MinimumPrefixLength

Gets or sets minimum characters before showing suggestions.

```csharp
public int MinimumPrefixLength { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `1` | Yes |

---

#### MaxSuggestions

Gets or sets the maximum number of suggestions to display.

```csharp
public int MaxSuggestions { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `5` | Yes |

---

### Appearance Properties

#### Placeholder

Gets or sets placeholder text when empty.

```csharp
public string? Placeholder { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string?` | `null` | Yes |

---

#### TokenBackgroundColor

Gets or sets the background color of tokens.

```csharp
public Color? TokenBackgroundColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses accent with alpha) | Yes |

---

#### TokenTextColor

Gets or sets the text color of tokens.

```csharp
public Color? TokenTextColor { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `Color?` | `null` (uses foreground) | Yes |

---

#### TokenCornerRadius

Gets or sets the corner radius of token chips.

```csharp
public double TokenCornerRadius { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `12` | Yes |

---

#### TokenSpacing

Gets or sets the spacing between tokens.

```csharp
public double TokenSpacing { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `double` | `4` | Yes |

---

#### ShowRemoveButton

Gets or sets whether tokens display a remove button.

```csharp
public bool ShowRemoveButton { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### RemoveButtonIcon

Gets or sets the icon for the remove button.

```csharp
public string RemoveButtonIcon { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"×"` | Yes |

---

### Behavior Properties

#### CreateTokenOnLostFocus

Gets or sets whether pending text becomes a token when focus is lost.

```csharp
public bool CreateTokenOnLostFocus { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### TrimTokens

Gets or sets whether tokens are trimmed of whitespace.

```csharp
public bool TrimTokens { get; set; }
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

### Validation Properties

#### IsRequired

Gets or sets whether at least one token is required.

```csharp
public bool IsRequired { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `false` | Yes |

---

#### MinimumTokens

Gets or sets the minimum number of tokens required.

```csharp
public int MinimumTokens { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `int` | `0` | Yes |

---

#### RequiredErrorMessage

Gets or sets the error message when required validation fails.

```csharp
public string RequiredErrorMessage { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `string` | `"At least one item is required"` | Yes |

---

#### IsValid (Read-only)

Gets whether the current tokens pass validation.

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

### TokenAdded

Occurs when a token is added.

```csharp
public event EventHandler<TokenEventArgs>? TokenAdded;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Token | `string` | The added token |
| Index | `int` | Position in the tokens list |

---

### TokenRemoved

Occurs when a token is removed.

```csharp
public event EventHandler<TokenEventArgs>? TokenRemoved;
```

---

### TokensChanged

Occurs when the tokens collection changes.

```csharp
public event EventHandler<TokensChangedEventArgs>? TokensChanged;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| AddedTokens | `IReadOnlyList<string>` | Newly added tokens |
| RemovedTokens | `IReadOnlyList<string>` | Removed tokens |
| AllTokens | `IReadOnlyList<string>` | Current token list |

---

### SuggestionSelected

Occurs when a suggestion is selected.

```csharp
public event EventHandler<string>? SuggestionSelected;
```

---

## Commands

### TokenAddedCommand

Executed when a token is added.

```csharp
public ICommand? TokenAddedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Token | `string` |

---

### TokenRemovedCommand

Executed when a token is removed.

```csharp
public ICommand? TokenRemovedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Token | `string` |

---

### TokensChangedCommand

Executed when tokens change.

```csharp
public ICommand? TokensChangedCommand { get; set; }
```

| Parameter | Type |
|-----------|------|
| Args | `TokensChangedEventArgs` |

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

### AddToken(string token)

Adds a token to the collection.

```csharp
public bool AddToken(string token)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| token | `string` | Token to add |

| Returns | Description |
|---------|-------------|
| `bool` | True if token was added, false if duplicate or max reached |

---

### RemoveToken(string token)

Removes a token from the collection.

```csharp
public bool RemoveToken(string token)
```

---

### RemoveTokenAt(int index)

Removes the token at the specified index.

```csharp
public void RemoveTokenAt(int index)
```

---

### Clear()

Removes all tokens.

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

## Keyboard Shortcuts

| Key | Description |
|-----|-------------|
| Enter | Create token from current text |
| Comma (or custom delimiter) | Create token from current text |
| Backspace (empty input) | Select/remove last token |
| Delete | Remove selected token |
| Arrow Left | Select previous token |
| Arrow Right | Select next token or move to input |
| Arrow Up/Down | Navigate suggestions |
| Escape | Close suggestions dropdown |
| Tab | Accept first suggestion or move focus |

---

## Usage Examples

### Basic Tag Input

```xml
<extras:TokenEntry Tokens="{Binding Tags, Mode=TwoWay}"
                   Placeholder="Add tags..."
                   Delimiter="," />
```

### Email Recipients

```xml
<extras:TokenEntry Tokens="{Binding Recipients}"
                   Placeholder="Enter email addresses"
                   Delimiter=",; "
                   AllowDuplicates="False"
                   TokenAddedCommand="{Binding OnRecipientAddedCommand}" />
```

### With Suggestions

```xml
<extras:TokenEntry Tokens="{Binding SelectedSkills}"
                   SuggestionsSource="{Binding AvailableSkills}"
                   ShowSuggestions="True"
                   MinimumPrefixLength="2"
                   MaxSuggestions="10"
                   Placeholder="Search skills..." />
```

### Limited Tokens

```xml
<extras:TokenEntry Tokens="{Binding Categories}"
                   MaxTokens="5"
                   Placeholder="Up to 5 categories" />
```

### Custom Appearance

```xml
<extras:TokenEntry Tokens="{Binding Labels}"
                   TokenBackgroundColor="#E3F2FD"
                   TokenTextColor="#1565C0"
                   TokenCornerRadius="8"
                   TokenSpacing="6"
                   RemoveButtonIcon="✕"
                   AccentColor="#2196F3" />
```

### With Validation

```xml
<extras:TokenEntry Tokens="{Binding RequiredTags}"
                   IsRequired="True"
                   MinimumTokens="2"
                   RequiredErrorMessage="Please add at least 2 tags"
                   ValidateCommand="{Binding OnValidatedCommand}" />
```

### Read-Only Display

```xml
<extras:TokenEntry Tokens="{Binding SelectedTags}"
                   IsReadOnly="True"
                   ShowRemoveButton="False" />
```

### Code-Behind

```csharp
// Create token entry
var tokenEntry = new TokenEntry
{
    Delimiter = ", ",
    MaxTokens = 10,
    AllowDuplicates = false,
    ShowSuggestions = true,
    MinimumPrefixLength = 2
};

// Set suggestions
tokenEntry.SuggestionsSource = new List<string>
{
    "JavaScript", "TypeScript", "Python", "C#", "Java",
    "React", "Angular", "Vue", "Node.js", "ASP.NET"
};

// Initialize with tokens
tokenEntry.Tokens = new ObservableCollection<string>
{
    "C#", "ASP.NET"
};

// Handle events
tokenEntry.TokenAdded += (sender, args) =>
{
    Console.WriteLine($"Added: {args.Token}");
};

tokenEntry.TokenRemoved += (sender, args) =>
{
    Console.WriteLine($"Removed: {args.Token}");
};

tokenEntry.TokensChanged += (sender, args) =>
{
    Console.WriteLine($"Tokens: {string.Join(", ", args.AllTokens)}");
};

// Programmatic control
tokenEntry.AddToken("SQL");
tokenEntry.RemoveToken("Java");

// Validate
var result = tokenEntry.Validate();
if (!result.IsValid)
{
    ShowErrors(result.Errors);
}
```

### MVVM with ObservableCollection

```csharp
// ViewModel
public class TaggingViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> _tags = new();

    [ObservableProperty]
    private IEnumerable<string> _suggestedTags;

    public TaggingViewModel()
    {
        SuggestedTags = LoadAvailableTags();
    }

    [RelayCommand]
    private void OnTokenAdded(string token)
    {
        // Validate or process new token
        LogAnalytics("tag_added", token);
    }

    [RelayCommand]
    private void OnTokenRemoved(string token)
    {
        LogAnalytics("tag_removed", token);
    }
}
```
