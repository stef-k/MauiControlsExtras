# TokenEntry API Reference

Full API documentation for the `MauiControlsExtras.Controls.TokenEntry` control.

## Namespace

```csharp
using MauiControlsExtras.Controls;
```

## Class Definition

```csharp
public partial class TokenEntry : TextStyledControlBase, IValidatable, IKeyboardNavigable, IClipboardSupport, IContextMenuSupport
```

## Inheritance

Inherits from [TextStyledControlBase](base-classes.md#textstyledcontrolbase). See base class documentation for inherited styling and typography properties.

## Interfaces

- [IValidatable](interfaces.md#ivalidatable) - Validation support
- [IKeyboardNavigable](interfaces.md#ikeyboardnavigable) - Keyboard navigation support
- [IClipboardSupport](interfaces.md#iclipboardsupport) - Copy, cut, and paste operations
- [IContextMenuSupport](interfaces.md#icontextmenusupport) - Right-click and long-press context menus

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

### Clipboard Properties

#### PasteDelimiters

Gets or sets the characters used to split clipboard text during paste operations.

```csharp
public char[] PasteDelimiters { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `char[]` | `[',', ';', '\n', '\t']` | Yes |

---

#### ShowDefaultContextMenu

Gets or sets whether the default Copy/Cut/Paste context menu is shown.

```csharp
public bool ShowDefaultContextMenu { get; set; }
```

| Type | Default | Bindable |
|------|---------|----------|
| `bool` | `true` | Yes |

---

#### CanCopy (Read-only)

Gets whether a copy operation is currently available.

```csharp
public bool CanCopy { get; }
```

| Type | Description |
|------|-------------|
| `bool` | True when a token is selected |

---

#### CanCut (Read-only)

Gets whether a cut operation is currently available.

```csharp
public bool CanCut { get; }
```

| Type | Description |
|------|-------------|
| `bool` | True when a token is selected |

---

#### CanPaste (Read-only)

Gets whether a paste operation is currently available.

```csharp
public bool CanPaste { get; }
```

| Type | Description |
|------|-------------|
| `bool` | True when control is enabled and not at max tokens |

---

#### ContextMenuItems

Gets the collection of custom context menu items.

```csharp
public ContextMenuItemCollection ContextMenuItems { get; }
```

| Type | Description |
|------|-------------|
| `ContextMenuItemCollection` | Custom items shown in context menu |

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

### Copying

Occurs before a copy operation. Can be cancelled.

```csharp
public event EventHandler<TokenClipboardEventArgs>? Copying;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Operation | `TokenClipboardOperation` | `Copy` |
| Tokens | `IReadOnlyList<string>` | Token being copied |
| Content | `string` | Clipboard text |
| Cancel | `bool` | Set to `true` to cancel |

---

### Cutting

Occurs before a cut operation. Can be cancelled.

```csharp
public event EventHandler<TokenClipboardEventArgs>? Cutting;
```

**Event Args:** Same as `Copying`, with `Operation` set to `Cut`.

---

### Pasting

Occurs before a paste operation. Can be cancelled.

```csharp
public event EventHandler<TokenClipboardEventArgs>? Pasting;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Operation | `TokenClipboardOperation` | `Paste` |
| Tokens | `IReadOnlyList<string>` | Tokens that will be added |
| Content | `string` | Raw clipboard text |
| Cancel | `bool` | Set to `true` to cancel |

---

### Pasted

Occurs after a paste operation completes.

```csharp
public event EventHandler<TokenClipboardEventArgs>? Pasted;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Tokens | `IReadOnlyList<string>` | Tokens that were added |
| SkippedTokens | `IReadOnlyList<string>` | Tokens that were skipped |
| SkipReasons | `IReadOnlyDictionary<string, string>` | Why each token was skipped |
| SuccessCount | `int` | Number of tokens added |

---

### ContextMenuOpening

Occurs before the context menu is displayed. Allows modification and cancellation.

```csharp
public event EventHandler<ContextMenuOpeningEventArgs>? ContextMenuOpening;
```

**Event Args:**

| Property | Type | Description |
|----------|------|-------------|
| Items | `ContextMenuItemCollection` | Menu items (modifiable) |
| Position | `Point` | Where menu will appear |
| TargetElement | `object?` | Token that triggered the menu |
| Cancel | `bool` | Set to `true` to cancel |

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

### CopyCommand

Executed after a copy operation.

```csharp
public ICommand? CopyCommand { get; set; }
```

| Parameter | Type | Description |
|-----------|------|-------------|
| Token | `string` | The copied token |

---

### CutCommand

Executed after a cut operation.

```csharp
public ICommand? CutCommand { get; set; }
```

| Parameter | Type | Description |
|-----------|------|-------------|
| Token | `string` | The cut token |

---

### PasteCommand

Executed after a paste operation.

```csharp
public ICommand? PasteCommand { get; set; }
```

| Parameter | Type | Description |
|-----------|------|-------------|
| Args | `TokenClipboardEventArgs` | Results of the paste operation |

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

### Copy()

Copies the selected token to the clipboard.

```csharp
public void Copy()
```

Does nothing if `CanCopy` is false.

---

### Cut()

Cuts the selected token to the clipboard.

```csharp
public void Cut()
```

Does nothing if `CanCut` is false.

---

### Paste()

Pastes tokens from the clipboard (synchronous, fire-and-forget).

```csharp
public void Paste()
```

Does nothing if `CanPaste` is false.

---

### PasteAsync()

Asynchronously pastes tokens from the clipboard.

```csharp
public async Task PasteAsync()
```

| Returns | Description |
|---------|-------------|
| `Task` | Completes when paste operation finishes |

---

### GetClipboardContent()

Gets the content that would be copied to the clipboard.

```csharp
public object? GetClipboardContent()
```

| Returns | Description |
|---------|-------------|
| `object?` | The selected token, or null if nothing can be copied |

---

### ShowContextMenu(Point? position)

Programmatically shows the context menu.

```csharp
public void ShowContextMenu(Point? position = null)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| position | `Point?` | Position to show menu, or null for default |

---

### ShowContextMenuAsync(Point? position, string? targetToken)

Asynchronously shows the context menu with optional target token context.

```csharp
public async Task ShowContextMenuAsync(Point? position = null, string? targetToken = null)
```

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
| Ctrl+C / ⌘C | Copy selected token |
| Ctrl+X / ⌘X | Cut selected token |
| Ctrl+V / ⌘V | Paste tokens from clipboard |

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
